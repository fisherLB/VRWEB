using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICore
{
    /// <summary>
    /// 缓存帮助类，可操作跨站点的缓存
    /// </summary>
    public class CacheHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public static bool Clear(string key,string site="")
        {
            if (string.IsNullOrWhiteSpace(site))
            {
                var url= System.Web.HttpContext.Current.Request.Url;
                site = url.Host + ":" + url.Port;
            }
 
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("key", key);

            string result= client.Post(string.Format("http://{0}/api/cache/Remove", site), dic);


            return false;


        }
        /// <summary>
        /// 删除指定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public static bool RemoveFuzzy(string key, string site = "")
        {
            if (string.IsNullOrWhiteSpace(site))
            {
                var url= System.Web.HttpContext.Current.Request.Url;
                site = url.Host + ":" + url.Port;
            }
 
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("key", key);

            string result = client.Post(string.Format("http://{0}/api/cache/RemoveFuzzy", site), dic);


            return false;


        }
    }
}
