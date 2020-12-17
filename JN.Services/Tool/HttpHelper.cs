
using JN.Services.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace JN.Services.Tool
{
    public static class HttpHelper
    {
        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="url"></param>
        /// <param name="head"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public static string Post(string url, IEnumerable<KeyValuePair<string, string>> head, IEnumerable<KeyValuePair<string, string>> form)
        {
            string result = null;
            HttpWebRequest Request = null;
            try
            {
                Request = (HttpWebRequest)WebRequest.Create(url);
                Request.Headers.Clear();
                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                Request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                Request.UserAgent = "OAuth2.0 Authorization Client";
                if (head != null && head.Count() > 0)
                {
                    foreach (var kv in head)
                    {
                        Request.Headers.Add(kv.Key, kv.Value + "");
                    }
                }

                if (form != null && form.Count() > 0)
                {
                    string formContent = string.Join("&", form.Select(s => string.Format("{0}={1}", s.Key, System.Web.HttpUtility.UrlEncode(s.Value + ""))));
                    byte[] formContentByte = System.Text.Encoding.UTF8.GetBytes(formContent);
                    Request.ContentLength = formContentByte.Length;
                    using (var inputStream = Request.GetRequestStream())
                    {
                        inputStream.Write(formContentByte, 0, formContentByte.Length);
                    }
                }
                else Request.ContentLength = 0;
                result = Request.ReadResponse();               
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public static string Post(string url, IEnumerable<KeyValuePair<string, string>> head)
        {
            string result = null;
            HttpWebRequest Request = null;
            try
            {
                Request = (HttpWebRequest)WebRequest.Create(url);
                Request.Headers.Clear();
                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                Request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                Request.UserAgent = "OAuth2.0 JN Client";
                if (head != null && head.Count() > 0)
                {
                    foreach (var kv in head)
                    {
                        Request.Headers.Add(kv.Key, kv.Value + "");
                    }
                }
                
                Request.ContentLength = 0;

                //Stream stream = Request.GetRequestStream();
                //StreamReader reader = new StreamReader(stream, System.Text.Encoding.Default);
                //string str = reader.ReadToEnd();   //url返回的值  

                result = Request.ReadResponse();
                logs.WriteLog("------- result:" + result);
                //WebResponse wResponse = wRequest.GetResponse();
                //Stream stream = wResponse.GetResponseStream();
                //StreamReader reader = new StreamReader(stream, System.Text.Encoding.Default);
                //string str = reader.ReadToEnd();   //url返回的值  
                //reader.Close();
                //wResponse.Close();  

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        private static string ReadResponse(this HttpWebRequest Request)
        {
            string result = null;
            HttpWebResponse Response = null;
            try
            {
                Response = (HttpWebResponse)Request.GetResponse();
            }
            catch (WebException ex)
            {
                Response = (HttpWebResponse)ex.Response;
            }

            using (StreamReader reader = new StreamReader(Response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }

            Response.Dispose();
            return result;
        }

        public static string Get(string url, IEnumerable<KeyValuePair<string, string>> head)
        {
            string result = null;
            HttpWebRequest Request = null;
            try
            {
                Request = (HttpWebRequest)WebRequest.Create(url);
                Request.Method = "GET";
                Request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                Request.Headers.Clear();
                Request.UserAgent = "OAuth2.0 Client";
                if (head != null && head.Count() > 0)
                {
                    foreach (var kv in head)
                    {
                        logs.WriteLog("------- kv.Key:" + kv.Key);
                        logs.WriteLog("------- kv.Value:" + kv.Value);
                        Request.Headers.Add(kv.Key, kv.Value + "");
                    }
                }

                Request.ContentLength = 0;

                result = Request.ReadResponse();
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public static void Add(this List<KeyValuePair<string, string>> src, string key, string value)
        {
            src.Add(new KeyValuePair<string, string>(key, value));
        }
    }
}
