using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using JN.Services.Manager;
using JN.Services.CustomException;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class TimeDepositController : BaseController
    {
        private readonly ITimeDepositService TimeDepositService;
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;


        public TimeDepositController(ISysDBTool SysDBTool,
         ITimeDepositService TimeDepositService,
            IUserService UserService,
            IActLogService ActLogService)
        {
            this.TimeDepositService = TimeDepositService;
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
        }


        public ActionResult Index(int? page)
        {
            ActMessage = "定期存款记录";
            var list = TimeDepositService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }
    }
}
