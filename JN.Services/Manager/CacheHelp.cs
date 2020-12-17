using JN.Data.Service;
using MvcCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JN.Services.Manager
{
    public class CacheHelp
    {

        private static string sysParamsKey = "SysParams";
        private static string getCurrency = "GetCurrency";
        private static string sysSetKey = "SysSet";
        private static string shopSetKey = "ShopSet";
        private static string shopProCateSetKey = "ShopProCateSet";

        #region 清空全部缓存(数据库操作专用)
        /// <summary>
        /// 清空全部缓存(数据库操作专用)
        /// </summary>
        public static void ClearCacheAllDB()
        {
            List<String> caches = MvcCore.Extensions.CacheExtensions.GetAllCache();
            foreach (var cachename in caches)
                MvcCore.Extensions.CacheExtensions.ClearCache(cachename);
            List<String> caches2 = Services.Tool.DataCache.GetAllCache();
            foreach (var cachename in caches2)
                Services.Tool.DataCache.ClearCache(cachename);
            Services.AdminLoginHelper.AdminUserLogout();
        }
        #endregion

        #region 清空全部缓存(选择性清除缓存保留)
        /// <summary>
        /// 清空全部缓存(选择性清除缓存保留)
        /// </summary>
        public static void ClearCacheAll()
        {
            //选择性清除缓存保留
            ClearSysParamsCache();//清除参数缓存
            ClearSysSetCache();//清除系统设置缓存

            List<String> caches2 = Services.Tool.DataCache.GetAllCache();
            foreach (var cachename in caches2)
                Services.Tool.DataCache.ClearCache(cachename);
            Services.AdminLoginHelper.AdminUserLogout();
        }
        #endregion

        #region 获取写入参数列表 -缓存

        /// <summary>
        /// 获取写入参数列表 -缓存
        /// </summary>
        /// <returns>返回参数列表</returns>
        public static List<Data.SysParam> GetSysParamsList()
        {
            if (CacheExtensions.CheckCache(sysParamsKey))
                return CacheExtensions.GetCache<List<Data.SysParam>>(sysParamsKey);
            else
            {
                //取所有
                var list = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List().ToList();
                CacheExtensions.SetCache(sysParamsKey, list);
                return list;
            }
        }
        #endregion

        #region 清除参数缓存
        /// <summary>
        /// 清除参数缓存
        /// </summary>
        public static void ClearSysParamsCache()
        {
            if (CacheExtensions.CheckCache(sysParamsKey))
            {
                CacheExtensions.ClearCache(sysParamsKey);
            }
        }
        #endregion

        #region 获取写入系统设置 -缓存

        /// <summary>
        /// 获取写入系统设置 -缓存
        /// </summary>
        /// <returns>返回系统设置</returns>
        public static Data.SysSetting GetSysSet()
        {
            if (CacheExtensions.CheckCache(sysSetKey))
                return CacheExtensions.GetCache<Data.SysSetting> (sysSetKey);
            else
            {
                //取所有
                var model = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().List().FirstOrDefault();
                CacheExtensions.SetCache(sysSetKey, model);
                return model;
            }
        }
        #endregion

        #region 清除系统设置缓存
        /// <summary>
        /// 清除系统设置缓存
        /// </summary>
        public static void ClearSysSetCache()
        {
            if (CacheExtensions.CheckCache(sysSetKey))
            {
                CacheExtensions.ClearCache(sysSetKey);
            }
        }
        #endregion


        #region 获取写入币种列表 -缓存

        /// <summary>
        /// 获取写入币种列表 -缓存
        /// </summary>
        /// <returns>返回参数列表</returns>
        public static List<Data.Currency> GetCurrencyList()
        {
            if (CacheExtensions.CheckCache(getCurrency))
                return CacheExtensions.GetCache<List<Data.Currency>>(getCurrency);
            else
            {
                //取所有
                var list = MvcCore.Unity.Get<ICurrencyService>().List().ToList();
                CacheExtensions.SetCache(getCurrency, list);
                return list;
            }
        }
        #endregion

        #region 清除币种缓存
        /// <summary>
        /// 清除币种缓存
        /// </summary>
        public static void ClearGetCurrencyCache()
        {
            if (CacheExtensions.CheckCache(getCurrency))
            {
                CacheExtensions.ClearCache(getCurrency);
            }
        }
        #endregion

        //#region 获取写入商城设置 -缓存

        ///// <summary>
        ///// 获取写入商城设置 -缓存
        ///// </summary>
        ///// <returns>返回系统设置</returns>
        //public static Data.ShopSysSetting GetShopSet()
        //{
        //    if (CacheExtensions.CheckCache(shopSetKey))
        //        return CacheExtensions.GetCache<Data.ShopSysSetting>(shopSetKey);
        //    else
        //    {
        //        //取所有
        //        var model = MvcCore.Unity.Get<JN.Data.Service.IShopSysSettingService>().List().FirstOrDefault();
        //        CacheExtensions.SetCache(shopSetKey, model);
        //        return model;
        //    }
        //}
        //#endregion

        //#region 清除商城设置缓存
        ///// <summary>
        ///// 清除商城设置缓存
        ///// </summary>
        //public static void ClearShopSetCache()
        //{
        //    if (CacheExtensions.CheckCache(shopSetKey))
        //    {
        //        CacheExtensions.ClearCache(shopSetKey);
        //    }
        //}
        //#endregion


        //#region 获取写入商品分类设置 -缓存

        ///// <summary>
        ///// 获取写入商品分类设置 -缓存
        ///// </summary>
        ///// <returns>返回商品分类设置</returns>
        //public static List<Data.Shop_Product_Category> GetShopProCateSet()
        //{
        //    if (CacheExtensions.CheckCache(shopProCateSetKey))
        //        return CacheExtensions.GetCache <List<Data.Shop_Product_Category>>(shopProCateSetKey);
        //    else
        //    {
        //        //取所有
        //        var list = MvcCore.Unity.Get<JN.Data.Service.IShop_Product_CategoryService>().List(x=>(x.IsShow??false)).ToList();
        //        CacheExtensions.SetCache(shopProCateSetKey, list);
        //        return list;
        //    }
        //}
        //#endregion

        //#region 清除商品分类设置缓存
        ///// <summary>
        ///// 清除商品分类设置缓存
        ///// </summary>
        //public static void ClearGetShopProCateSetSetCache()
        //{
        //    if (CacheExtensions.CheckCache(shopProCateSetKey))
        //    {
        //        CacheExtensions.ClearCache(shopProCateSetKey);
        //    }
        //}
        //#endregion
    }
}
