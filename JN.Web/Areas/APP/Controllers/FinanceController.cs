using JN.Data.Extensions;
using JN.Data.Service;
using JN.Services.CustomException;
using JN.Services.Manager;
using JN.Services.Tool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace JN.Web.Areas.APP.Controllers
{
    public class FinanceController : BaseController
    {
        object obj = new object();
        private static List<Data.SysParam> cacheSysParam = null;
        private readonly IUserService UserService;
        private readonly ITakeCashService TakeCashService;
        private readonly IRaCoinService RaCoinService;
        private readonly ITransferService TransferService;
        private readonly IReleaseDetailService ReleaseDetailService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly IExchangeDetailService ExchangeDetailService;
        private readonly IExchangeCurrencyService ExchangeCurrencyService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly ICurrencyService CurrencyService;
        private readonly IUserBankCardService UserBankCardService;
        private readonly IStockTradeService StockTradeService;
        private readonly IStockEntrustsTradeService StockEntrustsTradeService;
        private readonly IOtherTransferService OtherTransferService;
        public FinanceController(ISysDBTool SysDBTool,
            IUserService UserService,
            ITakeCashService TakeCashService,
            IRaCoinService RaCoinService,
            ITransferService TransferService,
            IReleaseDetailService ReleaseDetailService,
            IBonusDetailService BonusDetailService,
            IExchangeDetailService ExchangeDetailService,
            IExchangeCurrencyService ExchangeCurrencyService,
            IActLogService ActLogService,
            IWalletLogService WalletLogService,
            ICurrencyService CurrencyService,
            IUserBankCardService UserBankCardService,
            IStockTradeService StockTradeService,
            IStockEntrustsTradeService StockEntrustsTradeService,
             IOtherTransferService OtherTransferService
            )
        {
            this.UserService = UserService;
            this.TakeCashService = TakeCashService;
            this.RaCoinService = RaCoinService;
            this.TransferService = TransferService;
            this.ReleaseDetailService = ReleaseDetailService;
            this.BonusDetailService = BonusDetailService;
            this.ExchangeDetailService = ExchangeDetailService;
            this.ExchangeCurrencyService = ExchangeCurrencyService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
            this.CurrencyService = CurrencyService;
            this.StockTradeService = StockTradeService;
            this.UserBankCardService = UserBankCardService;
            this.StockEntrustsTradeService = StockEntrustsTradeService;
            this.OtherTransferService = OtherTransferService;
            cacheSysParam = Services.Manager.CacheHelp.GetSysParamsList() ;

        }

        public ActionResult Region()
        {
            ActMessage = ViewBag.Title;
            return View();
        }

        #region 财务中心首页
        /// <summary>
        /// 财务中心首页
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult Finance()
        {
            ViewBag.Title = "财务中心";
            ActMessage = ViewBag.Title;
            return View();
        }
        #endregion

        #region 帐户明细列表

        /// <summary>
        /// 帐户明细列表 Name:Lin  Time:2017年7月18日14:24:30
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult AccountDetail(int? coin, int? page)
        {
            ActMessage = "个人钱包流水";
            //int coinID = coin ?? 2002;
            //ViewBag.ParamList = cacheSysParam.Where(x => x.PID == 2000 && x.IsUse == true).OrderBy(x => x.Sort).ToList();
            //var list = WalletLogService.List(x => x.CoinID == coinID && x.UID == Umodel.ID).OrderByDescending(x => x.ID).ToList();
            var list = WalletLogService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID);

            if (Request.IsAjaxRequest())
                return View("_AccountDetail", list.WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));

            return View(list.WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }


        #endregion

        #region 奖金明细列表

        /// <summary>
        /// 奖金明细列表 Name:Lin  Time:2017年7月18日14:25:11
        /// </summary>
        /// <param name="page"></param>
        /// <param name="bid"></param>
        /// <returns></returns>
        public ActionResult BonusDetail(int? page)
        {
            ActMessage = "奖金明细表";
            int bid = Request["bid"].ToInt();

            var list = BonusDetailService.List(x => x.UID == Umodel.ID);
            if (bid == 0)
            {
                list = list.Where(x => x.BonusID == 1102);
            }
            else
            {
                list = list.Where(x => x.BonusID == bid);
            }
            if (Request.IsAjaxRequest())
                return View("_BonusDetail", list.WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));

            return View(list.WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        #endregion

        #region 成交查询

        /// <summary>
        /// 成交查询
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Transaction(int? page)
        {
            ActMessage = "成交查询";
            var list = StockTradeService.List(x => x.BuyUID == Umodel.ID || x.SellUID == Umodel.ID).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();

            if (!MvcCore.Extensions.CacheExtensions.CheckCache("curPriceList"))
            {
                Wallets.GetPriceList();
            }

            if (Request.IsAjaxRequest())
                return View("_Transaction", list.ToPagedList(page ?? 1, 20));

            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 委托查询

        /// <summary>
        /// 委托查询
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Order(int? page)
        {
            ActMessage = "委托查询";
            var list = StockEntrustsTradeService.List(x => x.UID == Umodel.ID).OrderByDescending(x => (x.Status == 0 || x.Status == 1)).ThenByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();

            if (Request.IsAjaxRequest())
                return View("_Order", list.ToPagedList(page ?? 1, 20));

            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 用户转帐
        public ActionResult ApplyTransfer(int? page)
        {

            var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => x.TranSwitch).Count() == 0 ? new JN.Data.Currency { ID = 0 } : MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => x.TranSwitch).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条

            if (c.ID == 0) return Redirect("/home/index");

            var cmodel = (string.IsNullOrEmpty(Request["curid"])) ? c : MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(Request["curid"].ToInt());

            if (!(Umodel.WalletLock ?? false))
            {
                ViewBag.Title = cmodel.CurrencyName + "转帐";
                ActMessage = ViewBag.Title;

            }
            else
            {
                ViewBag.ErrorMsg = "您的钱包转帐功能已被关闭，详情请联系管理员。";
                return View("Error");
            }


            ViewBag.Currency = cmodel;

            //币种充值列表
            var list = TransferService.List(x => x.UID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToList();

            //GetListTransactions(cmodel.ID);//刷新获取充值记录

            if (Request.IsAjaxRequest())//异步获取
                return View("_Transfer", list.ToPagedList(page ?? 1, 15));

            return View(list.ToPagedList(page ?? 1, 15));
        }

        [HttpPost]
        public ActionResult ApplyTransfer(FormCollection form)
        {
            ViewBag.Title = "用户转帐";
            Result result = new Result();
            result = JN.Services.Manager.Finance.UserTransfer(form, Umodel, UserService,cacheSysParam);
            return Json(result);
        }

        #region 转帐明细列表

        public ActionResult ApplyTransferList(int? page)
        {
            ViewBag.Title = "转帐列表";
            ActMessage = ViewBag.Title;
            var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => x.TranSwitch).Count() == 0 ? new JN.Data.Currency { ID = 0 } : MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => x.TranSwitch).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条

            if (c.ID == 0) return Redirect("/home/index");

            var cmodel = (string.IsNullOrEmpty(Request["curid"])) ? c : MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(Request["curid"].ToInt());

            if (!(Umodel.WalletLock ?? false))
            {
                ActMessage = cmodel.CurrencyName + "转帐";
            }
            else
            {
                ViewBag.ErrorMsg = "您的钱包转帐功能已被关闭，详情请联系管理员。";
                return View("Error");
            }


            ViewBag.Currency = cmodel;
            ViewBag.ID = Umodel.ID;

            //币种充值列表
            var list = TransferService.List(x => x.UID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToList();

            //GetListTransactions(cmodel.ID);//刷新获取充值记录

            if (Request.IsAjaxRequest())//异步获取
                return View("_Transfer", list.ToPagedList(page ?? 1, 15));
            return View(list.ToPagedList(page ?? 1, 15));
        }
        public JsonResult GetApplyTransferList()
        {
            int rows = 10;//每页多少条数据
            int page = Request["page"].ToInt();//第几页            

            var list = TransferService.List(x => x.UID == Umodel.ID || x.ToUID == Umodel.ID).OrderByDescending(x => x.ID);
            var list1 = list.ToPagedList(page, rows);
            int total = list.Count();//数据总数

            int totalpages = 1;
            if ((total % rows) > 0)
            {
                totalpages = total / rows >= 1 ? ((total / rows) + 1) : 1;
            }
            else
            {
                totalpages = total / rows > 1 ? total / rows : 1;
            }

            if (list1 != null)
            {
                return Json(new { result = "ok", data = list1, pages = totalpages });
            }
            else
            {
                return Json(new { result = "err" });
            }
        }
        public JsonResult GetApplyTransferList2(int? page)
        {
            int pageSize = 10;
            var list = TransferService.List(x => x.UID == Umodel.ID || x.ToUID == Umodel.ID).ToList();
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ApplyTransferDetail(int id)
        {
            ViewBag.Title = "转账详情";
            ActMessage = ViewBag.Title;
            var model = TransferService.SingleAndInit(x => x.ID == id);
            if (model != null && model.ID > 0)
            {
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }
        #endregion
        #endregion

        #region  转账 与A系统对接

        public ActionResult BindTransfer(int? page)
        {
            if (!Umodel.BindStatus)
            {
                return Redirect("/app/user/BingUser");
            }
            else
            {
                //币种充值列表
                var list = OtherTransferService.List(x => x.UID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToList();

                //GetListTransactions(cmodel.ID);//刷新获取充值记录

                if (Request.IsAjaxRequest())//异步获取
                    return View("_BindTransfer", list.ToPagedList(page ?? 1, 15));


                return View(list.ToPagedList(page ?? 1, 15));
            }
        }

        [HttpPost]
        public ActionResult BindTransfer(FormCollection form)
        {
            Result result = new Result();
            result = JN.Services.Manager.Finance.BindTransferMethod(form, Umodel);
            return Json(result);
        }
        #endregion

        #region 币种兑换
        /// <summary>
        /// 币种兑换
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyExchangeCur()
        {
            int excid = (Request["excid"] ?? "0").ToInt();
            var exchangecur = ExchangeCurrencyService.SingleAndInit(x=>x.ID==excid);
            if(exchangecur.ID>0 && !(exchangecur.IsUse??false))
            {
                ViewBag.ErrorMsg = "该币种兑换功能已被关闭，详情请联系管理员。";
                return View("Error");
            }
            ViewBag.CurrencyList = Services.Manager.CacheHelp.GetCurrencyList();
            ViewBag.ExchangeCurrencyList = MvcCore.Unity.Get<JN.Data.Service.IExchangeCurrencyService>().List(x => (x.IsUse ?? true)).ToList();
            return View(exchangecur);

            #region 未用
            //int fromID = (Request["fid"] ?? "0").ToInt();

            //if (!(Umodel.WalletLock ?? false))
            //{
            //    var cmodel = CurrencyService.SingleAndInit(x => x.ID == fromID && x.TranSwitch);
            //    if (fromID == 0)
            //    {
            //        var curlist = CurrencyService.List(x => x.TranSwitch).ToList();
            //        if (curlist.Any())
            //        {
            //            cmodel = curlist.FirstOrDefault();
            //            fromID = cmodel.ID;
            //        }
            //        else
            //            cmodel = new JN.Data.Currency();
            //    }
            //    int toID = (Request["toid"] ?? "0").ToInt();
            //    var toCModel = CurrencyService.SingleAndInit(x => x.ID == toID);
            //    if (toID != 0 && toCModel != null)
            //    {
            //        ViewBag.ToCoin = toCModel;
            //    }
            //    else
            //    {
            //        var curlist = CurrencyService.List(x => x.TranSwitch && x.ID != fromID).ToList();
            //        if (curlist.Any())
            //            ViewBag.ToCoin = curlist.FirstOrDefault();
            //        else
            //            ViewBag.ToCoin = new JN.Data.Currency();
            //    }

            //    return View(cmodel);
            //}
            //else
            //{
            //    ViewBag.ErrorMsg = "您的钱包转帐功能已被关闭，详情请联系管理员。";
            //    return View("Error");
            //}
            #endregion
        }
        [HttpPost]
        public ActionResult ApplyExchangeCur(FormCollection form)
        {
            Result result = new Result();
            result = JN.Services.Manager.Finance.ApplyExchange(form, Umodel, CurrencyService, ExchangeDetailService, ExchangeCurrencyService, SysDBTool);
            return Json(result);
        }
        /// <summary>
        /// 兑换记录
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult ExchangeDetail(int page = 1)
        {
            ActMessage = "兑换记录";
            var list = ExchangeDetailService.List(x => (x.UID == Umodel.ID)).OrderByDescending(x => x.ID);
            return View(list.ToPagedList(page, 5));
        }

        #endregion

        #region 转入虚拟币
        public ActionResult PayIn(int? page)
        {
            ViewBag.Title = "充值";
            ActMessage = ViewBag.Title;

            var cmode = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().SingleAndInit(1);
            return View(cmode);
        }


        /// <summary>
        /// 转入虚拟币
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetListTransactions()
        {
            ReturnResult<List<ResponseListTransactions>> result = new ReturnResult<List<ResponseListTransactions>>();
            int curid = Request["curid"].ToInt();
            //获取币种实体
            var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(curid);
            if (c == null) throw new Exception("获取失败，查无此币种");

            result = BitcionHelper.GetListTransactions(c.WalletKey, Umodel.UserName);
            foreach (var item in result.Data.Where(x => x.category == "receive"))
            {
                if (RaCoinService.List(x => x.Remark == item.txid).Count() <= 0)
                {
                    var wallet = cacheSysParam.SingleAndInit(x => x.ID == 2001);

                    //by Lin 2017年8月4日 11:23:08 
                    Wallets.changeWallet(Umodel.ID, item.amount.ToDecimal(), wallet.ID, "自助充值收入");

                    //写入提现表
                    RaCoinService.Add(new Data.RaCoin
                    {
                        ActualDrawMoney = item.amount.ToDecimal(),
                        WalletAddress = item.address,
                        CreateTime = DateTime.Now,
                        DrawMoney = item.amount.ToDecimal(),
                        Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                        Poundage = item.fee.ToDecimal(),
                        Direction = "in",
                        UID = Umodel.ID,
                        UserName = Umodel.UserName,
                        CurID = c.ID,
                        ReserveStr1 = item.timereceived.ToString(),
                        Remark = item.txid,
                        ReserveInt = item.confirmations,
                    });
                    SysDBTool.Commit();
                }
            }

            return this.JsonResult(result);
        }
        #endregion

        #region 转入转出
        /// <summary>
        /// 转入
        /// </summary>
        /// <returns></returns>
        public ActionResult TransferIn()
        {
            ActMessage = "转入";
            ViewBag.Title = ActMessage;
            return View();
        }

        /// <summary>
        /// 转出
        /// </summary>
        /// <returns></returns>
        public ActionResult TransferOut()
        {
            ActMessage = "转出";
            ViewBag.Title = ActMessage;
            return View();
        }
        [HttpPost]
        public ActionResult TransferOut(FormCollection form)
        {
            Result result = new Result();
            result = JN.Services.Manager.Finance.TransferOutCheck(form, Umodel);
            return Json(result);
        }
        /// <summary>
        /// 转出确认
        /// </summary>
        /// <returns></returns>
        public ActionResult TransferOutConfirm()
        {
            ActMessage = "转出确认";
            ViewBag.Title = ActMessage;
            string username = Request["username"];
            var toUser = UserService.Single(x => x.UserName == username || x.Mobile == username);
            if (toUser == null)
            {
                toUser = new Data.User();
            }
            ViewBag.ToUser = toUser;
            var cur = CurrencyService.Single(x => x.ID == 3);//转账余额账户
            return View(cur);
        }
        [HttpPost]
        public ActionResult TransferOutConfirm(FormCollection form)
        {
            Result result = new Result();
            result = JN.Services.Manager.Finance.DoTransferOut(form, Umodel, UserService, CurrencyService, TransferService, SysDBTool);
            return Json(result);
        }
        /// <summary>
        /// 转出记录
        /// </summary>
        /// <returns></returns>
        public ActionResult TransferOutRecord()
        {
            ActMessage = "转出记录";
            ViewBag.Title = ActMessage;

            return View();
        }
        /// <summary>
        /// 转入记录
        /// </summary>
        /// <returns></returns>
        public ActionResult TransferInRecord()
        {
            ActMessage = "转入记录";
            ViewBag.Title = ActMessage;

            return View();
        }

        /// <summary>
        /// 获取转出记录
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getTransferList(int? page)
        {
            int pageSize = 10;
            int status = Request["status"].ToInt();//0转出1转入
            IQueryable<JN.Data.Transfer> list;
            if (status == 0)//0转出
            {
                list = TransferService.List(x => x.UID == Umodel.ID);
            }
            else
            {
                list = TransferService.List(x => x.ToUID == Umodel.ID);
            }

            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 每日奖励
        /// <summary>
        /// 每日奖励
        /// </summary>
        /// <returns></returns>
        public ActionResult DayReward()
        {
            ActMessage = "每日奖励";
            ViewBag.Title = ActMessage;
            return View();
        }
        /// <summary>
        /// 获取奖励记录
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getDayRewardList(int? page)
        {
            int pageSize = 10;
            int status = Request["status"].ToInt();//0转出1转入
            var list = ReleaseDetailService.List(x => x.UID == Umodel.ID);

            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => !x.IsSign).ThenByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 领取奖励
        /// </summary>
        /// <param name="id">id值</param>
        /// <returns></returns>
        public ActionResult SignReward(int id)
        {
            Result result = new Result();
            try
            {
                var model = ReleaseDetailService.Single(x => x.ID == id && x.UID == Umodel.ID && x.IsSign == false);
                if (model == null) throw new CustomException("已经领过了，不要贪心哦！");
                if (model.EndTime < DateTime.Now) throw new CustomException("哎呀，奖励过期了！");

                var cur = CurrencyService.Single(x => x.ID == model.CurID);
                if (cur == null) throw new CustomException("钱包不存在！");

                //创建缓存
                string msg = Services.Tool.CacheHelp.CheckFromCommitCache(Umodel.ID.ToString() + model.ID, 60);
                if (!string.IsNullOrEmpty(msg)) throw new CustomException(msg);


                model.IsSign = true;
                model.SignTime = DateTime.Now;
                ReleaseDetailService.Update(model);
                Wallets.changeWallet(Umodel.ID, model.Money, (int)cur.WalletCurID, model.Description + ",奖励领取", cur);
                //清除缓存
                Services.Tool.CacheHelp.ClearFromCommitCache(Umodel.ID.ToString() + model.ID);
                result.Status = 200;
                result.Message = "操作成功！";

            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        /// <param name="id">id值</param>
        /// <returns></returns>
        public ActionResult SignRewardAll()
        {
            Result result = new Result();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("SignRewardAll" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("SignRewardAll" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                var releaseDetailList = ReleaseDetailService.List(x => x.UID == Umodel.ID && x.IsSign == false && x.EndTime > DateTime.Now).ToList();
                if (releaseDetailList.Count() <= 0) throw new CustomException("已经领过了，不要贪心哦！");

                var cur = CurrencyService.Single(x => x.ID == 3);
                if (cur == null) throw new CustomException("钱包不存在！");
                foreach (var item in releaseDetailList)
                {
                    var model = ReleaseDetailService.Single(item.ID);
                    model.IsSign = true;
                    model.SignTime = DateTime.Now;
                    ReleaseDetailService.Update(model);
                    Wallets.changeWallet(Umodel.ID, model.Money, (int)cur.WalletCurID, model.Description + ",奖励领取", cur);
                }
                result.Status = 200;
                result.Message = "操作成功！";

            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(Request.Url.ToString(), ex);
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("SignRewardAll" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }
        #endregion

        #region 帐户/帐户明细
        /// <summary>
        /// 帐户明细列表
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult UserWalletLog()
        {
            ViewBag.Title = "个人钱包流水";
            ActMessage = ViewBag.Title;
            return View();
        }
        /// <summary>
        /// 获取帐户流水数据
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getUserWalletLog(int? coin, int? page)
        {
            int pageSize = 10;
            var list = WalletLogService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.CreateTime);//显示所有

            var cmodelList = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List().ToList();
            var listdata = list.ToPagedList(page ?? 1, pageSize);//取数据

            string language = JN.Services.Resource.ResourceProvider.Culture;// "zh-CN";


            if (language != "zh-CN")
            {
                foreach (var item in listdata)
                {
                    item.CoinName = cmodelList.Single(x => x.WalletCurID == item.CoinID).EnSigns;
                }
            }


            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 钱包流水详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UserWalletLogDetail(int id)
        {
            ViewBag.Title = "钱包流水详情";
            var model = WalletLogService.Single(id);
            return View(model);
        }

        #endregion

        #region 提现
        public ActionResult Putforward()
        {
            ViewBag.Title = "提现";
            return View();
        }

        [HttpPost]
        public ActionResult ApplyTakeCash(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string amount = form["Amount"];
                string walletaddress = form["WalletAddress"];
                string password2 = form["password2"];

                var cur = CurrencyService.Single(x => x.WalletCurID == 3003);
                decimal d = JN.Services.Manager.Users.WalletCur((int)cur.WalletCurID, Umodel);
                if (!Umodel.IsActivation) throw new CustomException("您的帐号未激活或已过期，无法进行相关操作");
                if (Umodel.IsLock) throw new CustomException("您的帐号受限，无法进行相关操作");
                //if (!(Umodel.IsAuthentication??false)) throw new CustomException("您还没有实名验证，请您实名验证后再操作");
                //if (!(Umodel.IsMobile ?? false)) throw new CustomException("您还没有手机认证，请您手机认证后再操作");

                if (password2.ToMD5().ToMD5() != Umodel.Password2) throw new CustomException("交易密码不正确");
                if (amount.ToDecimal() <= 0) throw new CustomException("请填写正确的数量");

                //单笔限制
                if ((cur.TbMinLimit ?? 0) > 0 && (cur.TbMinLimit ?? 0) > amount.ToDecimal()) throw new CustomException("低于单笔提币额：" + cur.TbMinLimit);
                //单笔限制
                if ((cur.TbMaxLimit ?? 0) > 0 && (cur.TbMaxLimit ?? 0) < amount.ToDecimal()) throw new CustomException("超出单笔提币额：" + cur.TbMaxLimit);

                string remark = form["remark"];
                if (remark.Trim().Length > 100) throw new Exception("备注长度不能超过100个字节");

                var wallet = Users.WalletCur(3003, Umodel).ToDecimal();
                if (wallet < amount.ToDecimal()) throw new Exception("您的帐户余额不足");

                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    Wallets.doTakeCashRa(Umodel, amount.ToDecimal(), "会员提现", cur);
                    //Wallets.doTakeCash(Umodel,2001, drawmoney.ToDecimal(), remark,Umodel.BankName.ToInt(),"离线");
                    ts.Complete();
                    result.Status = 200;
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 充值
        public ActionResult Recharge()
        {
            ViewBag.Title = "充值";
            return View();
        }

        #region 充值汇款
        public ActionResult ApplyRemittance()
        {
            ActMessage = "充值";
            return View(Umodel);
        }

        [HttpPost]
        public ActionResult ApplyRemittance(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("ApplyRemittance" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("ApplyRemittance" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                string rechargeNumber = form["rechargeNumbers"];
                string tradeinPassword = form["tradeinPassword"];
                string Remark = form["remark"];//""; 
                string rechargeway = "线下充值";// form["rechargeway"];//汇款方式出

                //JN.Data.SysParam param1908 = cacheSysParam.Single(x => x.ID == 1908); //最低交易数量
                //decimal PARAM_LowNumber = param1908.Value2.ToDecimal();
                //decimal PARAM_HighNumber = param1908.Value3.ToDecimal();

                if (tradeinPassword == null) throw new Exception("交易密码不能为空");

                if (Umodel.Password2 != tradeinPassword.ToMD5().ToMD5()) throw new Exception("交易密码不正确");
                //if (String.IsNullOrEmpty(Umodel.RealName) || String.IsNullOrEmpty(Umodel.Mobile)
                //    || String.IsNullOrEmpty(Umodel.BankCard)
                //    )//|| String.IsNullOrEmpty(Umodel.Province) || String.IsNullOrEmpty(Umodel.IDCard)
                //{
                //    throw new Exception("您的资料不完整，无法进行相关操作");
                //}
                if (Umodel.IsLock || !Umodel.IsActivation) throw new Exception("您的帐号受限，无法进行相关操作");
                if (rechargeNumber == null || rechargeNumber.ToDecimal() <= 0) throw new Exception("交易数量不正确");
                //if (putonmoney.ToDecimal() % param1908.Value.ToDecimal() != 0) throw new Exception("投资金额需为" + param1908.Value + "的倍数");
                //if (putonmoney.ToDecimal() < PARAM_LowNumber) throw new Exception("申请交易数量不能低于" + PARAM_LowNumber + "");
                //if (putonmoney.ToDecimal() > PARAM_HighNumber) throw new Exception("申请交易数量不能高于" + PARAM_HighNumber + "");
                var param1902 = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3111);
                decimal jifen = rechargeNumber.ToDecimal() * param1902.Value2.ToDecimal();//rechargeNumber为充值的以太坊数量，param1902.Value为以太坊价格，param1902.Value2为翻倍数量
                HttpPostedFileBase file = Request.Files["PayImg"];
                string imgurl = "";
                if (Request.Files.Count == 0) throw new Exception("请上传支付图片凭证！");
                if ((file != null) && (file.ContentLength > 0))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {
                        ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                        return View("Error");
                    }
                    //20160711安全更新 ---------------- start
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    string mapPath = "/Upload/Remittance/" + Umodel.ID + "/";
                    if (!Directory.Exists(Request.MapPath(mapPath)))
                        Directory.CreateDirectory(Request.MapPath(mapPath));

                    if (Path.GetExtension(file.FileName).ToLower().Contains("aspx"))
                    {
                        var wlog = new Data.WarningLog();
                        wlog.CreateTime = DateTime.Now;
                        wlog.IP = Request.UserHostAddress;
                        if (Request.UrlReferrer != null)
                            wlog.Location = Request.UrlReferrer.ToString();
                        wlog.Platform = "会员";
                        wlog.WarningMsg = "试图上传木马文件";
                        wlog.WarningLevel = "严重";
                        wlog.ResultMsg = "拒绝";
                        wlog.UserName = Umodel.RealName;
                        MvcCore.Unity.Get<IWarningLogService>().Add(wlog);

                        Umodel.IsLock = true;
                        MvcCore.Unity.Get<IUserService>().Update(Umodel);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                        throw new Exception("试图上传木马文件，您的帐号已被冻结");
                    }

                    var fileName = Path.Combine(Request.MapPath(mapPath), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath(mapPath), 1024, 768, "EQU");
                        imgurl = mapPath + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("上传失败：" + ex.Message);
                    }
                    finally
                    {
                        System.IO.File.Delete(fileName); //删除原文件
                    }
                    //20160711安全更新  --------------- end
                }
                //写入交易表
                var model = new Data.Remittance
                {
                    CreateTime = DateTime.Now,
                    ChongNumber = Guid.NewGuid().ToString(),
                    PayOrderNumber = "",
                    Platform = 0,
                    RechargeAmount = rechargeNumber.ToDecimal(),
                    ReserveDecamal = rechargeNumber.ToDecimal(), //param1902.Value.ToDecimal(),//记录以太坊价格
                    ActualAmount = jifen,
                    RechargeWay = rechargeway,
                    Remark = Remark,
                    UID = Umodel.ID,
                    UserName = Umodel.UserName,
                    Status = (int)JN.Data.Enum.RechargeSatus.Wait,
                    RechargeDate = DateTime.Now, //rechargedate.ToDateTime()
                    PayImg = imgurl
                };
                MvcCore.Unity.Get<IRemittanceService>().Add(model);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("ApplyRemittance" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }
        #endregion

        #endregion

        #region 交易记录
        public ActionResult Transactionrecord()
        {
            ViewBag.Title = "交易记录";
            return View();
        }
        #endregion

        #region 账单详情1
        public ActionResult Details1()
        {
            ViewBag.Title = "账单详情";
            return View();
        }
        #endregion

        #region 充值提现账单
        public ActionResult Bill()
        {
            ViewBag.Title = "充值提现账单";
            return View();
        }
        #endregion

        #region 获取提现记录
        public ActionResult getCashList(int? page)
        {
            var list = MvcCore.Unity.Get<JN.Data.Service.ITakeCashService>().List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID);
            int pageSize = 10;
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取充值记录
        public ActionResult getRemittanceList(int? page)
        {
            var list = MvcCore.Unity.Get<JN.Data.Service.IRemittanceService>().List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID);
            int pageSize = 10;
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 账单详情2
        public ActionResult Details2(int id)
        {
            ViewBag.Title = "账单详情";
            var model = TakeCashService.SingleAndInit(x => x.ID == id && x.UID == Umodel.ID);
            return View(model);
        }
        #endregion

        #region 账单详情2
        public ActionResult Details3(int id)
        {
            ViewBag.Title = "账单详情";
            var model = MvcCore.Unity.Get<JN.Data.Service.IRemittanceService>().SingleAndInit(x => x.ID == id && x.UID == Umodel.ID);
            return View(model);
        }
        #endregion

        #region 汇率表
        public ActionResult Exchangerate()
        {
            ViewBag.Title = "汇率表";
            return View();
        }
        #endregion

        #region 买入
        public ActionResult Purchase()
        {
            var id = Request["advertiseid"].ToInt();
            var advertiseModel = MvcCore.Unity.Get<IAdvertiseService>().Single(id);
            ActMessage = (advertiseModel.Direction==0? "买入":"卖出")+advertiseModel.CurName;

            return View(advertiseModel);
        }
        #endregion

        #region 检测会员是否存在
        [HttpPost]
        public ActionResult TestingUser(string recusername)
        {
            //ReturnResult result = new ReturnResult();
            var recUser = UserService.Single(x => x.R3001 == recusername.Trim());
            if (recUser == null)
            {
                //result.Message = "会员不存在";
                //result.Status = 500;
                ////var shouji= "会员不存在";
                return Json(new { Status = 500, Message = "转账地址错误！", Name = "转账地址错误！", Shouji = "转账地址错误！" });
            }
            else
            {
                //result.Message = recUser.RealName;
                //result.Status = 200;
                //var shouji = Regex.Replace(recUser.Mobile, "(\\d{3})\\d{4}(\\d{4})", "*******$1$2");
                var shouji = Regex.Replace(recUser.Mobile, "(\\d{7})(\\d{4})", "*******$2");
                var name = "";
                if (recUser.RealName != "" && recUser.RealName != null)
                    name = ReplaceWithSpecialChar(recUser.RealName, 0, 1, '*');
                return Json(new { Status = 200, Message = recUser.UserName, Name = name, Shouji = shouji });
            }
            //return Json(result);
            //return Json(new { Status = 500, Message = "对不起，你的手机号不存在！" });
        }
        #endregion


        #region 替换字符
        /// <summary>
        /// 将传入的字符串中间部分字符替换成特殊字符
        /// </summary>
        /// <param name="value">需要替换的字符串</param>
        /// <param name="startLen">前保留长度</param>
        /// <param name="endLen">尾保留长度</param>
        /// <param name="replaceChar">特殊字符</param>
        /// <returns>被特殊字符替换的字符串</returns>
        private static string ReplaceWithSpecialChar(string value, int startLen = 4, int endLen = 4, char specialChar = '*')
        {
            try
            {
                int lenth = value.Length - startLen - endLen;

                string replaceStr = value.Substring(startLen, lenth);

                string specialStr = string.Empty;

                for (int i = 0; i < replaceStr.Length; i++)
                {
                    specialStr += specialChar;
                }

                value = value.Replace(replaceStr, specialStr);
            }
            catch (Exception)
            {
                throw;
            }

            return value;
        }
        #endregion

    }
}
