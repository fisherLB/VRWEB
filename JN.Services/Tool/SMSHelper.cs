using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;


namespace JN.Services.Tool
{
    public static class SMSHelper
    {


        #region 网建接口，支持批量
        /// <summary>
        /// 发送手机短信（网建接口，支持批量） http://www.smschinese.cn/
        /// </summary>
        /// <param name="mobile">手机号码,多个手机号以,号相隔</param>
        /// <param name="body">短信内容</param>
        public static bool WebChineseMSM(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                string url = "http://utf8.api.smschinese.cn/?Uid=" + sys.SMSUid + "&Key=" + sys.SMSKey + "&smsMob=" + mobile + "&smsText=" + body;
                string targeturl = url.Trim();
                try
                {
                    bool result = false;
                    HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                    hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                    hr.Method = "GET";
                    hr.Timeout = 30 * 60 * 1000;
                    WebResponse hs = hr.GetResponse();
                    Stream sr = hs.GetResponseStream();
                    StreamReader ser = new StreamReader(sr, Encoding.Default);
                    string content = ser.ReadToEnd();
                    string msg = "";
                    if (content.Substring(0, 1) == "0" || content.Substring(0, 1) == "1")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        if (content.Substring(0, 1) == "2") //余额不足
                        {
                            //"手机短信余额不足";
                        }
                        else
                        {
                            //短信发送失败的其他原因，请参看官方API
                        }
                        result = false;
                        msg = "发送失败，结果：" + content;
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = ex.Message });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                    return false;
                }
            }
            else
                return false;
        }
        #endregion

        #region  C123接口
        /// <summary>
        /// C123接口 http://www.c123.cn/
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool C123SMS(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                string url = "http://wapi.c123.cn/tx/?uid=" + sys.SMSUid + "&pwd=" + sys.SMSKey + "&mobile=" + mobile + "&content=" + System.Web.HttpUtility.UrlEncode(body, Encoding.GetEncoding("GB2312")).ToUpper();
                string targeturl = url.Trim();
                try
                {
                    bool result = false;
                    HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                    hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                    hr.Method = "GET";
                    hr.Timeout = 30 * 60 * 1000;
                    WebResponse hs = hr.GetResponse();
                    Stream sr = hs.GetResponseStream();
                    StreamReader ser = new StreamReader(sr, Encoding.Default);
                    string content = ser.ReadToEnd();
                    string msg = "";
                    if (content.Substring(0, 1) == "100")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        if (content.Substring(0, 1) == "102") //余额不足
                        {
                            //"手机短信余额不足";
                        }
                        else
                        {
                            //短信发送失败的其他原因，请参看官方API
                        }
                        result = false;
                        msg = "发送失败，结果：" + content;
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    return result;

                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
                return false;
        }

        #endregion

        #region 容联接口 (支持短信,语言验证码)
        /// <summary>
        /// C123接口 http://www.yuntongxun.com/
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool VoiceVerify(string mobile, string yzm, string displayNum)
        {
            string ret = null;
            CCPRestSDK.CCPRestSDK api = new CCPRestSDK.CCPRestSDK();
            bool isInit = api.init("sandboxapp.cloopen.com", "8883");
            api.setAccount(ConfigHelper.GetConfigString("Voice_Uid"), ConfigHelper.GetConfigString("Voice_Key"));
            api.setAppId(ConfigHelper.GetConfigString("Voice_Appid"));
            try
            {
                if (isInit)
                {
                    Dictionary<string, object> retData = api.VoiceVerify(mobile, yzm, displayNum, "2", "");
                    ret = getDictionaryData(retData);
                }
                else
                {
                    ret = "初始化失败";
                }
                return true;
            }
            catch (Exception exc)
            {
                ret = exc.Message;
                return false;
            }
        }

        private static string getDictionaryData(Dictionary<string, object> data)
        {
            string ret = null;
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Value != null && item.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    ret += item.Key.ToString() + "={";
                    ret += getDictionaryData((Dictionary<string, object>)item.Value);
                    ret += "};";
                }
                else
                {
                    ret += item.Key.ToString() + "=" +
                    (item.Value == null ? "null" : item.Value.ToString()) + ";";
                }
            }
            return ret;
        }
        #endregion

        #region 发送邮件

        /// <summary>发送email,默认是25端口
        /// 
        /// </summary>
        /// <param name="title">邮件标题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="toAdress">收件人</param>
        public static bool SendMail(string title, string body, string toAdress, string smtpEmailAddress, string smtpUserName, string smtpPassword, string smtpServer, int smtpPort)
        {
            try
            {
                System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
                try
                {
                    mail.To = toAdress;
                    mail.From = smtpEmailAddress;
                    mail.Subject = title;
                    mail.BodyFormat = System.Web.Mail.MailFormat.Html;
                    mail.Body = body;
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1"); //basic authentication
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", smtpUserName); //set your username here
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", smtpPassword); //set your password here
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", smtpPort);//set port
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", "true");//set is ssl   
                    System.Web.Mail.SmtpMail.SmtpServer = smtpServer;
                    System.Web.Mail.SmtpMail.Send(mail);
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                return false;

                //MailAddress to = new MailAddress(toAdress);
                //MailAddress from = new MailAddress(smtpEmailAddress);
                //System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(from, to);
                //message.IsBodyHtml = true; // 如果不加上这句那发送的邮件内容中有HTML会原样输出 
                //message.Subject = title; message.Body = body;
                //SmtpClient smtp = new SmtpClient();
                //smtp.UseDefaultCredentials = true;
                //smtp.Port = smtpPort;
                //smtp.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //smtp.EnableSsl = true;
                //smtp.Host = smtpServer;
                //message.To.Add(toAdress);
                //smtp.Send(message);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 云片短信验证接口
        /// <summary>
        /// 云片短信验证接口
        /// </summary>
        /// <param name="text">发送内容</param>
        /// <param name="mobile">发送手机号</param>
        /// <returns></returns>
        public static bool SMSYunPian(string mobile, string text)
        {
            string originallymobile = mobile;//记录原始的手机号
            try
            {              
                // 设置为您的apikey(https://www.yunpian.com)可查
                string apikey = ConfigHelper.GetConfigString("YunPianApiKey"); //"deace1d0a07b8a561b2913795695e25f";                  

                mobile = HttpUtility.UrlEncode(mobile, Encoding.UTF8);//+号格式转了

                // 发送内容  需要与云片网的模板一致
                ///text = "【NBC诺贝尔】Your verification code is:" + text + "。如非本人操作，请忽略本短信";
                // 获取user信息url
                // string url_get_user = "https://sms.yunpian.com/v2/user/get.json";
                // 智能模板发送短信url
                string url_send_sms = "https://sms.yunpian.com/v2/sms/single_send.json";
                // 指定模板发送短信url
                //string url_tpl_sms = "https://sms.yunpian.com/v2/sms/tpl_single_send.json";
                // 发送语音短信url
                // string url_send_voice = "https://voice.yunpian.com/v2/voice/send.json";
              
                string data_send_sms = "apikey=" + apikey + "&mobile=" + mobile + "&text=" + text;
      
                HttpPost(url_send_sms, data_send_sms);

                MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = originallymobile, SMSContent = text, CreateTime = DateTime.Now, ReturnValue = "1" });
                MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                return true;
            }
            catch (Exception ex)
            {
                MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = originallymobile, SMSContent = text, CreateTime = DateTime.Now, ReturnValue = "-1" });
                MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                return false;
            }
          
        }
        public static void HttpPost(string Url, string postDataStr)
        {
            byte[] dataArray = Encoding.UTF8.GetBytes(postDataStr);
            // Console.Write(Encoding.UTF8.GetString(dataArray));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = dataArray.Length;
            //request.CookieContainer = cookie;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader =
                    new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string res = reader.ReadToEnd();
                reader.Close();
                JN.Services.Manager.logs.WriteLog(res);   //写入日志
                Console.Write("\nResponse Content:\n" + res + "\n");
            }
            catch (Exception e)
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader =
                    new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string res = reader.ReadToEnd();
                JN.Services.Manager.logs.WriteLog(res);   //写入日志
                Console.Write(e.Message + e.ToString());
            }
        }
        #endregion


        #region 创瑞接口
        /// <summary>
        /// 发送手机短信（创瑞接口） http://sms.cr6868.com
        /// </summary>
        /// <param name="mobile">手机号码</param>
        /// <param name="body">短信内容</param>
        /// <param name="templateId">短信模板ID</param>
        public static bool CYmsm(string mobile, string body, string templateId)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                try
                {
                    bool result = false;
                    StringBuilder sms = new StringBuilder();

                    //mobile = HttpUtility.UrlEncode(mobile, Encoding.UTF8);//+号格式转了
                    mobile = mobile.Replace("+", "");//去除+号
                                                     //body = body.Replace("{", "##");  //{符号替换为##
                                                     //body = body.Replace("}", "##"); //}符号替换为##

                    string SMSsign = ConfigHelper.GetConfigString("SMSsign");//短信签名ID或签名【测试签名】

                    sms.AppendFormat("accesskey={0}", sys.SMSUid);//accesskey用户开发key
                    sms.AppendFormat("&secret={0}", sys.SMSKey);//accessSecret用户开发秘钥
                                                                //sms.AppendFormat("&sign={0}", sys.Sign);//接口短信签名
                    sms.AppendFormat("&sign={0}", SMSsign);//接口短信签名
                    sms.AppendFormat("&templateId={0}", templateId);//接口短信模板Id
                    sms.AppendFormat("&mobile={0}", mobile);
                    sms.AppendFormat("&content={0}", HttpUtility.UrlEncode(body, Encoding.UTF8));

                    string resp = PushToWeb("http://api.1cloudsp.com/intl/api/v2/send", sms.ToString(), Encoding.UTF8);
                    var ja = JObject.Parse(resp);
                    string msg = "";
                    if (ja["code"].ToString() == "0")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        result = false;
                        msg = "发送失败，结果：code_" + ja["code"] + "，msg_" + ja["msg"];
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg, IP = HttpContext.Current.Request.UserHostAddress });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    return result;
                }
                catch (Exception ex)
                {
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = ex.Message, IP = HttpContext.Current.Request.UserHostAddress });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    return false;
                }
            }
            else
                return false;
        }
        public static string PushToWeb(string weburl, string data, Encoding encode)
        {
            byte[] byteArray = encode.GetBytes(data);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(weburl));
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            Stream newStream = webRequest.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length);
            newStream.Close();

            //接收返回信息：
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            StreamReader aspx = new StreamReader(response.GetResponseStream(), encode);
            return aspx.ReadToEnd();
        }
        #endregion


        #region 获取价格  https://api.coinmarketcap.com/v1/ticker/
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
