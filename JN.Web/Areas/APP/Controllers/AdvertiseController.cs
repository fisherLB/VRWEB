using JN.Data.Extensions;
using JN.Data.Service;
using JN.Services.Manager;
using JN.Services.Tool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace JN.Web.Areas.APP.Controllers
{
    /// <summary>
    /// 点对点交易
    /// </summary>
    public class AdvertiseController : BaseController
    {
        private static Data.SysSetting syssetting = null;
        private static List<Data.SysParam> cacheSysParam = null;
        private readonly IUserService UserService;
        private readonly ICurrencyService CurrencyService;
        private readonly IAdvertiseService AdvertiseService;
        private readonly IChatingService ChatingService;
        private readonly IMessageService MessageService;
        private readonly IAdvertiseOrderService AdvertiseOrderService;
        private readonly ISysDBTool SysDBTool;

        public AdvertiseController(ISysDBTool SysDBTool,
            ICurrencyService CurrencyService,
            IUserService UserService,
            IAdvertiseService AdvertiseService,
            IMessageService MessageService,
            IChatingService ChatingService,
            IAdvertiseOrderService AdvertiseOrderService,
            ISysParamService SysParamService)
        {
            this.UserService = UserService;
            this.CurrencyService = CurrencyService;
            this.AdvertiseService = AdvertiseService;
            this.ChatingService = ChatingService;
            this.AdvertiseOrderService = AdvertiseOrderService;
            this.MessageService = MessageService;
            this.SysDBTool = SysDBTool;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
            syssetting = MvcCore.Unity.Get<ISysSettingService>().ListCache("sysSet").FirstOrDefault();
        }

        #region 数字资产+C2C交易首页
        /// <summary>
        /// 数字资产
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ActMessage = "数字资产";
            ViewBag.Title = ActMessage;
            return View();
        }

        /// <summary>
        /// C2C交易
        /// </summary>
        /// <returns></returns>
        public ActionResult Trade()
        {
            int fromcoid = Request["fromcoid"].ToInt();
            int tocoid = Request["tocoid"].ToInt();

            var cmodelList = CurrencyService.List(x => x.LineSwitch && x.TranSwitch).ToList();

            //var fromModel = fromcoid == 0 ? cmodelList.Where(x => (x.IsCashCurrency ?? false) == true).OrderBy(x => x.Sort).FirstOrDefault() : CurrencyService.SingleAndInit(x => x.ID == fromcoid);
            string fromName = fromcoid == 0 ? "现金" : CurrencyService.SingleAndInit(x => x.ID == fromcoid).CurrencyName;
            var toModel = tocoid == 0 ? cmodelList.Where(x => (x.IsCashCurrency ?? false) == false).OrderBy(x => x.Sort).FirstOrDefault() : CurrencyService.SingleAndInit(x => x.ID == tocoid);
            ActMessage = toModel.CurrencyName + fromName + "交易";
            ViewBag.Title = ActMessage;
            ViewBag.FromCoid = fromcoid;
            //ViewBag.CashCurrency = fromModel;            
            return View(toModel);
        }
        /// <summary>
        /// C2C交易首页获取广告列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult GetAdvertiseList(int? page)
        {
            int pageSize = 10;

            var cmodelList = CurrencyService.List(x => x.LineSwitch && x.TranSwitch).ToList();

            //int curid = (!string.IsNullOrEmpty(Request["curid"])) ? Request["curid"].ToInt() : cmodelList.Where(x => x.IsCashCurrency ?? false).OrderBy(x => x.Sort).FirstOrDefault().ID;
            //int coinid = (!string.IsNullOrEmpty(Request["coinid"])) ? Request["coinid"].ToInt() : cmodelList.Where(x => x.IsCashCurrency ?? true).OrderBy(x => x.Sort).FirstOrDefault().ID;

            int type = string.IsNullOrEmpty(Request["type"]) ? 1 : Request["type"].ToInt();

            var fenzhong = cacheSysParam.Single(x => x.ID == 2107).Value.ToInt();
            var endTime = DateTime.Now.AddMinutes(-fenzhong);

            //var list = AdvertiseService.ListInclude(x => x.OutTable_User).Where(x => x.Direction == type && x.Status == (int)JN.Data.Enum.AdvertiseStatus.Underway && x.OutTable_User.IsLock == false && x.CurID == curid && x.CoinID == coinid && x.UID != Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderBy(x => x.Price);
            var list = AdvertiseService.ListInclude(x => x.OutTable_User).Where(x => x.Direction == type && (x.Status == (int)JN.Data.Enum.AdvertiseStatus.Underway||(x.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed&& x.CreateTime > endTime)) && x.OutTable_User.IsLock == false /*&& x.CurID == curid && x.CoinID == coinid*/ && x.UID != Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderBy(x => x.Price).ToList();


            //var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            var listdata = list.OrderBy(x => x.Status).ThenBy(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                var num = AdvertiseOrderService.List(x => x.AdOderNO == item.OrderID).ToList();
                item.LocationID = num.Count();
                var weix = MvcCore.Unity.Get<IUserBankCardService>().List(x => x.UID == item.UID && x.BankNameID == 5001).Count();
                if (weix > 0)
                    item.CoinID = 5005;
                var zhifubao = MvcCore.Unity.Get<IUserBankCardService>().List(x => x.UID == item.UID && x.BankNameID == 5002).Count();
                if (zhifubao > 0)
                    item.CoinName = "1";
                var yhk = MvcCore.Unity.Get<IUserBankCardService>().List(x => x.UID == item.UID && x.BankNameID != 5001 && x.BankNameID != 5002).Count();
                if (yhk > 0)
                    item.LocationName = "1";

                if (item.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed)
                {
                    item.IsEnableSecurity = true;
                }
            }

            //foreach (var item in list)
            //{
            //    var num = AdvertiseOrderService.List(x => x.AdOderNO == item.OrderID).ToList();
            //    item.LocationID = num.Count();
            //    var weix = MvcCore.Unity.Get<IUserBankCardService>().List(x => x.UID == Umodel.ID && x.BankNameID == 5005).Count();
            //    if (weix > 0)
            //        item.CoinID = 5005;
            //    var zhifubao = MvcCore.Unity.Get<IUserBankCardService>().List(x => x.UID == Umodel.ID && x.BankNameID == 5006).Count();
            //    if (zhifubao > 0)
            //        item.CoinName = "1";
            //    var yhk = MvcCore.Unity.Get<IUserBankCardService>().List(x => x.UID == Umodel.ID && x.BankNameID != 5005 && x.BankNameID != 5006).Count();
            //    if (yhk > 0)
            //        item.LocationName = "1";
            //}

            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
                                                                                      //return Json(new { data = listdata.ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 我的广告列表
        /// <summary>
        /// 我的广告列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Order(int? type, int? status, int page = 1)
        {
            ActMessage = "出售广告列表";
            int direction = Request["direction"].ToInt();
            if (direction == 1)
            {
                ActMessage = "购买广告列表";
            }
            ViewBag.Title = ActMessage;
            return View();
        }
        /// <summary>
        /// 我的广告列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getOrderList(int? curID, int? status, int? page)
        {
            int pageSize = 10;
            int direction = Request["direction"].ToInt();
            int fromcoid = 1;
            var list = AdvertiseService.List(x => x.UID == Umodel.ID && x.Direction == direction/* && x.CoinID == fromcoid*/);
            if (curID.HasValue)
            {
                list = list.Where(x => x.CurID == curID);
            }
            //if (status.HasValue)
            //{
            //    list = list.Where(x => x.Status == status);
            //}

            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 我的广告列表详情
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult OrderDetail(int id)
        {
            ActMessage = "广告列表详情";
            ViewBag.Title = ActMessage;
            var model = AdvertiseService.SingleAndInit(x => x.ID == id && x.UID == Umodel.ID);
            return View(model);
        }
        #endregion

        #region 订单列表
        /// <summary>
        /// 订单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult AdvertiseOrder()
        {
            ActMessage = "订单列表";
            ViewBag.Title = ActMessage;
            return View();
        }

        /// <summary>
        /// 获取广告列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult GetAdvertiseOrderList(int? page)
        {
            int pageSize = 10;
            int fromcoid = 1;
            int status = (Request["status"] == null || Request["status"].ToInt() == 0) ? 1 : Request["status"].ToInt();
            IQueryable<JN.Data.AdvertiseOrder> list = AdvertiseOrderService.List(x => (x.BuyUID == Umodel.ID || x.SellUID == Umodel.ID) && x.CoinID == fromcoid);
            if (status == 1)
            {
                list = list.Where(x => x.Status >= 0 && x.Status < (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived);
            }
            else if (status < 0)
            {
                list = list.Where(x => x.Status < 0);
            }
            else if (status == 5)
            {
                list = list.Where(x => x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived);
            }
            int uid = 0;
            foreach (var item in list)
            {
                uid = item.BuyUID == Umodel.ID ? item.SellUID : item.BuyUID;
                item.Remark = UserService.SingleAndInit(x => x.ID == uid).HeadFace;
            }

            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <returns></returns>
        public ActionResult AdvertiseOrderDetail(int? id)
        {
            ActMessage = "订单详情";
            ViewBag.Title = ActMessage;
            if (id == null)
            {
                return Redirect("/app/home/index");
            }
            var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
            if (AdvertiseOrder == null) return Redirect("/app/home/index");

            return View(AdvertiseOrder);
        }
        #endregion      

        #region 发布广告
        /// <summary>
        /// 发布购买
        /// </summary>
        /// <returns></returns>
        public ActionResult Buy()
        {
            int fromcoid = 1;//Request["fromcoid"].ToInt();
            int tocoid = Request["tocoid"].ToInt();

            var cmodelList = CurrencyService.List().ToList();

            string fromName = fromcoid == 0 ? "现金" : CurrencyService.SingleAndInit(x => x.ID == fromcoid).CurrencyName;
            //var fromModel = fromcoid == 0 ? cmodelList.Where(x => (x.IsCashCurrency ?? false) == true).OrderBy(x => x.Sort).FirstOrDefault() : CurrencyService.SingleAndInit(x => x.ID == fromcoid);
            var toModel = tocoid == 0 ? cmodelList.Where(x => (x.IsCashCurrency ?? false) == false).OrderBy(x => x.Sort).FirstOrDefault() : CurrencyService.SingleAndInit(x => x.ID == tocoid);

            ViewBag.Title = "发布广告";
            ActMessage = ViewBag.Title;
            ViewBag.FromCoid = fromcoid;
            return View(toModel);
        }
        /// <summary>
        /// 发布出售
        /// </summary>
        /// <returns></returns>
        public ActionResult Sell()
        {
            int fromcoid = Request["fromcoid"].ToInt();
            int tocoid = Request["tocoid"].ToInt();

            var cmodelList = CurrencyService.List(x => x.LineSwitch && x.TranSwitch).ToList();

            string fromName = fromcoid == 0 ? "现金" : CurrencyService.SingleAndInit(x => x.ID == fromcoid).CurrencyName;
            //var fromModel = fromcoid == 0 ? cmodelList.Where(x => (x.IsCashCurrency ?? false) == true).OrderBy(x => x.Sort).FirstOrDefault() : CurrencyService.SingleAndInit(x => x.ID == fromcoid);
            var toModel = tocoid == 0 ? cmodelList.Where(x => (x.IsCashCurrency ?? false) == false).OrderBy(x => x.Sort).FirstOrDefault() : CurrencyService.SingleAndInit(x => x.ID == tocoid);

            ViewBag.Title = "发布广告";
            ActMessage = ViewBag.Title;
            //ViewBag.CashCurrency = fromModel;
            ViewBag.FromCoid = fromcoid;
            return View(toModel);
        }

        /// <summary>
        /// 发布广告提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult NewAdvertise(FormCollection fc)
        {
            var entity = new Data.Advertise();
            TryUpdateModel(entity, fc.AllKeys);//获取全部name的值

            Result result = new Result();
            result = Advertises.DoNewAdvertise(fc, entity, Umodel, AdvertiseService, CurrencyService, SysDBTool);
            return Json(result);
        }
        #endregion

        #region 付款、收货、取消、下架、投诉等操作

        #region 确认付款
        public ActionResult Confirmed(int? id)
        {
            if (id == null)
            {
                return Redirect("/app/home/index");
            }
            var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
            if (AdvertiseOrder == null) return Redirect("/app/home/index");

            return View(AdvertiseOrder);
        }

        /// <summary>
        /// 确认付款
        /// </summary>
        /// <returns></returns>
        public ActionResult Confirmpayment(int id, string payimg)
        {
            //HttpPostedFileBase file = Request.Files["imgurl"]; //接收上传文件
            Result result = new Result();
            result = Advertises.DoConfirmpayment(id, payimg, Umodel, AdvertiseOrderService, UserService, SysDBTool);
            return Json(result);
        }

        /// <summary>
        /// 付款截图
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowPayImage(int id)
        {
            ViewBag.Title = "付款截图";
            var mModel = AdvertiseOrderService.Single(id);
            return View(mModel);
        }
        #region 付款凭证图片上传
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public ActionResult UpMainPic()
        {
            //string fileName = Request["fileName"]; 
            System.Collections.Hashtable hash = UpPic1();
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public Hashtable UpPic1()
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                string oldLogo = "/Upload/PayImg/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 10240, System.Web.HttpContext.Current.Server.MapPath(oldLogo), "Pay_" + Umodel.UserName + "_");
                hash["error"] = 0;
                hash["url"] = oldLogo + img;
                return hash;
            }
            catch (Exception ex)
            {
                hash["error"] = 1;
                hash["message"] = ex.Message;
                return hash;
            }
        }
        #endregion
        #endregion

        #region 确认收货
        /// <summary>
        /// 确认收货
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmReceipt(int id)
        {
            Result result = new Result();
            result = Advertises.DoConfirmReceipt(id, Umodel, AdvertiseOrderService, CurrencyService, UserService, SysDBTool);
            return Json(result);
        }
        #endregion

        #region 取消订单
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmCancel(int id)
        {
            Result result = new Result();
            result = Advertises.DoConfirmCancel(id, Umodel, AdvertiseOrderService, CurrencyService, AdvertiseService, SysDBTool);
            return Json(result);
        }
        #endregion

        #region 下架广告
        /// <summary>
        /// 下架广告
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelAdvertise()
        {
            Result result = new Result();
            result = Advertises.DoCancelAdvertise(Umodel, AdvertiseService, AdvertiseOrderService, CurrencyService, SysDBTool);
            return Json(result);
        }
        #endregion

        #region 投诉提交
        /// <summary>
        /// 投诉
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Write(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["imgurl"];//获取上传文件
            Result result = new Result();
            result = Advertises.DoComplaintWrite(form, file, Umodel, MessageService, UserService, SysDBTool);
            return Json(result);
        }
        #endregion

        #region 投诉上传
        //投诉上传
        public ActionResult complaintUpMainPic()
        {
            System.Collections.Hashtable hash = complaintUpPic1();
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 投诉上传
        /// </summary>
        /// <returns></returns>
        public Hashtable complaintUpPic1()
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                string oldLogo = "/Upload/Complaint/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 1024, System.Web.HttpContext.Current.Server.MapPath(oldLogo), Umodel.UserName + "_");
                hash["error"] = 0;
                hash["url"] = oldLogo + img;
                return hash;
            }
            catch (Exception ex)
            {
                hash["error"] = 1;
                hash["message"] = ex.Message;
                logs.WriteErrorLog("投诉图片上传失败：", ex);
                return hash;
            }
        }
        #endregion
        #endregion       

        #region 购买/出售币种提交    
        /// <summary>
        /// 购买提交
        /// </summary>
        /// <returns>返回Json</returns>
        [HttpPost]
        public ActionResult BuySubmit(FormCollection fc)
        {
            Result result = new Result();
            result = Advertises.DoBuy(fc, Umodel, AdvertiseService, AdvertiseOrderService, CurrencyService, UserService, SysDBTool);
            return Json(result);
        }

        /// <summary>
        /// 出售提交
        /// </summary>
        /// <returns>返回Json</returns>
        [HttpPost]
        public ActionResult SellSubmit(FormCollection fc)
        {
            Result result = new Result();
            result = Advertises.DoSell(fc, Umodel, AdvertiseService, AdvertiseOrderService, CurrencyService, UserService, SysDBTool);
            return Json(result);
        }
        #endregion

        #region 会员信息
        /// <summary>
        /// 会员信息
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult UserInfo()
        {
            int uid = Request["uid"].ToInt();
            var onUser = UserService.Single(x => x.ID == uid);
            if (onUser == null) return Redirect("/app/home/index");
            ViewBag.Title = "会员信息";
            return View(onUser);
        }

        #endregion

        #region 获取K线图
        public ActionResult AppKline()
        {

            ViewBag.Title = "App版K线图";
            int curid = Request["curid"].ToInt();
            var cmodelList = CurrencyService.List(x => x.LineSwitch && x.TranSwitch).ToList();
            var model = curid == 0 ? cmodelList.Where(x => (x.IsCashCurrency ?? false) == false).OrderBy(x => x.Sort).FirstOrDefault() : CurrencyService.SingleAndInit(x => x.ID == curid);

            return View(model);
        }
        /// <summary>
        /// 获取K线图
        /// </summary>
        /// <param name="curid">币种id</param>
        /// <param name="m">分钟</param>
        /// <param name="count">条数，默认300</param>
        /// <returns></returns>
        public ActionResult GetKLine(int curid, int m = 1, int count = 300)
        {
            ReturnResult<List<APIKlineModel>> result = new ReturnResult<List<APIKlineModel>>();
            result.Data = new List<APIKlineModel>();
            result.Status = ReturnResultStatus.Succeed.GetShortValue();
            result.Message = "数据获取成功";
            List<List<double?>> list = new List<List<double?>>();
            //获取产品模型
            var cur = JN.Services.Manager.CacheTransactionDataHelper.GetCurrencyModel(curid);
            if (cur != null)
            {
                if (m == 5)
                {
                    foreach (var item in MvcCore.Unity.Get<JN.Data.Service.IPriceTracking5MinService>().List(x => x.ProductID == cur.ID).OrderByDescending(d => d.CreateTime).Take(count).ToList().OrderBy(d => d.CreateTime).ToList())
                    {
                        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                        long timeStamp = (long)((item.Time ?? DateTime.Now) - startTime).TotalMilliseconds; // 相差毫秒
                        result.Data.Add(new APIKlineModel()
                        {
                            Close = item.ClosePrice.ToDecimal(),
                            Hight = item.MaxPrice.ToDecimal(),
                            Lowest = item.MinPrice.ToDecimal(),
                            Open = item.OpenPrice.ToDecimal(),
                            Volumns = item.Volume.ToDecimal(),
                            Time = (item.Time ?? DateTime.Now).ToString("MM-dd HH:mm"),//timeStamp//(item.Time ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")
                            XTime = timeStamp
                        });
                    }
                }
                else if (m == 300)
                {
                    foreach (var item in MvcCore.Unity.Get<JN.Data.Service.IPriceTracking300MinService>().List(x => x.ProductID == cur.ID).OrderByDescending(d => d.CreateTime).Take(count).ToList().OrderBy(d => d.CreateTime).ToList())
                    {
                        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                        long timeStamp = (long)((item.Time ?? DateTime.Now) - startTime).TotalMilliseconds; // 相差毫秒
                        result.Data.Add(new APIKlineModel()
                        {
                            Close = item.ClosePrice.ToDecimal(),
                            Hight = item.MaxPrice.ToDecimal(),
                            Lowest = item.MinPrice.ToDecimal(),
                            Open = item.OpenPrice.ToDecimal(),
                            Volumns = item.Volume.ToDecimal(),
                            Time = (item.Time ?? DateTime.Now).ToString("MM-dd HH:mm"),//(item.Time ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")
                            XTime = timeStamp
                        });
                    }
                }
                else if (m == 1440)
                {
                    foreach (var item in MvcCore.Unity.Get<JN.Data.Service.IPriceTracking1DayService>().List(x => x.ProductID == cur.ID).OrderByDescending(d => d.Time).Take(count).ToList().OrderBy(d => d.Time).ToList())
                    {
                        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                        long timeStamp = (long)((item.Time ?? DateTime.Now) - startTime).TotalMilliseconds; // 相差毫秒
                        result.Data.Add(new APIKlineModel()
                        {
                            Close = item.ClosePrice.ToDecimal(),
                            Hight = item.MaxPrice.ToDecimal(),
                            Lowest = item.MinPrice.ToDecimal(),
                            Open = item.OpenPrice.ToDecimal(),
                            Volumns = item.Volume.ToDecimal(),
                            Time = timeStamp.ToString(),//(item.Time ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")
                            XTime = timeStamp
                        });
                    }
                }
            }
            return this.JsonResult(result);
        }
        #endregion


        /// <summary>
        /// 获取图表数据
        /// </summary>
        /// <returns></returns>
        public JsonResult getLineData()
        {
            int coinid = Request["coinid"].ToInt();
            var list = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().List(x => x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived && x.CoinID == coinid).Select(x => new { x.CreateTime, x.Price }).Take(10).ToList();

            List<decimal> kdata = new List<decimal>();
            List<string> kdate = new List<string>();

            foreach (var item in list)
            {
                kdata.Add(item.Price);
                kdate.Add(item.CreateTime.ToShortDateString());
            }

            return Json(new { kdata = kdata, kdate = kdate }, JsonRequestBehavior.AllowGet);
        }
    }
}
