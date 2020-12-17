using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using System.Data.Entity.Validation;
using JN.Services.CustomException;
using JN.Services.Manager;
using System.Data.Common;
using System.Transactions;
using System.Data.Entity.SqlServer;
using JN.Data.Extensions;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class AdvertiseController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;
        private readonly IUserService UserService;
        private readonly IAdvertiseOrderService AdvertiseOrderService;
        private readonly IAdvertiseService AdvertiseService;
        private readonly ICurrencyService CurrencyService;
        private readonly ISysDBTool SysDBTool;

        public AdvertiseController(ISysDBTool SysDBTool,
            IUserService UserService,
            IAdvertiseOrderService AdvertiseOrderService,
            IAdvertiseService AdvertiseService,
            ICurrencyService CurrencyService)
        {
            this.UserService = UserService;
            this.AdvertiseOrderService = AdvertiseOrderService;
            this.AdvertiseService = AdvertiseService;
            this.CurrencyService = CurrencyService;
            this.SysDBTool = SysDBTool;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
        }

        #region 交易订单管理
        /// <summary>
        ///交易订单管理
        /// </summary>
        /// <param name="page">当前页</param>
        /// <returns></returns>
        public ActionResult OrderList(int? page)
        {
            ActMessage = "交易订单管理";
            ViewBag.Title = ActMessage;
            var list = AdvertiseOrderService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            int Status = Request["Status"].ToInt();
            if (Status != 0)
            {
                list = list.Where(x => x.Status == Status);
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 在线购买广告管理
        /// <summary>
        /// 在线购买广告管理
        /// </summary>
        /// <param name="page">当前页</param>
        /// <returns></returns>

        public ActionResult AdvertiseBuyList(int? page)
        {
            ActMessage = "购买广告管理";
            ViewBag.Title = ActMessage;

            var list = AdvertiseService.List(x => x.Direction == 1).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            int Status = Request["Status"].ToInt();
            if (Status != 0)
            {
                list = list.Where(x => x.Status == Status);
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 在线出售广告管理
        /// <summary>
        /// 在线出售广告管理
        /// </summary>
        /// <param name="page">当前页</param>
        /// <returns></returns>
        public ActionResult AdvertiseSellList(int? page)
        {
            ActMessage = "出售广告管理";
            ViewBag.Title = ActMessage;

            var list = AdvertiseService.List(x => x.Direction == 0).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            int Status = Request["Status"].ToInt();
            if (Status != 0)
            {
                list = list.Where(x => x.Status == Status);
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
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
            result = Advertises.DoCancelAdvertise(new User(), AdvertiseService, AdvertiseOrderService, CurrencyService, SysDBTool, true);
            return Json(result);
        }
        #endregion

        #region 确认付款
        /// <summary>
        /// 确认付款
        /// </summary>
        /// <returns></returns>
        public ActionResult Confirmpayment(int id)
        {
            ActMessage = "确认付款";
            ViewBag.Title = ActMessage;
            Result result = new Result();

            var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x => x.ID == AdvertiseOrder.BuyUID);

            result = Advertises.DoConfirmpayment(id, "", onUser, AdvertiseOrderService, UserService, SysDBTool, true);
            return Json(result);
        }
        #endregion

        #region 确认收货
        /// <summary>
        /// 确认收货
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmReceipt(int id)
        {
            ActMessage = "确认收货";
            ViewBag.Title = ActMessage;

            Result result = new Result();
            result = Advertises.DoConfirmReceipt(id, new User(), AdvertiseOrderService, CurrencyService, UserService, SysDBTool, true);
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
            ActMessage = "取消订单";
            ViewBag.Title = ActMessage;

            Result result = new Result();
            result = Advertises.DoConfirmCancel(id, new User(), AdvertiseOrderService, CurrencyService, AdvertiseService, SysDBTool, true);
            return Json(result);
        }
        #endregion

    }
}
