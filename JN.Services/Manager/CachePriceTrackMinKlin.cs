using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcCore.Extensions;
using JN.Data.Service;
using System.Data.Entity.SqlServer;
using JN.Data.Enum;
using JN.Data;

namespace JN.Services.Manager
{
    /// <summary>
    /// time:2017年8月29日 18:30:46  name:lin  alt:交易所K线图
    /// </summary>
    public class CachePriceTrackMinKlin
    {

        private static string prefixKey = "Min_";

        #region 获取写入1分钟模型

        /// <summary>
        /// 获取1分钟模型
        /// </summary>
        /// <returns></returns>
        public static Data.PriceTracking1Min Get1Min()
        {
            string key = prefixKey + "1Min";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.PriceTracking1Min>(key);
            }
            else
            { 
                //取最后一条
                var data = MvcCore.Unity.Get<JN.Data.Service.IPriceTracking1MinService>().List().OrderByDescending(x => x.ID).ToList();
                if (data!=null && data.Count > 0)
                {
                    Set1Min(data.FirstOrDefault());
                    return data.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 写入1分钟模型
        /// </summary>
        /// <param name="mode"></param>
        public static void Set1Min(Data.PriceTracking1Min mode)
        {
            string key = prefixKey + "1Min";

            CacheExtensions.SetCache(key, mode);
        } 


        #endregion

        #region 获取写入5分钟模型

        /// <summary>
        /// 获取5分钟模型
        /// </summary>
        /// <returns></returns>
        public static Data.PriceTracking5Min Get5Min()
        {
            string key = prefixKey + "5Min";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.PriceTracking5Min>(key);
            }
            else
            {
                //取最后一条
                var data = MvcCore.Unity.Get<JN.Data.Service.IPriceTracking5MinService>().List().OrderByDescending(x => x.ID).ToList();
                if (data != null && data.Count > 0)
                {
                    Set5Min(data.FirstOrDefault());
                    return data.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 写入5分钟模型
        /// </summary>
        /// <param name="mode"></param>
        public static void Set5Min(Data.PriceTracking5Min mode)
        {
            string key = prefixKey + "5Min";

            CacheExtensions.SetCache(key, mode);
        }


        #endregion

        #region 获取写入15分钟模型

        /// <summary>
        /// 获取15分钟模型
        /// </summary>
        /// <returns></returns>
        public static Data.PriceTracking15Min Get15Min()
        {
            string key = prefixKey + "15Min";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.PriceTracking15Min>(key);
            }
            else
            {
                //取最后一条
                var data = MvcCore.Unity.Get<JN.Data.Service.IPriceTracking15MinService>().List().OrderByDescending(x => x.ID).ToList();
                if (data != null && data.Count > 0)
                {
                    Set15Min(data.FirstOrDefault());
                    return data.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 写入15分钟模型
        /// </summary>
        /// <param name="mode"></param>
        public static void Set15Min(Data.PriceTracking15Min mode)
        {
            string key = prefixKey + "15Min";

            CacheExtensions.SetCache(key, mode);
        }


        #endregion

        #region 获取写入30分钟模型

        /// <summary>
        /// 获取30分钟模型
        /// </summary>
        /// <returns></returns>
        public static Data.PriceTracking30Min Get30Min()
        {
            string key = prefixKey + "30Min";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.PriceTracking30Min>(key);
            }
            else
            {
                //取最后一条
                var data = MvcCore.Unity.Get<JN.Data.Service.IPriceTracking30MinService>().List().OrderByDescending(x => x.ID).ToList();
                if (data != null && data.Count > 0)
                {
                    Set30Min(data.FirstOrDefault());
                    return data.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 写入30分钟模型
        /// </summary>
        /// <param name="mode"></param>
        public static void Set30Min(Data.PriceTracking30Min mode)
        {
            string key = prefixKey + "30Min";

            CacheExtensions.SetCache(key, mode);
        }


        #endregion

        #region 获取写入60分钟模型

        /// <summary>
        /// 获取60分钟模型
        /// </summary>
        /// <returns></returns>
        public static Data.PriceTracking60Min Get60Min()
        {
            string key = prefixKey + "60Min";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.PriceTracking60Min>(key);
            }
            else
            {
                //取最后一条
                var data = MvcCore.Unity.Get<JN.Data.Service.IPriceTracking60MinService>().List().OrderByDescending(x => x.ID).ToList();
                if (data != null && data.Count > 0)
                {
                    Set60Min(data.FirstOrDefault());
                    return data.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 写入60分钟模型
        /// </summary>
        /// <param name="mode"></param>
        public static void Set60Min(Data.PriceTracking60Min mode)
        {
            string key = prefixKey + "60Min";

            CacheExtensions.SetCache(key, mode);
        }


        #endregion

        #region 获取写入300分钟模型

        /// <summary>
        /// 获取300分钟模型
        /// </summary>
        /// <returns></returns>
        public static Data.PriceTracking300Min Get480Min()
        {
            string key = prefixKey + "300Min";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.PriceTracking300Min>(key);
            }
            else
            {
                //取最后一条
                var data = MvcCore.Unity.Get<JN.Data.Service.IPriceTracking300MinService>().List().OrderByDescending(x => x.ID).ToList();
                if (data != null && data.Count > 0)
                {
                    Set480Min(data.FirstOrDefault());
                    return data.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 写入480分钟模型
        /// </summary>
        /// <param name="mode"></param>
        public static void Set480Min(Data.PriceTracking300Min mode)
        {
            string key = prefixKey + "300Min";

            CacheExtensions.SetCache(key, mode);
        }


        #endregion

        #region 获取写入1天模型

        /// <summary>
        /// 获取1天模型
        /// </summary>
        /// <returns></returns>
        public static Data.PriceTracking1Day Get1Day()
        {
            string key = prefixKey + "1Day";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.PriceTracking1Day>(key);
            }
            else
            {
                //取最后一条
                var data = MvcCore.Unity.Get<JN.Data.Service.IPriceTracking1DayService>().List().OrderByDescending(x => x.ID).ToList();
                if (data != null && data.Count > 0)
                {
                    Set1Day(data.FirstOrDefault());
                    return data.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 写入1天模型
        /// </summary>
        /// <param name="mode"></param>
        public static void Set1Day(Data.PriceTracking1Day mode)
        {
            string key = prefixKey + "1Day";

            CacheExtensions.SetCache(key, mode);
        }


        #endregion
    }
}
