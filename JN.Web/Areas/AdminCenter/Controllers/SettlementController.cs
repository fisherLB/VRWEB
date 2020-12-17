using JN.Data;
using JN.Data.Service;
using JN.Services.Manager;
using MvcCore.Controls;
using MvcCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class SettlementController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;
        private static List<Data.Currency> cacheCurrency = null;
        private readonly ISysSettingService SysSettingService;
        private readonly IUserService UserService;
        private readonly ISysParamService SysParamService;
        private readonly ISysDBTool SysDBTool;

        private readonly IMachineOrderService MachineOrderService;
        private readonly ISettlementService SettlementService;
        private readonly IAdvertiseOrderService AdvertiseOrderService;

        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly IReleaseService ReleaseService;
        private readonly IReleaseDetailService ReleaseDetailService;
        private readonly ICurrencyService CurrencyService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ITransferService TransferService;

        public SettlementController(ISysDBTool SysDBTool,

            IMachineOrderService MachineOrderService,
              ISettlementService SettlementService,
                 IAdvertiseOrderService AdvertiseOrderService,

            ISysSettingService SysSettingService,
            IUserService UserService,
            ISysParamService SysParamService,
            IActLogService ActLogService,
            IWalletLogService WalletLogService,
                IReleaseService ReleaseService,
                IReleaseDetailService ReleaseDetailService,
                ICurrencyService CurrencyService,
        IBonusDetailService BonusDetailService,
        ITransferService TransferService)
        {
            this.SysSettingService = SysSettingService;
            this.UserService = UserService;
            this.SysParamService = SysParamService;
            this.SysDBTool = SysDBTool;

            this.MachineOrderService = MachineOrderService;
            this.SettlementService = SettlementService;
            this.AdvertiseOrderService = AdvertiseOrderService;

            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
            this.ReleaseService = ReleaseService;
            this.ReleaseDetailService = ReleaseDetailService;
            this.CurrencyService = CurrencyService;
            this.BonusDetailService = BonusDetailService;
            this.TransferService = TransferService;
            cacheSysParam =Services.Manager.CacheHelp.GetSysParamsList();
            cacheCurrency = JN.Services.Manager.CacheHelp.GetCurrencyList();
        }

        public ActionResult ReleaseList(int? page)
        {
            ViewBag.Title = "释放历史";
            ActMessage = ViewBag.Title;
            var list = ReleaseService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult ReleaseDetail(int? page)
        {
            ViewBag.Title = "释放详情";
            ActMessage = ViewBag.Title;
            //int period = Request["period"].ToInt();
            //IQueryable<Data.ReleaseDetail> list = ReleaseDetailService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            //if (period > 0)
            //{
            //    list.Where(x => x.Period == period);
            //}
            var list = ReleaseDetailService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult DayRelease()
        {
            ViewBag.Title = "静态收益释放";
            ActMessage = ViewBag.Title;

            var model = ReleaseService.List().OrderByDescending(x => x.ID).FirstOrDefault();
            if (model != null)
                return View(model);
            else
                return View();
        }

        public ActionResult doRelease()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache(Amodel.ID + "doRelease"))//检测缓存是否存在，区分大小写
                    throw new Exception("请不要重复提交");
                MvcCore.Extensions.CacheExtensions.SetCache(Amodel.ID + "doRelease", Amodel.ID + "doRelease", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                //线程里无法使用session
                DataCache.SetCache("StartTime", DateTime.Now);
                DataCache.SetCache("TotalRow", 0);
                DataCache.SetCache("CurrentRow", 0);
                DataCache.SetCache("TransInfo", "开始进行结算");
                //Thread thread = new Thread(new ParameterizedThreadStart(delegate { threadproc((fhzl ?? 0)); }));
                //thread.Start();
                //Thread thread = new Thread(new ThreadStart(threadproc));
                //thread.Start();
                //using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                //{
                //    //Balances.SubMineCost(UserService);
                //    //Balances.AdvanceMills();
                //    //Balances.ExtractProfit(cacheSysParam,cacheCurrency);
                //    //Balances.Bonus1102(UserService);
                //    //Balances.Bonus1103(UserService);
                //    //Balances.UpMachine(MachineOrderService, SysDBTool);
                //    ts.Complete();
                //}
                Balances.ExtractProfit(cacheSysParam, cacheCurrency,1);
                result.Status = 200;
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(Request.Url.ToString(), ex);

                result.Message = ex.Message;
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache(Amodel.ID + "doRelease");
            }
            return Json(result);
        }

        private void threadproc()
        {
            try
            {
                //Balances.CalculateRelease(ReleaseService, ReleaseDetailService, UserService, WalletLogService, CurrencyService, SysDBTool, cacheSysParam, 1);//每日释放
                //Balances.SubMineCost(UserService);
                //Balances.AdvanceMills();
                //Balances.ExtractProfit(cacheSysParam);
                //Balances.Bonus1102(UserService);
                //Balances.Bonus1103(UserService);
                //Balances.UpMachine(MachineOrderService, SysDBTool);

            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
        }

        #region 补发释放
        public ActionResult BFRelease(string time)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(time)) throw new Exception("请输入补发日期");
                DateTime tiembf = DateTime.Parse(time);
                //线程里无法使用session
                DataCache.SetCache("StartTime", DateTime.Now);
                DataCache.SetCache("TotalRow", 0);
                DataCache.SetCache("CurrentRow", 0);
                DataCache.SetCache("TransInfo", "开始进行结算");
                //Thread thread = new Thread(new ParameterizedThreadStart(delegate { threadproc((fhzl ?? 0)); }));
                //thread.Start();
                Balances.ReissueDynamicRelease(tiembf);
                return Json(new { result = "ok", msg = "发放成功！" });
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(Request.Url.ToString(), ex);
                return Json(new { result = "err", msg = ex.Message });
            }
        }
        #endregion

        #region 同步旧兑换金额
        public ActionResult TB()
        {
            try
            {
                Balances.Synchronization();
                return Json(new { result = "ok", msg = "发放成功！" });
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(Request.Url.ToString(), ex);
                return Json(new { result = "err", msg = ex.Message });
            }
        }
        #endregion


        public ActionResult VIPDayBonus()
        {
            ViewBag.Title = "每日流通";
            ActMessage = ViewBag.Title;
            return View();
        }

        public ActionResult doVIPBonus()
        {
            try
            {
                //线程里无法使用session
                DataCache.SetCache("StartTime2", DateTime.Now);
                DataCache.SetCache("TotalRow2", 0);
                DataCache.SetCache("CurrentRow2", 0);
                DataCache.SetCache("TransInfo2", "开始进行结算");
                //Thread thread = new Thread(new ParameterizedThreadStart(delegate { threadproc((fhzl ?? 0)); }));
                //thread.Start();
                Thread thread = new Thread(new ThreadStart(threadproc2));
                thread.Start();
                return Json(new { result = "ok", msg = "发放成功！" });
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(Request.Url.ToString(), ex);
                return Json(new { result = "err", msg = ex.Message });
            }
        }

        private void threadproc2()
        {
            try
            {
                Balances.VIPDayBonus(BonusDetailService, TransferService, UserService, WalletLogService, CurrencyService, SysDBTool, SysSettingService, cacheSysParam, 1);//每日VIP奖金
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
        }


        public JsonResult getproc()
        {
            var proc = new
            {
                StartTime = DataCache.GetCache("StartTime"),
                CurrentRow = DataCache.GetCache("CurrentRow"),
                TotalRow = DataCache.GetCache("TotalRow"),
                TransInfo = DataCache.GetCache("TransInfo")
            };
            return Json(new { result = "ok", data = proc }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getproc2()
        {
            var proc = new
            {
                StartTime = DataCache.GetCache("StartTime2"),
                CurrentRow = DataCache.GetCache("CurrentRow2"),
                TotalRow = DataCache.GetCache("TotalRow2"),
                TransInfo = DataCache.GetCache("TransInfo2")
            };
            return Json(new { result = "ok", data = proc }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MiningBonus()
        {
            ViewBag.Title = "每月结算";
            ActMessage = ViewBag.Title;

            //var model = SettlementService.List(x => x.BalanceType == 0).OrderByDescending(x => x.ID).FirstOrDefault();
            //if (model != null)
            //    return View(model);
            //else
            return View();
        }


        public ActionResult FH()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache(Amodel.ID + "FH"))//检测缓存是否存在，区分大小写
                    throw new Exception("请不要重复提交");
                MvcCore.Extensions.CacheExtensions.SetCache(Amodel.ID + "FH", Amodel.ID + "FH", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                Balances.Bonus1104(cacheSysParam, cacheCurrency,1);
                result.Status = 200;
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);

                result.Message = ex.Message;
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache(Amodel.ID + "FH");
            }
            return Json(result);
        }

        public ActionResult FHFJ()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache(Amodel.ID + "FHFJ"))//检测缓存是否存在，区分大小写
                    throw new Exception("请不要重复提交");
                MvcCore.Extensions.CacheExtensions.SetCache(Amodel.ID + "FHFJ", Amodel.ID + "FHFJ", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                Balances.Bonus1105(cacheSysParam, cacheCurrency,1);
                result.Status = 200;
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);

                result.Message = ex.Message;
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache(Amodel.ID + "FHFJ");
            }
            return Json(result);
        }


        public ActionResult FHjd()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache(Amodel.ID + "FHjd"))//检测缓存是否存在，区分大小写
                    throw new Exception("请不要重复提交");
                MvcCore.Extensions.CacheExtensions.SetCache(Amodel.ID + "FHjd", Amodel.ID + "FHjd", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                Balances.Bonus1108(cacheSysParam, cacheCurrency,1);
                result.Status = 200;
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);

                result.Message = ex.Message;
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache(Amodel.ID + "FHjd");
            }
            return Json(result);
        }

        //public ActionResult UpTradingVolume()
        //{
        //    try
        //    {
        //        using (var ts = new System.Transactions.TransactionScope())
        //        {
        //            Balances.Update_TradingVolume(UserService, SysDBTool, SettlementService, cacheSysParam, 1);
        //            ts.Complete();

        //            //Balances.CalculateRelease(ReleaseService, UserService, BonusDetailService, WalletLogService, SysDBTool, cacheSysParam, 1);
        //            //Bonus.MiningBonus(UserService, WalletLogService, BonusDetailService, SettlementService, SysDBTool, 1);
        //            return Content("ok");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
        //        return Content(ex.Message);
        //    }
        //}


        public ActionResult FHShareholder()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache(Amodel.ID + "FHShareholder"))//检测缓存是否存在，区分大小写
                    throw new Exception("请不要重复提交");
                MvcCore.Extensions.CacheExtensions.SetCache(Amodel.ID + "FHShareholder", Amodel.ID + "FHShareholder", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                Balances.Bonus1109(cacheSysParam, cacheCurrency,1);
                result.Status = 200;
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);

                result.Message = ex.Message;
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache(Amodel.ID + "FHShareholder");
            }
            return Json(result);
        }

    }
}
