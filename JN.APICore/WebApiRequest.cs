using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APICore.Util;

namespace APICore
{
    /// <summary>
    /// webapi 客户端请求,自动加上签名
    /// </summary>
    public class WebApiClientRequest
    {

        private const string SIGN = "sign";
        private const string TIMESTAMP = "timestamp";
        private const string NONCESTR = "nonceStr";

        /// <summary>
        /// 发起Get 请求，访问需要签名的webapi接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string Get(string url,Dictionary<string,string> parameters)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string nonceStr = Guid.NewGuid().ToString("N");
            WebUtils web = new WebUtils();
            APIDictionary dic = new APIDictionary(parameters);

            IDictionary<string, string> sortedTxtParams = new SortedDictionary<string, string>(dic);
            dic = new APIDictionary(sortedTxtParams);


            web.Header.Add(TIMESTAMP, timestamp);
            web.Header.Add(NONCESTR, nonceStr);
            dic.Add(TIMESTAMP, timestamp);
            dic.Add(NONCESTR, nonceStr);
            web.Header.Add(SIGN, APISignature.RSASign(dic, ApiConfig.SecretKey));


            return web.DoGet(url, sortedTxtParams, ApiConfig.Charset);

        }
        /// <summary>
        /// 发起Post 请求，访问需要签名的webapi接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string Post(string url,Dictionary<string,string> parameters)
        {

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string nonceStr = Guid.NewGuid().ToString("N");
            WebUtils web = new WebUtils();
            APIDictionary dic = new APIDictionary(parameters);
           
            IDictionary<string, string> sortedTxtParams = new SortedDictionary<string, string>(dic);
            dic = new APIDictionary(sortedTxtParams);


            web.Header.Add(TIMESTAMP, timestamp);
            web.Header.Add(NONCESTR, nonceStr);
            dic.Add(TIMESTAMP, timestamp);
            dic.Add(NONCESTR, nonceStr);
            web.Header.Add(SIGN, APISignature.RSASign(dic, ApiConfig.SecretKey));
           

            return web.DoPost(url, sortedTxtParams, ApiConfig.Charset);

        }

    }
}
