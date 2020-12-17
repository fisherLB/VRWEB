using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace APICore
{
    /// <summary>
    /// 配置站点配置- 各缓存站点域名
    /// </summary>
   public class CacheSiteConfig : BaseConfig
    {
        /// <summary>
        /// 超管后台域名
        /// </summary>
        public static string Admin
        {
            get
            {
                 
                return GetAppKey("CacheSite_Admin","ptadmin.58wzj.com");
            }
        }
        /// <summary>
        /// 超管后台域名
        /// </summary>
        public static string ShopManager
        {
            get
            {

                return GetAppKey("CacheSite_ShopManager", "shop.58wzj.com");
            }
        }

        /// <summary>
        /// 超管后台域名
        /// </summary>
        public static string WebApp
        {
            get
            {

                return GetAppKey("CacheSite_WebApp", "platform.58wzj.com");
            }
        }


       

    }
}
