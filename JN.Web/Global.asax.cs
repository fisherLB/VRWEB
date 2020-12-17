using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace JN.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcCoreConfig.RegisterMvcCore();
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            // 在出现未处理的错误时运行的代码 
            Exception ex = Server.GetLastError().GetBaseException();

            string header = "";
            string ip = "本机线程";
            string url = "无地址，多线程执行";
            if (Request != null)
            {
                foreach (var item in Request.Headers.AllKeys)
                {
                    header += string.Format("{0}:{1},", item, Request.Headers[item]);
                }

                ip = "";
                url = Request.Url.ToString();
            }

            //记录日志
            JN.Services.Manager.logs.WriteErrorLog(url, ex);
             
 

        }

        
    //解决票据HttpContext.Current.User.Identity.IsAuthenticated为false问题
    protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie cookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                try
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                    FormsIdentity id = new FormsIdentity(ticket);
                    GenericPrincipal principal = new GenericPrincipal(id, new string[] { ticket.UserData });
                    Context.User = principal;//存到HttpContext.User中
                }
                catch
                {


                }
            }
        }
       
        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            MvcCoreConfig.Request_End();
        }
    }
}
