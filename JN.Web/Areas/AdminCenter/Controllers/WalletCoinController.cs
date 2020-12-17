using JN.Data.Service;
using JN.Services.Manager;
using JN.Services.Tool;
using MvcCore.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class WalletCoinController : BaseController 
    {
        private static List<Data.SysParam> cacheSysParam = null; 
        private readonly IUserService UserService;
        private readonly IRaCoinService RaCoinService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly ICurrencyService CurrencyService;

        public WalletCoinController(ISysDBTool SysDBTool, 
            IUserService UserService,
            IRaCoinService RaCoinService, 
            IActLogService ActLogService,
             ICurrencyService CurrencyService, 
            IWalletLogService WalletLogService)
        {
            this.UserService = UserService;
            this.RaCoinService = RaCoinService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.CurrencyService = CurrencyService;
            this.WalletLogService = WalletLogService;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();

        }

        #region 币种转出
        public ActionResult TakeCashRa(int? status, int? page)
        {
            ActMessage = "提币明细";
            int Status = status ?? 0;
            var list = RaCoinService.ListInclude(x => x.OutTable_Currency).Where(x => x.Status == Status && x.Direction == "out").OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            int curid = Request["curid"].ToInt();
            if (curid != 0)
            {
                list = list.Where(x => x.CurID == curid);
            }
            var orderlist = list.OrderByDescending(x => x.ID);
            if (Status == 3)
            {
                orderlist = list.OrderByDescending(x => x.PassTime);
            }
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult TakeCashRaCommand(int id)
        {
            ActMessage = "币种提币操作（审核、拒绝）";
            string CacheKey = "TakeCashRa" + id.ToString();
            if (CacheExtensions.CheckCache(CacheKey))//查找缓存是否存在
            {
                ViewBag.ErrorMsg = "正在处理数据,请耐心等待";
                return View("Error");
            }
            CacheExtensions.SetCache(CacheKey, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 10);//每10分钟 

            string commandtype = Request["commandtype"];
            var model = RaCoinService.Single(id);
            if (model == null)
            {
                ViewBag.ErrorMsg = "无效的ID";
                CacheExtensions.ClearCache(CacheKey);//移除缓存
                return View("Error");
            }
            if (commandtype.ToLower() == "dopass")
            {
                if (model.Status == (int)JN.Data.Enum.TakeCaseStatus.Wait)
                {
                    int c = model.CurID;
                    var curmodel = CurrencyService.Single(x => x.ID == c);
                    if ((curmodel.IsAutomatic ?? false))//是否是自动的
                    {
                        ReturnResult<string> result = new ReturnResult<string>();
                        result = WalletsAPI.WalletTransfer(curmodel, model.WalletAddress, model.ActualDrawMoney, model.ID.ToString(), model.UserName, model.Tag);
                        //转账成功，保存交易ID
                        if (result.Status == ReturnResultStatus.Succeed.GetShortValue())
                        {
                            model.Status = (int)JN.Data.Enum.TakeCaseStatus.Deal;
                            model.PassTime = DateTime.Now;
                            model.Txid = result.Data;
                            RaCoinService.Update(model);
                            SysDBTool.Commit();
                        }
                        else
                        {
                            ViewBag.ErrorMsg = result.Message;
                            CacheExtensions.ClearCache(CacheKey);//移除缓存
                            return View("Error");
                        }
                    }
                    else
                    {
                        model.Status = (int)JN.Data.Enum.TakeCaseStatus.Deal;
                        model.PassTime = DateTime.Now;
                        RaCoinService.Update(model);
                        SysDBTool.Commit();
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "当前状态不可审核。";
                    CacheExtensions.ClearCache(CacheKey);//移除缓存
                    return View("Error");
                }
            }
            else if (commandtype.ToLower() == "docancel")
            {
                if (model.Status < (int)JN.Data.Enum.TakeCaseStatus.Payed)
                {
                    var c = CurrencyService.Single(x => x.ID == model.CurID);//多币的情况下需要重新设计
                    //NEO手续费外扣
                    decimal takeCashRaMoney = c.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.NEO ? model.DrawMoney + model.Poundage : model.DrawMoney;
                    Wallets.changeWallet(model.UID, takeCashRaMoney, (int)c.WalletCurID, c.CurrencyName + "拒绝转出提币申请", c);
                    model.Status = (int)JN.Data.Enum.TakeCaseStatus.Refusal;
                    RaCoinService.Update(model);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "成功取消提币申请！";
                    CacheExtensions.ClearCache(CacheKey);//移除缓存
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "提币已支付，不能取消。";
                    CacheExtensions.ClearCache(CacheKey);//移除缓存
                    return View("Error");
                }
            }
            CacheExtensions.ClearCache(CacheKey);//移除缓存
            return RedirectToAction("TakeCashRa", "WalletCoin");
        }
        #endregion

        #region 币种转入
        public ActionResult RemittanceRa(int? page)
        {
            ActMessage = "币种转入明细";
            var list = RaCoinService.ListInclude(x => x.OutTable_Currency).Where(x => x.Direction == "in").WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            int curid = Request["curid"].ToInt();
            if (curid != 0)
            {
                list = list.Where(x => x.CurID == curid);
            }
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 提交审核{手动审核}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RechargeCommand()
        {
            ActMessage = "审核币种转入（通过、拒绝）";
            int id = Request["id"].ToInt();
            string CacheKey = "RechargeRa" + id.ToString();
            if (CacheExtensions.CheckCache(CacheKey))//查找缓存是否存在
            {
                ViewBag.ErrorMsg = "正在处理数据,请耐心等待";
                return View("Error");
            }
            CacheExtensions.SetCache(CacheKey, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 10);//每10分钟  

            string commandtype = Request["commandtype"];
            var model = RaCoinService.Single(x=>x.ID==id && x.Direction=="in");
            if (model != null)
            {
                if (commandtype.ToLower() == "dopass")
                {
                    if (model.Status == (int)JN.Data.Enum.TakeCaseStatus.Wait)
                    {
                        int c = model.CurID;
                        var curmodel = CurrencyService.Single(x => x.ID == c);
                        model.Status = (int)JN.Data.Enum.TakeCaseStatus.Deal;
                        RaCoinService.Update(model);
                        Wallets.changeWallet(model.UID, model.ActualDrawMoney, (int)curmodel.WalletCurID, curmodel.CurrencyName + "充值收入", curmodel);
                        ViewBag.SuccessMsg = "操作成功！";
                        CacheExtensions.ClearCache(CacheKey);//移除缓存
                        return View("Success");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "当前状态不可审核。";
                        CacheExtensions.ClearCache(CacheKey);//移除缓存
                        return View("Error");
                    }
                }
                else if (commandtype.ToLower() == "docancel")
                {
                    if (model.Status < (int)JN.Data.Enum.TakeCaseStatus.Payed)
                    {
                        var c = CurrencyService.Single(x => x.ID == model.CurID);//多币的情况下需要重新设计
                        //Wallets.changeWallet(model.UID, model.DrawMoney, (int)c.WalletCurID, c.CurrencyName + "拒绝转出提币申请", c);
                        model.Status = (int)JN.Data.Enum.TakeCaseStatus.Cancel;
                        RaCoinService.Update(model);
                        SysDBTool.Commit();
                        ViewBag.SuccessMsg = "操作成功！";
                        CacheExtensions.ClearCache(CacheKey);//移除缓存
                        return View("Success");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "操作失败。";
                        CacheExtensions.ClearCache(CacheKey);//移除缓存
                        return View("Error");
                    }
                }
                return RedirectToAction("RemittanceRa", "WalletCoin");
            }
            else
            {
                ViewBag.ErrorMsg = "无效的ID";
                CacheExtensions.ClearCache(CacheKey);//移除缓存
                return View("Error");
            }
        }
        #endregion   

        public ActionResult CurrencyListEcx(int? page)
        {

            var list = CurrencyService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }


        public ActionResult CurrencyListIco(int? page)
        {
            var list = CurrencyService.List(x => x.IsICO == true).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        #region 添加币种

        public ActionResult CurrencyAddAndModify(int? id)
        {
            ActMessage = "修改币种";
            if (id.HasValue)
                return View(CurrencyService.Single(id));
            else
            {
                ActMessage = "添加币种";
                return View(new Data.Currency());
            }
        }


        [HttpPost]
        public ActionResult CurrencyAddAndModify(FormCollection form)
        {
            try
            {
                var entity = CurrencyService.SingleAndInit(form["ID"].ToInt());
                string  CurrencyName = form["CurrencyName"];
                string BuyPoundage = form["BuyPoundage"];
                string SellPoundage = form["SellPoundage"];
                string Increase = form["Increase"];

                string LineSwitch = form["LineSwitch"];
                string TranSwitch = form["TranSwitch"];
                //string IsRegReward = form["IsRegReward"];
                string IsFreeze = form["IsFreeze"];
                string IsInvitaReward = form["IsInvitaReward"];
                string CurID = form["CurID"];
                string IsLimit = form["IsLimit"];
                string IsEthereum = form["IsEthereum"];
                string IsAutomatic = form["IsAutomatic"];
                string platform = form["platform"];
                //开启关闭时间
                string StartTime = form["StartTime"];
                string StopTime = form["StopTime"];

                if (string.IsNullOrEmpty(CurrencyName))
                {
                    ViewBag.ErrorMsg = "请您填写币种名称！";
                    return View("Error");
                }

                
                if (!platform.ToBool())//交易所
                {
                    if (string.IsNullOrEmpty(BuyPoundage))
                    {
                        ViewBag.ErrorMsg = "请您填写卖出手续费！";
                        return View("Error");
                    }
                    if (string.IsNullOrEmpty(SellPoundage))
                    {
                        ViewBag.ErrorMsg = "请您填写买入手续费！";
                        return View("Error");
                    }

                    //if (string.IsNullOrEmpty(Increase))
                    //{
                    //    ViewBag.ErrorMsg = "请您填写每天最高涨幅！";
                    //    return View("Error");
                    //}
                    //if (string.IsNullOrEmpty(StartTime) || string.IsNullOrEmpty(StopTime))
                    //{
                    //    ViewBag.ErrorMsg = "请您填写开启交易时间段！";
                    //    return View("Error");
                    //}
                    //if (!StringHelp.IsDateString(StartTime))
                    //{
                    //    ViewBag.ErrorMsg = "开启时间格式不正确！";
                    //    return View("Error");
                    //}
                    //if (!StringHelp.IsDateString(StopTime))
                    //{
                    //    ViewBag.ErrorMsg = "关闭时间不正确！";
                    //    return View("Error");
                    //}

                }


                TryUpdateModel(entity, form.AllKeys);
                //entity.LineSwitch = LineSwitch.ToBool();
                entity.TranSwitch = TranSwitch.ToBool();
                entity.IsInvitaReward = IsInvitaReward.ToBool();
                entity.IsICO = platform.ToBool();
                entity.IsLimit = IsLimit.ToBool();
                entity.IsEthereum = IsEthereum.ToBool();
                //entity.IsRegReward = IsRegReward.ToBool();
                entity.IsFreeze = IsFreeze.ToBool();
                entity.IsAutomatic = IsAutomatic.ToBool();

                HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (!string.IsNullOrEmpty(file.FileName))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {
                        ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                        return View("Error");
                    }
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    //20160709更新缩略图 ------------开始
                   // var fileName = Path.Combine(Request.MapPath("~/Upload"), newfilename);
                    try
                    {
                        var fileName = Path.Combine(Request.MapPath("~/Upload"), newfilename);
                      
                            file.SaveAs(fileName);
                            imgurl = "/Upload/" + newfilename;

                        //file.SaveAs(fileName);
                        //var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/Upload/"), 1024, 768, "EQU");
                        //System.IO.File.Delete(fileName); //删除原文件
                        //imgurl = "/Upload/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("上传失败：" + ex.Message);
                    }
                    //System.IO.File.Delete(fileName); //删除原文件
                    //20160709更新 ------------结束
                }
              

                if (entity.ID > 0)
                {
                    if (!string.IsNullOrEmpty(file.FileName)) { entity.CurrencyLogo = imgurl; }
                    //int cint = CurID.ToInt();
                    //var c = CurrencyService.Single(x => x.WalletCurID == cint);
                    //if (c != null)
                    //{
                    //    ViewBag.ErrorMsg = "对不起，您选择的钱包已被使用";
                    //    return View("Error");
                    //}
                    entity.WalletCurID = CurID.ToInt();
                    CurrencyService.Update(entity);
                }
                else
                {
                    if (CurID.ToInt() == 0)
                    {
                        ViewBag.ErrorMsg = "请您选择使用钱包！";
                        return View("Error");
                    }
                    int cint=CurID.ToInt();
                    var c = CurrencyService.Single(x => x.WalletCurID == cint);
                   if (c != null)
                   {
                       ViewBag.ErrorMsg = "对不起，您选择的钱包已被使用";
                       return View("Error");
                   }
                    entity.WalletCurID = CurID.ToInt();
                    entity.CurrencyLogo = imgurl;
                    entity.CreateTime = DateTime.Now;
                    CurrencyService.Add(entity);
                }

                SysDBTool.Commit();
                Services.Manager.CacheHelp.ClearGetCurrencyCache();

                Users.ClearCacheAll();//清空缓存
                //写入缓存
                JN.Services.Manager.CacheTransactionDataHelper.SetCurrencyModel(entity.ID,entity);
                ViewBag.SuccessMsg = "币种修改/添加成功！";
                return View("Success");
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
                return View("Error");
            }
        }
        #endregion


        ///// <summary>
        ///// 提币
        ///// </summary>
        ///// <param name="address"></param>
        ///// <param name="amount"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult AjaxTibi(string address, double? amount)
        //{
        //    ReturnResult<string> result = new ReturnResult<string>();

        //    result = BitcionHelper.Transfer(address, amount.GetValueOrDefault());

        //    //转账成功，保存交易ID
        //    if (result.Status == ReturnResultStatus.Succeed.GetShortValue())
        //    {
        //        //交易ID：result.Data
        //        //Todo...
        //        //
        //    }

        //    return this.JsonResult(result);

        //}


        #region 2018-3 Lee 新版以太坊api
        /// <summary>
        /// 自动检测到帐记录
        /// </summary>
        /// <returns></returns>
        public ActionResult StartSyncTakeCash()
        {
            DataCache.SetCache("StartTime", DateTime.Now);
            DataCache.SetCache("TotalRow", 0);
            DataCache.SetCache("CurrentRow", 0);
            DataCache.SetCache("TransInfo", "任务进度(未开始)");

            Thread thread = new Thread(new ThreadStart(WalletsAPI.EthContract_GetSyncTakeCash));
            thread.Start();
            return Content("ok");
        }
        
        public JsonResult getproc_getsynctakecash()
        {
            ActMessage = "获取同步提币代理实时进度_代币";
            var proc = new
            {
                StartTime = DataCache.GetCache("StartTime"),
                CurrentRow = DataCache.GetCache("CurrentRow"),
                TotalRow = DataCache.GetCache("TotalRow"),
                TransInfo = DataCache.GetCache("TransInfo")
            };
            return Json(new { result = "ok", data = proc }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 2018-8-28 pengyh XRP同步交易记录

        /// <summary>
        /// 自动检测到帐记录
        /// </summary>
        /// <returns></returns>
        public ActionResult StartSyncTakeCash_xpr()
        {
            ActMessage = "同步提币代理结果_瑞波币";

            DataCache.SetCache("StartTime", DateTime.Now);
            DataCache.SetCache("TotalRow", 0);
            DataCache.SetCache("CurrentRow", 0);
            DataCache.SetCache("TransInfo", "任务进度(未开始)");

            Thread thread = new Thread(new ThreadStart(WalletsAPI.Xrp_GetSyncTakeCash));
            thread.Start();
            return Content("ok");
        }

        public JsonResult getproc_getsynctakecash_xpr()
        {
            ActMessage = "获取同步提币代理实时进度_瑞波币";

            var proc = new
            {
                StartTime = DataCache.GetCache("StartTime"),
                CurrentRow = DataCache.GetCache("CurrentRow"),
                TotalRow = DataCache.GetCache("TotalRow"),
                TransInfo = DataCache.GetCache("TransInfo")
            };
            return Json(new { result = "ok", data = proc }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}