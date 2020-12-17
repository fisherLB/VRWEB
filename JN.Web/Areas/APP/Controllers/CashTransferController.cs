using JN.Data.Extensions;
using JN.Data.Service;
using JN.Services.CustomException;
using JN.Services.Manager;
using JN.Services.Tool;
using MvcCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace JN.Web.Areas.APP.Controllers
{
    public class CashTransferController : BaseController
    {
        object obj = new object();
        private static List<Data.SysParam> cacheSysParam = null; 
        private readonly IUserService UserService;
        private readonly ITakeCashService TakeCashService;
        private readonly IRaCoinService RaCoinService;
        private readonly ITransferService TransferService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly ICurrencyService CurrencyService;
        public CashTransferController(ISysDBTool SysDBTool,
            IUserService UserService,
            ITakeCashService TakeCashService,
            IRaCoinService RaCoinService,
            ITransferService TransferService,
            IActLogService ActLogService,
            IWalletLogService WalletLogService,
            ICurrencyService CurrencyService
            )
        {
            this.UserService = UserService;
            this.TakeCashService = TakeCashService;
            this.RaCoinService = RaCoinService;
            this.TransferService = TransferService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
            this.CurrencyService = CurrencyService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();

        }

        #region 币种转出提币和列表

        /// <summary>
        /// 币种转出提币
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult CurTransferOut(int? page)
        {
            var c = CurrencyService.List(x => x.LineSwitch && !(x.IsCashCurrency ?? false)).Count()==0 ? new JN.Data.Currency { ID = 0 } : CurrencyService.List(x => x.LineSwitch && !(x.IsCashCurrency ?? false)).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条

            if (c.ID == 0) return Redirect("/home/index");
            int curid = Request["curid"].ToInt();
            var cur = (curid == 0) ? c : CurrencyService.Single(x => x.ID == curid);
            
            ActMessage = cur.CurrencyName + "转出";
            return View(cur);
        }

        /// <summary>
        /// 提币列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult CurTransferOutList()
        {
            ActMessage = "提币列表";

            var c = CurrencyService.List(x => x.LineSwitch && !(x.IsCashCurrency ?? false)).Count() == 0 ? new JN.Data.Currency { ID = 0 } : CurrencyService.List(x => x.LineSwitch && !(x.IsCashCurrency ?? false)).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条
            if (c.ID == 0) return Redirect("/app/home/index");
            int curid = Request["curid"].ToInt();
            var cur = (curid == 0) ? c : CurrencyService.Single(x => x.ID == curid);
            ViewBag.Currency = cur;

            return View();
        }
        /// <summary>
        /// 获取提币列表数据
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getCurTransferOutList(int? page)
        {
            int pageSize = 10;
            int curid = Request["curid"].ToInt();

            var list = RaCoinService.List(x => x.UID == Umodel.ID && x.Direction == "out" && x.CurID == curid);
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提币详情
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult CurTransferOutDetail(int id)
        {
            ActMessage = "提币详情";
            ViewBag.Title = ActMessage;
            var model = RaCoinService.ListInclude(x => x.OutTable_Currency).Where(x => x.ID == id && x.UID == Umodel.ID).FirstOrDefault();
            return View(model);
        }
        #endregion

        #region 申请及取消提币

        /// <summary>
        /// 人民币提币，异步 Name:Lin  Time:2017年7月18日14:25:51
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult RMBCash()
        {
            var c = CurrencyService.List(x => x.LineSwitch && (x.IsCashCurrency ?? false)).Count() == 0 ? new JN.Data.Currency { ID = 0 } : CurrencyService.List(x => x.LineSwitch && (x.IsCashCurrency ?? false)).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条

            if (c.ID == 0) return Redirect("/app/home/index");
            int curid = Request["curid"].ToInt();
            var cur = (curid == 0) ? c : CurrencyService.Single(x => x.ID == curid);

            ViewBag.Title = cur.CurrencyName + "提现";
            ActMessage = ViewBag.Title;
            ViewBag.Currency = cur;

            return View();
        }

        /// <summary>
        /// 人民币提币提交
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RMBCash(FormCollection form)
        {
            ViewBag.Title = "人民币提币提交";
            Result result = new Result();
            result = JN.Services.Manager.Finance.WithdrawCash(form, Umodel, CurrencyService);
            return Json(result);
        }
        /// <summary>
        /// 提现列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult RMBCashList()
        {
            ActMessage = "提现列表";
            ViewBag.Title = ActMessage;

            var c = CurrencyService.List(x => x.LineSwitch && (x.IsCashCurrency ?? false)).Count() == 0 ? new JN.Data.Currency { ID = 0 } : CurrencyService.List(x => x.LineSwitch &&(x.IsCashCurrency ?? false)).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条
            if (c.ID == 0) return Redirect("/app/home/index");
            int curid = Request["curid"].ToInt();
            var cur = (curid == 0) ? c : CurrencyService.Single(x => x.ID == curid);
            ViewBag.Currency = cur;

            return View();
        }
        /// <summary>
        /// 获取提现列表数据
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getRMBCashList(int? page)
        {
            int pageSize = 10;
            int curid = Request["curid"].ToInt();

            var list = TakeCashService.List(x => x.UID == Umodel.ID && x.CurID == curid);
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提现详情
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult RMBCashDetail(int id)
        {
            ActMessage = "提现详情";
            ViewBag.Title = ActMessage;
            var model = TakeCashService.SingleAndInit(x => x.ID == id && x.UID == Umodel.ID);
            return View(model);
        }

        /// <summary>
        /// 取消提币
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CancelTakeCash(int id)
        {
            ViewBag.Title = "取消提币";
            Result result = new Result();
            var model = TakeCashService.Single(id);
            if (model != null && model.UID == Umodel.ID)
            {
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    result = JN.Services.Manager.Finance.CancelTakeCash(id, TakeCashService);
                    if (result.Status == 200)
                    {
                        ts.Complete();
                    }
                }
            }
            else {
                result.Message = "非法操作";
            }

            return Json(result);
         
        }

        #endregion

        #region 转出提币币种

        /// <summary>
        /// 转出提币币种 Name：Lin  Time：2017年7月18日14:32:41
        /// </summary>
        /// <returns></returns>
        public ActionResult RaSend()
        {
            ActMessage = "转出提币币种";
            return View();
        }
        /// <summary>
        /// 转出xx币种 Name：Lin  Time：2017年7月18日14:32:41
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RaSend(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (CacheExtensions.CheckCache("RaSend" + Umodel.ID))//查找缓存是否存在
                {
                    throw new CustomException("正在处理数据,请耐心等待。");
                }
                CacheExtensions.SetCache("RaSend" + Umodel.ID, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 10);//每1分钟   

                string amount = form["Amount"];
                string walletaddress = form["WalletAddress"];
                string tag = form["Tag"];
                string password2 = form["password2"];
                string curid = form["curid"];
                if (string.IsNullOrEmpty(curid)) throw new CustomException("对不起，参数接收失败！");

                int c = curid.ToInt();
                var cur = CurrencyService.Single(x => x.ID == c);
                if (cur == null) throw new CustomException("错误的参数");
                decimal d = JN.Services.Manager.Users.WalletCur((int)cur.WalletCurID, Umodel);

                if (Umodel.IsLock) throw new CustomException("您的帐号受限，无法进行相关操作");
                //if (!(Umodel.IsAuthentication??false)) throw new CustomException("您还没有实名验证，请您实名验证后再操作");
                //if (!(Umodel.IsMobile ?? false)) throw new CustomException("您还没有手机认证，请您手机认证后再操作");

                if (password2.ToMD5().ToMD5()!=Umodel.Password2) throw new CustomException("交易密码不正确");
                if (amount.ToDecimal() <= 0) throw new CustomException("请填写正确的数量");

                if (cur.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.NEO)//是NEO
                {
                    if (amount.ToDecimal() % 1 != 0) throw new CustomException("该币种提现必须为整数");
                    if (d < (amount.ToDecimal() * (1 + cur.TbPoundage.ToDecimal()))) throw new CustomException("余额不足");
                }
                else //非NEO
                {
                    if (d < amount.ToDecimal()) throw new CustomException("余额不足");
                }

                //单笔限制
                if ((cur.TbMinLimit ?? 0) > 0 && (cur.TbMinLimit ?? 0) > amount.ToDecimal()) throw new CustomException("低于单笔提币额：" + cur.TbMinLimit);
                //单笔限制
                if ((cur.TbMaxLimit ?? 0) > 0 && (cur.TbMaxLimit ?? 0) < amount.ToDecimal()) throw new CustomException("超出单笔提币额：" + cur.TbMaxLimit);


                if (!WalletsAPI.ValidateAddress(walletaddress, cur)) throw new CustomException("无效的地址");
                if (cur.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ripple || cur.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EOS)//是XRP
                {
                    if (string.IsNullOrWhiteSpace(tag)) throw new CustomException("请填写您的账户Tag");
                }

                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    Wallets.doTakeCashRa(Umodel, amount.ToDecimal(), walletaddress, cur, tag);
                    ts.Complete();
                    result.Status = 200;
                }
                #endregion
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
            finally
            {
                CacheExtensions.ClearCache("RaSend" + Umodel.ID);  //清空
            }
            return Json(result);
        }
        
        #endregion

        #region 取消转出提币

        /// <summary>
        /// 取消转出 Name:Lin  Time：2017年7月18日14:27:23
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CancelSendRa(int id)
        {
            var model = RaCoinService.Single(id);
            if (model != null && model.UID == Umodel.ID)
            {
                if (model.Status == (int)JN.Data.Enum.TakeCaseStatus.Wait)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //查找币种
                        var c = CurrencyService.Single(x => x.ID == model.CurID);
                        Wallets.changeWallet(model.UID, model.DrawMoney, (int)c.WalletCurID, "取消提币" + c.CurrencyName,c);
                        model.Status = (int)JN.Data.Enum.TakeCaseStatus.Cancel;
                        RaCoinService.Update(model);
                        SysDBTool.Commit();
                        ts.Complete();
                        return Json(new { result = "ok", msg = "取消成功！" });
                        //ActMessage = "成功取消提币申请！";
                        //return View("Success");
                    }
                }
                else
                {
                    return Json(new { result = "Error", msg = "您的提币申请无法取消！" });
                }
            }
            return Json(new { result = "Error", msg = "记录不存在或已被删除！" });

        } 
        #endregion

        #region 验证提币地址是否有效

        /// <summary>
        /// 验证提币地址是否有效
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AjaxValidateAddress(string address)
        {
            ReturnResult<bool> result = new ReturnResult<bool>();

            result = BitcionHelper.ValidateAddress("QianbaoWebApiHost", address);

            return this.JsonResult(result);

        } 
        #endregion


    }
}
