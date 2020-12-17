using JN.Data.Service;
using JN.Services.Tool;
using MvcCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace JN.Services.Manager
{
    /// <summary>
    /// 获取当前价格
    /// </summary>
    /// <returns></returns>
    public partial class PriceHelps
    {
        private static string prefixKey = "CurrentPrice_";


        #region 获取 钱洁通交易系统当前价格
        /// <summary>
        /// 获取 交易系统当前价格
        /// </summary>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public static decimal GetQJTPrice()
        {
            decimal currPrice = 0;
            try
            {
                string url2 = ConfigHelper.GetConfigString("QJTPriceUrl");// "http://116.31.100.202:8068/home/GetPrice/QJT";
                //创建请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url2);

                //GET请求
                request.Method = "GET";
                request.ReadWriteTimeout = 5000;
                request.ContentType = "text/html;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                //返回内容
                string retString = myStreamReader.ReadToEnd();

                JsonObject newObj7 = new JsonObject(retString);
                currPrice = Math.Round(Convert.ToDecimal(newObj7["price"].Value), 4);
                return currPrice;
            }
            catch (Exception ex)
            {
                return currPrice;
            }
        }



        /// <summary>
        /// 获取X币当前价格
        /// </summary>
        /// <param name="currency">币种实体</param>
        /// <returns></returns>
        public static decimal getcurrentprice(Data.Currency currency)
        {
            decimal price = 0;
            if ((currency.GetPriceType ?? 0) == 1)//自己定义的代币
            {
                if (currency.TranPrice == 0)
                    price = currency.OriginalPrice;
                else
                    price = currency.TranPrice;
            }
            else if ((currency.GetPriceType ?? 0) == 2)//对接另一个第三方
            {
                if (GetCachePrice(currency.CurrencyName, ref price) && price != 0)
                {
                    price = price + (currency.Increase ?? 0);
                }
                else
                {
                    price = GetUSDckPrice("https://www.bcex.ca/coins/markets", currency.English).ToDecimal() + (currency.Increase ?? 0);
                    string key = prefixKey + currency.CurrencyName;
                    CacheExtensions.SetCache(key, price, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);
                }
            }
            else if ((currency.GetPriceType ?? 0) == 3)//对接钱洁通第三方
            {
                if (GetCachePrice(currency.CurrencyName, ref price) && price != 0)
                {
                    price = price + (currency.Increase ?? 0);
                }
                else
                {
                    price = GetQJTPrice();
                    if (price == 0)//暂时那么用
                    {
                        price = currency.OriginalPrice + (currency.Increase ?? 0);
                    }
                    string key = prefixKey + currency.CurrencyName;
                    CacheExtensions.SetCache(key, price, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);
                }
            }
            else//获取第三方价格
            {
                if (GetCachePrice(currency.CurrencyName, ref price) && price != 0)
                {
                    price = price + (currency.Increase ?? 0);
                }
                else
                {
                    //美元单位：StringHelp.GetPrice("https://api.coinmarketcap.com/v1/ticker/" + currency.English, "usd")
                    //人民币单位：StringHelp.GetPrice("https://api.coinmarketcap.com/v1/ticker/" + cur.English + "/?convert=CNY", "CNY")
                    price = GetPrice("https://api.coinmarketcap.com/v1/ticker/" + currency.English, "usd").ToDecimal() + (currency.Increase ?? 0);
                    string key = prefixKey + currency.CurrencyName;
                    CacheExtensions.SetCache(key, price, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);
                }
            }
            return price;
        }


        /// <summary>
        /// 获取昨日收盘价
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static decimal getyescloseprice(Data.Currency currency)
        {
            //先找日K线表的除去今日的最后一条，有的话取关盘价
            var chart = MvcCore.Unity.Get<IAdvertiseChartDayService>().List(x => x.CurID == currency.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) != 0).OrderByDescending(x => x.StockDate).ToList().FirstOrDefault();
            if (chart != null)
                return chart.ClosePrice;
            else //没有就找发行价
            {
                return currency.OriginalPrice;
            }
        }

        /// <summary>
        /// 价格缓存
        /// </summary>
        /// <returns></returns>
        public static bool GetCachePrice(string CurrencyName, ref decimal Price)
        {
            string key = prefixKey + CurrencyName;

            if (CacheExtensions.CheckCache(key))
            {
                Price = CacheExtensions.GetCache<decimal>(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        //https://www.bcex.ca/coins/markets
        /// <summary>
        /// 获取实时价格
        /// </summary>
        /// <param name="url">第三方请求地址</param>
        /// <param name="cname">查找币种名称英文简写</param>
        /// <param name="parameters">请求参数，格式参数1=值1&参数2=值2 示例 "a=1&b=3"</param>
        /// <returns></returns>
        public static string GetUSDckPrice(string url, string cname, string parameters = null)
        {
            try
            {
                var ja = StringHelp.RequestJson(url, parameters, "GET");
                if (ja["data"]["ckusd"][5]["coin_from"].ToString() == cname.ToLower())
                {
                    return ja["data"]["ckusd"][5]["current"].ToString();
                }
                else
                {
                    foreach (var item in ja["data"]["ckusd"])
                    {
                        if (item["coin_from"].ToString() == cname.ToLower())
                        {
                            return item["current"].ToString();
                        }
                    }
                    return "--";
                }
            }
            catch
            {
                return "--";
            }
        }
        /// <summary>
        /// 获取实时价格
        /// </summary>
        /// <returns></returns>
        public static string GetPrice(string url, string cname)
        {
            try
            {
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据  
                Byte[] pageData = MyWebClient.DownloadData(url); //从指定网站下载数据  
                string pageHtml = Encoding.Default.GetString(pageData);
                JArray ja = (JArray)JsonConvert.DeserializeObject(pageHtml);
                string ja1a = ja[0]["price_" + cname.ToLower()].ToString();
                return ja1a;
                //或者  
                //return "--";

                //if (dic.backOrderId)
                //{
                //    Dictionary<string, object> dic_data = (Dictionary<string, object>)dic["data"];
                //    return dic_data;
                //}
            }
            catch (Exception webEx)
            {
                JN.Services.Manager.logs.WriteErrorLog(System.Web.HttpContext.Current.Request.Url.ToString(), webEx);
                return "--";
                // return "出错"+webEx;
            }

        }

        #endregion
    }
}