using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace JN.Services.Tool
{
    /// <summary>
    /// RPC帮助类
    /// </summary>
    internal class RPCHelper
    {

        private string url;

        private string jsonrpc = "2.0";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">地址，如:http://127.0.0.1:20187</param>
        public RPCHelper(string url)
        {
            this.url = url;
        }

        public T CallMethod<T>(string method, JObject Params)
        {
            return JsonConvert.DeserializeObject<T>(CallMethod(method, Params));
        }
        public string CallMethod(string method, JObject Params)
        {

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            /// important, otherwise the service can't desirialse your request properly
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";

            JObject joe = new JObject();
            //joe.Add(new JProperty("jsonrpc", jsonrpc));
            //joe.Add(new JProperty("id", "1"));
            joe.Add(new JProperty("method", method));
            // params is a collection values which the method requires..
            if (Params == null || Params.Count == 0)
            {
                joe.Add(new JProperty("params", new JArray()));
            }
            else
            {
                joe.Add(new JProperty("params", new JArray(Params)));
            }

            // serialize json for the request
            string s = JsonConvert.SerializeObject(joe);

            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            webRequest.ContentLength = byteArray.Length;
            System.IO.Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

           // HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            using (WebResponse webResponse = webRequest.GetResponse())
            {
                using (Stream str = webResponse.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(str))
                    {
                        return  sr.ReadToEnd();
                    }
                }
            }


            //Encoding encoding = Encoding.UTF8;
            //return GetResponseAsString(webResponse, encoding);
        }

        private string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            StringBuilder result = new StringBuilder();
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);

                // 按字符读取并写入字符串缓冲
                int ch = -1;
                while ((ch = reader.Read()) > -1)
                {
                    // 过滤结束符
                    char c = (char)ch;
                    if (c != '\0')
                    {
                        result.Append(c);
                    }
                }
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }

            return result.ToString();
        }

    }
}
