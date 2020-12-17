using JN.Data.Enum;
using JN.Data.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Web;
using JN.Data;
using System.Net;
using System.IO;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using JN.Data.Extensions;
using System.Data.Common;
using System.Transactions;

namespace JN.Services.Manager
{
    /// <summary>
    /// 交易所交易逻辑
    /// </summary>
    public partial class Stocks
    {
        private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();

        /// <summary>
        /// 清除缓存后重新加载
        /// </summary>
        public static void HeavyLoad()
        {
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        }

        //获取昨日收盘价
        public static decimal getyescloseprice(Data.Currency currency)
        {
            //先找日K线表的除去今日的最后一条，有的话取关盘价
            var chart = MvcCore.Unity.Get<IStockChartDayService>().List(x => x.CurID == currency.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) != 0).OrderByDescending(x => x.StockDate).ToList().FirstOrDefault();
            if (chart != null)
                return chart.ClosePrice;
            else //没有就找发行价
            {
                return currency.OriginalPrice;
            }
        }

        /// <summary>
        /// 获得X币当前价格
        /// </summary>
        /// <returns></returns>
        public static decimal getcurrentprice(Data.Currency currency)
        {
            var chart = MvcCore.Unity.Get<IStockChartDayService>().List(x => x.CurID == currency.ID).OrderByDescending(x => x.StockDate).ToList().FirstOrDefault();
            if (chart != null)
                return chart.ClosePrice;
            else //没有就找发行价
            {
                return currency.OriginalPrice;
            }

            //var cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 6000).ToList();
            //var param = MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 2101);
            //decimal currentprice = param.Value.ToDecimal() + ((currency.TotalSelling / param.Value2.ToDecimal()).ToInt() * param.Value3.ToDecimal());
            //return currentprice;

            //return getyescloseprice(currency);//两个价格一致
        }

        public static decimal getopenprice(Data.Currency currency)
        {
            //先找日K线表的最后一条，有的话取开盘价
            var chart = MvcCore.Unity.Get<IStockChartDayService>().List(x=>x.CurID == currency.ID).OrderByDescending(x => x.StockDate).ToList().FirstOrDefault();
            if (chart != null)
                return chart.OpenPrice;
            else //没有就找发行价
            {
                //var issue = MvcCore.Unity.Get<ICurrencyService>().List(x => x.ID == currency.ID).OrderByDescending(x => x.ID).ToList().FirstOrDefault();
                //if (currency.OriginalPrice != null)
                //    return issue.Price;
                //else
                //    return 0;
                return currency.OriginalPrice;
            }
        }

        #region 清除所有币种缓存
        /// <summary>
        /// 清除所有交易缓存
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static void ClearAllTradeCache()
        {
            var list = MvcCore.Unity.Get<ICurrencyService>().List(x => (x.GetPriceType ?? 0) == 1).ToList();

            foreach (var item in list)
            {
                string key = "TradePrice_" + item.ID + DateTime.Now.ToString("yyyy-MM-dd");//价格缓存

                if (MvcCore.Extensions.CacheExtensions.CheckCache(key))
                {
                    MvcCore.Extensions.CacheExtensions.ClearCache(key);
                }
            }    
        }
        #endregion

        #region 计算收益币种后冻结金额
        /// <summary>
        /// 计算收益币种后冻结金额 （20%直接交易，80%进入冻结）
        /// </summary>
        /// <param name="etrade">交易实体</param>
        /// <param name="_totalamount">收益币种</param>
        /// <param name="currency">币种实体</param> 
        /// <param name="_tradeNo">订单号</param>
        public static void CurIsFreezeWallet(int UserID, decimal _totalamount, JN.Data.Currency currency, string desc)
        {
            var onUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().SingleAndInit(UserID);
            decimal FreezeMoney = _totalamount * 0;//进入冻结比例
            if(FreezeMoney>0)
                Wallets.changeWallet(onUser.ID, FreezeMoney, (int)currency.WalletCurID, desc + "，实际：" + _totalamount.ToDouble() + ";进入冻结：" + FreezeMoney.ToDouble(), currency, true);//进入冻结钱包，每周释放5%

            _totalamount = _totalamount - FreezeMoney;//剩余余额
            Wallets.changeWallet(onUser.ID, _totalamount, (int)currency.WalletCurID, desc + "，实际：" + _totalamount.ToDouble() + ";进入冻结：" + FreezeMoney.ToDouble(), currency, false);//进入主钱包，可直接交易

        } 
        #endregion

        #region 聚合汇率
        /// <summary>
        /// 转换汇率
        /// </summary>
        /// <param name="roleid">转换的编码</param>
        public static decimal ExchangePrice(string roleid)
        {
            decimal juhePrice = 1;
            try
            {
                if (roleid == "bitcoin" || roleid == "ethereum" || roleid == "litecoin")
                {
                    juhePrice = JN.Services.Tool.StringHelp.GetPrice("https://api.coinmarketcap.com/v1/ticker/" + roleid, roleid, "usd", true).ToDecimal();
                }                           
                else
                {
                    string url2 = "http://op.juhe.cn/onebox/exchange/currency";
                    string appkey = Services.Tool.ConfigHelper.GetConfigString("juhe"); //配置您申请的appkey          

                    var parameters3 = new Dictionary<string, string>();

                    parameters3.Add("from", "USD"); //转换汇率前的货币代码
                    parameters3.Add("to", roleid); //转换汇率成的货币代码
                    parameters3.Add("key", appkey);//你申请的key

                    string result2 = sendPost(url2, parameters3, "get");
                    JsonObject newObj7 = new JsonObject(result2);
                    if (newObj7["error_code"].Value == "0")
                    {
                        JsonProperty re = newObj7["result"].Items[0];
                        // List<dataflow> list = new List<dataflow>();
                        //foreach (var item in flows)
                        //{
                        //list.Add(new dataflow { currencyF = re.Object["currencyF"].Value, currencyF_Name = re.Object["currencyF_Name"].Value, currencyT = re.Object["currencyT"].Value, currencyT_Name = re.Object["currencyT_Name"].Value, currencyFD = re.Object["currencyFD"].Value, exchange = re.Object["exchange"].Value, result = re.Object["result"].Value});
                        //}
                        // return Json(new { Status = 200, result = re.Object["exchange"].Value }, JsonRequestBehavior.AllowGet);
                        juhePrice = re.Object["exchange"].Value.ToDecimal();
                        return juhePrice;
                    }                 
                }
                return juhePrice;
            }
            catch (Exception)
            {
                return 0;
            }            
        }
      
        public class dataflow
        {
            public string currencyF { get; set; }
            public string currencyF_Name { get; set; }
            public string currencyT { get; set; }
            public string currencyT_Name { get; set; }
            public string currencyFD { get; set; }
            public string exchange { get; set; }
            public string result { get; set; }
            public string updateTime { get; set; }
        }
        /// <summary>
        /// Http (GET/POST)
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="parameters">请求参数</param>
        /// <param name="method">请求方法</param>
        /// <returns>响应内容</returns>
        static string sendPost(string url, IDictionary<string, string> parameters, string method)
        {
            if (method.ToLower() == "post")
            {
                HttpWebRequest req = null;
                HttpWebResponse rsp = null;
                System.IO.Stream reqStream = null;
                try
                {
                    req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = method;
                    req.KeepAlive = false;
                    req.ProtocolVersion = HttpVersion.Version10;
                    req.Timeout = 5000;
                    req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    byte[] postData = Encoding.UTF8.GetBytes(BuildQuery(parameters, "utf8"));
                    reqStream = req.GetRequestStream();
                    reqStream.Write(postData, 0, postData.Length);
                    rsp = (HttpWebResponse)req.GetResponse();
                    Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
                    return GetResponseAsString(rsp, encoding);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                finally
                {
                    if (reqStream != null) reqStream.Close();
                    if (rsp != null) rsp.Close();
                }
            }
            else
            {
                //创建请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "?" + BuildQuery(parameters, "utf8"));

                //GET请求
                request.Method = "GET";
                request.ReadWriteTimeout = 5000;
                request.ContentType = "text/html;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                //返回内容
                string retString = myStreamReader.ReadToEnd();
                return retString;
            }
        }

        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        static string BuildQuery(IDictionary<string, string> parameters, string encode)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;
            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name))//&& !string.IsNullOrEmpty(value)
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }
                    postData.Append(name);
                    postData.Append("=");
                    if (encode == "gb2312")
                    {
                        postData.Append(HttpUtility.UrlEncode(value, Encoding.GetEncoding("gb2312")));
                    }
                    else if (encode == "utf8")
                    {
                        postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                    }
                    else
                    {
                        postData.Append(value);
                    }
                    hasParam = true;
                }
            }
            return postData.ToString();
        }

        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应文本</returns>
        static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            System.IO.Stream stream = null;
            StreamReader reader = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }

        #endregion
    }
}