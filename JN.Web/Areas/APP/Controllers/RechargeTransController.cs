using JN.Data;
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
    public class RechargeTransController : BaseController
    {
        object obj = new object();
        private static List<Data.SysParam> cacheSysParam = null; 
        private readonly IUserService UserService;
        private readonly IRaCoinService RaCoinService;
        private readonly IRemittanceService RemittanceService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly ICurrencyService CurrencyService;
        public RechargeTransController(ISysDBTool SysDBTool,
            IUserService UserService,
            IRemittanceService RemittanceService,
            IActLogService ActLogService,
            ICurrencyService CurrencyService,
            IWalletLogService WalletLogService,
            IRaCoinService RaCoinService
            )
        {
            this.UserService = UserService;
            this.RemittanceService = RemittanceService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.CurrencyService = CurrencyService;
            this.WalletLogService = WalletLogService;
            this.RaCoinService = RaCoinService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();

        }

        #region 人民币充值和充值列表
        /// <summary>
        /// 人民币充值
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult RMBRecharge(int? page)
        {
            ViewBag.Title = cacheSysParam.SingleAndInit(x => x.ID == 2001).Name + "充值";
            ActMessage = ViewBag.Title;
            var list = RemittanceService.List(x => x.UID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID);
            if (Request.IsAjaxRequest())
                return View("_RMBRecharge", list.ToPagedList(page ?? 1, 20));

            return View(list.ToPagedList(page ?? 1, 20));
        }
        /// <summary>
        /// 人民币充值提交
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RMBRecharge(FormCollection form)
        {

            ViewBag.Title = "人民币充值提交";
            Result result = new Result();
            result=JN.Services.Manager.Finance.RMBRecharge(form, Umodel, RemittanceService);
            JN.Data.Remittance model = new Remittance();
            if (result.Status == 200)
            {
               int remid= result.Message.ToInt();
               model = RemittanceService.SingleAndInit(remid);
            }
            return Json(new { Status = result.Status, data = model, Message=result.Message});
        }
        /// <summary>
        /// 取消充值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult CancelRemittance(int id)
        {
                ViewBag.Title = "取消充值";
                Result result = new Result();
                var model = RemittanceService.Single(id);
                if (model != null && model.UID == Umodel.ID)
                {
                    result = JN.Services.Manager.Finance.CancelRemittance(id, RemittanceService);
                }
                else {
                    result.Message = "非法操作!";
                }
                return Json(result);
        }
        #endregion    

        #region 币种转入充值

        /// <summary>
        /// xx币转入
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult CurTransferEnter()
        {
            var c = CurrencyService.List(x => x.LineSwitch && x.IsRecharge && !(x.IsCashCurrency ?? false)).Count() == 0 ? new JN.Data.Currency { ID = 0 } : CurrencyService.List(x => x.LineSwitch && x.IsRecharge && !(x.IsCashCurrency ?? false)).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条
            if (c.ID == 0) return Redirect("/app/home/index");
            int curid = Request["curid"].ToInt();
            var cur = (curid == 0) ? c : CurrencyService.Single(x => x.ID == curid);

            ActMessage = cur.CurrencyName + "转入";
            return View(cur);
        }

        /// <summary>
        /// 充值列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult CurTransferEnterList()
        {
            ActMessage = "充值列表";

            var c = CurrencyService.List(x => x.LineSwitch && x.IsRecharge && !(x.IsCashCurrency ?? false)).Count() == 0 ? new JN.Data.Currency { ID = 0 } : CurrencyService.List(x => x.LineSwitch && x.IsRecharge && !(x.IsCashCurrency ?? false)).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条
            if (c.ID == 0) return Redirect("/app/home/index");
            int curid = Request["curid"].ToInt();
            var cur = (curid == 0) ? c : CurrencyService.Single(x => x.ID == curid);
            ViewBag.Currency = cur;

            return View();
        }
        /// <summary>
        /// 获取充值列表数据
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getCurTransferEnterList(int? page)
        {
            int pageSize = 10;
            int curid = Request["curid"].ToInt();

            var list = RaCoinService.List(x => x.UID == Umodel.ID && x.Direction == "in" && x.CurID == curid);
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 充值详情
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult CurTransferEnterDetail(int id)
        {
            ActMessage = "充值详情";
            ViewBag.Title = ActMessage;
            var model = RaCoinService.SingleAndInit(x => x.ID == id && x.UID == Umodel.ID);
            return View(model);
        }
        #endregion


        #region 币种流水交易记录

        /// <summary>
        /// xx币种流水交易记录
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActionResult CurEnterAccountDetail(int? page)
        {

            var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => x.TranSwitch).Count() == 0 ? new JN.Data.Currency { ID = 0 } : MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => x.TranSwitch).OrderBy(x => x.ID).FirstOrDefault();//获取最后一条

            if (c.ID == 0) return Redirect("/home/index");

            var cmodel = (string.IsNullOrEmpty(Request["curid"])) ? c : MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(Request["curid"].ToInt());

            ViewBag.Title = cmodel.CurrencyName + "充值";

            ActMessage = ViewBag.Title;
            ViewBag.Currency = cmodel;

            //币种充值列表
            var list = WalletLogService.List(x => x.UID == Umodel.ID && x.CoinID==cmodel.WalletCurID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).OrderByDescending(x => x.ID).ToList();

           // GetListTransactions(cmodel.ID);//刷新获取充值记录

            if (Request.IsAjaxRequest())//异步获取
                return View("_AccountDetail", list.ToPagedList(page ?? 1, 10));

            return View(list.ToPagedList(page ?? 1, 10));

        }
        #endregion
   
        #region 生成离线地址
        /// <summary>
        /// 充值生成地址
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AjaxCreateNewAddress()
        {
            Result result = new Result();
            try
            {
                if (CacheExtensions.CheckCache("AjaxCreateNewAddress" + Umodel.ID))//查找缓存是否存在
                {
                    throw new CustomException("正在处理数据,请耐心等待。");
                }
                CacheExtensions.SetCache("AjaxCreateNewAddress" + Umodel.ID, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 10);//每10分钟

                int curid = Request["curid"].ToInt();
                //获取币种实体
                var cur = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(curid);
                if (cur == null) throw new Exception("获取失败，查无此币种");
                result= WalletsAPI.GetWalletAddress(Umodel, cur);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试！";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            finally
            {
                CacheExtensions.ClearCache("AjaxCreateNewAddress" + Umodel.ID);  //清空
            }
            return this.JsonResult(result);
        }
        #endregion

        #region 写入转入表并获取转入充值记录
        /// <summary>
        /// 获取转入记录 Name:Lin  Time:2017年7月18日14:29:21
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetListTransactions()
        {
            ReturnResult<List<ResponseListTransactions>> result = new ReturnResult<List<ResponseListTransactions>>();
            try
            {
                if (CacheExtensions.CheckCache("GetListTransactions" + Umodel.ID))//查找缓存是否存在
                {
                    throw new CustomException("正在处理数据,请耐心等待。");
                }
                CacheExtensions.SetCache("GetListTransactions" + Umodel.ID, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 10);//每1分钟
                int curid = Request["curid"].ToInt();
                //获取币种实体
                var curmodel = CurrencyService.Single(x => x.ID == curid);
                if (curmodel == null) throw new CustomException("获取失败，查无此币种");

                result = WalletsAPI.GetListTransactions(Umodel, curmodel, RaCoinService, SysDBTool);
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试！";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            finally
            {
                CacheExtensions.ClearCache("GetListTransactions" + Umodel.ID);  //清空
            }
            return Json(result);
        }
        #endregion


        #region 线下充值（需要后台审核）
        /// <summary>
        /// 以太坊网络充值（需要后台审核） Name:Lin  Time:2017年7月18日14:29:21
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RechargeEthereum()
        {
            Result result = new Result();
            int curid = Request["curid"].ToInt();
            decimal amount = Request["Amount"].ToDecimal();
            string Address = Request["Address"];
            string message = Request["message"];
            string password2 = Request["password2"];
            try
            {

                //获取币种实体
                var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == curid && !(x.IsAutomatic ?? false));
                if (c == null) throw new CustomException("获取失败，查无此币种，或者不是以太坊网络币种");
                if (Umodel.IsLock) throw new CustomException("您的帐号受限，无法进行相关操作");
               // if (!(Umodel.IsAuthentication ?? false)) throw new CustomException("您还没有实名验证，请您实名验证后再操作");
                if (!(Umodel.IsMobile ?? false)) throw new CustomException("您还没有手机认证，请您手机认证后再操作");

               // if (password2.ToMD5().ToMD5() != Umodel.Password2) throw new CustomException("交易密码不正确");

                if (amount <= 0) throw new CustomException("充值数量必须大于0");

                //if (string.IsNullOrEmpty(Address)) throw new Exception("接收地址不能为空");
                //Wallets.changeWallet(Umodel.ID, item.amount.ToDecimal(), (int)c.WalletCurID, c.CurrencyName + "充值收入", c);

                //写入充值表
                RaCoinService.Add(new Data.RaCoin
                {
                    ActualDrawMoney = amount,
                    WalletAddress = Address,
                    CreateTime = DateTime.Now,
                    DrawMoney = amount,
                    Status = (int)JN.Data.Enum.TakeCaseStatus.Wait,
                    Poundage = 0,
                    Direction = "in",
                    UID = Umodel.ID,
                    UserName = Umodel.UserName,
                    ReserveStr1 = "",
                    Remark = message,
                    ReserveInt = 0,
                    CurID = c.ID
                });
                SysDBTool.Commit();
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
        #endregion


        #region 检查充值金额（预留）

        /// <summary>
        /// 检查充值金额
        /// </summary>
        /// <param name="address">真正生产环境这里不应该传转账地址，应该传充值订单id，以防别人利用漏洞</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AjaxCheckRecharge()
        {
            int curid = Request["curid"].ToInt();
            //获取币种实体
            var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(curid);
            if (c == null) throw new Exception("获取失败，查无此币种");

            ReturnResult<double> result = new ReturnResult<double>();
            string address = Umodel.Racoin;
            result = BitcionHelper.GetSqlServerAddressReceivedByAddress(c.WalletKey, address);
            //如果成功，保存充值金额到订单并更新订单
            if (result.Status == ReturnResultStatus.Succeed.GetShortValue())
            {
                if (result.Data > 0)
                {
                    //todo 保存充值金额到订单并更新订单
                    //Wallets.doTakeCashRa(Umodel, 0 - amount.ToDecimal(), walletaddress);
                }
                else
                {
                    result = BitcionHelper.GetSqlServerAddressReceivedByAddress(c.WalletKey, address, 0);
                    if (result.Status == ReturnResultStatus.Succeed.GetShortValue())
                    {
                        if (result.Data > 0)
                        {
                            result.Status = ReturnResultStatus.NoData.GetShortValue();
                            result.Message = string.Format("已检测到你有转账:{0},但这笔交易的状态还没有完成，属于待确认状态，请过一段时间后再进行确认！", result.Data);
                            return this.JsonResult(result);
                        }

                    }
                    result.Status = ReturnResultStatus.NoData.GetShortValue();
                    result.Message = "未检测到您的充值金额，请确认是否转账成功";
                }
            }

            return this.JsonResult(result);
        }

        #endregion

        #region 获取支付方式
        //
        //获取付款方式信息
        public JsonResult getparamInfo(string paramName)
        {
            var data = cacheSysParam.SingleAndInit(x => x.Name == paramName && x.PID == 3900);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        #endregion


    }
}
