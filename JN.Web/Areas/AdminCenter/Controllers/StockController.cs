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
using JN.Data.Extensions;
using System.Data.Entity.SqlServer;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class StockController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISysParamService SysParamService;
        private readonly ICurrencyService CurrencyService;
        private readonly IAdvertiseOrderService AdvertiseOrderService;
        private readonly IAdvertiseService AdvertiseService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;

        public StockController(ISysDBTool SysDBTool,
            IUserService UserService,
            ICurrencyService CurrencyService,
            IAdvertiseOrderService AdvertiseOrderService,
            IAdvertiseService AdvertiseService,
            ISysParamService SysParamService,
            IActLogService ActLogService, 
            IWalletLogService WalletLogService)
        {
            this.UserService = UserService;
            this.CurrencyService = CurrencyService;
            this.AdvertiseOrderService = AdvertiseOrderService;
            this.AdvertiseService = AdvertiseService;
            this.SysParamService = SysParamService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
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
            ActMessage = "在线购买广告管理";
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
            ActMessage = "在线出售广告管理";
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
            ReturnResult result = new ReturnResult();
            try
            {
                int adid = Request["adid"].ToInt();
                var ad = AdvertiseService.Single(x => x.ID == adid);
                if (ad == null) throw new CustomException("传入ID错误"); 

                if (ad.Status == (int)JN.Data.Enum.AdvertiseStatus.Underway )
                {
                    //查看此订单有没有子单正在进行的
                    var adorder = AdvertiseOrderService.Single(x => x.AdID == ad.ID && x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder && x.Status < (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived);
                    if (adorder == null)
                    {
                        if (ad.Direction == 0)//如果是出售单，返还币种给用户
                        {
                            var c = CurrencyService.Single(x => x.ID == ad.CurID);
                            //decimal quantity = ad.HaveQuantity.ToDecimal() - (((ad.Quantity ?? 0) - (ad.HaveQuantity ?? 0)) * c.SellPoundage);
                            //Wallets.changeWallet(ad.UID, ad.HaveQuantity.ToDecimal(), (int)c.WalletCurID, "来自出售广告单[" + ad.OrderID + "]的下架退还", c);
                            decimal quantity = ad.Quantity.ToDecimal() - ((ad.Quantity - ad.HaveQuantity) *(1+ c.SellPoundage));
                            Wallets.changeWallet(ad.UID, quantity, (int)c.WalletCurID, "来自出售广告单[" + ad.OrderID + "]的下架退还", c);
                        }

                        ad.Status = (int)JN.Data.Enum.AdvertiseStatus.Cancel;
                        AdvertiseService.Update(ad);
                        SysDBTool.Commit();
                        result.Status = 200;
                    }
                    else throw new CustomException("当前广告单有未完成的交易，不能下架");
                }
                else throw new CustomException("当前状态不能下架");


            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
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

            ReturnResult result = new ReturnResult();
            try
            {

                var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
                if (AdvertiseOrder == null) throw new CustomException("没有记录");

                //购买人是自己，且订单为已下单
                if (AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder )
                {
                    AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.PendingPayment;
                    AdvertiseOrder.DeliveryTime = DateTime.Now.AddMinutes(SysParamService.Single(x => x.ID == 3002).Value.ToInt());//确认收货时间
                    AdvertiseOrderService.Update(AdvertiseOrder);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
                else { throw new CustomException("当前状态不能操作"); }

            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
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

            ReturnResult result = new ReturnResult();
            try
            {
                //查找订单
                var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
                if (AdvertiseOrder == null) throw new CustomException("没有记录");

                //必须是未完成的
                if (AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder || AdvertiseOrder.Status==(int)JN.Data.Enum.AdvertiseOrderStatus.PendingPayment)
                {
                    AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.Cancel;                 
                 

                    //把币返还到钱包或者订单中
                    var Advertise = AdvertiseService.Single(x => x.ID == AdvertiseOrder.AdID);
                    var c = CurrencyService.Single(x => x.ID == Advertise.CurID);
                   
                    if (Advertise.Direction == 0)
                    {
                        Advertise.HaveQuantity += (AdvertiseOrder.Quantity);//返还余额
                        if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed) Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Underway;//如果订单是已完成状态，改成正在进行
                        AdvertiseService.Update(Advertise);
                    }
                    else
                    {
                        Advertise.HaveQuantity += AdvertiseOrder.Quantity;//返还余额
                        if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed) Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Underway;//如果订单是已完成状态，改成正在进行
                        AdvertiseService.Update(Advertise);
                        Wallets.changeWallet(AdvertiseOrder.SellUID, (AdvertiseOrder.Quantity + AdvertiseOrder.SellPoundage), (int)c.WalletCurID, "取消订单[" + AdvertiseOrder.OrderID + "]返还", c);
                    }

                    AdvertiseOrderService.Update(AdvertiseOrder);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
                else { throw new CustomException("当前状态不能操作"); }

            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion
    }
}
