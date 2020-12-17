using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Filters;
using JN.Services.Manager;
using JN.Services.Tool;
using JN.Services.CustomException;
using System.Data.Entity.Validation;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class FinanceController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ITakeCashService TakeCashService;
        private readonly ITransferService TransferService;
        private readonly IRemittanceService RemittanceService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ICurrencyService CurrencyService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly ILuckDrawSumLogService LuckDrawSumLogService;
        private readonly IOtherTransferService OtherTransferService;
        private readonly ITeamAchievementService TeamAchievementService;

        public FinanceController(ISysDBTool SysDBTool,
            IUserService UserService,
        ITakeCashService TakeCashService,
            ITransferService TransferService,
            IRemittanceService RemittanceService,
            IBonusDetailService BonusDetailService,
            ICurrencyService CurrencyService,
            IActLogService ActLogService,
            IWalletLogService WalletLogService,
            ILuckDrawSumLogService LuckDrawSumLogService,
            IOtherTransferService OtherTransferService,
            ITeamAchievementService TeamAchievementService)
        {
            this.UserService = UserService;
            this.TakeCashService = TakeCashService;
            this.TransferService = TransferService;
            this.RemittanceService = RemittanceService;
            this.BonusDetailService = BonusDetailService;
            this.CurrencyService = CurrencyService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
            this.LuckDrawSumLogService = LuckDrawSumLogService;
            this.OtherTransferService = OtherTransferService;
            this.TeamAchievementService = TeamAchievementService;
            cacheSysParam = Services.Manager.CacheHelp.GetSysParamsList();

        }

        public ActionResult Account(int? page)
        {
            //动态构建查询
            var list = UserService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult Delivery()
        {
            ActMessage = "赠送电子币";
            //ViewData["SysParamList"] = new SelectList(cacheSysParam.Where(x => x.PID == 2000 && x.IsUse).OrderBy(x => x.ID).ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Delivery(FormCollection form)
        {
            string username = form["username"];
            string coinid = form["coinid"];
            string deliverynumber = form["deliverynumber"];
            string remark = form["remark"];
            var result = new ReturnResult();
            try
            {
                var onUser = UserService.Single(x => x.UserName.Equals(username.Trim()));
                if (onUser == null) throw new CustomException("用户不存在");
                if (remark.Trim().Length > 100) throw new CustomException("备注长度不能超过100个字节");
                int currid = coinid.ToInt();
                if (currid < 3000)//如果是人民币
                {
                    Wallets.changeWallet(onUser.ID, deliverynumber.ToDecimal(), coinid.ToInt(), "ZH：" + remark + "(" + Amodel.AdminName + ")");
                }
                else
                {
                    var curr = CurrencyService.Single(x => x.WalletCurID == currid);


                    decimal d = JN.Services.Manager.Users.WalletCur(currid, onUser);
                    //if (currid == 3001 && deliverynumber.ToDecimal() > 0) throw new CustomException("派送数量不正确,QJT只能赠送负数");
                    //if (d + deliverynumber.ToDecimal() < 0 && currid != 3001) throw new CustomException("派送数量不正确");

                    Wallets.changeWallet(onUser.ID, deliverynumber.ToDecimal(), coinid.ToInt(), "ZH：" + remark + "(" + Amodel.AdminName + ")", curr);
                    if (currid == 3002)
                    {
                        onUser.ReserveDecamal2 = (onUser.ReserveDecamal2 ?? 0) + deliverynumber.ToDecimal();
                        Users.UpdateLevel(onUser);//会员升级判断
                        //Users.UpdateLevel(onUser.ID);//会员升级判断
                    }
                }
                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);

        }

        public ActionResult DeliveryDetail(int? coin, int? page)
        {
            ActMessage = "赠送电子币明细";
            //int coinID = coin ?? 2002;
            //ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 2000 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            //var list = WalletLogService.List(x => x.CoinID == coinID && x.Description.Contains("ZH：")).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            var list = WalletLogService.List(x => x.Description.Contains("ZH：")).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult TakeCash(int? status, int? page)
        {
            ActMessage = "提币明细";
            int Status = status ?? 0;
            var list = TakeCashService.List(x => x.Status == Status).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Transfer(int? page)
        {
            ActMessage = "转帐明细";
            var list = TransferService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Statistics(int? page)
        {
            var parameters = new[]{
                new System.Data.SqlClient.SqlParameter(){ ParameterName="1", Value=1 }
            };
            var list = SysDBTool.Execute<Data.Extensions.View_Statistics>("SELECT [jstime],[newuser],[takecash],[income],[outlay],[profit] FROM [View_Statistics]", parameters);
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult TakeCashCommand(int id)
        {
            string commandtype = Request["commandtype"];
            var model = TakeCashService.Single(id);
            if (model != null)
            {
                if (commandtype.ToLower() == "dopass")
                {
                    model.Status = (int)JN.Data.Enum.TakeCaseStatus.Passed;
                }
                else if (commandtype.ToLower() == "dopay")
                {
                    model.Status = (int)JN.Data.Enum.TakeCaseStatus.Payed;
                    model.PayTime = DateTime.Now;
                }
                else if (commandtype.ToLower() == "docancel")
                {
                    if (model.Status < (int)JN.Data.Enum.TakeCaseStatus.Payed)
                    {
                        var cur = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().SingleAndInit(x => x.ID == (model.CurID ?? 0));
                        model.Status = (int)JN.Data.Enum.TakeCaseStatus.Refusal;
                        TakeCashService.Update(model);
                        Wallets.changeWallet(model.UID, model.DrawMoney, (int)model.CurID, "拒绝提币申请", cur);
                        ViewBag.SuccessMsg = "成功取消提币申请！";
                        return View("Success");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "提币已支付，不能取消。";
                        return View("Error");
                    }
                }
                TakeCashService.Update(model);
                SysDBTool.Commit();
                return RedirectToAction("TakeCash", "Finance");
            }
            else
            {
                ViewBag.ErrorMsg = "无效的ID";
                return View("Error");
            }
        }



        public ActionResult Remittance(int? status, int? page)
        {
            int Status = status ?? 1;
            var list = RemittanceService.List(x => x.Status == Status).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult RemittanceCommand(int id, string commandtype)
        {
            Data.Remittance model = RemittanceService.Single(id);
            if (model != null)
            {
                if (commandtype.ToLower() == "dopass")
                {
                    var cur = CurrencyService.Single(x => x.WalletCurID == 3003);
                    Wallets.changeWallet(model.UID, model.ActualAmount ?? 0, 3003, "用户充值", cur);
                    //Wallets.changeWallet(model.UID, model.RechargeAmount, 2001, "用户充值");
                    model.Status = (int)JN.Data.Enum.RechargeSatus.Sucess;
                }
                else if (commandtype.ToLower() == "docancel")
                    model.Status = (int)JN.Data.Enum.RechargeSatus.Un;
                RemittanceService.Update(model);
                SysDBTool.Commit();
                return RedirectToAction("Remittance", "Finance");
            }
            else
            {
                ViewBag.ErrorMsg = "无效的ID";
                return View("Error");
            }
        }

        public ActionResult AccountDetail(int? coin, int? page)
        {
            ActMessage = "财务流水";
            //ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 2000 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            ViewBag.ParamList = CurrencyService.List(x => x.LineSwitch && !x.IsICO).ToList();
            var list = WalletLogService.List().OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (coin.HasValue)
                list = list.Where(x => x.CoinID == (coin ?? 3003));
            //var list = WalletLogService.List().OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult BonusDetail(int? bonusid, int? page)
        {
            ActMessage = "奖金明细";
            ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 1100 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            var list = BonusDetailService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            IQueryable<JN.Data.BonusDetail> newlist = null;
            if (bonusid.HasValue)
            {
                newlist = list.Where(x => x.BonusID == (bonusid ?? 0));
                return View(newlist.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult LuckDrawLog(int? page)
        {
            ActMessage = "抽奖记录";
            //动态构建查询
            var list = LuckDrawSumLogService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult OtherTransferList(int? page)
        {
            var list = OtherTransferService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }


        #region 赠送矿机
        public ActionResult MinerDelivery()
        {
            ActMessage = "赠送矿机";
            //ViewData["SysParamList"] = new SelectList(cacheSysParam.Where(x => x.PID == 2000 && x.IsUse).OrderBy(x => x.ID).ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult MinerDelivery(FormCollection form)
        {
            string username = form["username"];
            string coinid = form["coinid"];
            string deliverynumber = form["deliverynumber"];
            string remark = form["remark"];

            decimal addupInterest = form["AddupInterest"].ToDecimal();
            int reserveInt1 = form["ReserveInt1"].ToInt();

            var result = new ReturnResult();
            try
            {
                var onUser = UserService.Single(x => x.UserName.Equals(username.Trim()));
                if (onUser == null) throw new CustomException("用户不存在");
                if (remark.Trim().Length > 100) throw new CustomException("备注长度不能超过100个字节");
                int productid = coinid.ToInt();
                //var curr = CurrencyService.Single(x => x.WalletCurID == currid);
                //Wallets.changeWallet(onUser.ID, deliverynumber.ToDecimal(), coinid.ToInt(), "ZH：" + remark, curr);
                var product = MvcCore.Unity.Get<JN.Data.Service.IShopProductService>().Single(productid);
                if (product != null)
                {
                    //写入汇款表
                    var model = new Data.MachineOrder
                    {
                        InvestmentNo = GetOrderNumber(),
                        InvestmentType = 1,
                        UID = onUser.ID,
                        UserName = onUser.UserName,
                        ApplyInvestment = product.RealPrice,
                        SettlementMoney = 0,
                        IsBalance = true,
                        BuyNum = 1,
                        Status = (int)Data.Enum.RechargeSatus.Sucess,
                        CreateTime = DateTime.Now,
                        AddupInterest = addupInterest,//累计收益
                        ProductID = product.ID,//矿机ID
                        ProductName = product.ProductName,//产皮名称
                        TopBonus = product.TopBonus,
                        ShopID = product.ShopID,
                        TimesType = product.TimesType ?? 0,
                        Duration = product.Duration ?? 1,
                        PayWay = "系统后台赠送",
                        WaitExtractIncome = 0,
                        ImageUrl = product.ImageUrl,
                        LastProfitTime = DateTime.Now,
                        ReserveBoolean1 = false,
                        ReserveDecamal1 = (product.Performance ?? 0),
                        ReserveInt2 = (int)(product.TopBonus / product.TimesType ?? 0),
                        ReserveInt1 = reserveInt1
                    };
                    MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Add(model);
                    //Bonus.AddupUp(model.ID);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);

        }

        //生成真实订单号
        public string GetOrderNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "M";
            result += dateTime.Year.ToString().Substring(2, 2) + dateTime.Month + dateTime.Day;//年月日
            int hour = dateTime.Hour;
            int minu = dateTime.Minute;
            int secon = dateTime.Second;
            int count = hour * 20 + minu * 10 + secon * 5;
            int lengh = count.ToString().Length;
            string temp = count.ToString();
            string bu = "";
            if (lengh < 5)//不足5位前面补0
            {
                for (int i = 1; i < 5 - lengh; i++)
                {
                    bu += "0";
                }
            }
            result += bu + temp;
            result += Services.Tool.StringHelp.GetRandomNumber(4);//4位随机数字
            if (IsHave(result))
            {
                return GetOrderNumber();
            }
            return result;
        }
        //检查订单号是否重复
        private bool IsHave(string number)
        {
            return MvcCore.Unity.Get<Data.Service.IMachineOrderService>().List(x => x.InvestmentNo == number).Count() > 0;
        }
        #endregion

        #region 矿机列表
        public ActionResult MachineOrder(int? page)
        {
            var list = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            string status = Request["isbalance"];
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "1")
                    list = list.Where(x => x.IsBalance);
                else
                    list = list.Where(x => !x.IsBalance);
            }
            //if (Request["IsExport"] == "1")
            //{
            //    string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
            //    MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
            //    return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            //}
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }
        #endregion


        #region 编辑矿机
        public ActionResult ModifyCommand(int? id)
        {
            ActMessage = "编辑用户资料";
            //ViewBag.UserName = UserService.Single(id).UserName.ToString();
            var model = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Single(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult ModifyCommand(FormCollection fc)
        {
            try
            {
                var entity = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                //if (ClassId.Trim().Length == 0)
                if (entity.ID > 0)
                    MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Update(entity);
                else
                {
                    ViewBag.ErrorMsg = "无此矿机。";
                    return View("Error");
                }
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "商品修改/发布成功！";
                return View("Success");
            }
            catch (DbEntityValidationException ex)
            {
                //foreach (var item in ex.EntityValidationErrors)
                //{
                //    foreach (var item2 in item.ValidationErrors)
                //        error += string.Format("{0}:{1}\r\n", item2.PropertyName, item2.ErrorMessage);
                //}
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
                return View("Error");
            }
        }
        #endregion

        #region 月度个人业绩   以及 团队业绩

        public ActionResult TeamAchievementDetail(int? bonusid, int? page)
        {
            ActMessage = "月度业绩";
            var list = TeamAchievementService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, 20));
        }
        #endregion
    }
}