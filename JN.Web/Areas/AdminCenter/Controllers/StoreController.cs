using JN.Data.Service;
using JN.Services.Manager;
using MvcCore.Controls;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Collections.Generic;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class StoreController : BaseController
    {
        private readonly IShop_InfoService ShopInfoService;
        private readonly ISysDBTool SysDBTool;
        public StoreController(
            ISysDBTool SysDBTool,
            IShop_InfoService ShopInfoService
            )
        {
            this.ShopInfoService = ShopInfoService;
            this.SysDBTool = SysDBTool;
            
        }
        /// <summary>
        /// 商铺列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int? page)
        {
            var list = ShopInfoService.List(x => x.IsActivation == true).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);

            return View(list.ToPagedList(page ?? 1, 20));
        }
        /// <summary>
        /// 关闭商铺(冻结店铺)
        /// </summary>
        /// <returns></returns>
        public ActionResult CloseStore(int sid)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var shopModel = ShopInfoService.List(x => x.ID == sid).First();
                shopModel.IsLock = true;
                shopModel.Status = -2;
                ShopInfoService.Update(shopModel);
                var list = MvcCore.Unity.Get<JN.Data.Service.IShop_ProductService>().List(x => x.SId == shopModel.ID).ToList();//显示所有
                MvcCore.Unity.Get<Data.Service.IShop_ProductService>().Update(new Data.Shop_Product(), new Dictionary<string, string>() { { "Status", "0" } }, "SId=" + shopModel.ID);//下架该店铺所有商品
                SysDBTool.Commit();

                result.Message = "关闭店铺！";
                result.Status = 200;

            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
            //var shopModel = ShopInfoService.List(x => x.ID==sid).First();
            //shopModel.IsLock = true;
            //ShopInfoService.Update(shopModel);
            //SysDBTool.Commit();
            //return View(this.Index());
        }

        /// <summary>
        /// 开启商铺
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenStore(int sid)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var shopModel = ShopInfoService.List(x => x.ID == sid).First();
                shopModel.IsLock = false;
                shopModel.Status = 1;
                ShopInfoService.Update(shopModel);
                SysDBTool.Commit();

                result.Message = "开启店铺！";
                result.Status = 200;

            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 商品审核列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyStore(int? page)
        {
            var list = ShopInfoService.List(x => x.IsActivation==false && x.Status ==  (int)JN.Data.Enum.ShopInfoStatus.Application).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            return View(list.ToPagedList(page ?? 1, 20));
            
        }

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <returns></returns>
        public ActionResult DoAdopt(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var shopModel=ShopInfoService.Single(id.ToInt());
                shopModel.IsActivation = true;
                shopModel.IsLock = false;
                shopModel.Status = (int)JN.Data.Enum.ShopInfoStatus.Business;
                ShopInfoService.Update(shopModel);
                SysDBTool.Commit();
                result.Message = "审核通过！";
                result.Status = 200;

            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 审核不通过
        /// </summary>
        /// <returns></returns>
        public ActionResult DoNoAdopt(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var shopModel = ShopInfoService.Single(id.ToInt());
                shopModel.IsActivation = false;
                //shopModel.IsLock = true;
                shopModel.Status = (int)JN.Data.Enum.ShopInfoStatus.Refuse;
                ShopInfoService.Update(shopModel);
                SysDBTool.Commit();
                result.Message = "审核不通过！";
                result.Status = 200;

            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShopInfoModify(int? id)
        {
            ActMessage = "店铺详细资料";
            return View(ShopInfoService.Single(id));
        }

    }

}