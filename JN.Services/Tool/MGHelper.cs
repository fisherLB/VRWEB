using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Drawing;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using JN.Data.Extensions;

namespace JN.Services.Tool
{
    public static class MGHelper
    {        
        #region 获取价格
        /// <summary>      
        /// 获取MG价格 http://www.mindasset.com/exchange/converRateByValues
        /// 获取MG兑换成人民币的价格 http://www.mindasset.com/deal/getCurrentcoin
        /// </summary>
        public static string MG(string url)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);

            string values = "1250.01,0.015125121,12.6588713777,4.2782";
            string fromCode = "USD";
            string toCode = "USD";

           // string url = "http://www.mindasset.com/exchange/converRateByValues?values=" + values + "&fromCode=" + fromCode + "&toCode=" + toCode;
            
                string targeturl = url.Trim();
                try
                {
                    HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                    hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                    hr.Method = "Post";
                    hr.Timeout = 30 * 60 * 1000;
                    WebResponse hs = hr.GetResponse();
                    Stream sr = hs.GetResponseStream();
                    StreamReader ser = new StreamReader(sr, Encoding.Default);
                    string content = ser.ReadToEnd();

                   // ViewMG_Statistics viewMG = JsonExtensions.FromJson<Data.Extensions.ViewMG_Statistics>(content);

                    //if (viewMG != null)
                    //{
                    //    string[] mgArray = viewMG.data.values;
                    //    return mgArray[1];
                    //}

                    return content;
                }
                catch (Exception ex)
                {
                    return "0";
                }
            }
        #endregion
    }
}
