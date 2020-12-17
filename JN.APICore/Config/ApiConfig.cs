using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Configuration;
namespace APICore
{
    /// <summary>
    /// api 参数配置
    /// </summary>
    class ApiConfig
    {
        /// <summary>
        /// 从配置文件获取 SecretKey 值 
        /// </summary>
        public static string SecretKey
        {
            get
            {
                string key = (string)System.Web.HttpRuntime.Cache.Get("WebApiSecretKey");
                if (string.IsNullOrWhiteSpace(key))
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Contains("WebApiSecretKey"))
                    {
                        key = ConfigurationManager.AppSettings["WebApiSecretKey"];
                    }
                    else
                    {
                        Configuration config = WebConfigurationManager.OpenWebConfiguration("~");

                        key = "wzj!@#123";
                        config.AppSettings.Settings.Add("WebApiSecretKey", key);
                        config.Save();
                        System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                    }
                    
                    System.Web.HttpRuntime.Cache.Add("WebApiSecretKey", key,null,DateTime.Now.AddDays(1),System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default,null);
                }

                return key;
            }
        }

        public static string Charset
        {
            get{
                return "UTF-8";
            }
        }
        public static string SignType
        {
            get
            {
                return "RSA";
            }
        }
         
 



    }
}
