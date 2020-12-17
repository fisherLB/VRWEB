using JN.Services.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.APP.Controllers
{
    public class HqController : BaseController
    {
        //
        // GET: /Game/

        #region 
        /// <summary>
        /// 
        /// </summary> 
        public ActionResult Index()
        {
            try
            {
                var url = @"https://api.coinmarketcap.com/v1/ticker/";
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据  
                Byte[] pageData = MyWebClient.DownloadData(url); //从指定网站下载数据  
                string pageHtml = Encoding.Default.GetString(pageData);
                JArray ja = (JArray)JsonConvert.DeserializeObject(pageHtml);

                ViewBag.CoinPrices = ja;
            }
            catch (Exception webEx)
            {
                logs.WriteErrorLog(System.Web.HttpContext.Current.Request.Url.ToString(), webEx);
            }

            return View();
        }

       
        #endregion

        

    }
}
