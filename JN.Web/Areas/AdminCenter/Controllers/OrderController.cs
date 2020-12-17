using JN.Data.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Manager;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IShop_OrderService Shop_OrderService;
        private readonly IShop_Order_DetailsService Shop_Order_DetailsService;
        private readonly IShop_ProductService Shop_ProductService;
        private readonly ISysSettingService SysSettingService;
        private static List<Data.SysParam> cacheSysParam = null;
        public OrderController(
            ISysDBTool SysDBTool,
            IUserService UserService,
             IShop_OrderService Shop_OrderService,
             IShop_Order_DetailsService Shop_Order_DetailsService,
             IShop_ProductService Shop_ProductService,
            ISysSettingService SysSettingService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.Shop_OrderService = Shop_OrderService;
            this.Shop_Order_DetailsService = Shop_Order_DetailsService;
            this.Shop_ProductService = Shop_ProductService;
            this.SysSettingService = SysSettingService;
            this.ActLogService = ActLogService;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 4000).ToList();
        }

        /// <summary>
        /// 订单统计
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 交易记录
        /// </summary>
        /// <returns></returns>
        public ActionResult TradingRecords(int?page)
        {

            //动态构建查询
            var list = Shop_OrderService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.CreateTime).ToList();
            string status =Request["Status"];
            if (!string.IsNullOrEmpty(status))
            {
                int Status = int.Parse(status);
                list = list.Where(x => x.Status == Status).ToList();
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToList().ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 退款管理
        /// </summary>
        /// <returns></returns>
        public ActionResult RefundManagement()
        {
            return View();
        }

        /// <summary>
        /// 中止交易
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CancelShopOrder(int id)
        {
            var model = Shop_OrderService.Single(id);
            if (model != null)
            {
                //投诉的订单货买家刚付款的订单
                if (model.Status == (int)Data.Enum.ShopOrderStatus.Complaint || model.Status == (int)Data.Enum.ShopOrderStatus.Sales)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        decimal totalMoney = 0;
                        List<JN.Data.Shop_Order_Details> proList = Shop_Order_DetailsService.List(x => x.OrderNumber == model.OrderNumber).ToList();
                      
                        if (proList != null)
                        {
                            foreach (JN.Data.Shop_Order_Details odItem in proList)
                            {
                                //详细订单
                                odItem.Status = (int)JN.Data.Enum.ShopOrderStatus.Cancel;         
                                Shop_Order_DetailsService.Update(odItem);

                                //商品库存增加
                                var product = Shop_ProductService.Single(odItem.GoodsId);
                                product.Stock = product.Stock + odItem.ByCount;
                                Shop_ProductService.Update(product);

                                totalMoney += odItem.TotalFee;
                            }

                            
                        }
                        model.Status = (int)Data.Enum.ShopOrderStatus.Cancel;
                        model.CancelTime = DateTime.Now;
                        model.CancelReason = "后台取消";
                        Shop_OrderService.Update(model);
                        SysDBTool.Commit();
                        int WalletId = model.BuyMsg.ToInt();
                        var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == WalletId);
                        Wallets.changeWallet((int)model.BuyerId, totalMoney, c.WalletCurID ?? 3003, "商城退款", c);
                        //Wallets.changeWallet((int)model.BuyerId, totalMoney, model.BuyMsg.ToInt(), "商城退款");
                        ts.Complete();
                    }
                    //退款

                    ActPacket = model;
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "成功取消订单！";
                    ActMessage = ViewBag.SuccessMsg;
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易状态无法取消。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        [HttpPost]
        public ActionResult doDeliver(int id, string logistics)
        {
            var model = Shop_OrderService.Single(id);
            if (string.IsNullOrEmpty(logistics))
            {
                return Content("Error");
            }
            if (model != null)
            {
                model.Status = (int)Data.Enum.ShopOrderStatus.Transaction;
                model.Remark = logistics;
                Shop_OrderService.Update(model);
                SysDBTool.Commit();

                List<JN.Data.Shop_Order_Details> proList = Shop_Order_DetailsService.List(x => x.OrderNumber == model.OrderNumber).ToList();
                foreach (JN.Data.Shop_Order_Details odItem in proList)
                {
                    odItem.Status = (int)JN.Data.Enum.ShopOrderStatus.Transaction;
                    odItem.Remark = logistics;
                    odItem.DeliverTime = DateTime.Now;
                    Shop_Order_DetailsService.Update(odItem);
                }
                SysDBTool.Commit();
                ActMessage = "订单发货成功！";
                return Content("ok");
            }
            return Content("Error");
        }

        //确认收货
        public ActionResult doFinishOrder(int id)
        {
            JN.Data.Shop_Order_Details OrderDetailModel = Shop_Order_DetailsService.Single(x => x.DetailsId == id);// BLL.Shop_Orderdetails.SelectModel(String.Format("DetailsId='{0}'", DetailsId));
            if (OrderDetailModel == null)
            {
                ViewBag.ErrorMsg = "操作失败。";
                return View("Error");
            }

            var c3002 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == 2);//积分钱包
            var c3003 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == 3);//积分钱包
            decimal RebateRatio = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3302).Value.ToDecimal();//返还积分比例
            decimal Rebate = OrderDetailModel.TotalFee * RebateRatio;//返还积分金额
            var buyModel = UserService.SingleAndInit(x => x.ID == OrderDetailModel.UserId);//购买商品的会员
            decimal FeeRatio = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3301).Value.ToDecimal();//商品手续费
            decimal fee = OrderDetailModel.TotalFee * FeeRatio;//商品手续费金额
            //获取商家信息
            JN.Data.Shop_Info shopModel = MvcCore.Unity.Get<JN.Data.Service.IShop_InfoService>().Single(OrderDetailModel.ShopId);// SandMem.BLL.User.SelectModel(String.Format("sid='{0}'", OrderDetailModel.ShopId));
            //获取订单信息
            JN.Data.Shop_Order orderInfo = Shop_OrderService.Single(x => x.OrderNumber == OrderDetailModel.OrderNumber); //ShopInfoService.Single(OrderDetailModel.ShopId);
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {

                OrderDetailModel.Status = (int)JN.Data.Enum.ShopOrderStatus.Deal;
                OrderDetailModel.DeliveryTime = DateTime.Now;
                Shop_Order_DetailsService.Update(OrderDetailModel);

                #region 买家返还积分
                Wallets.changeWallet(buyModel.ID, Rebate, c3002.WalletCurID ?? 3002, "购买商品" + OrderDetailModel.OrderNumber + "获得返利" + OrderDetailModel.TotalFee + "×" + RebateRatio + "=" + Rebate, c3002);
                if ((int)c3002.WalletCurID == 3002)
                {
                    buyModel.ReserveDecamal2 = (buyModel.ReserveDecamal2 ?? 0) + Rebate;
                }
                Users.UpdateLevel(buyModel);//会员升级判断
                #endregion

                #region 卖家得钱
                Wallets.changeWallet(shopModel.UID, OrderDetailModel.TotalFee - fee, c3003.WalletCurID ?? 3003, "会员【" + orderInfo.Buyer + "】购买商品" + OrderDetailModel.OrderNumber + "获得" + OrderDetailModel.TotalFee + "(手续费：" + fee + ")", c3003);
                #endregion
                //判断是否全部确认收货
                //if (Shop_Order_DetailsService.List(x => x.OrderNumber == OrderDetailModel.OrderNumber && x.Status == (int)JN.Data.Enum.ShopOrderStatus.Deal).Count() == Shop_Order_DetailsService.List(x => x.OrderNumber == OrderDetailModel.OrderNumber).Count())//有购物车时需添加
                //{
                //全部确认收货后更新订单表状态
                //JN.Data.Shop_Order orderModel = BLL.Shop_Order.SelectModel(String.Format("OrderNumber='{0}'", OrderDetailModel.OrderNumber));
                orderInfo.DealTime = DateTime.Now;
                orderInfo.Status = (int)JN.Data.Enum.ShopOrderStatus.Deal;
                Shop_OrderService.Update(orderInfo);
                //}
                SysDBTool.Commit();
                ts.Complete();
            }
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "收货成功！";
            ActMessage = ViewBag.SuccessMsg;
            return View("Success");
           // return Json(new { result = "ok", refMsg = "收货成功！" },JsonRequestBehavior.AllowGet);
        }

    }
}