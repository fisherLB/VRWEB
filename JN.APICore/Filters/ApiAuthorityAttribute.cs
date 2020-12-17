using APICore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace APICore
{
    public class ApiAuthorityAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private const string SIGN = "sign";
        private const string TIMESTAMP = "timestamp";
        private const string NONCESTR = "nonceStr";

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            APIDictionary dic = new APIDictionary();

            //==========防止重放攻击 start

            //判断请求是过期
            if (actionContext.Request.Headers.Contains(TIMESTAMP))
            {
                string timestamp = actionContext.Request.Headers.GetValues(TIMESTAMP).FirstOrDefault();
                dic.Add(TIMESTAMP, timestamp);
                DateTime dt = DateTime.Now.AddDays(-1);
                if (DateTime.TryParse(timestamp, out dt))
                {
                    if (dt.AddMinutes(2) < DateTime.Now)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            string sign = null;
            if (actionContext.Request.Headers.Contains(SIGN))
            {
                sign = actionContext.Request.Headers.GetValues(SIGN).FirstOrDefault();

                //缓存里是否有签名，如果有说明已经请求过，直接返回false
                if (HttpRuntime.Cache.Get(SIGN + "_" + sign) == null)
                {
                    HttpRuntime.Cache.Add(SIGN + "_" + sign, sign, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                }
                else
                {
                    return false;
                }

            }
            else {
                return false;
            }
            if (actionContext.Request.Headers.Contains(NONCESTR))
            {
                dic.Add(NONCESTR, actionContext.Request.Headers.GetValues(NONCESTR).FirstOrDefault()); 
            }
            else
            {
                return false;
            }


            //==========防止重放攻击 end


            HttpContextBase context = (HttpContextBase)actionContext.Request.Properties["MS_HttpContext"];//获取传统context     
            HttpRequestBase request = context.Request;//定义传统request对象


            if (actionContext.Request.Method == HttpMethod.Get)
            {
                foreach (var item in request.QueryString.AllKeys)
                {

                    dic.Add(item, request.QueryString[item]);

                }
            }

            if (actionContext.Request.Method==HttpMethod.Post)
            {
                foreach (var item in request.Form.AllKeys)
                {

                    dic.Add(item, request.Form[item]);

                }    
            }
            

            return APISignature.RSASign(dic, ApiConfig.SecretKey) == sign;
        }
       
        


        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            
            var  result = "{\"Status\": 404,\"Message\" = \"你没有权限操作，请联系系统管理员！\"}";
            //var response = actionContext.Request.CreateResponse(HttpStatusCode.OK,new StringContent(result, System.Text.Encoding.UTF8, "application/json")); 
            //actionContext.Response = response;


            var response = actionContext.Request.CreateResponse();
            response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
            response.StatusCode = HttpStatusCode.OK;
            actionContext.Response = response;
            //base.HandleUnauthorizedRequest(actionContext);
        }

    }
}
