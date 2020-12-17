using JN.Data;
using JN.Data.Enum;
using JN.Services.Manager;
using JN.Services.Tool;
using MvcCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace JN.Services
{
    public static class UserLoginHelper
    {


        //登录缓存键前缀
        /// <summary>
        /// 登录缓存键前缀
        /// </summary>
        static string cacheUserName { get { return typeof(Data.User).Name + "_"; } }
        static Data.User _adminloginuser = null;

        /// <summary>
        /// 缓存前缀
        /// </summary>
        private static string prefixKey
        {
            get
            {
                return cacheUserName;
            }
        }
        /// <summary>
        /// 设置cookie
        /// </summary>
        /// <param name="token"></param>
        public static void Login(string token)
        {

            #region 存入 Cookie 票据

            // 设置Ticket信息
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                 1,
                token,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                false,
                Tool.IPHelper.GetClientIp()

                );

            // 加密验证票据
            string strTicket = FormsAuthentication.Encrypt(ticket);




            // 使用新userdata保存cookie
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strTicket);
            //cookie.Expires = DateTime.Now.AddMinutes(30);
            cookie.Path = "/";

            HttpContext.Current.Response.Cookies.Add(cookie);

            

            HttpContext.Current.Request.Cookies.Add(cookie);

           
            #endregion
        }

        
        public static Data.User GetUserLoginBy(string keyword, string password)
        {

            string pwd = password.ToMD5().ToMD5();
            var userService = MvcCore.Unity.Get<Data.Service.IUserService>();
            var account = userService.Single(a => (a.UserName == keyword.Trim() || a.Mobile==keyword.Trim()) && a.Password == pwd);
            if (account != null)
            {



                if (account.ReserveStr3 == null)
                {
                    account.ReserveStr3 = Guid.NewGuid().ToString("N");
                    userService.Update(account);
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                }
                else
                {
                    ////如果想实现同一账号同一时间只能一处登录，就用开放以下这段代码

                    string key = prefixKey + account.ReserveStr3;
                    MvcCore.Extensions.CacheExtensions.ClearCache(key);

                    account.ReserveStr3 = Guid.NewGuid().ToString("N");
                    userService.Update(account);
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                }

                Login(account.ReserveStr3);

                return account;
            }
            return null;
        }

        public static Data.User GetSysLoginBy(Data.User account, Data.AdminUser adminuser)
        {
            if (adminuser != null && adminuser.IsPassed)
            {
                _adminloginuser = account;
                if (account != null)
                {
                    var tempUser = CurrentUser();
                    if (tempUser != null  )
                    {
                        if (tempUser.ID == account.ID)
                        {
                            return account;
                        }
                        else
                        {
                            string key = prefixKey + tempUser.ReserveStr3;
                            MvcCore.Extensions.CacheExtensions.ClearCache(key);
                        }
                       
                    }
                    

                    var userService = MvcCore.Unity.Get<Data.Service.IUserService>();
                    account = userService.Single(account.ID);
                    if (account.ReserveStr3 == null)
                    {
                        account.ReserveStr3 = Guid.NewGuid().ToString("N");
                        userService.Update(account);
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                    }
                    else
                    {
                        ////如果想实现同一账号同一时间只能一处登录，就用开放以下这段代码

                        string key = prefixKey + account.ReserveStr3;
                        MvcCore.Extensions.CacheExtensions.ClearCache(key);

                        account.ReserveStr3 = Guid.NewGuid().ToString("N");
                        userService.Update(account);
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                       
                    }
                    Login(account.ReserveStr3);
                    return account;
                }
            }
                return null;
            
        }

        //获取当前用户登录对象
        /// <summary>
        /// 获取当前用户登录对象
        /// <para>当没登陆或者登录信息不符时，这里返回 null </para>
        /// </summary>
        /// <returns></returns>
        public static Data.User CurrentUser()
        {

            HttpCookie cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticket = null;
            if (cookie != null)
            {
                try
                {
                    ticket = FormsAuthentication.Decrypt(cookie.Value);

                    if (HttpContext.Current.User.Identity.IsAuthenticated)
                    {
                        FormsIdentity id = new FormsIdentity(ticket);
                        GenericPrincipal principal = new GenericPrincipal(id, new string[] { ticket.UserData });
                        HttpContext.Current.User = principal;//存到HttpContext.User中
                    }
                }
                catch
                {


                }
            }
            else
            {
                return null;
            }

            //客户端凭证验证不通过，要求重新登录
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
                return null;

           // var ticket = ((System.Web.Security.FormsIdentity)HttpContext.Current.User.Identity).Ticket;
            //客户端IP不一样，要求重新登录

            //if (ticket.UserData != Tool.IPHelper.GetClientIp())
            //    return null;
            if (ticket == null)
            {
                return null;
            }
            string key = prefixKey + ticket.Name;

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Data.User>(key);
            }
            else
            {
                var userService = MvcCore.Unity.Get<Data.Service.IUserService>();
                var user = userService.Single(d => d.ReserveStr3 == ticket.Name);
                if (user != null)
                {
                    CacheExtensions.SetCache(key, user, CacheTimeType.ByMinutes, 30);
                }
                return user;

            }
            //校验用户是否已经登录
            //var user = HttpContext.Current.Session[CookieName_User] as Data.User;
            //if (user != null) return user;
            //else
            //{
            //    if (HttpContext.Current.Request.Cookies[CookieName_User] != null && HttpContext.Current.Request.Cookies[CookieName_Tonen] != null)
            //    {
            //        string keyword = HttpContext.Current.Request.Cookies[CookieName_User].Value;
            //        string token = HttpContext.Current.Request.Cookies[CookieName_Tonen].Value;
            //        string pwd = token.Substring(32);
            //        var account = MvcCore.Unity.Get<Data.Service.IUserService>().Single(a => a.UserName == keyword.Trim() && a.Password == pwd);
            //        if (account != null) return account;
            //    }
            //}
            //return null;
        }

        //登出
        /// <summary>
        /// 登出
        /// </summary>
        public static void UserLogout()
        {
            if (CheckUserLogin())
            {
                //获取用户ID
                var id = HttpContext.Current.User.Identity.Name;
                FormsAuthentication.SignOut();
                _adminloginuser = null;
                RemoveUser(id);
            }
        }

        /// <summary>
        /// 与A系统对接登陆
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Data.User GetUserLoginBy_To(string keyword, string password)
        {
            var userService = MvcCore.Unity.Get<Data.Service.IUserService>();
            var account = userService.Single(a => (a.UserName == keyword.Trim() || a.Mobile == keyword.Trim() || ((a.IsEmail ?? false) && a.Email == keyword.Trim())) && a.Password == password);
            if (account != null)
            {


               
                if (account.ReserveStr3 == null)
                {
                    account.ReserveStr3 = Guid.NewGuid().ToString("N");
                    userService.Update(account);
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                }
                else
                {
                    ////如果想实现同一账号同一时间只能一处登录，就用开放以下这段代码

                    string key = prefixKey + account.ReserveStr3;
                    MvcCore.Extensions.CacheExtensions.ClearCache(key);

                    account.ReserveStr3 = Guid.NewGuid().ToString("N");
                    userService.Update(account);
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                }
                return account;
            }
            return null;
        }

        //移除指定用户ID的登录缓存
        /// <summary>
        /// 移除指定用户ID的登录缓存
        /// </summary>
        /// <param name="ID"></param>
        public static void RemoveUser(string ID)
        {
            //MvcCore.Extensions.CacheExtensions.ClearCache(cacheUserName + ID);

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie.Expires = DateTime.Now.AddMinutes(-30);
            cookie.Path = "/";

            HttpContext.Current.Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            _adminloginuser = null;
        }

        //判断当前访问是否有用户登录
        /// <summary>
        /// 判断当前访问是否有用户登录
        /// </summary>
        /// <returns></returns>
        public static bool CheckUserLogin()
        {
            return CurrentUser() != null;
            //var user = HttpContext.Current.Session[CookieName_User] as Data.AdminUser;
            //if (user != null)
            //{
            //    return true;
            //}
            //else
            //{
            //    if (HttpContext.Current.Request.Cookies[CookieName_User] != null && HttpContext.Current.Request.Cookies[CookieName_Tonen] != null)
            //    {
            //        string keyword = HttpContext.Current.Request.Cookies[CookieName_User].Value;
            //        string token = HttpContext.Current.Request.Cookies[CookieName_Tonen].Value;
            //        string pwd = token.Substring(32);
            //        var account = MvcCore.Unity.Get<Data.Service.IUserService>().Single(a => a.UserName == keyword.Trim() && a.Password == pwd);
            //        if (account != null) return true;
            //    }
            //}
            //return false;
        }

        //当前在线用户数量
        /// <summary>
        /// 当前在线用户数量
        /// </summary>
        public static int UserCount
        {
            get
            {
                return MvcCore.Extensions.CacheExtensions.GetAllCache().Where(s => s.StartsWith(prefixKey)).Count();
            }
        }
    }

    public static class AdminLoginHelper
    {
        //登录缓存键前缀
        /// <summary>
        /// 登录缓存键前缀
        /// </summary>
        static string cacheAdminName { get { return typeof(Data.AdminUser).Name + "_"; } }
        static string CookieName_AdUser = (ConfigHelper.GetConfigString("DBName") + "_AdUser").ToMD5();
        static string CookieName_AdTonen = (ConfigHelper.GetConfigString("DBName") + "_AdTonen").ToMD5();
        public static Data.AdminUser GetAdminLoginBy(string keyword, string password)
        {
            string pwd = password.ToMD5().ToMD5();
            var account = MvcCore.Unity.Get<Data.Service.IAdminUserService>().Single(a => a.AdminName == keyword.Trim() && a.Password == pwd);
            if (account != null)
            {
                //MvcCore.Extensions.CacheExtensions.SetCache(cacheAdminName + account.ID, account, MvcCore.Extensions.CacheTimeType.ByMinutes, 60);//写入缓存
                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_AdUser, keyword, 30);
                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_AdTonen, keyword.ToMD5() + "" + pwd, 30);
                return account;
            }
            return null;
        }
        private static string FormsCookieName { get { return FormsAuthentication.FormsCookieName + "admin"; } }
        //获取当前用户登录对象
        /// <summary>
        /// 获取当前用户登录对象
        /// <para>当没登陆或者登录信息不符时，这里返回 null </para>
        /// </summary>
        /// <returns></returns>
        public static Data.AdminUser CurrentAdmin()
        {
          
            if (!HttpContext.Current.Request.Cookies.AllKeys.Contains(FormsCookieName))
            {
                return null;
            }
            HttpCookie cookie = HttpContext.Current.Request.Cookies[FormsCookieName];
            FormsAuthenticationTicket ticket = null;
            if (cookie != null)
            {
                try
                {

                    ticket = FormsAuthentication.Decrypt(cookie.Value);
                    if (ticket == null)
                    {
                        return null;
                    }

                    FormsIdentity id = new FormsIdentity(ticket);
                    GenericPrincipal principal = new GenericPrincipal(id, new string[] { ticket.UserData });
                    HttpContext.Current.User = principal;//存到HttpContext.User中
                }
                catch
                {


                }
            }
            else
            {
                return null;
            }

            //客户端凭证验证不通过，要求重新登录
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
                return null;

             //客户端IP不一样，要求重新登录
           // var ticket = ((System.Web.Security.FormsIdentity)HttpContext.Current.User.Identity).Ticket;
            //if (ticket.UserData != HttpContext.Current.Request.ClientIP())
            //    return null;

            string key = CookieName_AdUser + ticket.Name;

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<AdminUser>(key);
            }
            else
            {
                int id = 0;
             
                if (!int.TryParse(ticket.Name,out id))
                {
                    return null;
                }

                var account = MvcCore.Unity.Get<Data.Service.IAdminUserService>().Single(id);
               
                if (account!=null)
                {
                    CacheExtensions.SetCache(key, account);
                }
              

                return account;

            }

            //return null;


            ////校验用户是否已经登录
            //var adminuser = HttpContext.Current.Session[CookieName_AdUser] as Data.AdminUser;
            //if (adminuser != null) return adminuser;
            //else
            //{
            //    if (HttpContext.Current.Request.Cookies[CookieName_AdUser] != null && HttpContext.Current.Request.Cookies[CookieName_AdTonen] != null)
            //    {
            //        string keyword = HttpContext.Current.Request.Cookies[CookieName_AdUser].Value;
            //        string token = HttpContext.Current.Request.Cookies[CookieName_AdTonen].Value;
            //        string pwd = token.Substring(32);
            //        var account = MvcCore.Unity.Get<Data.Service.IAdminUserService>().Single(a => a.AdminName == keyword.Trim() && a.Password == pwd);
            //        if (account != null) return account;
            //    }
            //}
            //return null;
        }

        //登出
        /// <summary>
        /// 登出
        /// </summary>
        public static void AdminUserLogout()
        {
            if (CheckAdminLogin())
            {
                HttpContext.Current.Session.Clear();


                HttpCookie hc = HttpContext.Current.Request.Cookies[FormsCookieName];
                if (hc!=null)
                {
                    hc.Expires = DateTime.Now.AddDays(-1);
                    hc.Path = "/";
                    HttpContext.Current.Response.Cookies.Add(hc);
                }
             

                HttpCookie hc1 = HttpContext.Current.Request.Cookies[CookieName_AdUser];
                if (hc1!=null)
                {
                    hc1.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Current.Response.Cookies.Add(hc1);
                }
             

                HttpCookie hc2 = HttpContext.Current.Request.Cookies[CookieName_AdTonen];
                if (hc2!=null)
                {
                    hc2.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Current.Response.Cookies.Add(hc2);
                }
                
            }
        }


        //判断当前访问是否有用户登录
        /// <summary>
        /// 判断当前访问是否有用户登录
        /// </summary>
        /// <returns></returns>
        public static bool CheckAdminLogin()
        {

            return CurrentAdmin() != null;

            ////校验用户是否已经登录
            //var adminuser = HttpContext.Current.Session[CookieName_AdUser] as Data.AdminUser;
            //if (adminuser != null)
            //{
            //    return true;
            //}
            //else
            //{
            //    if (HttpContext.Current.Request.Cookies[CookieName_AdUser] != null && HttpContext.Current.Request.Cookies[CookieName_AdTonen] != null)
            //    {
            //        string keyword = HttpContext.Current.Request.Cookies[CookieName_AdUser].Value;
            //        string token = HttpContext.Current.Request.Cookies[CookieName_AdTonen].Value;
            //        string pwd = token.Substring(32);
            //        var account = MvcCore.Unity.Get<Data.Service.IAdminUserService>().Single(a => a.AdminName == keyword.Trim() && a.Password == pwd);
            //        if (account != null) return true;
            //    }
            //}

            //return false;
        }
   
    }
}
