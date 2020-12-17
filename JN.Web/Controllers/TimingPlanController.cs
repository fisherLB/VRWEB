using JN.Data.Service;
using JN.Services.Manager;
using JN.Services.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Controllers
{
    public class TimingPlanController : Controller
    {
        private static List<Data.SysParam> cacheSysParam = null;
        private static List<Data.Currency> cacheCurrency = null;
        private readonly IReleaseDetailService ReleaseDetailService;
        private readonly IUserService UserService;
        private readonly IMachineOrderService MachineOrderService;
        private readonly ISysSettingService SysSettingService;
        private readonly ISettlementService SettlementService;
        private readonly ISysDBTool SysDBTool;
        private readonly ITransferService TransferService;
        private readonly IWalletLogService WalletLogService;
        private readonly IReleaseService ReleaseService;
        private readonly ICurrencyService CurrencyService;
        private readonly IBonusDetailService BonusDetailService;


        public TimingPlanController(ISysDBTool SysDBTool,
            IReleaseDetailService ReleaseDetailService,
            IUserService UserService,
                 ISettlementService SettlementService,
             IMachineOrderService MachineOrderService,
            ISysSettingService SysSettingService,
            ITransferService TransferService,
            IWalletLogService WalletLogService,
            IReleaseService ReleaseService,
            ICurrencyService CurrencyService,
            IBonusDetailService BonusDetailService)
        {
            this.ReleaseDetailService = ReleaseDetailService;
            this.UserService = UserService;
            this.SysSettingService = SysSettingService;
            this.SysDBTool = SysDBTool;
            this.SettlementService = SettlementService;
            this.MachineOrderService = MachineOrderService;
            this.TransferService = TransferService;
            this.WalletLogService = WalletLogService;
            this.ReleaseService = ReleaseService;
            this.CurrencyService = CurrencyService;
            this.BonusDetailService = BonusDetailService;
            cacheSysParam = Services.Manager.CacheHelp.GetSysParamsList();
            cacheCurrency = Services.Manager.CacheHelp.GetCurrencyList();
        }
        //
        // GET: /Admin/User/
        public ActionResult Index()
        {
            bool isExec = false;
            DateTime starttime = DateTime.Now;
            string ExecProcess = Request["ExecProcess"];
            switch (ExecProcess)
            {
                case "plan1":
                    isExec = plan1();
                    break;
                case "plan2":
                    isExec = plan2();
                    break;
                case "plan3":
                    isExec = plan3();
                    break;
                case "plan4":
                    isExec = plan4();
                    break;
                    //case "plan5":
                    //    isExec = plan5();
                    //    break;
            }
            string msg = (isExec ? "成功" : "失败") + "执行作业计划“" + ExecProcess + "”，时间在" + DateTime.Now.ToString() + "，用时：" + DateTimeDiff.DateDiff(starttime, DateTime.Now, "ms") + "毫秒";
            ViewBag.msg = msg;
            logs.WindowsServiceWriteLog(msg);
            return View();
        }
        //定时计划1
        public bool plan1()
        {
            bool flag = false;
            try
            {
                Balances.ExtractProfit(cacheSysParam, cacheCurrency, 0);
                flag = true;
            }
            catch (Exception ex)
            {
                logs.WindowsServiceWriteLog(ex.Message);
            }
            return flag;
        }

        //定时计划2
        public bool plan2()
        {
            bool flag = false;
            try
            {
                Balances.Bonus1104(cacheSysParam, cacheCurrency, 0);
                flag = true;
            }
            catch (Exception ex)
            {
                logs.WindowsServiceWriteLog(ex.Message);
            }
            return flag;
        }


        //市场推广佣金
        public bool plan3()
        {
            bool flag = false;
            try
            {
                Balances.Bonus1105(cacheSysParam, cacheCurrency, 0);
                flag = true;
            }
            catch (Exception ex)
            {
                logs.WindowsServiceWriteLog(ex.Message);
            }
            return flag;
        }

        //定时计划4
        public bool plan4()
        {
            bool flag = false;
            try
            {
                Balances.Bonus1108(cacheSysParam, cacheCurrency, 0);
                flag = true;
            }
            catch (Exception ex)
            {
                logs.WindowsServiceWriteLog(ex.Message);
            }
            return flag;
        }

        //定时计划5
        public bool plan5()
        {
            bool flag = false;
            try
            {
                Balances.Bonus1109(cacheSysParam, cacheCurrency, 0);
                flag = true;
            }
            catch (Exception ex)
            {
                logs.WindowsServiceWriteLog(ex.Message);
            }
            return flag;
        }
    }
}
