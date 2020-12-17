using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using JN.Services.Tool;
using System.Text;
using System.Data.Entity.SqlServer;
using JN.Services.Manager;
using JN.Data.Extensions;
using JN.Services.CustomException;
using System.Data.Entity.Validation;
using System.IO;

namespace JN.Web.Areas.APP.Controllers
{
 
    public class MarketController : BaseController
    {
        #region 初始化，构造函数
        private readonly IUserService UserService;
        private readonly ISupplyHelpService SupplyHelpService;
        private readonly IAcceptHelpService AcceptHelpService;
        private readonly IMatchingService MatchingService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ISysSettingService SysSettingService;
        private readonly IUserBankCardService UserBankCardService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        public MarketController(ISysDBTool SysDBTool,
            IUserService UserService,
            ISupplyHelpService SupplyHelpService,
            IAcceptHelpService AcceptHelpService,
            IMatchingService MatchingService,
            IBonusDetailService BonusDetailService,
            ISysSettingService SysSettingService,
            IUserBankCardService UserBankCardService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SupplyHelpService = SupplyHelpService;
            this.AcceptHelpService = AcceptHelpService;
            this.MatchingService = MatchingService;
            this.BonusDetailService = BonusDetailService;
            this.SysSettingService = SysSettingService;
            this.UserBankCardService = UserBankCardService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
           
        }

        #endregion

        public ActionResult Buy()
        {
            ViewBag.Title = "买入";
            return View();
        }

        #region 买入记录
        public ActionResult BuyRecord()
        {
            ViewBag.Title = "买入记录";
            return View();
        }
        public ActionResult GetBuyRecord(int? page)
        {
            var list = SupplyHelpService.List(x => x.UID == Umodel.ID);
            //if (Status == 1)
            //{
            //    list = list.Where(x => x.Status >= (int)JN.Data.Enum.HelpStatus.NoMatching && x.Status <= (int)JN.Data.Enum.HelpStatus.PartOfDeal);
            //}
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.ReserveStr1 = typeof(JN.Data.Enum.HelpStatus).GetEnumDesc(item.Status);
            }
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 提供帮助
        [HttpPost]
        //public ActionResult SupplyHelp(FormCollection fc)
        public ActionResult SupplyHelp(int type, string pwd)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("SupplyHelp" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("SupplyHelp" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                List<JN.Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 8000).ToList();
                //代码运行时间监控 --------------------- 
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                #region 接收数据
                //string SupplyAmount = fc["supplyamount"];
                string Type = type.ToString();// fc["type"];
                string PayWay = Umodel.BankName;// fc["payway"];
                //string day = fc["day"];
                #endregion
                var param1600 = cacheSysParam.Single(x => x.ID == Type.Trim().ToInt());
                if (param1600 == null) throw new CustomException("请选择买入类型");


                #region 验证数据
                if (!Umodel.IsActivation) throw new CustomException("您账号还未激活，不能操作此功能！");


                string tradePassword = pwd;// fc["tradepassword"];
                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException("交易密码不正确");
                //var SupplyList = SupplyHelpService.List(x => x.UID == Umodel.ID).ToList().OrderByDescending(x => x.CreateTime).FirstOrDefault();
                //if (SupplyList != null)    //如果提现了就不能复投
                //{
                //    var acceptList = AcceptHelpService.Single(x => x.UID == Umodel.ID && x.CreateTime > SupplyList.CreateTime);
                //    if (acceptList == null) throw new CustomException("无法执行当前操作！");
                //}

                var bankDefault = UserBankCardService.Single(x => x.UID == Umodel.ID && x.IsDefault);
                if (bankDefault == null) throw new CustomException("请先设置默认银行卡");
                //if (string.IsNullOrEmpty(Umodel.BankCard) && string.IsNullOrEmpty(Umodel.WeiXin) && string.IsNullOrEmpty(Umodel.AliPay))
                //    throw new CustomException("您还未填写任何一个收款帐号（银行卡、支付宝），请到设置中完善资料！");

                //if (string.IsNullOrEmpty(PayWay)) throw new CustomException("请选择付款方式！");
                //if (SupplyAmount.ToDecimal() <= 0) throw new CustomException("请您填写提供帮助的金额！");

                #endregion


                #region 排单限制
                //var Param = cacheSysParam.Single(c => c.ID == Convert.ToInt32(3001));
                //if (Umodel.UserLevel == 1003)
                //{
                //    Param = cacheSysParam.Single(c => c.ID == Convert.ToInt32(3003));
                //}
                //else if (Umodel.UserLevel== 1002)
                //{
                //    Param = cacheSysParam.Single(c => c.ID == Convert.ToInt32(3002));
                //}
                //else
                //{
                //    Param = cacheSysParam.Single(c => c.ID == Convert.ToInt32(3001));
                //}
                decimal ExchangeAmount = param1600.Value.ToDecimal();// *cacheSysParam.SingleAndInit(x => x.ID == 3801).Value.ToDecimal();　//汇率参数


                //decimal minmoney = Param.Value.Split('-')[0].ToDecimal(); //提供帮助金额限制参数(基准最小) 
                //decimal maxmoney = Param.Value.Split('-')[1].ToDecimal(); //提供帮助金额限制参数(基准最大)
                //int beisu = Param.Value2.ToInt(); //金额倍数

                //var LastList = SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status > 0).ToList();
                //Data.SupplyHelp LastSup = null;

                //if (LastList.Count() > 0)
                //{
                //    LastSup = LastList.OrderByDescending(x => x.CreateTime).First();
                //}
                //// var notFinishSup = SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status < (int)Data.Enum.HelpStatus.AllMatching && x.Status > 0 && x.OrderType == 0).Count();
                //bool isfrist = false;
                //var PaiDanTimesLimit = cacheSysParam.Single(x => x.ID == 3012).Value.ToInt();
                //if (LastSup != null)
                //{
                //    //  var Mmodel = MatchingService.Single(x => x.SupplyNo == LastSup.SupplyNo);
                //    var checkTime = LastSup.CreateTime.ToDateTime().AddMinutes(PaiDanTimesLimit);
                //    if (DateTime.Now < checkTime)
                //    {
                //        throw new CustomException("你的下一单可排单时间为：" + checkTime.ToString());
                //    }
                //}
                //else
                //{
                //    isfrist = true;
                //    ExchangeAmount = cacheSysParam.Single(x => x.ID == 3011).Value.ToDecimal();
                //}

                //if (ExchangeAmount < minmoney) throw new CustomException("对不起，排单金额不能少于" + minmoney + "");
                ////if (ExchangeAmount > Umodel.InvesAccount) throw new CustomException("对不起，您的单笔排单金额最高为" + Umodel.InvesAccount + "");
                //if (ExchangeAmount > maxmoney) throw new CustomException("对不起，排单金额不能大于" + maxmoney + "");

                //if (ExchangeAmount % beisu != 0) throw new CustomException("金额必须是" + beisu + "的倍数！");


                //var PaiDanParam = cacheSysParam.SingleAndInit(x => x.ID == 3005)
                //decimal PaiDanM = cacheSysParam.SingleAndInit(x => x.ID == 3005).Value.ToDecimal();   //扣除排单币
                //if (PaiDanM.ToDecimal() > Umodel.Wallet2003)
                //{
                //    throw new CustomException("对不起，你的激活币不足");
                //}

                #endregion

                #region 插入数据，计算奖金
                DateTime _endtime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3203).Value.ToInt());  //订单到期时间;               
                //decimal _accruarate = cacheSysParam.SingleAndInit(x => x.ID == cooid.ToInt()).Value3.ToDecimal();  //基础利息
                //int _day = cacheSysParam.SingleAndInit(x => x.ID == 1102).Value.ToInt();//日收益天数 by Annie
                // logs.WriteLog("supplyhelp运行时间：－－开始事务" + sw.ElapsedMilliseconds);
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    var model = new Data.SupplyHelp();
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.SupplyAmount = param1600.Value.ToDecimal(); //申请金额
                    model.ExchangeAmount = ExchangeAmount; //汇率金额
                    model.CreateTime = DateTime.Now;
                    model.Status = (int)JN.Data.Enum.HelpStatus.NoMatching;  //状态
                    model.IsTop = false;  //是否置顶
                    model.IsRepeatQueuing = false; //是否重新排队
                    model.HaveMatchingAmount = 0; //已匹配数量
                    model.HaveAcceptAmount = 0; //
                    model.PayWay = PayWay;  //付款方式
                    model.EndTime = _endtime;
                    model.SupplyNo = Data.Extensions.SupplyHelp.GetSupplyNo();  //单号  //不能放外面，导致重号
                    model.AccrualDay = 0; //已结算利息天数
                    model.SurplusAccrualDay = cacheSysParam.SingleAndInit(x => x.ID == 3120).Value.ToInt();
                    model.AccrualMoney = 0; //已产生的利息
                    model.IsAccrualEffective = false; //利息是否生效（匹配并验证付款后才生效）
                    model.IsAccruaCount = true; //是否还计算利息 (超过30天或有接受订单产生后不再计算利息)
                    model.TotalMoney = model.ExchangeAmount;//本单总额（含利息）
                    model.AccruaRate = 0;
                    model.AreaType = 1;
                    model.OrderType = Type.Trim().ToInt();
                    model.OrderOrigin = 0; //会员自己的提供单
                    model.OrderMoney = ExchangeAmount;
                    model.publicSupply = "";
                    //model.BankID = bankDefault.ID;
                    //model.IsFirst = isfrist;
                    SupplyHelpService.Add(model);
                    SysDBTool.Commit();

                    //  logs.WriteLog("supplyhelp运时间：－－开始计算奖金" + sw.ElapsedMilliseconds);

                    //Wallets.changeWallet(Umodel.ID, -PaiDanM, 2003, "排单【" + model.SupplyNo + "】扣除激活币");

                    //Bonus.Bonus1104(model);//计算团队奖（有奖金烧伤）
                    //Bonus.Bonus1102(model);//计算利息


                    ts.Complete();
                    result.Status = 200;
                }

                //if (SysSettingService.Single(1).MatchingMode == 1)
                //{
                //    string outMsg = "";
                //    MMM.Matching("", "", ref outMsg); //自动匹配
                //}

                //代码运行时间监控 --------------------- 
                //   sw.Stop();
                //  logs.WriteLog("supplyhelp运行时间：" + sw.ElapsedMilliseconds);
                //Console.WriteLine(sw.ElapsedMilliseconds); //代码运行时间  
                #endregion
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("SupplyHelp" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }

        #endregion

        #region 买入中心
        public ActionResult BuyCenter()
        {
            ViewBag.Title = "买入记录";
            return View();
        }
        public ActionResult GetBuyCenter(int? page,int? type)
        {
            string[] userIds = GetUserID();
            var list = AcceptHelpService.List(x => x.UID != Umodel.ID && !userIds.Contains(x.UID.ToString()) && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal);
            if (type != null)
            {
                list = list.Where(x => x.OrderType == type);
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                var aa = UserService.Single(item.UID);
                item.ReserveStr1 = UserService.Single(item.UID).HeadFace ?? "/Theme/App/images/member.png";
            }
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 买入提交
        public ActionResult Buyconfirm(int id, string pwd)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("Buyconfirm" + id))
                {
                    throw new CustomException("该订单正已经被别人买入，请选择其他订单");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("Buyconfirm" + id, id, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                var aModel = AcceptHelpService.Single(id);
                if (aModel == null) throw new CustomException("错误的单子，请稍后再试！");
                if (aModel.Status != (int)JN.Data.Enum.HelpStatus.NoMatching)
                {
                    throw new CustomException("该订单已经被买入，请选择其他订单");
                }
                var onUser = UserService.Single(Umodel.ID);
                if (aModel.UID == onUser.ID) throw new CustomException("不能抢自己的单");
                if (Umodel.Password2 != pwd.ToMD5().ToMD5()) throw new CustomException("交易密码不正确");

                if (!onUser.IsActivation) throw new CustomException("您账号还未激活，不能操作此功能！");
                if (onUser.IsLock) throw new CustomException("您账号已经被冻结，不能操作此功能！");
                var bankDefault = UserBankCardService.Single(x => x.UID == Umodel.ID && x.IsDefault);
                if (bankDefault == null) throw new CustomException("请先设置默认银行卡");
                //if (string.IsNullOrEmpty(onUser.BankCard))
                //    throw new CustomException("您还未填写任何一个收款帐号（银行卡），请先完善资料");
                DateTime _endtime = DateTime.Now.AddMinutes(MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3203).Value.ToInt());  //订单到期时间;  
                string outMsg = "";
                string SupNo = "";
                decimal M = 0;
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    var model = new Data.SupplyHelp();
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.SupplyAmount = aModel.AcceptAmount;//aModel.ExchangeAmount-aModel.HaveMatchingAmount; //申请金额
                    model.ExchangeAmount = aModel.ExchangeAmount;// aModel.ExchangeAmount - aModel.HaveMatchingAmount; //汇率金额
                    model.CreateTime = DateTime.Now;
                    model.Status = (int)JN.Data.Enum.HelpStatus.NoMatching; //状态
                    model.IsTop = false;  //是否置顶
                    model.IsRepeatQueuing = false; //是否重新排队
                    model.HaveMatchingAmount = 0; //已匹配数量
                    model.HaveAcceptAmount = 0; //
                    model.PayWay = aModel.PayWay;  //付款方式
                    model.EndTime = _endtime;
                    model.SupplyNo = Data.Extensions.SupplyHelp.GetSupplyNo();  //单号  //不能放外面，导致重号
                    model.AccrualDay = 0; //已结算利息天数
                    model.SurplusAccrualDay = 0; //(天)
                    model.AccrualMoney = 0; 
                    model.IsAccrualEffective = false; //利息是否生效（匹配并验证付款后才生效）
                    model.IsAccruaCount = true; //是否还计算利息 (超过30天或有接受订单产生后不再计算利息)
                    model.TotalMoney = model.ExchangeAmount; //本单总额（含利息）
                    model.AccruaRate = 0;
                    model.OrderType = aModel.OrderType;
                    model.OrderOrigin = 2; //抢单
                    model.OrderMoney = aModel.ExchangeAmount;
                    model.publicSupply = "";
                    model.BankID = aModel.BankID;
                    SupplyHelpService.Add(model);
                    SysDBTool.Commit();
                    SupNo = model.SupplyNo;
                    ts.Complete();
                }
                if (SupNo != "")
                {
                    var smodel = SupplyHelpService.Single(x => x.SupplyNo == SupNo);
                    MMM.Matching(smodel.ID.ToString(), aModel.ID.ToString(), ref outMsg); //自动匹配
                    //Bonus.Bonus1104(smodel, M);//计算团队奖（有奖金烧伤）
                    //Bonus.Bonus1102(Umodel, SupplyAmount, smodel.SupplyNo);//计算利息
                }
                result.Status = 200;
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
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("Buyconfirm" + id);//清除缓存
            }
            return Json(result);
        }
        #endregion

        public ActionResult Sell()
        {
            ViewBag.Title = "卖出";
            return View();
        }


        #region 卖出记录
        public ActionResult SellRecord()
        {
            ViewBag.Title = "卖出记录";
            return View();
        }
        public ActionResult GetSellRecord(int? page)
        {
            var list = AcceptHelpService.List(x => x.UID == Umodel.ID);
            //if (Status == 1)
            //{
            //    list = list.Where(x => x.Status >= (int)JN.Data.Enum.HelpStatus.NoMatching && x.Status <= (int)JN.Data.Enum.HelpStatus.PartOfDeal);
            //}
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.ReserveStr1 = typeof(JN.Data.Enum.HelpStatus).GetEnumDesc(item.Status);
            }
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 接受帮助
        [HttpPost]
        public ActionResult AcceptHelp(int bankDefaultID, string type, string pwd)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("AcceptHelp" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("AcceptHelp" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                List<JN.Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 8000).ToList();
                #region 接受数据
                string PayWay = Umodel.BankName;// fc["payway"];
                //decimal AcceptAmount = fc["acceptamount"].ToDecimal();
                string Type = type;// fc["type"];
                int CoinID = 3;// fc["coinid"].ToInt();
                //string FormUrl = fc["formurl"]; 
                #endregion

                #region 验证数据

                if (!Umodel.IsActivation) throw new CustomException("您账号还未激活，不能操作此功能！");

                string tradePassword = pwd;// fc["tradepassword"];
                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException("交易密码不正确");

                //decimal acceptWallet = 0;
                //if (CoinID == 2001)
                //{
                //    acceptWallet = Umodel.Wallet2001;
                //    //if (Umodel.Wallet2001Lock ?? false) throw new CustomException("你的钱包已被冻结，请联系管理员！");
                //}
                //else if (CoinID == 2002)
                //{
                //    acceptWallet = Umodel.Wallet2002;
                //    //if (Umodel.Wallet2002Lock ?? false) throw new CustomException("你的钱包已被冻结，请联系管理员！");
                //}
                //else if (CoinID == 2003)
                //{
                //    acceptWallet = Umodel.Wallet2003;
                //    //if (Umodel.Wallet2003Lock ?? false) throw new CustomException("你的钱包已被冻结，请联系管理员！");
                //}
                var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == CoinID);
                decimal acceptWallet = Users.WalletCur(c.WalletCurID ?? 0, Umodel);//钱包余额
                var bankDefault = UserBankCardService.Single(x => x.ID == bankDefaultID && x.UID == Umodel.ID);
                if (bankDefault == null) throw new CustomException("请先设置默认银行卡");
                //if (string.IsNullOrEmpty(PayWay)) throw new CustomException("请选择付款方式！");
                //if (AcceptAmount <= 0) throw new CustomException("请充输入接受帮助金额！");
                var param1600 = cacheSysParam.Single(x => x.ID == Type.Trim().ToInt());
                if (param1600 == null) throw new CustomException("请选择卖出类型");
                decimal ExchangeAmount = param1600.Value.ToDecimal() * (1-cacheSysParam.SingleAndInit(x => x.ID == 3209).Value.ToDecimal());//15%手续费
                if (acceptWallet < param1600.Value.ToDecimal()) throw new CustomException("你的余额不足！");

                //Decimal bili = cacheSysParam.Single(x => x.ID == 3005).Value.ToDecimal();  //扣除排单币百分比
                //var futouM = bili * AcceptAmount;
                //if (futouM.ToDecimal() > (Umodel.Wallet2004 == null ? 0 : Umodel.Wallet2004))
                //{
                //    throw new CustomException("对不起，你的排单不足");
                //}

                // int accType = fc["accType"].ToInt(); //是否进入抢单 by Annie

                #endregion

                #region 所有限制
                //decimal beisu = 0;
                ////decimal maxmoney = 0;
                //decimal minmoney = 0;
                ////获取当月第一天和最后一天
                //DateTime nowtime = DateTime.Now;
                //DateTime Begintime = new DateTime(nowtime.Year, nowtime.Month, 1);
                //DateTime Lasttime = Begintime.AddMonths(1).AddDays(-1);
                //var jtCount = 0;// 动态钱包提现次数
                //var dtCount = 0;  //静态钱包提现次数

                //var daytakeList = AcceptHelpService.List(x => x.AcceptAmount > 0 && x.UID == Umodel.ID && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).ToList();   //当日提现条数
                //var param3009 = cacheSysParam.SingleAndInit(x => x.ID == 3009);
                //if (daytakeList.Count >= param3009.Value.ToInt()) throw new CustomException("今日提现次数已上限！");


                //if (CoinID == 2003)    //动态钱包
                //{
                //    //if (daytakeList.Where(x => x.CoinID != 2003).Count() > 0) throw new CustomException("当日仅限提现同一个钱包！");

                //    minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3007).Value2.ToDecimal();//单笔最少额度
                //    decimal beisu = cacheSysParam.SingleAndInit(x => x.ID == 3007).Value.ToDecimal();//倍数
                //    //maxmoney = cacheSysParam.SingleAndInit(x => x.ID == 3007).Value3.ToDecimal();  //日提现封顶额度
                //    var summoney = AcceptHelpService.List(x => x.AcceptAmount > 0 && x.UID == Umodel.ID && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Count() > 0 ? AcceptHelpService.List(x => x.AcceptAmount > 0 && x.UID == Umodel.ID && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Sum(x => x.AcceptAmount) : 0;   //当日提现总金额


                //    //if (ExchangeAmount > maxmoney) throw new CustomException("每日提现金额不能大于" + maxmoney + "！");
                //    //if (ExchangeAmount > maxmoney - summoney) throw new CustomException("今日可提现金额为" + (maxmoney - summoney) + "！");
                //    if (ExchangeAmount < minmoney) throw new CustomException("提现金额不能低于" + minmoney + "！");
                //    if (ExchangeAmount % beisu != 0) throw new CustomException("金额必须是" + beisu + "的倍数！");
                //}
                //if (CoinID == 2002)    //静态钱包
                //{
                //    //if (daytakeList.Where(x => x.CoinID != 2002).Count() > 0) throw new CustomException("当日仅限提现同一个钱包！");
                //    minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3007).Value2.ToDecimal();//单笔最少额度
                //    beisu = cacheSysParam.SingleAndInit(x => x.ID == 3007).Value.ToDecimal();//倍数
                //    //maxmoney = Bonus.GetBiLi(Umodel, cacheSysParam.Where(x => x.PID == 1400).OrderByDescending(x => x.ID).ToList());//月最高金额
                //    // var todayacceptlist = AcceptHelpService.List(x => x.Status > 0 && x.CreateTime > Begintime && x.CreateTime < Lasttime && x.UID == Umodel.ID);
                //    //decimal todayacceptmoney = todayacceptlist.Count() > 0 ? todayacceptlist.Sum(x => x.ExchangeAmount) : 0;

                //    //if ((todayacceptmoney + ExchangeAmount) > maxmoney)
                //    //{
                //    //    throw new CustomException("最高提现：" + maxmoney + "");
                //    //}
                //}
                //else if (CoinID == 2001)
                //{
                //    var sup = SupplyHelpService.List(x => x.UID == Umodel.ID && x.Status == (int)JN.Data.Enum.HelpStatus.AllDeal && x.IsAccruaCount && !(x.IsTakeOut ?? false)).Count();
                //    if (sup <= 0) throw new CustomException("卖出果实必须要有一个正在成长中的订单。");
                //    //if (daytakeList.Where(x => x.CoinID != 2001).Count() > 0) throw new CustomException("当日仅限提现同一个钱包！");
                //    minmoney = cacheSysParam.SingleAndInit(x => x.ID == 3006).Value2.ToDecimal();//单笔最少额度
                //    beisu = cacheSysParam.SingleAndInit(x => x.ID == 3006).Value.ToDecimal();//倍数
                //}
                //if (ExchangeAmount < minmoney) throw new CustomException("提现金额不能低于" + minmoney + "！");
                //if (ExchangeAmount % beisu != 0) throw new CustomException("金额必须是" + beisu + "的倍数！");
                #endregion

                #region 事务操作
                DateTime _endtime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3203).Value.ToInt()); //订单到期时间
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    var model = new Data.AcceptHelp();
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.CoinID = c.ID;  //币种
                    model.CoinName = c.EnSigns;  //币种名称
                    model.AcceptAmount = ExchangeAmount; //接受金额
                    model.ExchangeAmount = ExchangeAmount; //汇力转换后金额
                    model.HaveMatchingAmount = 0;  //已匹配金额
                    model.CreateTime = DateTime.Now;
                    model.Status = (int)JN.Data.Enum.HelpStatus.NoMatching;
                    model.PayWay = PayWay;  //付款方式
                    model.IsTop = false; //是否置顶
                    model.IsRepeatQueuing = false; //是否重新排队
                    model.EndTime = _endtime;
                    model.AcceptNo = Data.Extensions.AcceptHelp.GetAcceptNo();
                    model.OrderType = Type.Trim().ToInt();
                    model.fee = param1600.Value.ToDecimal() - model.ExchangeAmount;
                    //model.BankID = bankDefault.ID;
                    AcceptHelpService.Add(model);//向接受表添加纪录
                    SysDBTool.Commit();
                    Wallets.changeWallet(Umodel.ID, 0 - param1600.Value.ToDecimal(), c.WalletCurID ?? 0, "卖出订单“" + model.AcceptNo + "”扣除", c);
                    //Wallets.changeWallet(Umodel.ID, -ExchangeAmount, 2004, "接收帮助【" + model.AcceptNo + "】扣除排单币");

                    ts.Complete();
                    result.Status = 200;
                }

                //if (SysSettingService.Single(1).MatchingMode == 1)
                //{
                //    string outMsg = "";
                //    MMM.Matching("", "", ref outMsg); //自动匹配
                //}
                #endregion
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
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("AcceptHelp" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }


        #endregion

        #region 卖出中心
        public ActionResult SellCenter()
        {
            ViewBag.Title = "卖出记录";
            return View();
        }
        public ActionResult GetSellCenter(int? page, int? type)
        {
            string[] userIds = GetUserID();
            var list = SupplyHelpService.List(x => x.UID != Umodel.ID && x.Status > 0 && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)JN.Data.Enum.HelpStatus.AllDeal && !userIds.Contains(x.UID.ToString()));
            if (type != null)
            {
                list = list.Where(x => x.OrderType == type);
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                string HeadFace = UserService.Single(item.UID).HeadFace;
                item.ReserveStr1 = string.IsNullOrEmpty(HeadFace) ? "/Theme/App/images/member.png" : HeadFace;
            }
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
           
        }
        #endregion

        #region 卖出提交
        public ActionResult Sellconfirm(int id, string pwd)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("Sellconfirm" + id))
                {
                    throw new CustomException("该订单正已经被别人出售，请选择其他订单");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("Sellconfirm" + id, id, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                var sModel = SupplyHelpService.Single(id);
                if (sModel == null) throw new CustomException("错误的单子，请稍后再试！");
                if (sModel.Status != (int)JN.Data.Enum.HelpStatus.NoMatching)
                {
                    throw new CustomException("该订单已经被出售，请选择其他订单");
                }
                var onUser = UserService.Single(Umodel.ID);
                if (sModel.UID == onUser.ID) throw new CustomException("不能抢自己的单");
                if (Umodel.Password2 != pwd.ToMD5().ToMD5()) throw new CustomException("交易密码不正确");
                if (!onUser.IsActivation) throw new CustomException("您账号还未激活，不能操作此功能！");
                if (onUser.IsLock) throw new CustomException("您账号已经被冻结，不能操作此功能！");
                var bankDefault = UserBankCardService.Single(x => x.UID == Umodel.ID && x.IsDefault);
                if (bankDefault == null) throw new CustomException("请先设置默认银行卡");
                //if (string.IsNullOrEmpty(onUser.BankCard))
                //    throw new CustomException("您还未填写任何一个收款帐号（银行卡），请先完善资料");
                var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == 3);
                decimal acceptWallet = Users.WalletCur(c.WalletCurID ?? 0, Umodel);//钱包余额
                decimal fee = MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3209).Value.ToDecimal();//15%手续费
                if (acceptWallet < sModel.ExchangeAmount * (1 + fee)) throw new CustomException("你的余额不足！");
                string outMsg = "";
                string AccNo = "";
                DateTime _endtime = DateTime.Now.AddMinutes(MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3203).Value.ToInt()); //订单到期时间
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    sModel.BankID = bankDefault.ID;
                    SupplyHelpService.Update(sModel);

                    var model = new Data.AcceptHelp();
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.CoinID = c.ID;  //币种
                    model.CoinName = c.EnSigns;  //币种名称
                    model.AcceptAmount = sModel.SupplyAmount; //接受金额
                    model.ExchangeAmount = sModel.ExchangeAmount; //汇力转换后金额
                    model.HaveMatchingAmount = 0;  //已匹配金额
                    model.CreateTime = DateTime.Now;
                    model.Status = (int)JN.Data.Enum.HelpStatus.NoMatching;
                    model.PayWay = sModel.PayWay;  //付款方式
                    model.IsTop = false; //是否置顶
                    model.IsRepeatQueuing = false; //是否重新排队
                    model.EndTime = _endtime;
                    model.AcceptNo = Data.Extensions.AcceptHelp.GetAcceptNo();
                    model.OrderType = sModel.OrderType;
                    model.BankID = bankDefault.ID;
                    model.fee = model.ExchangeAmount * fee;
                    AcceptHelpService.Add(model);//向接受表添加纪录
                    SysDBTool.Commit();
                    Wallets.changeWallet(Umodel.ID, 0 - model.ExchangeAmount * (1+ fee), c.WalletCurID ?? 0, "卖出订单“" + model.AcceptNo + "”扣除", c);
                    AccNo = model.AcceptNo;
                    result.Status = 200;
                    ts.Complete();
                }
                if (AccNo != "")
                {
                    var aModel = AcceptHelpService.Single(x => x.AcceptNo == AccNo);
                    MMM.Matching(sModel.ID.ToString(), aModel.ID.ToString(), ref outMsg); //自动匹配
                    //Bonus.Bonus1104(smodel, M);//计算团队奖（有奖金烧伤）
                    //Bonus.Bonus1102(Umodel, SupplyAmount, smodel.SupplyNo);//计算利息
                }
                result.Status = 200;
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
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("Sellconfirm" + id);//清除缓存
            }
            return Json(result);
        }
        #endregion

        #region 冻结用户转为数组
        public string[] GetUserID()
        {
            var lockUserList = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.IsLock == true).ToList();
            string lockUsers = "";
            if (lockUserList.Count() > 0)
            {
                foreach (var item in lockUserList)
                {
                    string userId = item.ID.ToString();
                    lockUsers += "," + userId;
                }
            }
            string[] userIds = lockUsers.TrimEnd(',').TrimStart(',').Split(',');
            return userIds;
        }
        #endregion

        #region 匹配单列表
        // 确认
        public ActionResult Confirmed()
        {
            var Status = Request["Status"].ToInt();
            ViewBag.transaction = (Status == 1) ? "等待付款" : (Status == 3) ? "等待确认" : "已完成";
            ViewBag.Title = ViewBag.transaction;
            return View();
        }
        public ActionResult GetConfirmed(int? page, int? Status)
        {
            var list = MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptUID == Umodel.ID || x.SupplyUID == Umodel.ID);
            if (Status != null)
            {
                    list = list.Where(x => x.Status == Status);
            }
            else
            {
                list = list.Where(x => x.Status != (int)JN.Data.Enum.MatchingStatus.UnPaid && x.Status != (int)JN.Data.Enum.MatchingStatus.Paid);
            }

            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.ReserveStr1 = "@T(" + typeof(JN.Data.Enum.MatchingStatus).GetEnumDesc(item.Status) + ")";
                string HeadFace = (item.AcceptUID == Umodel.ID ? UserService.Single(item.AcceptUID).HeadFace : UserService.Single(item.SupplyUID).HeadFace);
                item.ReserveStr2 = string.IsNullOrEmpty(HeadFace) ? "/Theme/App/images/member.png" : HeadFace;
            }
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 匹配单详情
        // 确认
        public ActionResult MatchDetails(int id)
        {
            ViewBag.Title = "匹配单详情";
            var mModel = MatchingService.Single(id);
            return View(mModel);
        }
        #endregion

        #region 留言评论
        /// <summary>
        /// 留言评论
        /// </summary>
        /// <returns></returns>
        public ActionResult SendLeavword()
        {
            string matchingno = Request["matchingno"];
            string msgcontent = Request["msgcontent"];
            if (string.IsNullOrEmpty(msgcontent))
                return Json(new { result = "error", msg = "对不起，请填写内容！" });

            var entity = new Data.LeaveWord();
            entity.CreateTime = DateTime.Now;
            entity.Content = msgcontent;
            entity.UID = Umodel.ID;
            entity.UserName = Umodel.UserName;
            entity.MatchingNo = matchingno;
            entity.MsgType = "咨询";

            MvcCore.Unity.Get<ILeaveWordService>().Add(entity);
            SysDBTool.Commit();
            if (entity.ID > 0)
                return Json(new { result = "ok", msg = "留言成功！" });
            else
                return Json(new { result = "error", msg = "留言错误！" });
        }

        #endregion

        #region 取消

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelSupplyQueuing(int id)
        {
            var sModel = SupplyHelpService.Single(id);
            if (sModel != null)
            {
                if (sModel.UID != Umodel.ID) return showmsg("非法操作");
                if (sModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        MMM.CancelSupplyHelp(sModel.SupplyNo, "自行取消");
                        ts.Complete();
                    }
                    return showmsg("成功退出队列");
                }
                else
                    return showmsg("当前提供订单状态不可退出");
            }
            else
                return showmsg("不存在的记录");
        }

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelAcceptQueuing(int id)
        {
            var aModel = AcceptHelpService.Single(id);
            if (aModel != null)
            {
                if (aModel.UID != Umodel.ID) return showmsg("非法操作");
                if (aModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        MMM.CancelAcceptHelp(aModel.AcceptNo, "自行取消");
                        ts.Complete();
                    }
                    return showmsg("成功退出队列");
                }
                else
                    return showmsg("当前接受订单状态不可退出");
            }
            else
                return showmsg("不存在的记录");
        }
        #endregion

        #region 付款截图

        /// <summary>
        /// 付款截图
        /// </summary>
        /// <returns></returns>
        public ActionResult ProofImageUrl(int id)
        {
            ViewBag.Title = "付款截图";
            var mModel = MatchingService.Single(id);
            return View(mModel);
        }
        #endregion

        #region 确认付款
        /// <summary>
        /// 确认拨款
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ConfirmPay(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {

                string vcode = (Session["SMSValidateCode"] ?? "").ToString();
                Session.Remove("SMSValidateCode");
                string code = form["smscode"];

                #region 手机验证码
                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);
                //if (MvcCore.Unity.Get<ISysParamService>().Single(c => c.ID == 4509).Value.ToInt() > 0)//开启手机验证码
                //{
                //    if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                //        throw new CustomException("验证码错误");
                //}

                #endregion

                string id = form["id"];
                string content = form["content"];
                var mModel = MatchingService.Single(id.ToInt());
                if (mModel.SupplyUID != Umodel.ID) throw new CustomException("非法操作");
                if (mModel == null) throw new CustomException("记录不存在");
                string imgurl = "";
                if (Request.Files.Count == 0) throw new CustomException("请您上传凭证！");
                HttpPostedFileBase file = Request.Files[0];
                if ((file != null) && (file.ContentLength > 0))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                        throw new CustomException("非法上传，您只可以上传图片格式的文件！");
                    //20160711安全更新 ---------------- start
                    var newfilename = "PAY_" + Umodel.UserName + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(Request.MapPath("~/Content/Resource")))
                        Directory.CreateDirectory(Request.MapPath("~/Content/Resource"));

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
                        wlog.UserName = Umodel.UserName;
                        MvcCore.Unity.Get<IWarningLogService>().Add(wlog);

                        Umodel.IsLock = true;
                        Umodel.LockTime = DateTime.Now;
                        Umodel.LockReason = "试图上传木马文件";
                        UserService.Update(Umodel);
                        SysDBTool.Commit();
                        throw new CustomException("试图上传木马文件，您的帐号已被冻结");
                    }

                    var fileName = Path.Combine(Request.MapPath("~/Content/Resource"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/Content/Resource/"), 1024, 768, "EQU");
                        imgurl = "/Content/Resource/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new CustomException("上传失败：" + ex.Message);
                    }
                    finally
                    {
                        System.IO.File.Delete(fileName); //删除原文件
                    }
                    //20160711安全更新  --------------- end
                }

                if (mModel.Status > (int)Data.Enum.MatchingStatus.Delayed) throw new CustomException("当前订单状态不可付款");
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        var entity = new Data.LeaveWord();
                        entity.CreateTime = DateTime.Now;
                        entity.Content = content;
                        entity.UID = Umodel.ID;
                        entity.UserName = Umodel.UserName;
                        entity.MatchingNo = mModel.MatchingNo;
                        entity.MsgType = "付款留言";

                        MvcCore.Unity.Get<ILeaveWordService>().Add(entity);
                        SysDBTool.Commit();
                    }
                    mModel.ProofImageUrl = imgurl;
                    mModel.Status = (int)Data.Enum.MatchingStatus.Paid;
                    mModel.PayTime = DateTime.Now;
                    mModel.VerifiedEndTime = DateTime.Now.AddMinutes(MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3208).Value.ToInt()); //订单付款后确认时限参数

                    //给提供单计算利息冻结时间
                    //MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Update(new Data.SupplyHelp(), new Dictionary<string, string>() { { "AreaType", "1" }, { "ReserveDate1", DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 1102).Value.ToInt()).ToString() } },
                    //  "SupplyNo='" + mModel.SupplyNo + "'");

                    MatchingService.Update(mModel);
                    SysDBTool.Commit();




                    //2小时内付款有奖励
                    //Bonus.Bonus1105(mModel);

                    ts.Complete();
                    result.Status = 200;
                }

                if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3523).Value == "1") //付款成功是否通知接受单会员
                {
                    var acceptUser = UserService.Single(x => x.ID == mModel.AcceptUID);
                    if (acceptUser != null)
                        SMSHelper.WebChineseMSM(acceptUser.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3523).Value2.Replace("{ORDERNUMBER}", mModel.MatchingNo).Replace("{USERNAME}", acceptUser.UserName));
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

        /// <summary>
        /// 延时付款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelayedPay(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var mModel = MatchingService.Single(id);
                if (mModel.SupplyUID != Umodel.ID) throw new CustomException("非法操作");
                if (mModel == null) throw new CustomException("记录不存在");
                if (mModel.Status != (int)Data.Enum.MatchingStatus.UnPaid) throw new CustomException("当前订单状态不可延时付款");
                mModel.Status = (int)Data.Enum.MatchingStatus.Delayed;
                mModel.PayEndTime = (mModel.PayEndTime ?? DateTime.Now).AddMinutes(MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3206).Value2.ToInt()); //付款截止时间;
                MatchingService.Update(mModel);
                SysDBTool.Commit();
                result.Status = 200;
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

        /// <summary>
        /// 拒绝付款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult RefusePay(int id)
        //{
        //    ReturnResult result = new ReturnResult();
        //    try
        //    {
        //        var mModel = MatchingService.Single(id);
        //        if (mModel.SupplyUID != Umodel.ID) throw new CustomException("非法操作");
        //        if (mModel == null) throw new CustomException("记录不存在");


        //        if ((mModel.FromUID ?? 0) == 0) //订单没转移过
        //        {
        //            var onUser = MvcCore.Unity.Get<IUserService>().Single(mModel.SupplyUID);
        //            if (onUser != null)
        //            {
        //                //订单转移到推荐人
        //                if (onUser.RefereeID > 0)
        //                {
        //                    //同时生成一个提供单才可计算利息
        //                    var model = new Data.SupplyHelp();
        //                    model.UID = onUser.RefereeID;
        //                    model.UserName = onUser.RefereeUser;
        //                    model.SupplyAmount = mModel.MatchAmount; //申请金额
        //                    model.ExchangeAmount = mModel.MatchAmount; //汇率金额
        //                    model.CreateTime = DateTime.Now;
        //                    model.Status = (int)Data.Enum.HelpStatus.AllMatching;  //状态
        //                    model.IsTop = false;  //是否置顶
        //                    model.IsRepeatQueuing = false; //是否重新排队
        //                    model.HaveMatchingAmount = mModel.MatchAmount; //已匹配数量
        //                    model.HaveAcceptAmount = 0; //
        //                    model.PayWay = "";  //付款方式
        //                    model.EndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3203).Value.ToInt());  //订单到期时间
        //                    model.SupplyNo = Data.Extensions.SupplyHelp.GetSupplyNo();  //单号
        //                    model.AccrualDay = 0; //已结算利息天数
        //                    model.SurplusAccrualDay = 0; //(天)
        //                    model.AccrualMoney = 0; //已产生的利息
        //                    model.IsAccrualEffective = false; //利息是否生效（匹配并验证付款后才生效）
        //                    model.IsAccruaCount = true; //是否还计算利息 (超过30天或有接受订单产生后不再计算利息)
        //                    model.TotalMoney = model.ExchangeAmount; //本单总额（含利息）
        //                    model.AccruaRate = cacheSysParam.SingleAndInit(x => x.ID == 1102).Value2.ToDecimal();  //基础利息
        //                    model.OrderType = 1;
        //                    model.OrderOrigin = 1; //转移单标记
        //                    model.OrderMoney = mModel.MatchAmount;
        //                    MvcCore.Unity.Get<ISupplyHelpService>().Add(model);
        //                    MvcCore.Unity.Get<ISysDBTool>().Commit();

        //                    var newMatchItem = mModel.ToModel<Data.Matching>();
        //                    newMatchItem.SupplyUID = onUser.RefereeID;
        //                    newMatchItem.SupplyUserName = onUser.RefereeUser;
        //                    newMatchItem.SupplyNo = model.SupplyNo;
        //                    newMatchItem.MatchingNo = Data.Extensions.Matching.GetOrderNumber();
        //                    newMatchItem.CreateTime = DateTime.Now;
        //                    newMatchItem.PayEndTime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3206).Value2.ToInt()); //付款截止时间
        //                    newMatchItem.Status = (int)Data.Enum.MatchingStatus.UnPaid; //未付款
        //                    newMatchItem.Remark = "来自“" + mModel.SupplyUserName + "”拒绝付款的订单转移，原单号：" + mModel.MatchingNo;
        //                    newMatchItem.FromUID = mModel.SupplyUID;
        //                    newMatchItem.FromUserName = mModel.SupplyUserName;
        //                    MvcCore.Unity.Get<IMatchingService>().Add(newMatchItem);
        //                    MvcCore.Unity.Get<ISysDBTool>().Commit();

        //                    mModel.Status = (int)JN.Data.Enum.MatchingStatus.Cancel;
        //                    mModel.CancelTime = DateTime.Now;
        //                    mModel.CanceReason = "拒绝付款,订单转移到推荐人“" + newMatchItem.SupplyUserName + "”，新单号为：" + newMatchItem.MatchingNo;
        //                    MvcCore.Unity.Get<IMatchingService>().Update(mModel);
        //                    MvcCore.Unity.Get<ISysDBTool>().Commit();
        //                }

        //                //对供单用户帐号冻结处理
        //                onUser.IsLock = true;
        //                onUser.LockTime = DateTime.Now;
        //                onUser.LockReason = "拒绝付款后触发冻结，单号：" + mModel.MatchingNo + "";
        //                MvcCore.Unity.Get<IUserService>().Update(onUser);
        //                MvcCore.Unity.Get<ISysDBTool>().Commit();
        //            }

        //        }
        //        else //进入抢单池
        //        {
        //            //对推荐人扣除100元
        //            decimal PARAM_KQBL = cacheSysParam.SingleAndInit(x => x.ID == 3206).Value3.ToDecimal();
        //            decimal kqje = Math.Min(500, mModel.MatchAmount * PARAM_KQBL);
        //            Wallets.changeWallet(mModel.SupplyUID, 0 - kqje, 2003, "拒绝付款下属会员转移的订单");

        //            var newMatchItem = MvcCore.Unity.Get<IMatchingService>().Single(mModel.ID);
        //            newMatchItem.Remark = "拒绝付款下属会员“" + newMatchItem.FromUserName + "”转移的订单“" + newMatchItem.MatchingNo + "”，扣除奖金并进入抢单池";
        //            newMatchItem.IsOpenBuying = true;
        //            MvcCore.Unity.Get<IMatchingService>().Update(newMatchItem);
        //            MvcCore.Unity.Get<ISysDBTool>().Commit();
        //        }
        //        SysDBTool.Commit();
        //        result.Status = 200;
        //    }
        //    catch (CustomException ex)
        //    {
        //        result.Message = ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Message = "网络系统繁忙，请稍候再试!";
        //        logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
        //    }
        //    return Json(result);
        //}

        #endregion

        #region 确认收款
        [HttpPost]
        public ActionResult FinshPay(int id, int type)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                int comfir = type;// fc["comfir"].ToInt();
                //int id = fc["id"].ToInt();
                var mModel = MatchingService.Single(id);
                if (mModel.AcceptUID != Umodel.ID) throw new CustomException("非法操作");
                if (mModel == null) throw new CustomException("记录不存在");
                if (mModel.Status != (int)Data.Enum.MatchingStatus.Paid) throw new CustomException("当前订单状态不可确认");
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    if (comfir == 1) //选择确认收到汇款
                    {
                        //结算提供单利息，奖金并更新成交状态

                        Bonus.Settlement(mModel);
                        //mModel.VerifiedEndTime = DateTime.Now;
                        mModel.AllDealTime = DateTime.Now;
                        mModel.Status = (int)Data.Enum.MatchingStatus.Verified;


                        //Bonus.FreezeMoney(mModel, SupplyHelpService,UserService);   //冻结本金和利息

                        //var user = SupplyHelpService.Single(x => x.SupplyNo == mModel.SupplyNo);
                        //Bonus.newBonus1104(user, mModel.MatchAmount);   //动态奖

                    }
                    else
                    {
                        //没有收到汇款
                        mModel.Status = (int)Data.Enum.MatchingStatus.Falsehood;
                    }
                    MatchingService.Update(mModel);
                    SysDBTool.Commit();
                    ts.Complete();
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
        #endregion

        #region 假的汇款
        [HttpPost]
        public ActionResult Falsehood(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                int comfir = 2;// fc["comfir"].ToInt();
                int id = fc["id"].ToInt();
                var mModel = MatchingService.Single(id);
                if (mModel.AcceptUID != Umodel.ID) throw new CustomException("非法操作");
                if (mModel == null) throw new CustomException("记录不存在");
                if (mModel.Status != (int)Data.Enum.MatchingStatus.Paid) throw new CustomException("当前订单状态不可确认");
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    if (comfir == 1) //选择确认收到汇款
                    {
                        //结算提供单利息，奖金并更新成交状态
                        Bonus.Settlement(mModel);
                        mModel.VerifiedEndTime = DateTime.Now;
                        mModel.Status = (int)Data.Enum.MatchingStatus.Verified;
                    }
                    else
                    {
                        //没有收到汇款
                        mModel.Status = (int)Data.Enum.MatchingStatus.Falsehood;
                    }
                    MatchingService.Update(mModel);
                    SysDBTool.Commit();
                    ts.Complete();
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
        #endregion

        #region 评价
        public ActionResult Evaluate(int id)
        {
            var model = MatchingService.Single(id);
            if (model != null) return View(model);
            return showmsg("记录不存在");
        }
        [HttpPost]
        public ActionResult Evaluate(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                int id = fc["id"].ToInt();
                int Grade = fc["Grade"].ToInt();//评级
                string Evaluate = "已评价";// fc["Evaluate"].ToString();//评价
                var mModel = MatchingService.Single(id);
                if (mModel == null) throw new CustomException("记录不存在");
                if (mModel.AcceptUID != Umodel.ID && mModel.SupplyUID != Umodel.ID) throw new CustomException("非法操作");
                if (mModel.Status != (int)Data.Enum.MatchingStatus.Verified) throw new CustomException("当前订单状态不可评价");
                if (Grade <= 0) throw new CustomException("请给对方打个分吧");
                //if (string.IsNullOrEmpty(Evaluate)) throw new CustomException("请输入评语");
                if (mModel.SupplyUID == Umodel.ID) //选择确认收到汇款
                {
                    mModel.AccGrade = Grade;
                    mModel.AccEvaluate = Evaluate;

                }
                else
                {
                    mModel.SupGrade = Grade;
                    mModel.SupEvaluate = Evaluate;
                }
                if ((mModel.AccGrade ?? 0) > 0 && !string.IsNullOrEmpty(mModel.AccEvaluate) && (mModel.SupGrade ?? 0) > 0 && !string.IsNullOrEmpty(mModel.SupEvaluate))
                {
                    mModel.Status = (int)Data.Enum.MatchingStatus.Evaluate;
                }
                MatchingService.Update(mModel);
                SysDBTool.Commit();
                result.Status = 200;
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
        #endregion
    } 
 



}
