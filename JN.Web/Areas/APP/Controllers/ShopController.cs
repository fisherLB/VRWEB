using System;
using System.Collections.Generic;
using JN.Services.Manager;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;
using JN.Services.CustomException;
using JN.Services.Tool;
using System.Collections;

namespace JN.Web.Areas.APP.Controllers
{
    public class ShopController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IShop_ReceiptAddressService Shop_ReceiptAddressService;
        private readonly ISysDBTool SysDBTool;
        private readonly IShop_InfoService ShopInfoService;
        private readonly IUserService UserService;
        private readonly IShop_OrderService Shop_OrderService;
        private readonly IShop_Order_DetailsService Shop_Order_DetailsService;
        private readonly IShop_ProductService Shop_ProductService;
        private readonly IShop_CommentsService Shop_CommentsService;
        private readonly IShop_CartService Shop_CartService;
        private readonly IShop_FavoritesService Shop_FavoritesService;
        private readonly IShop_Product_SKUService Shop_Product_SKUService;
        private readonly IShop_Tmp_Pro_ImgService Shop_Tmp_Pro_ImgService;
        private readonly IShop_SPECService Shop_SPECService;
        private readonly IShop_Product_CategoryService Shop_Product_CategoryService;
        public ShopController(IShop_ReceiptAddressService Shop_ReceiptAddressService,
            IUserService UserService,
            IShop_InfoService ShopInfoService,
            ISysDBTool SysDBTool,
            IShop_OrderService Shop_OrderService,
            IShop_Order_DetailsService Shop_Order_DetailsService,
            IShop_ProductService Shop_ProductService,
            IShop_CommentsService Shop_CommentsService,
            IShop_FavoritesService Shop_FavoritesService,
            IShop_Product_SKUService Shop_Product_SKUService,
            IShop_Tmp_Pro_ImgService Shop_Tmp_Pro_ImgService,
            IShop_SPECService Shop_SPECService,
            IShop_CartService Shop_CartService,
            IShop_Product_CategoryService Shop_Product_CategoryService
            )
        {
            this.Shop_ReceiptAddressService = Shop_ReceiptAddressService;
            this.UserService = UserService;
            this.ShopInfoService = ShopInfoService;
            this.SysDBTool = SysDBTool;
            this.Shop_OrderService = Shop_OrderService;
            this.Shop_Order_DetailsService = Shop_Order_DetailsService;
            this.Shop_CommentsService = Shop_CommentsService;
            this.Shop_CartService = Shop_CartService;
            this.Shop_FavoritesService = Shop_FavoritesService;
            this.Shop_Product_SKUService = Shop_Product_SKUService;
            this.Shop_Tmp_Pro_ImgService = Shop_Tmp_Pro_ImgService;
            this.Shop_SPECService = Shop_SPECService;
            this.Shop_ProductService = Shop_ProductService;
            this.Shop_Product_CategoryService = Shop_Product_CategoryService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
        }

        #region 商城首页
        public ActionResult index()
        {
            ActMessage = "商城中心";
            return View();
        }

        public ActionResult indexShop_Product(int id)
        {
            ActMessage = "商城中心";
            ViewBag.Shop_InfoID = id;
            return View();
        }
        /// <summary>
        /// 获取商品列表
        /// </summary>
        public ActionResult getShop_Product(int? page, int? CategoryID, string search, int shopinfoID)
        {
            //var lockShopInfoList = ShopInfoService.List(x => x.IsLock == true).ToList();
            //string lockshopInfo = "";
            //if (lockShopInfoList.Count() > 0)
            //{
            //    foreach (var item in lockShopInfoList)
            //    {
            //        string shopInfoId = item.ID.ToString();
            //        lockshopInfo += "," + shopInfoId;
            //    }
            //}
            //string[] shopInfoIds = lockshopInfo.TrimEnd(',').TrimStart(',').Split(',');
            //var list = Shop_ProductService.List(x => (x.IsPass ?? false) && x.Status && x.Stock > 0 && !lockshopInfo.Contains(x.SId.ToString()));//显示所有
            var list = Shop_ProductService.List(x => (x.IsPass ?? false) && x.Status && x.Stock > 0 && x.SId == shopinfoID);//显示所有
            if (CategoryID != null && CategoryID != 0)
            {
                list = list.Where(x => x.ClassPath.Contains("," + CategoryID + ","));
            }
            if (search != null && search.Length > 0)
            {
                list = list.Where(x => x.ProductName.Contains(search) || x.ShopName.Contains(search) || x.Info.Contains(search));
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.IsNew).ThenByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.ImageUrl = JN.Services.Tool.StringHelp.GetFirstStr(item.ImageUrl);
            }
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult getShop_Product2(int? page, int? CategoryID, string search)
        {
            //var lockShopInfoList = ShopInfoService.List(x => x.IsLock == true).ToList();
            //string lockshopInfo = "";
            //if (lockShopInfoList.Count() > 0)
            //{
            //    foreach (var item in lockShopInfoList)
            //    {
            //        string shopInfoId = item.ID.ToString();
            //        lockshopInfo += "," + shopInfoId;
            //    }
            //}
            //string[] shopInfoIds = lockshopInfo.TrimEnd(',').TrimStart(',').Split(',');
            //var list = Shop_ProductService.List(x => (x.IsPass ?? false) && x.Status && x.Stock > 0 && !lockshopInfo.Contains(x.SId.ToString()));//显示所有
            var list = Shop_ProductService.List(x => (x.IsPass ?? false) && x.Status && x.Stock > 0 && x.IsOfflineProduct == true);//显示所有
            if (CategoryID != null && CategoryID != 0)
            {
                list = list.Where(x => x.ClassPath.Contains("," + CategoryID + ","));
            }
            if (search != null && search.Length > 0)
            {
                list = list.Where(x => x.ProductName.Contains(search) || x.ShopName.Contains(search) || x.Info.Contains(search));
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.IsNew).ThenByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.ImageUrl = JN.Services.Tool.StringHelp.GetFirstStr(item.ImageUrl);
            }
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region 获取店铺

        public ActionResult getShop_Info(int? page, int? CategoryID, string search)
        {
            var list = MvcCore.Unity.Get<JN.Data.Service.Shop_InfoService>().List(x => x.Status == 1 && x.IsOfflineShop == false);//显示所有
            if (search != null && search.Length > 0)
            {
                list = list.Where(x => x.ShopIntro.Contains(search) || x.ShopName.Contains(search));
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            //foreach (var item in listdata)
            //{
            //    item.ImageUrl = JN.Services.Tool.StringHelp.GetFirstStr(item.ImageUrl);
            //}
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult getShop_Info2(int? page, int? CategoryID, string search)
        {
            var list = MvcCore.Unity.Get<JN.Data.Service.Shop_InfoService>().List(x => x.Status == 1 && x.IsOfflineShop == true);//显示所有
            if (search != null && search.Length > 0)
            {
                list = list.Where(x => x.ShopIntro.Contains(search) || x.ShopName.Contains(search));
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            //foreach (var item in listdata)
            //{
            //    item.ImageUrl = JN.Services.Tool.StringHelp.GetFirstStr(item.ImageUrl);
            //}
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        public ActionResult Shop()
        {
            ActMessage = "商城首页";
            return View();
        }
        #region 收藏夹

        #region 我的收藏
        /// <summary>
        /// 我的收藏
        /// </summary>
        /// <returns></returns>
        public ActionResult MyKeep()
        {
            var list = Shop_FavoritesService.List(x => x.Uid == Umodel.ID).OrderByDescending(x => x.createtime).ToList();
            return View(list.ToList());
        }

        #endregion

        #region 加入收藏
        /// <summary>
        /// 加入收藏
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public ActionResult AddFavorites(int pid)
        {
            ReturnResult result = new ReturnResult();
            var model = Services.UserLoginHelper.CurrentUser();//用户
            if (model == null)
            {
                result.Status = 500;
                return Json(result, JsonRequestBehavior.AllowGet);
                //return Redirect("/ShopCenter/Login");
            }
            var pModel = Shop_ProductService.Single(x => x.Id == pid);//商品
            var entity = new JN.Data.Shop_Favorites();

            var falist = Shop_FavoritesService.List(x => x.Uid == model.ID && x.Pid == pModel.Id).ToList();//验证是否已经收藏过该商品
            if (falist.Count() > 0)
            {
                result.Message = "您已经收藏过该商品！";
                result.Status = 110;
            }
            else
            {
                //判断用户是否登录
                if (model != null)
                {
                    entity.Uid = model.ID;
                    entity.UserName = model.UserName;
                    entity.Sid = (int)pModel.SId;
                    entity.ShopName = pModel.ShopName;
                    entity.Pid = (int)pModel.Id;
                    entity.ProductName = pModel.ProductName;
                    entity.createtime = DateTime.Now;
                    Shop_FavoritesService.Add(entity);
                    SysDBTool.Commit();

                    result.Message = "加入收藏成功！";
                    result.Status = 200;
                }
                else
                {
                    result.Message = "您还没有登录，请登录后再重试！";
                    result.Status = 123;

                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除收藏
        public ActionResult DelMykeep()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string strPid = Request["pid"]; ;
                if (string.IsNullOrEmpty(strPid)) throw new CustomException("操作失败");
                JN.Data.User onUser = JN.Services.UserLoginHelper.CurrentUser();
                if (onUser == null) throw new CustomException("登录超时");
                int pId;
                int.TryParse(strPid, out pId);
                Shop_FavoritesService.List(x => x.ID == 1).ToList().Distinct();
                var cartModel = Shop_FavoritesService.Single(x => x.Pid == pId && x.Uid == onUser.ID);
                if (cartModel == null) if (onUser == null) throw new CustomException("您为收藏该商品");
                Shop_FavoritesService.Delete(cartModel.ID);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "删除成功！";
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

        #endregion

        #region 购物车

        #region 我的购物车
        /// <summary>
        /// 我的购物车
        /// </summary>
        /// <returns></returns>
        public ActionResult MyShopCart()
        {
            return View();
        }
        public JsonResult checkOnUser()
        {
            JN.Data.User onUser = JN.Services.UserLoginHelper.CurrentUser();//验证是否登录
            if (onUser == null)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            return Json("ok", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 加入购物车
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <returns></returns>
        public ActionResult AddMyShopCart()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                JN.Data.User onUser = JN.Services.UserLoginHelper.CurrentUser();//验证是否登录
                if (onUser == null) throw new CustomException("请登录");

                string sPid = Request["pid"];//商品

                int pid;
                int.TryParse(sPid, out pid);

                var proModel = Shop_ProductService.Single(pid);//验证商品
                if (proModel == null) throw new CustomException("操作失败，商品不存在");
                var shopModel = ShopInfoService.Single(int.Parse(proModel.SId.ToString()));//验证店铺
                if (shopModel == null) throw new CustomException("操作失败，商品不存在");

                if (Shop_CartService.List(x => x.Uid == onUser.ID && x.Pid == pid).Count() > 0) throw new CustomException("操作失败，购物车已存在该商品");//验证购物车是否有商品

                JN.Data.Shop_Cart cartModel = new Data.Shop_Cart();
                cartModel.Pid = int.Parse(proModel.Id.ToString());
                cartModel.ProductName = proModel.ProductName;
                cartModel.Sid = int.Parse(proModel.SId.ToString());
                cartModel.ShopName = proModel.ShopName;
                cartModel.CreateTime = DateTime.Now;
                cartModel.Uid = onUser.ID;
                cartModel.UserName = onUser.UserName;
                Shop_CartService.Add(cartModel);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "删除成功！";
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

        #region 删除购物车商品
        /// <summary>
        /// 删除购物车商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelMyShopCart()
        {
            string strPid = Request["pid"];
            string strSid = Request["sid"];
            if (string.IsNullOrEmpty(strPid))
            {
                return Json(new { result = "操作失败！" });
            }
            JN.Data.User onUser = JN.Services.UserLoginHelper.CurrentUser();
            if (onUser == null)
            {
                return Json(new { result = "操作失败！" });
            }
            int pId, sId;
            int.TryParse(strPid, out pId);
            int.TryParse(strSid, out sId);

            Shop_CartService.List(x => x.ID == 1).ToList().Distinct();

            var cartModel = Shop_CartService.Single(x => x.Pid == pId && x.Uid == onUser.ID && x.Sid == sId);
            if (cartModel == null)
            {
                return Json(new { result = "操作失败！" });
            }
            Shop_CartService.Delete(cartModel.ID);
            SysDBTool.Commit();
            return Content("ok");
        }
        #endregion

        #endregion

        #region 购买(商品详情)
        /// <summary>
        /// 立刻购买(订单信息)
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductInfo(int? id)
        {
            JN.Data.Shop_Product pmodel = Shop_ProductService.Single(id);
            //ViewBag.CategoryHtml = JN.Services.Manager.Category.GetCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));
            string viewName = "ProductInfo";
            return View(viewName, pmodel);
        }

        #region 立刻购买
        /// <summary>
        /// 立刻购买
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult OrderInfo(int pid)
        {
            JN.Data.Shop_Product proModel = Shop_ProductService.Single(pid);
            return View(proModel);
        }

        #endregion

        #region 确认支付
        /// <summary>
        /// 确认支付
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ProductInfo(FormCollection form)
        {
            var onUser = Services.UserLoginHelper.CurrentUser();
            ReturnResult result = new ReturnResult();
            try
            {
                if (onUser == null)
                {
                    throw new CustomException("请您先登录再支付！");
                }
                if (MvcCore.Extensions.CacheExtensions.CheckCache("ProductInfo" + onUser.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("ProductInfo" + onUser.ID, onUser.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                var pwd = form["pwd"];
                string pids = form["pids"];//商品ID
                string pCounts = form["pCounts"];//商品数量
                string PayWay = "3001";//form["PayWay"];//支付方式默认2001
                //string Addressee = form["Addressee"];//收件人姓名
                //string AddresseeId = form["AddresseeId"];//收件人地址
                //string Tel = form["Tel"];//收件人电话
                //string Postcode = form["Postcode"];//收件人邮编
                //string Points = form["Points"];//积分抵现 =1
                //string sids = Request["sids"]);
                if (Umodel.Password2 != pwd.Trim().ToMD5().ToMD5()) throw new CustomException("交易密码不正确");
                JN.Data.Shop_ReceiptAddress receiptModel = Shop_ReceiptAddressService.Single(x => x.UID == Umodel.ID && x.IsDefault == true);
                if (pids.Length == 0)
                {
                    throw new CustomException("请选择商品");
                }
                if (pCounts.Length == 0)
                {
                    throw new CustomException("请选择商品数量");
                }
                //if (PayWay.Length == 0)
                //{
                //throw new CustomException("请选择支付方式");
                //}
                //if (!PayWay.Equals("2001"))
                //{
                //throw new CustomException("下单失败，支付方式不正确");
                //}
                string[] sPids = pids.Split('|');
                string[] spCounts = pCounts.Split('|');
                if (spCounts.Length != sPids.Length)
                {
                    throw new CustomException("支付失败");
                }
                //int ReceiptAddressId = AddresseeId.ToInt();
                //JN.Data.Shop_ReceiptAddress receiptModel = Shop_ReceiptAddressService.Single(x => x.ID == ReceiptAddressId && x.UID == onUser.ID);
                if (receiptModel == null)
                {
                    throw new CustomException("收货地址不存在");
                }
                decimal totalMoney = 0;
                decimal totalMoneyFJC = 0;

                decimal blXS = cacheSysParam.Single(x => x.ID == 3107).Value.ToDecimal();
                decimal blXX = cacheSysParam.Single(x => x.ID == 3108).Value.ToDecimal();

                List<JN.Data.Shop_Order> orderList = new List<JN.Data.Shop_Order>();
                List<JN.Data.Shop_Order_Details> OrderdetailList = new List<JN.Data.Shop_Order_Details>();
                int status = 0;
                if (!"10000".Equals(PayWay))
                {
                    status = 1;
                }
                DateTime orderTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //先按Pid生成订单，然后再合单
                for (int i = 0; i < sPids.Length; i++)
                {
                    if (String.IsNullOrEmpty(sPids[i]))
                        continue;

                    JN.Data.Shop_Product proModel = Shop_ProductService.Single(sPids[i].ToInt());// BLL.Product.SelectModel(int.Parse(sPids[i]));
                    JN.Data.Shop_Info InfoModel = ShopInfoService.Single(proModel.SId);
                    //不能购买自己店铺的商品
                    if (InfoModel.UID == onUser.ID)
                    {
                        throw new CustomException("支付失败，不能购买自己店铺的商品 ~ " + proModel.ProductName);
                    }
                    //判断商品数量是否足够
                    if (proModel.Stock < int.Parse(spCounts[i]))
                    {
                        throw new CustomException("购买失败,商品库存不足 ~ " + proModel.ProductName);
                    }

                    JN.Data.Shop_Order oModel = new JN.Data.Shop_Order();
                    //oModel.Pid = int.Parse(sPids[i]);
                    //oModel.ProductName = proModel.ProductName;
                    oModel.Sid = int.Parse(proModel.SId.ToString());
                    oModel.ShopName = proModel.ShopName;
                    oModel.TotalCount = int.Parse(spCounts[i]);//总数
                    oModel.Buyer = onUser.UserName;
                    oModel.BuyerId = onUser.ID;
                    oModel.RealMoney = proModel.RealPrice;//总价/ rate
                    //oModel.ToldCash = proModel.CostPrice;//总价
                    //oModel.CreateTime = DateTime.Now;
                    oModel.BuyMsg = PayWay;

                    if (proModel.IsOfflineProduct == true)
                    {
                        oModel.Remark = "线下商品直接完成交易";
                        oModel.DealTime = DateTime.Now;
                        oModel.Status = (int)JN.Data.Enum.ShopOrderStatus.Deal;
                    }


                    orderList.Add(oModel);

                    JN.Data.Shop_Order_Details orderDetailModel = new JN.Data.Shop_Order_Details();

                    orderDetailModel.UserId = onUser.ID;
                    orderDetailModel.Status = status;
                    orderDetailModel.ByCount = int.Parse(spCounts[i]);
                    orderDetailModel.ShopId = int.Parse(proModel.SId.ToString());
                    orderDetailModel.ShopName = proModel.ShopName;
                    orderDetailModel.GoodsId = int.Parse(sPids[i]);
                    orderDetailModel.GoodsName = proModel.ProductName;
                    orderDetailModel.Img = proModel.ImageUrl;
                    orderDetailModel.CreateTime = orderTime;
                    orderDetailModel.OneFee = decimal.Parse((proModel.RealPrice).ToString());//单价
                    orderDetailModel.TotalFee = decimal.Parse((proModel.RealPrice).ToString()) * int.Parse(spCounts[i]);//总价

                    if (proModel.IsOfflineProduct == true)
                    {
                        orderDetailModel.Status = (int)JN.Data.Enum.ShopOrderStatus.Deal;
                        orderDetailModel.DeliveryTime = DateTime.Now;
                    }

                    //orderDetailModel.SkuId = int.Parse(PayWay);
                    //if ("2001".Equals(PayWay))
                    //{
                    //    orderDetailModel.SkuId = 2001;
                    //}else if("2002".Equals(PayWay)){
                    //    //orderDetailModel.OneFee = decimal.Parse(proModel.RealPrice.ToString());//单价
                    //    //orderDetailModel.TotalFee = decimal.Parse(proModel.RealPrice.ToString()) * int.Parse(spCounts[i]);//总价
                    //    orderDetailModel.SkuId = 2002;
                    //}
                    //else if ("10000".Equals(PayWay))
                    //{
                    //    //orderDetailModel.OneFee = decimal.Parse(proModel.RealPrice.ToString());//单价
                    //    //orderDetailModel.TotalFee = decimal.Parse(proModel.RealPrice.ToString()) * int.Parse(spCounts[i]);//总价
                    //    orderDetailModel.SkuId = 10000;
                    //}

                    OrderdetailList.Add(orderDetailModel);
                    //SandMem.BLL.Shop_Orderdetails.Add(orderDetailModel);
                    totalMoney += orderDetailModel.TotalFee;
                    if (proModel.IsOfflineProduct == true)
                    {
                        totalMoneyFJC += orderDetailModel.TotalFee * blXX;
                    }
                    else
                    {
                        totalMoneyFJC += orderDetailModel.TotalFee * blXS;
                    }

                }
                int walletid = PayWay.Trim().ToInt();
                var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == walletid);
                var c3003 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3003);
                if ("2001".Equals(PayWay))
                {
                    if ((onUser.Wallet2001) < Convert.ToDecimal(totalMoney))
                    {
                        throw new CustomException("支付失败，您的余额不足");
                    }
                }
                else
                {

                    decimal acceptWallet = Users.WalletCur(c.WalletCurID ?? 0, Umodel);//钱包余额
                    if (acceptWallet < Convert.ToDecimal(totalMoneyFJC))
                    {
                        throw new CustomException("支付失败，您的余额不足");
                    }

                    decimal acceptWallet2 = Users.WalletCur(3003, Umodel);//钱包余额
                    if (acceptWallet < Convert.ToDecimal(totalMoney - totalMoneyFJC))
                    {
                        throw new CustomException("支付失败，您的余额不足");
                    }
                }

                for (int i = 0; i < orderList.Count(); i++)
                {
                    for (int j = i + 1; j < orderList.Count(); j++)
                    {
                        if (orderList[i].Sid == orderList[j].Sid)
                        {
                            orderList[i].TotalCount += orderList[j].TotalCount;//总数量
                            orderList[i].RealMoney += orderList[j].RealMoney;//总价钱
                            //orderList[i].ToldCash += orderList[j].ToldCash;//总价钱
                            orderList.RemoveAt(j);
                        }
                    }
                }
                string orders = "";
                foreach (JN.Data.Shop_Order item in orderList)
                {
                    item.OrderNumber = GetOrderNumber();//生成订单编号
                    item.CreateTime = orderTime;
                    item.Addressee = receiptModel.Addressee;//收件人
                    item.Address = receiptModel.Detail;//收件地址
                    item.Phone = receiptModel.Phone;//收件电话
                    //item.Postcode = Postcode;
                    item.Status = status;//已支付 =1 未付=0

                    Shop_OrderService.Add(item);

                    foreach (JN.Data.Shop_Order_Details ordetail in OrderdetailList)
                    {
                        if (ordetail.ShopId == item.Sid)
                        {
                            ordetail.OrderNumber = item.OrderNumber;
                        }
                    }
                    if (orders.Length > 0)
                    {
                        orders = orders + "," + item.OrderNumber;
                    }
                    else
                    {
                        orders = item.OrderNumber;
                    }
                }
                foreach (JN.Data.Shop_Order_Details item in OrderdetailList)
                {
                    Shop_Order_DetailsService.Add(item);//新增到订单详情表
                }

                //减库存
                for (int i = 0; i < sPids.Length; i++)
                {
                    JN.Data.Shop_Product proModel = Shop_ProductService.Single(sPids[i].ToInt());
                    proModel.Stock = proModel.Stock - int.Parse(spCounts[i]);
                    Shop_ProductService.Update(proModel);
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    SysDBTool.Commit();
                    if ("2001".Equals(PayWay))
                    {
                        Wallets.changeWallet(onUser.ID, -totalMoney, 2001, orderTime + "买入商品扣除");
                    }
                    else
                    {
                        Wallets.changeWalletNoCommitAddup(onUser.ID, -(totalMoney - totalMoneyFJC), 3003, orderTime + "买入商品扣除", c3003);//积分减少
                        Wallets.changeWallet(Umodel.ID, 0 - totalMoneyFJC, c.WalletCurID ?? 3003, orderTime + "买入商品扣除", c);
                        //Wallets.changeWallet(onUser.ID, -totalMoney, 2002, orderTime + "买入商品扣除");
                    }



                    //if ("10000".Equals(PayWay))//在线支付
                    //{
                    //string orderNo = GetMOBNo();

                    ////写入奖金表
                    //BonusDetailService.Add(new Data.BonusDetail
                    //{
                    //    BonusMoney = Request.Form["amt"].ToDecimal(),
                    //    BonusID = 2001,
                    //    BonusName = cacheSysParam.Single(x => x.ID == 2001).Name,
                    //    CreateTime = DateTime.Now,
                    //    Description = "充值",
                    //    IsBalance = false,
                    //    UID = Umodel.ID,
                    //    UserName = Umodel.UserName,
                    //    ReserveStr1 = orderNo,
                    //});
                    //SysDBTool.Commit();

                    //Response.Redirect("http://pay.yfshkj.top/pay.aspx?amt=" + Request.Form["amt"] + "&bankCode=" + Request.Form["bankCode"] + "&orderNo=" + orderNo);

                    //Response.Redirect("/Order/PayCheck?dt=" + orderTime.ToString());

                    //return Json(new { result = "ok", refMsg = "", param = "/Order/PayCheck?orders=" + orders });

                    //return Redirect("/Order/PayCheck?dt=" + orderTime.ToString());
                    //}

                    if (cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.ToInt() == 1)  //会员下单后，商家需要收到一条短信通知
                    {
                        //短信内容
                        string content = cacheSysParam.SingleAndInit(x => x.ID == 3503).Value2;

                        //找出店铺
                        List<int> shopListID = orderList.Select(d => d.Sid).Distinct().ToList();
                        var shopList = ShopInfoService.List(x => shopListID.Contains(x.ID)).OrderBy(x => x.ID).ToList();

                        foreach (var item in shopList)
                        {
                            SMSHelper.WebChineseMSM(item.Tel, content);
                        }
                    }
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
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("ProductInfo" + onUser.ID);//清除缓存
            }
            return Json(result);
        }
        #endregion

        #region 立刻购买  （有购物车）
        /// <summary>
        /// 立刻购买
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //public ActionResult OrderInfo(string pids, string pCounts)
        //{
        //    List<JN.Data.Shop_Order_Details> orderList = new List<JN.Data.Shop_Order_Details>();

        //    string viewName = "OrderInfo";
        //    var model = Services.UserLoginHelper.CurrentUser();
        //    if (model == null)
        //    {
        //        return Redirect("/APP0916/LoSgin");
        //        //return View("Index", "Login");
        //    }

        //    if (string.IsNullOrEmpty(pids) || string.IsNullOrEmpty(pCounts))
        //    {
        //        return View(viewName, orderList);
        //    }

        //    string[] sPids = pids.Split('|');
        //    string[] spCounts = pCounts.Split('|');
        //    if (spCounts.Length != sPids.Length)
        //    {
        //        return View(viewName);
        //    }

        //    for (int i = 0; i < sPids.Length; i++)
        //    {
        //        if (String.IsNullOrEmpty(sPids[i]))
        //            continue;

        //        JN.Data.Shop_Product proModel = Shop_ProductService.Single(int.Parse(sPids[i]));
        //        if (proModel == null)
        //            continue;
        //        JN.Data.Shop_Order_Details oModel = new Data.Shop_Order_Details();
        //        oModel.GoodsId = int.Parse(sPids[i]);
        //        oModel.GoodsName = proModel.ProductName;
        //        oModel.ShopId = int.Parse(proModel.SId.ToString());
        //        oModel.ShopName = proModel.ShortName;
        //        oModel.ByCount = int.Parse(spCounts[i]);
        //        oModel.UserId = model.ID;

        //        orderList.Add(oModel);
        //    }

        //    return View(viewName, orderList);
        //}

        #endregion

        #region 确认支付 （有购物车）
        /// <summary>
        /// 确认支付
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult ProductInfo(FormCollection form)
        //{
        //    var onUser = Services.UserLoginHelper.CurrentUser();

        //    if (onUser == null)
        //    {
        //        return Json(new { result = "erro", refMsg = "请您先登录再支付！" });
        //    }

        //    string pids = form["pids"];//商品ID集合
        //    string pCounts = form["pCounts"];//商品数量集合
        //    string PayWay = form["PayWay"];//支付方式默认2001
        //    //string Addressee = form["Addressee"];//收件人姓名
        //    string AddresseeId = form["AddresseeId"];//收件人地址
        //    //string Tel = form["Tel"];//收件人电话
        //    //string Postcode = form["Postcode"];//收件人邮编
        //    //string Points = form["Points"];//积分抵现 =1
        //    //string sids = Request["sids"]);

        //    if (pids.Length == 0)
        //    {
        //        return Json(new { result = "erro", refMsg = "请选择商品" });
        //    }
        //    if (pCounts.Length == 0)
        //    {
        //        return Json(new { result = "erro", refMsg = "请选择商品数量" });
        //    }
        //    if (PayWay.Length == 0)
        //    {
        //        return Json(new { result = "erro", refMsg = "请选择支付方式" });
        //    }


        //    if (!PayWay.Equals("2001"))
        //    {
        //        return Json(new { result = "erro", refMsg = "下单失败，支付方式不正确" });
        //    }
        //    if (string.IsNullOrEmpty(AddresseeId))
        //    {
        //        return Json(new { result = "erro", refMsg = "请填写收货地址" });
        //    }

        //    //if (Tel.Length == 0)
        //    //{
        //    //    return Json(new { result = "erro", refMsg = "请输入收获人手机号码" });
        //    //}

        //    string[] sPids = pids.Split('|');
        //    string[] spCounts = pCounts.Split('|');
        //    if (spCounts.Length != sPids.Length)
        //    {
        //        return Json(new { result = "erro", refMsg = "支付失败" });
        //    }
        //    int ReceiptAddressId = AddresseeId.ToInt();
        //    JN.Data.Shop_ReceiptAddress receiptModel = Shop_ReceiptAddressService.Single(x => x.ID == ReceiptAddressId && x.UID == onUser.ID);
        //    if (receiptModel == null)
        //    {
        //        return Json(new { result = "erro", refMsg = "收货地址不存在" });
        //    }

        //    decimal totalMoney = 0;
        //    List<JN.Data.Shop_Order> orderList = new List<JN.Data.Shop_Order>();
        //    List<JN.Data.Shop_Order_Details> OrderdetailList = new List<JN.Data.Shop_Order_Details>();
        //    int status = 0;
        //    if (!"10000".Equals(PayWay))
        //    {
        //        status = 1;
        //    }


        //    DateTime orderTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //    //先按Pid生成订单，然后再合单
        //    for (int i = 0; i < sPids.Length; i++)
        //    {
        //        if (String.IsNullOrEmpty(sPids[i]))
        //            continue;

        //        JN.Data.Shop_Product proModel = Shop_ProductService.Single(sPids[i].ToInt());// BLL.Product.SelectModel(int.Parse(sPids[i]));
        //        JN.Data.Shop_Info InfoModel = ShopInfoService.Single(proModel.SId);
        //        //不能购买自己店铺的商品
        //        if (InfoModel.UID == onUser.ID)
        //        {
        //            return Json(new { result = "erro", refMsg = "支付失败，不能购买自己店铺的商品 ~ " + proModel.ProductName });
        //        }
        //        //判断商品数量是否足够
        //        if (proModel.Stock < int.Parse(spCounts[i]))
        //        {
        //            return Json(new { result = "erro", refMsg = "购买失败,商品库存不足 ~ " + proModel.ProductName });
        //        }

        //        JN.Data.Shop_Order oModel = new JN.Data.Shop_Order();
        //        //oModel.Pid = int.Parse(sPids[i]);
        //        //oModel.ProductName = proModel.ProductName;
        //        oModel.Sid = int.Parse(proModel.SId.ToString());
        //        oModel.ShopName = proModel.ShopName;
        //        oModel.TotalCount = int.Parse(spCounts[i]);//总数
        //        oModel.Buyer = onUser.UserName;
        //        oModel.BuyerId = onUser.ID;
        //        oModel.RealMoney = proModel.RealPrice;//总价/ rate
        //        //oModel.ToldCash = proModel.CostPrice;//总价
        //        //oModel.CreateTime = DateTime.Now;
        //        oModel.BuyMsg = PayWay;

        //        orderList.Add(oModel);

        //        JN.Data.Shop_Order_Details orderDetailModel = new JN.Data.Shop_Order_Details();

        //        orderDetailModel.UserId = onUser.ID;
        //        orderDetailModel.Status = status;
        //        orderDetailModel.ByCount = int.Parse(spCounts[i]);
        //        orderDetailModel.ShopId = int.Parse(proModel.SId.ToString());
        //        orderDetailModel.ShopName = proModel.ShopName;
        //        orderDetailModel.GoodsId = int.Parse(sPids[i]);
        //        orderDetailModel.GoodsName = proModel.ProductName;
        //        orderDetailModel.Img = proModel.ImageUrl;
        //        orderDetailModel.CreateTime = orderTime;
        //        orderDetailModel.OneFee = decimal.Parse((proModel.RealPrice).ToString());//单价
        //        orderDetailModel.TotalFee = decimal.Parse((proModel.RealPrice).ToString()) * int.Parse(spCounts[i]);//总价

        //        //orderDetailModel.SkuId = int.Parse(PayWay);
        //        //if ("2001".Equals(PayWay))
        //        //{
        //        //    orderDetailModel.SkuId = 2001;
        //        //}else if("2002".Equals(PayWay)){
        //        //    //orderDetailModel.OneFee = decimal.Parse(proModel.RealPrice.ToString());//单价
        //        //    //orderDetailModel.TotalFee = decimal.Parse(proModel.RealPrice.ToString()) * int.Parse(spCounts[i]);//总价
        //        //    orderDetailModel.SkuId = 2002;
        //        //}
        //        //else if ("10000".Equals(PayWay))
        //        //{
        //        //    //orderDetailModel.OneFee = decimal.Parse(proModel.RealPrice.ToString());//单价
        //        //    //orderDetailModel.TotalFee = decimal.Parse(proModel.RealPrice.ToString()) * int.Parse(spCounts[i]);//总价
        //        //    orderDetailModel.SkuId = 10000;
        //        //}

        //        OrderdetailList.Add(orderDetailModel);
        //        //SandMem.BLL.Shop_Orderdetails.Add(orderDetailModel);
        //        totalMoney += orderDetailModel.TotalFee;
        //    }

        //    if ("2001".Equals(PayWay))
        //    {
        //        if ((onUser.Wallet2001) < Convert.ToDecimal(totalMoney))
        //        {
        //            return Json(new { result = "erro", refMsg = "支付失败，您的余额不足" });
        //        }
        //    }
        //    else
        //    {
        //        if ((onUser.Wallet2002) < Convert.ToDecimal(totalMoney))
        //        {
        //            return Json(new { result = "erro", refMsg = "支付失败，您的余额不足" });
        //        }
        //    }

        //    for (int i = 0; i < orderList.Count(); i++)
        //    {
        //        for (int j = i + 1; j < orderList.Count(); j++)
        //        {
        //            if (orderList[i].Sid == orderList[j].Sid)
        //            {
        //                orderList[i].TotalCount += orderList[j].TotalCount;//总数量
        //                orderList[i].RealMoney += orderList[j].RealMoney;//总价钱
        //                //orderList[i].ToldCash += orderList[j].ToldCash;//总价钱
        //                orderList.RemoveAt(j);
        //            }
        //        }
        //    }

        //    string orders = "";

        //    foreach (JN.Data.Shop_Order item in orderList)
        //    {
        //        item.OrderNumber = GetOrderNumber();//生成订单编号
        //        item.CreateTime = orderTime;
        //        item.Addressee = receiptModel.Addressee;//收件人
        //        item.Address = receiptModel.Detail;//收件地址
        //        item.Phone = receiptModel.Phone;//收件电话
        //        //item.Postcode = Postcode;
        //        item.Status = status;//已支付 =1 未付=0

        //        Shop_OrderService.Add(item);

        //        foreach (JN.Data.Shop_Order_Details ordetail in OrderdetailList)
        //        {
        //            if (ordetail.ShopId == item.Sid)
        //            {
        //                ordetail.OrderNumber = item.OrderNumber;
        //            }
        //        }
        //        if (orders.Length > 0)
        //        {
        //            orders = orders + "," + item.OrderNumber;
        //        }
        //        else
        //        {
        //            orders = item.OrderNumber;
        //        }
        //    }
        //    foreach (JN.Data.Shop_Order_Details item in OrderdetailList)
        //    {
        //        Shop_Order_DetailsService.Add(item);//新增到订单详情表
        //    }

        //    //减库存
        //    for (int i = 0; i < sPids.Length; i++)
        //    {
        //        JN.Data.Shop_Product proModel = Shop_ProductService.Single(sPids[i].ToInt());
        //        proModel.Stock = proModel.Stock - int.Parse(spCounts[i]);
        //        Shop_ProductService.Update(proModel);
        //    }

        //    SysDBTool.Commit();

        //    if ("2001".Equals(PayWay))
        //    {
        //        Wallets.changeWallet(onUser.ID, -totalMoney, 2001, orderTime + "买入商品扣除");
        //    }
        //    else
        //    {
        //        Wallets.changeWallet(onUser.ID, -totalMoney, 2002, orderTime + "买入商品扣除");
        //    }



        //    //if ("10000".Equals(PayWay))//在线支付
        //    //{
        //    //string orderNo = GetMOBNo();

        //    ////写入奖金表
        //    //BonusDetailService.Add(new Data.BonusDetail
        //    //{
        //    //    BonusMoney = Request.Form["amt"].ToDecimal(),
        //    //    BonusID = 2001,
        //    //    BonusName = cacheSysParam.Single(x => x.ID == 2001).Name,
        //    //    CreateTime = DateTime.Now,
        //    //    Description = "充值",
        //    //    IsBalance = false,
        //    //    UID = Umodel.ID,
        //    //    UserName = Umodel.UserName,
        //    //    ReserveStr1 = orderNo,
        //    //});
        //    //SysDBTool.Commit();

        //    //Response.Redirect("http://pay.yfshkj.top/pay.aspx?amt=" + Request.Form["amt"] + "&bankCode=" + Request.Form["bankCode"] + "&orderNo=" + orderNo);

        //    //Response.Redirect("/Order/PayCheck?dt=" + orderTime.ToString());

        //    //return Json(new { result = "ok", refMsg = "", param = "/Order/PayCheck?orders=" + orders });

        //    //return Redirect("/Order/PayCheck?dt=" + orderTime.ToString());
        //    //}

        //    if (cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.ToInt() == 1)  //会员下单后，商家需要收到一条短信通知
        //    {
        //        //短信内容
        //        string content = cacheSysParam.SingleAndInit(x => x.ID == 3503).Value2;

        //        //找出店铺
        //        List<int> shopListID = orderList.Select(d => d.Sid).Distinct().ToList();
        //        var shopList = ShopInfoService.List(x => shopListID.Contains(x.ID)).OrderBy(x => x.ID).ToList();

        //        foreach (var item in shopList)
        //        {
        //            SMSHelper.WebChineseMSM(item.Tel, content);
        //        }
        //    }


        //    return Json(new { result = "ok", refMsg = "订单已支付成功！" });
        //}

        #endregion

        #region 生成真实订单号
        public static string GetOrderNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "S";
            result += dateTime.Year.ToString().Substring(2, 2) + dateTime.Month + dateTime.Day;//年月日
            int hour = dateTime.Hour;
            int minu = dateTime.Minute;
            int secon = dateTime.Second;
            int count = hour * 20 + minu * 10 + secon * 5;
            int lengh = count.ToString().Length;
            string temp = count.ToString();
            string bu = "";
            if (lengh < 5)//不足5位前面补0
            {
                for (int i = 1; i < 5 - lengh; i++)
                {
                    bu += "0";
                }
            }
            result += bu + temp;
            result += StringHelp.GetRandomNumber(4);//4位随机数字
            if (IsHave(result))
            {
                return GetOrderNumber();
            }
            return result;
        }

        #endregion

        #region 检查订单号是否重复
        private static bool IsHave(string number)
        {
            var pro = MvcCore.Unity.Get<JN.Data.Service.IShop_OrderService>().Single(x => x.OrderNumber == number);
            if (pro == null)
                return false;
            return true;
        }
        #endregion

        #region 购买成功
        /// <summary>
        /// 购买成功
        /// </summary>
        /// <returns></returns>
        public ActionResult SuccessPay()
        {
            string viewName = "SuccessPay";
            int Addresseeid = int.Parse(Request["id"]);
            var list = Shop_ReceiptAddressService.Single(x => x.ID == Addresseeid);
            return View(viewName, list);
        }
        #endregion

        #endregion



        #region 我的订单
        /// <summary>
        /// 我的订单
        /// </summary>
        /// <returns></returns>
        public ActionResult MyOrder(string css = "tab1")
        {
            ViewBag.css = css;
            //var listOrderDetails = Shop_Order_DetailsService.List(x => x.UserId == Umodel.ID);
            //return View(listOrderDetails.ToList());
            return View();
        }
        /// <summary>
        /// 获取订单列表
        /// </summary>
        public ActionResult getShop_Order_Details(int? page, int? status)
        {
            var list = Shop_Order_DetailsService.List(x => x.UserId == Umodel.ID);//显示所有
            if (status >= 0)
            {
                list = list.Where(x => x.Status == status);
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.Img = JN.Services.Tool.StringHelp.GetFirstStr(item.Img);
                item.Remark = typeof(JN.Data.Enum.ShopOrderStatus).GetEnumDesc(item.Status);
            }
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }
        //确认收货
        public ActionResult GerProduct(int DetailsId)
        {

            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("GerProduct" + DetailsId))
                {
                    throw new CustomException("该订单正结算中，请勿重复点击");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("GerProduct" + DetailsId, DetailsId, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                //string DetailsId = Request["DetailsId"];
                //int id = 0;
                //if (!int.TryParse(DetailsId, out id))
                //{
                //    return Content("操作失败！");
                //}
                JN.Data.Shop_Order_Details OrderDetailModel = Shop_Order_DetailsService.Single(x => x.DetailsId == DetailsId);
                if (OrderDetailModel == null)
                {
                    throw new CustomException("该订单不存在，请联系管理员");
                }
                if (OrderDetailModel.Status != (int)JN.Data.Enum.ShopOrderStatus.Transaction)
                {
                    throw new CustomException("操作失败！订单状态不正确");
                }


                //获取商家信息
                JN.Data.Shop_Info shopModel = ShopInfoService.Single(OrderDetailModel.ShopId);// SandMem.BLL.User.SelectModel(String.Format("sid='{0}'", OrderDetailModel.ShopId));
                //获取订单信息
                JN.Data.Shop_Order orderInfo = Shop_OrderService.Single(x => x.OrderNumber == OrderDetailModel.OrderNumber); //ShopInfoService.Single(OrderDetailModel.ShopId);


                var c3002 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == 2);//积分钱包
                var c3003 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == 3);//积分钱包
                decimal RebateRatio = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3302).Value.ToDecimal();
                decimal Rebate = OrderDetailModel.TotalFee * RebateRatio;
                var buyModel = UserService.SingleAndInit(x => x.ID == OrderDetailModel.UserId);

                decimal FeeRatio = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3301).Value.ToDecimal();
                decimal fee = OrderDetailModel.TotalFee * FeeRatio;
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {

                    OrderDetailModel.Status = (int)JN.Data.Enum.ShopOrderStatus.Deal;
                    OrderDetailModel.DeliveryTime = DateTime.Now;
                    Shop_Order_DetailsService.Update(OrderDetailModel);

                    #region 买家返还积分
                    //Wallets.changeWallet(buyModel.ID, Rebate, c3002.WalletCurID ?? 3002, "购买商品" + OrderDetailModel.OrderNumber + "获得返利" + OrderDetailModel.TotalFee + "×" + RebateRatio + "=" + Rebate, c3002);
                    //if ((int)c3002.WalletCurID == 3002)
                    //{
                    //    buyModel.ReserveDecamal2 = (buyModel.ReserveDecamal2 ?? 0) + Rebate;
                    //}
                    //Users.UpdateLevel(buyModel);//会员升级判断
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
                    result.Status = 200;
                    ts.Complete();
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
                MvcCore.Extensions.CacheExtensions.ClearCache("GerProduct" + DetailsId);//清除缓存
            }
            return Json(result);

        }

        #region 取消订单
        /// <summary>
        /// 取消订单
        /// </summary>
        [HttpPost]
        public ActionResult CancelOrder(string orderNumber)
        {
            var onUser = Services.UserLoginHelper.CurrentUser();
            ReturnResult result = new ReturnResult();
            try
            {
                if (onUser == null)
                {
                    throw new CustomException("请您先登录再取消！");
                }
                if (MvcCore.Extensions.CacheExtensions.CheckCache("CancelOrder" + onUser.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("CancelOrder" + onUser.ID, onUser.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                var shop_Order = Shop_OrderService.Single(x => x.OrderNumber == orderNumber);
                if (onUser == null)
                {
                    throw new CustomException("找不到该订单！");
                }
                if (shop_Order.Status != (int)Data.Enum.ShopOrderStatus.Sales)
                {
                    throw new CustomException("该订单状态无法取消！");
                }
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    List<JN.Data.Shop_Order_Details> proList = Shop_Order_DetailsService.List(x => x.OrderNumber == shop_Order.OrderNumber).ToList();
                    decimal totalMoney = 0;
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
                    shop_Order.Status = (int)Data.Enum.ShopOrderStatus.Cancel;
                    shop_Order.CancelTime = DateTime.Now;
                    shop_Order.CancelReason = "自己取消";
                    Shop_OrderService.Update(shop_Order);
                    SysDBTool.Commit();
                    int WalletId = shop_Order.BuyMsg.ToInt();
                    var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == WalletId);
                    Wallets.changeWallet((int)shop_Order.BuyerId, totalMoney, c.WalletCurID ?? 3003, "商城退款", c);
                    ts.Complete();
                    result.Status = 200;
                }
                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("CancelOrder" + onUser.ID);//清除缓存
            }
            return Json(result);
        }
        #endregion

        //提交评论
        public ActionResult DoComments()
        {
            string Pid = Request["Pid"];
            string ProComment = Request["ProComment"];
            string shopComment = Request["shopComment"];
            string proStar = Request["proStar"];
            string shopStart = Request["shopStart"];
            string wlStart = Request["wlStart"];
            string OrderId = Request["OrderId"];

            if (ProComment.Length == 0)
            {
                return Json(new { result = "erro", refMsg = "商品评论不能为空" });
            }

            if (shopComment.Length == 0)
            {
                return Json(new { result = "erro", refMsg = "服务评论不能为空" });
            }
            if (proStar.Length == 0)
            {
                return Json(new { result = "erro", refMsg = "请选择商品描述星级" });
            }
            if (shopStart.Length == 0)
            {
                return Json(new { result = "erro", refMsg = "请选择卖家服务星级" });
            }
            if (wlStart.Length == 0)
            {
                return Json(new { result = "erro", refMsg = "请选择物流服务星级" });
            }
            int iPid = Pid.ToInt();
            var porModel = Shop_ProductService.Single(x => x.Id == iPid);
            if (porModel == null)
            {
                return Json(new { result = "erro", refMsg = "操作失败" });
            }
            if (Shop_CommentsService.List(x => x.OrderNumber == OrderId && x.pid == iPid).Count() > 0)
            {
                return Json(new { result = "erro", refMsg = "该商品已评价" });
            }

            JN.Data.Shop_Comments comModel = new JN.Data.Shop_Comments();
            comModel.Uid = Umodel.ID;
            comModel.UserName = Umodel.RealName;
            comModel.CreateTime = DateTime.Now;
            comModel.pid = iPid;
            comModel.sid = porModel.SId.ToInt();
            comModel.proComment = ProComment;
            comModel.shopComment = shopComment;
            comModel.proStar = float.Parse(proStar);
            comModel.shopStart = float.Parse(shopStart);
            comModel.wlStart = float.Parse(wlStart);
            comModel.IsLock = true;
            comModel.Content = shopComment;
            comModel.OrderNumber = OrderId;
            comModel.AnswerId = false;

            Shop_CommentsService.Add(comModel);//添加一条评论

            var odModel = Shop_Order_DetailsService.Single(x => x.OrderNumber == OrderId && x.GoodsId == iPid);
            odModel.IsComment = true;
            Shop_Order_DetailsService.Update(odModel);//更改订单详细表状态

            SysDBTool.Commit();
            return Json(new { result = "ok", refMsg = "评论成功！" });
        }

        //投诉  17091601未用到
        [HttpPost]
        public ActionResult DoComplaint()
        {


            string OrderNumber = Request["OrderNumber"];
            string reason = Request["reason"];//快递公司

            if (String.IsNullOrEmpty(reason))
            {
                return Json(new { result = "erro", refMsg = "请填写原因" });
            }

            JN.Data.Shop_Order orderModel = Shop_OrderService.Single(x => x.OrderNumber == OrderNumber);
            if (orderModel == null)
            {
                return Json(new { result = "erro", refMsg = "操作失败，数据错误" });
            }
            if (orderModel.Status != (int)JN.Data.Enum.ShopOrderStatus.Sales)
            {
                return Json(new { result = "erro", refMsg = "请检查订单状态" });
            }

            //确认是否达到投诉时限
            var param6002 = MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 6002);
            if ((DateTimeDiff.DateDiff_Sec(orderModel.CreateTime, DateTime.Now) / 60) < param6002.Value.ToInt())
            {
                return Json(new { result = "erro", refMsg = "未到投诉时限" });
            }

            List<JN.Data.Shop_Order_Details> proList = Shop_Order_DetailsService.List(x => x.OrderNumber == OrderNumber).ToList();
            if (proList == null)
            {
                return Json(new { result = "erro", refMsg = "操作失败，数据错误" });
            }
            orderModel.Status = (int)JN.Data.Enum.ShopOrderStatus.Complaint;
            orderModel.ComplaintTime = DateTime.Now;
            orderModel.ComplaintReason = reason;
            Shop_OrderService.Update(orderModel);
            SysDBTool.Commit();

            foreach (JN.Data.Shop_Order_Details odItem in proList)
            {
                odItem.Status = (int)JN.Data.Enum.ShopOrderStatus.Complaint;
                //odItem.Remark = ExpressCompany + " : " + ExpressOrdersNo;
                //odItem.DeliverTime = DateTime.Now;
                Shop_Order_DetailsService.Update(odItem);
            }

            SysDBTool.Commit();
            return Json(new { result = "ok", refMsg = "操作成功，请耐心等待后台审核" });
        }
        #endregion

        #region 订单详情
        /// <summary>
        /// 我的订单
        /// </summary>
        /// <returns></returns>
        public ActionResult MyOrderDetail(int id)
        {
            var OrderDetail = Shop_Order_DetailsService.SingleAndInit(x => x.DetailsId == id);
            return View(OrderDetail);
        }
        #endregion

        #region 申请开店/修改店铺资料
        /// <summary>
        /// 申请开店
        /// </summary>
        /// <returns></returns>
        public ActionResult AddShopInfo()
        {
            ActMessage = "申请开店";
            var model = ShopInfoService.SingleAndInit(x => x.UID == Umodel.ID);
            if (model.ID > 0)
            {
                ActMessage = "修改店铺资料";
            }
            ViewBag.Title = ActMessage;
            return View(model);
        }

        [HttpPost]
        /// <summary>
        /// 申请开店
        /// </summary>
        /// <returns></returns>
        public ActionResult AddShopInfo(FormCollection fc)
        {

            ReturnResult result = new ReturnResult();
            try
            {
                var entity = ShopInfoService.SingleAndInit(fc["ID"].ToInt());

                TryUpdateModel(entity, fc.AllKeys);
                if (entity.ID > 0)
                {
                    if (entity.Status != (int)JN.Data.Enum.ShopInfoStatus.Refuse) throw new CustomException("该店铺状态无法更新");
                    if (string.IsNullOrEmpty(entity.ShopName) || string.IsNullOrEmpty(entity.ShopIntro)) throw new CustomException("店铺名称、店铺简介不能为空");
                    entity.CreateTime = DateTime.Now;
                    entity.Status = 0;
                    ShopInfoService.Update(entity);
                }
                else
                {
                    if (string.IsNullOrEmpty(entity.ShopName)) throw new CustomException("商铺名称不能为空");
                    if (string.IsNullOrEmpty(entity.ShopIntro)) throw new CustomException("店铺简介不能为空");
                    if (string.IsNullOrEmpty(entity.Tel)) throw new CustomException("电话不能为空");
                    //if (!StringHelp.IsPhone(entity.Tel) && !StringHelp.IsTel(entity.Tel)) throw new CustomException("请输入正确的电话号码");
                    if (string.IsNullOrEmpty(entity.ShopQQ)) throw new CustomException("客服不能为空");
                    if (string.IsNullOrEmpty(entity.Address)) throw new CustomException("店铺地址不能为空");
                    if (string.IsNullOrEmpty(entity.ShopInfoImg)) throw new CustomException("请上传营业资质相关图片");

                    if (ShopInfoService.List(x => x.ShopName == entity.ShopName).ToList().Count() > 0) throw new CustomException("商铺名称已经被使用"); //return Content("商铺名称已经被使用");
                    if (ShopInfoService.List(x => x.UID == Umodel.ID).ToList().Count() > 0) throw new CustomException("您的开店申请正在审核中.."); //return Content("您的开店申请正在审核中..");

                    entity.UID = Umodel.ID;
                    entity.UserName = Umodel.UserName;
                    entity.CreateTime = DateTime.Now;
                    entity.ShopName = fc["ShopName"];
                    entity.ShopIntro = fc["ShopIntro"];
                    entity.ShopClass = 1;
                    entity.ShopLevel = 1;
                    entity.Tel = fc["Tel"];
                    entity.ShopQQ = fc["ShopQQ"];
                    entity.Address = fc["Address"];
                    entity.IsLock = false;
                    entity.IsOfflineShop = fc["IsOfflineShop"].ToBool();
                    entity.Status = 0;

                    ShopInfoService.Add(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                //result.Message = "网络系统繁忙，请稍候再试!";
                //logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                result.Message = StringHelp.FormatErrorString(ex.Message);
            }
            return Json(result);
        }
        #endregion

        #region 我的商品管理
        /// <summary>
        /// 我的商品管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ShopProduct(int? page)
        {
            return View();
        }
        /// <summary>
        /// 获取店铺商品列表
        /// </summary>
        public ActionResult getProductList(int? page)
        {
            var myShop = ShopInfoService.Single(x => x.UID == Umodel.ID);
            //var list = Shop_ProductService.List(x => x.SId == myShop.ID && (x.IsPass ?? false) && x.Status && x.Stock > 0);//显示所有
            var list = Shop_ProductService.List(x => x.SId == myShop.ID);//显示所有
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.ImageUrl = JN.Services.Tool.StringHelp.GetFirstStr(item.ImageUrl);
            }
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 店铺商品
        /// <summary>
        /// 店铺商品
        /// </summary>
        /// <returns></returns>
        public ActionResult ShopStore(int sid)
        {
            var Shopinfo = ShopInfoService.SingleAndInit(x => x.ID == sid && x.Status == (int)JN.Data.Enum.ShopInfoStatus.Business);
            return View(Shopinfo);
        }
        /// <summary>
        /// 获取店铺商品列表
        /// </summary>
        public ActionResult getShopStore(int? page, int sid)
        {
            var Shopinfo = ShopInfoService.SingleAndInit(x => x.ID == sid);
            var list = Shop_ProductService.List(x => x.SId == Shopinfo.ID);//显示所有
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.ImageUrl = JN.Services.Tool.StringHelp.GetFirstStr(item.ImageUrl);
            }
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 产品主图图片上传
        //产品主图图片上传
        public ActionResult UpMainPic(string sid)
        {
            string strSid = sid;
            System.Collections.Hashtable hash = UpPic1(strSid);
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传产品主图到文件夹
        /// <param name="dir">文件夹名称 "/Upload//UserSpace/" + userId + "/" + dir + "/";</param>
        /// <param name="userId">订单从表ID</param>
        /// <returns></returns>
        public Hashtable UpPic1(string sid)
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                logs.WriteLog("sid:" + sid);
                string oldLogo = "/Upload/ShopInfo/" + sid + "/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 1024, System.Web.HttpContext.Current.Server.MapPath(oldLogo));

                hash["error"] = 0;
                hash["url"] = oldLogo + img;
                logs.WriteLog("上传成功：" + oldLogo + img);
                return hash;
            }
            catch (Exception ex)
            {
                hash["error"] = 1;
                hash["message"] = ex.Message;
                logs.WriteErrorLog("上传失败2：", ex);
                return hash;
            }
        }
        //商品临时图片上传
        public ActionResult UpInfoPic()
        {
            System.Collections.Hashtable hash = UpPic2("Temp");
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传菜品临时图片到文件夹
        /// <param name="dir">文件夹名称 "/Upload//UserSpace/" + userId + "/" + dir + "/";</param>
        /// <param name="userId">订单从表ID</param>
        /// <returns></returns>
        public Hashtable UpPic2(string dir)
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                string oldLogo = "/Upload/Shop/" + dir + "/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 800, System.Web.HttpContext.Current.Server.MapPath(oldLogo));
                hash["error"] = 0;
                hash["url"] = oldLogo + img;
                JN.Data.Shop_Tmp_Pro_Img imgInfo = new JN.Data.Shop_Tmp_Pro_Img();
                imgInfo.CreateTime = DateTime.Now;
                imgInfo.ProId = 0;
                imgInfo.ImgUrl = oldLogo + img;

                Shop_Tmp_Pro_ImgService.Add(imgInfo);
                SysDBTool.Commit();
                return hash;
            }
            catch (Exception ex)
            {
                hash["error"] = 1;
                hash["message"] = ex.Message;
                return hash;
            }
        }

        //商品临时图片列表
        public ActionResult TempPic()
        {
            var refundImages = Shop_Tmp_Pro_ImgService.List(x => x.ProId == 0);
            string viewName = "TempPic";
            if (refundImages != null && refundImages.Count() > 0)
            {
                return View(viewName, refundImages.OrderByDescending(x => x.CreateTime).Take(10).ToList());
            }
            return View(viewName);
        }

        //插入临时图片到编辑器
        public ActionResult InserTempImg()
        {
            string hidId = Request["hidId"];
            string url = Request["url"];
            string saveUrl = "/Upload/Shop/" + hidId + "/";
            string reUrl = IOHelper.FileCopyNewName(url, saveUrl);
            if (string.IsNullOrEmpty(reUrl))
            {
                return Json(new { result = "插入内容失败！" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "ok", url = saveUrl + reUrl }, JsonRequestBehavior.AllowGet);
            }
        }

        //删除临时内容图片
        public ActionResult DelTempImg()
        {
            string imgId = Request["imgId"];
            int id;
            int.TryParse(imgId, out id);
            JN.Data.Shop_Tmp_Pro_Img model = Shop_Tmp_Pro_ImgService.Single(id);
            if (model != null)
            {
                Shop_Tmp_Pro_ImgService.Delete(id);
                SysDBTool.Commit();
                //IOHelper.DelByMapPath(model.ImgUrl);
                IOHelper.DeleteFile(model.ImgUrl);
            }
            return Content("ok");
        }
        #endregion


        #region 添加/编辑商品
        /// <summary>
        /// 添加/编辑商品
        /// </summary>
        /// <returns></returns>
        public ActionResult AddProduct(int? id)
        {
            string viewName = "AddProduct";
            if (id.HasValue)
            {
                ActMessage = "编辑商品";
                ViewBag.Title = ActMessage;
                var productModel = new JN.Data.Shop_Product();
                try
                {
                    productModel = Shop_ProductService.Single(id);
                }
                catch (Exception ex)
                {

                    logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                }
                return View(viewName, productModel);
            }
            else
            {
                ActMessage = "添加商品";
                ViewBag.Title = ActMessage;
                return View(viewName, new Data.Shop_Product());
            }

        }

        /// <summary>
        /// 添加/编辑商品
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddProduct(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("AddProduct" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("AddProduct" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                var entity = Shop_ProductService.SingleAndInit(fc["ID"].ToInt());
                var entity2 = new JN.Data.Shop_SPEC();//商品规格表
                var myShop = ShopInfoService.Single(x => x.UID == Umodel.ID);//店铺表
                var shopClass = Shop_Product_CategoryService.Single(fc["GoodsClassId"].ToInt());
                if (shopClass == null)
                {
                    result.Message = "请选择商品分类";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                string Categroy1 = fc["Categroy1"];
                string Categroy2 = fc["Categroy2"];
                string Categroy3 = fc["Categroy3"];
                string Categroy4 = fc["Categroy4"];
                string Categroy5 = fc["Categroy5"];

                string Ppacth = "0,";
                if (!string.IsNullOrEmpty(Categroy1))
                {
                    Ppacth += Categroy1 + ",";
                    entity.GoodsClassId = int.Parse(Categroy1);
                }
                if (!string.IsNullOrEmpty(Categroy2))
                {
                    Ppacth += Categroy2 + ",";
                    entity.GoodsClassId = int.Parse(Categroy2);
                }
                if (!string.IsNullOrEmpty(Categroy3))
                {
                    Ppacth += Categroy3 + ",";
                    entity.GoodsClassId = int.Parse(Categroy3);
                }
                if (!string.IsNullOrEmpty(Categroy4))
                {
                    Ppacth += Categroy4 + ",";
                    entity.GoodsClassId = int.Parse(Categroy4);
                }
                if (!string.IsNullOrEmpty(Categroy5))
                {
                    Ppacth += Categroy5 + ",";
                    entity.GoodsClassId = int.Parse(Categroy5);
                }

                TryUpdateModel(entity, fc.AllKeys);
                if (string.IsNullOrEmpty(Ppacth))
                {
                    result.Message = "请选择商品分类";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(entity.ProductName))
                {
                    result.Message = "请输入商品名称";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(fc["ImageUrl"]))
                {
                    result.Message = "请上传商品图片";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(fc["Specifications"]))
                {
                    result.Message = "请输入商品规格";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (entity.RealPrice <= 0)
                {
                    result.Message = "请填写销售价格";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                if (entity.Id > 0)
                {
                    int puid = fc["ID"].ToInt();
                    entity.SId = myShop.ID;
                    entity.ShopName = myShop.ShopName;
                    //entity.ClassPath = Ppacth;//shopClass.Ppacth;
                    entity.CreateTime = DateTime.Now;
                    //entity.ProductName = fc["ProductName"];
                    //entity.ImageUrl = fc["hidMainimgSrc"];  
                    entity.ImageUrl = entity.ImageUrl.Replace(";;", ";");
                    //entity.GoodsClassId = fc["gClass"].ToInt();
                    //entity.AfterSsales = fc["AfterSsales"];
                    //entity.RealPrice = fc["RealPrice"].ToDecimal();
                    //entity.CostPrice = fc["CostPrice"].ToDecimal();
                    entity.Info = fc["Info"];//简介
                    entity.InfoMation = fc["txtContent"];//详细信息
                    entity.Inclueding = false;
                    //entity.Stock = fc["Stock"].ToInt();
                    //entity.Status = true;//已上架
                    Shop_ProductService.Update(entity);
                }
                else
                {
                    entity.SId = myShop.ID;
                    entity.ShopName = myShop.ShopName;
                    entity.IsOfflineProduct = myShop.IsOfflineShop;
                    entity.ClassPath = Ppacth; //shopClass.Ppacth;
                    entity.CreateTime = DateTime.Now;
                    //entity.ProductName = fc["ProductName"];
                    entity.ImageUrl = entity.ImageUrl.Replace(";;", ";");
                    //entity.GoodsClassId = fc["gClass"].ToInt();
                    //entity.AfterSsales = fc["AfterSsales"];
                    //entity.RealPrice = fc["RealPrice"].ToDecimal();
                    //entity.CostPrice = fc["CostPrice"].ToDecimal();
                    entity.Info = fc["Info"];//简介
                    entity.InfoMation = fc["txtContent"];//详细信息
                    entity.Inclueding = false;
                    //entity.Stock = fc["Stock"].ToInt();//通用库存
                    entity.Status = false;//未上架
                    entity.IsPass = false;//每件商品都需要后台审核
                    Shop_ProductService.Add(entity);
                    SysDBTool.Commit();

                    int pid = int.Parse(entity.Id.ToString());
                    var action = new JN.Data.Shop_Product_SKU();
                    //var action = Shop_Product_SKUService.SingleAndInit();
                    action.SKU_Name = "通用";
                    //action.ImgUrl = fc["hidMainimgSrc"];
                    action.Stock = fc["Stock"].ToInt();
                    action.Pid = entity.Id.ToInt();
                    action.CreateTime = DateTime.Now;
                    Shop_Product_SKUService.Add(action);
                    SysDBTool.Commit();

                    //添加商品规格
                    entity2.CreateTime = DateTime.Now;
                    entity2.PID = int.Parse(entity.Id.ToString());
                    entity2.Price = decimal.Parse(fc["RealPrice"]);
                    entity2.Value = fc["Specifications"];
                    Shop_SPECService.Add(entity2);
                }
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
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("AddProduct" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }

        public ActionResult GetCategroy(int ParentId)
        {
            var childList = MvcCore.Unity.Get<JN.Data.Service.IShop_Product_CategoryService>().List(x => x.ParentId == ParentId);
            if (childList != null && childList.Count() > 0)
            {
                return Content(childList.ToList().ToJson());
            }
            return Content(new JN.Data.Shop_Product_Category().ToJson());
        }

        #endregion

        #region 商品上架
        /// <summary>
        /// 商品上架
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult OnProduct(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var product = Shop_ProductService.SingleAndInit(id);
                if (product != null)
                {
                    var shopModel = MvcCore.Unity.Get<JN.Data.Service.IShop_InfoService>().Single(x => x.UID == Umodel.ID && x.IsActivation && x.Status == 1);
                    if (shopModel == null)
                    {
                        result.Message = "店铺未在营业状态！无法上架商品";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    //商品未审核无法上架
                    if (product.IsPass == false)
                    {
                        result.Message = "商品审核后才能上架！";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    product.Status = true;
                    Shop_ProductService.Update(product);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
            //return Json(result);
        }
        #endregion

        #region 商品下架
        /// <summary>
        /// 商品下架
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult OffProduct(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var product = Shop_ProductService.SingleAndInit(id.ToInt());
                if (product != null)
                {
                    string name = product.ShopName;
                    product.Status = false;
                    Shop_ProductService.Update(product);
                    SysDBTool.Commit();
                    result.Status = 200;
                    result.Message = "下架成功！";
                }
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
            //return Json(result);
            //return Content("下架成功！");
        }
        #endregion

        #region 店铺订单
        /// <summary>
        /// 店铺订单
        /// </summary>
        /// <returns></returns>
        public ActionResult ShopOrder(string css = "tab1")
        {
            ViewBag.css = css;
            var shopInfo = ShopInfoService.Single(x => x.UID == Umodel.ID);
            var listOrder = Shop_OrderService.List(x => x.Sid == shopInfo.ID);
            return View(listOrder.ToList());
        }
        /// <summary>
        /// 获取店铺订单列表
        /// </summary>
        public ActionResult getShop_Order(int? page, int? status)
        {
            var shopInfo = ShopInfoService.Single(x => x.UID == Umodel.ID);
            var list = Shop_Order_DetailsService.List(x => x.ShopId == shopInfo.ID);//显示所有
            if (status >= 0)
            {
                list = list.Where(x => x.Status == status);
            }
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            foreach (var item in listdata)
            {
                item.Img = JN.Services.Tool.StringHelp.GetFirstStr(item.Img);
                item.Remark = typeof(JN.Data.Enum.ShopOrderStatus).GetEnumDesc(item.Status);

            }
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);
            //var shopInfo = ShopInfoService.Single(x => x.UID == Umodel.ID);
            //var list = Shop_OrderService.List(x => x.Sid == shopInfo.ID);//显示所有
            //if (status >= 0)
            //{
            //    list = list.Where(x => x.Status == status);
            //}
            //int pageSize = 10;
            //var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            //foreach (var item in listdata)
            //{
            //    item.Img = JN.Services.Tool.StringHelp.GetFirstStr(item.Img);
            //    item.Remark = typeof(JN.Data.Enum.ShopOrderStatus).GetEnumDesc(item.Status);
            //}
            //int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            ////int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            //return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }
        //发货
        [HttpPost]
        public ActionResult DeliverGoods()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string OrderNumber = Request["OrderNumber"];
                string Remark = Request["Remark"];
                //string ExpressCompany = Request["ExpressCompany"];//快递公司
                //string ExpressOrdersNo = Request["ExpressOrdersNo"];//订单号

                //if (String.IsNullOrEmpty(ExpressCompany) || String.IsNullOrEmpty(ExpressOrdersNo))
                //{
                //    return Json(new { result = "erro", refMsg = "请输入快递公司和订单号" });
                //}
                if (String.IsNullOrEmpty(Remark))
                {
                    throw new CustomException("请输入快递公司和订单号");
                }

                JN.Data.Shop_Order orderModel = Shop_OrderService.Single(x => x.OrderNumber == OrderNumber);
                if (orderModel == null)
                {
                    throw new CustomException("该订单不存在");
                }
                if (orderModel.Status != (int)JN.Data.Enum.ShopOrderStatus.Sales)
                {
                    throw new CustomException("该订单当前状态无法发货");
                }
                List<JN.Data.Shop_Order_Details> proList = Shop_Order_DetailsService.List(x => x.OrderNumber == OrderNumber).ToList();
                if (proList == null)
                {
                    throw new CustomException("该订单不存在！");
                }
                orderModel.Status = (int)JN.Data.Enum.ShopOrderStatus.Transaction;
                orderModel.Remark = Remark;//ExpressCompany + " : " + ExpressOrdersNo;
                Shop_OrderService.Update(orderModel);
                SysDBTool.Commit();

                foreach (JN.Data.Shop_Order_Details odItem in proList)
                {
                    odItem.Status = (int)JN.Data.Enum.ShopOrderStatus.Transaction;
                    odItem.Remark = Remark;//ExpressCompany + " : " + ExpressOrdersNo;
                    odItem.DeliverTime = DateTime.Now;
                    Shop_Order_DetailsService.Update(odItem);
                }

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

        #region 地址管理

        #region 我的收货地址
        /// <summary>
        /// 收货地址
        /// </summary>
        /// <returns></returns>
        public ActionResult MyAddress()
        {
            //ViewBag.CategoryHtml = JN.Services.Manager.Category.GetCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));

            //string viewName = "ReceiptAddress";
            //var list = Shop_ReceiptAddressService.List(x => x.UID == Umodel.ID);
            //return View(viewName, list.ToList());
            return View();
        }
        /// <summary>
        /// 获取帐户收货地址
        /// </summary>
        public ActionResult getMyAddress(int? page)
        {
            var list = Shop_ReceiptAddressService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.CreateTime);//显示所有
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.CreateTime).ToPagedList(page ?? 1, pageSize);//取数据
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            //int totalPage = (list.Count() + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 设置默认地址
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetDefaultDeliveryAddress(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var list = Shop_ReceiptAddressService.List(x => x.UID == Umodel.ID).ToList();
                Shop_ReceiptAddressService.Update(new JN.Data.Shop_ReceiptAddress(), new Dictionary<string, string>() { { "IsDefault", "0" } }, "UID=" + Umodel.ID + " AND  ID != " + id);
                SysDBTool.Commit();

                var addresslist = list.Where(x => x.ID == id);
                if (addresslist.Count() > 0)
                {
                    var addressModel = addresslist.FirstOrDefault();
                    addressModel.IsDefault = true;
                    Shop_ReceiptAddressService.Update(addressModel);
                    SysDBTool.Commit();
                    result.Status = 200;
                    result.Message = "设置成功！";
                }
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 增加/修改地址
        /// <summary>
        /// 增加/修改地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ModifyMyAddress(int? id)
        {
            ViewBag.Title2 = "新增收货地址";
            //ViewBag.CategoryHtml = JN.Services.Manager.Category.GetCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));
            var Address = new JN.Data.Shop_ReceiptAddress();
            string viewName = "ModifyMyAddress";
            if (id.HasValue)
            {
                Address = Shop_ReceiptAddressService.Single(id);
                ViewBag.Title2 = "编辑收货地址";
            }
            return View(viewName, Address);
        }

        [HttpPost]
        public ActionResult ModifyMyAddress(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ReceiptAddressService.SingleAndInit(fc["id"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                if (entity.ID > 0)
                {
                    if (string.IsNullOrEmpty(entity.Province)) throw new CustomException("请选择省份");
                    if (string.IsNullOrEmpty(entity.City)) throw new CustomException("请选择城市市");
                    if (string.IsNullOrEmpty(entity.County)) throw new CustomException("请选择地区/县");
                    if (string.IsNullOrEmpty(entity.Detail)) throw new CustomException("请输入您的详细地址");
                    if (string.IsNullOrEmpty(entity.Addressee) || string.IsNullOrEmpty(entity.Phone)) throw new CustomException("收件人、联系电话不能为空");

                    entity.CreateTime = DateTime.Now;
                    entity.IsDefault = (fc["che"] == "on") ? true : false;
                    Shop_ReceiptAddressService.Update(entity);
                    SysDBTool.Commit();
                    result.Message = "编辑成功！";
                }
                else
                {
                    if (string.IsNullOrEmpty(entity.Province)) throw new CustomException("请选择省份");
                    if (string.IsNullOrEmpty(entity.City)) throw new CustomException("请选择城市市");
                    if (string.IsNullOrEmpty(entity.County)) throw new CustomException("请选择地区/县");
                    if (string.IsNullOrEmpty(entity.Detail)) throw new CustomException("请输入您的详细地址");
                    if (string.IsNullOrEmpty(entity.Addressee) || string.IsNullOrEmpty(entity.Phone)) throw new CustomException("收件人、联系电话不能为空");
                    entity.UID = Umodel.ID;
                    entity.CreateTime = DateTime.Now;
                    entity.IsDefault = (fc["che"] == "on") ? true : false;
                    Shop_ReceiptAddressService.Add(entity);
                    SysDBTool.Commit();
                    result.Message = "添加成功！";
                }
                if (fc["che"] == "on")//是否默认
                {
                    Shop_ReceiptAddressService.Update(new JN.Data.Shop_ReceiptAddress(), new Dictionary<string, string>() { { "IsDefault", "0" } }, "UID=" + Umodel.ID + " AND  ID != " + entity.ID);
                    SysDBTool.Commit();
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
            return Json(result);
        }
        #endregion

        #region 删除地址
        /// <summary>
        /// 删除地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelReceiptAddress(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = Shop_ReceiptAddressService.Single(id);
                if (model != null)
                {
                    Shop_ReceiptAddressService.Delete(id);
                    SysDBTool.Commit();
                    result.Message = "删除成功！";
                    result.Status = 200;
                    ViewBag.SuccessMsg = "删除成功！";
                }
                else
                {
                    result.Message = "删除失败！";
                    result.Status = 180;
                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Json(result);
        }
        #endregion

        #endregion


        #region 显示我的矿机
        public ActionResult getMOList(int? page, bool isbalance = true)
        {
            var list = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID);
            int pageSize = 10;
            var listdata = list.ToPagedList(page ?? 1, pageSize);//取数据
            var product = new JN.Data.ShopProduct();
            foreach (var item in listdata)
            {
                product = MvcCore.Unity.Get<JN.Data.Service.IShopProductService>().Single(item.ProductID);
                item.ReserveDate1 = item.CreateTime.AddMinutes(product.Duration ?? 0);//临时存放显示，不保存
                item.ImageUrl = product.ImageUrl;
                item.ReserveInt1 = (item.ReserveInt1 ?? 0);
            }
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
