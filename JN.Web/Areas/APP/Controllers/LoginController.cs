using JN.Data;
using JN.Data.Enum;
using JN.Data.Extensions;
using JN.Data.Service;
using JN.Services;
using JN.Services.CustomException;
using JN.Services.Manager;
using JN.Services.Tool;
using System;
using System.Data.Entity.Validation;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections.Generic;
using System.Linq;

namespace JN.Web.Areas.APP.Controllers
{
    public class LoginController : Controller
    {
        private static List<JN.Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public LoginController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService)
        {    
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
        }
        public ActionResult Index()
        {

            ViewBag.Title = "登录系统";
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sys.IsOpenUp)
            {
                Response.Write(sys.CloseHint);
                Response.End();
            }

            return View();
        }

        [HttpPost]
        public JsonResult Index(string username, string password, string lang, string code, string mobileCode)
        {
            string oldurl = Request["oldUrl"];
            string vcode = (Session["UserValidateCode"] ?? "").ToString();
            Session.Remove("UserValidateCode");

            ReturnResult result = new ReturnResult();
            try
            {
                //if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                //    throw new CustomException("图形验证码错误，请重新输入！");

                //string vmobilecode = (Session["SMSValidateLogin"] ?? "").ToString();
                //if (Session["TryLoginUsers"] != null && Session["TryLoginUsers"].ToString() == username)
                //{
                //    if (string.IsNullOrEmpty(vmobilecode) || string.IsNullOrEmpty(mobileCode) || !vmobilecode.Equals(mobileCode, StringComparison.InvariantCultureIgnoreCase))
                //        throw new CustomException("手机验证码错误");
                //}

                if (string.IsNullOrEmpty(username) | string.IsNullOrEmpty(password))
                    throw new CustomException("用户名或密码不能为空");
                UserLoginHelper.UserLogout();
                var entity = UserLoginHelper.GetUserLoginBy(username, password);
                if (entity != null)
                {
                    if (entity.IsLock) throw new CustomException("您的帐号已被冻结,请联系你的推荐人!");
                    if (!entity.IsActivation) throw new CustomException("你的账号未激活，请联系你的推荐人!");
                    var log = new ActLog();
                    log.ActContent = "用户“" + username + "”登录成功！";
                    log.CreateTime = DateTime.Now;
                    log.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        log.Location = Request.UrlReferrer.ToString();
                    log.Platform = "用户";
                    log.Source = "";// JN.Services.Tool.StringHelp.IPGetCity(Request.UserHostAddress);
                    log.UID = entity.ID;
                    log.UserName = entity.UserName;
                    ActLogService.Add(log);

                    //更新最后一次登录时间 
                    entity.LastLoginTime = DateTime.Now;
                    entity.LastLoginIP = Request.UserHostAddress;
                    UserService.Update(entity);

                    SysDBTool.Commit();

                    //每天登录奖励
                    //if (entity.IsActivation)
                    //{
                    //    if (MvcCore.Unity.Get<IBonusDetailService>().List(x => x.UID == entity.ID && x.BonusID == 1106 && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Count() <= 0)
                    //    {
                    //        decimal PARAM_SDJJ = MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 1106).Value.ToDecimal();
                    //        Bonus.UpdateUserWallet(PARAM_SDJJ, 1106, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 1106).Name, "Fc生态系统活跃度生成奖金", entity.ID, "Addup1106", true);
                    //    }
                    //}

                    result.Status = 200;
                    if (entity.IsActivation)
                        result.Message = oldurl ?? "/app/home/index?lang=" + lang;
                    else
                        result.Message = oldurl ?? "/app/home/index?lang=" + lang;

                    //如果勾选记住密码，则保存密码一个星期
                    //DateTime expiration = DateTime.Now.AddMinutes(20);
                    ////if (rp == "1")
                    ////    expiration = DateTime.Now.AddDays(7);

                    //// 设置Ticket信息
                    //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    //    1,
                    //    entity.ID.ToString(),
                    //    DateTime.Now,
                    //    expiration,
                    //    false, LoginInfoType.User.ToString());

                    //// 加密验证票据
                    //string strTicket = FormsAuthentication.Encrypt(ticket);

                    //// 使用新userdata保存cookie
                    //HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strTicket);
                    //cookie.Expires = ticket.Expiration;
                    //this.Response.Cookies.Add(cookie);
                }
                else
                {
                    result.Status = 201;
                    Session["TryLoginUsers"] = username;
                    throw new CustomException("用户名或密码错误");
                }
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
           

        #region  生成验证码

        /// <summary>
        /// 生成验证码（注册）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCodeImage()
        {
            JN.Services.Tool.CodeRend cd = new JN.Services.Tool.CodeRend();
            string code = StringHelp.GettRandomCode(4);//4位随机数字、字母// cd.CreateVerifyCode(4);
            Session["UserValidateCode"] = code.ToLowerInvariant();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.Drawing.Bitmap image = cd.CreateImageCode(code);
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return File(ms.GetBuffer(), "image/JPEG");
        }
        #endregion


        public ActionResult getip()
        { 
            string ip=Request["ip"];
           string s= JN.Services.Tool.StringHelp.IPGetCity(ip);
           return Content(s);
        }
        #region 找回密码
        /// <summary>
        /// 找回密码第一步
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPwd()
        {
            ViewBag.Title = "找回密码";
            var sys = MvcCore.Unity.Get<ISysSettingService>().ListCache("sysSet").FirstOrDefault();

            if (!sys.IsOpenUp)
            {
                Response.Write(sys.CloseHint);
                Response.End();
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetPwd(FormCollection fc)
        {
            Result result = new Result();
            result = Users.DoGetPwd(fc, UserService, SysDBTool, true);
            return Json(result);
        }

        /// <summary>
        /// 找回密码第二步
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPwd2()
        {
            ViewBag.Title = "找回密码";
            return View();
        }

        [HttpPost]
        public JsonResult GetPwd2(FormCollection fc)
        {
            Result result = new Result();
            result = Users.DoGetPwd2(fc, UserService, SysDBTool, true);//跳转app
            return Json(result);
        }

        /// <summary>
        /// 找回密码短信验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult SendMobileGetPassword()
        {
            Result result = new Result();
            string mobile = Request["mobile"];
            result = Users.SendRegOrPWMobileMsm(mobile, "GetPwd", true);
            return Json(result);
        }
        #endregion


        #region 异步登录入口   //与A系统对接登陆
        //[HttpPost]
        public ActionResult LoginIndex(string username, string token)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (string.IsNullOrEmpty(username) | string.IsNullOrEmpty(token))
                    throw new CustomException("操作超时");
                JN.Data.User model = MvcCore.Unity.Get<IUserService>().Single(x => x.UserName == username); //accountrelations.GetModel("RelationUserName='" + username + "' and Status=100");
                if (model == null) throw new CustomException("未绑定用户系统账号，请登陆后立即绑定！");
                //{
                //    ViewBag.Title = "未绑定账号！";
                //    ViewBag.msg = "未绑定用户系统账号，请登陆后立即绑定！";
                //    ViewBag.url = "/user/login";
                //    return View("msg");
                //}
                string bjpassword = (model.UserName + model.ID + DateTime.Now.ToString("yyyy-MM-dd HH:mm")).ToMD5().ToMD5();
                //  logs.WriteLog("RelationUserName:" + model.RelationUserName + ",RelationUID:" + model.RelationUID+",bjpassword:"+bjpassword);
                if (token != bjpassword)// return Redirect("/User/Login");
                {
                    //ViewBag.Title = "登陆账号或密码不正确！";
                    //ViewBag.msg = "登陆账号或密码不正确！";
                    //ViewBag.url = "/usercenter/login";
                    //return View("Error");
                    return Redirect("/app/login");
                }

                var entity = JN.Services.UserLoginHelper.GetUserLoginBy_To(username, model.Password);
                if (entity == null) //return Redirect("/User/Login");
                {
                    //  logs.WriteLog("onuser == null");
                    //ViewBag.Title = "登陆账号或密码不正确！";
                    //ViewBag.msg = "登陆账号或密码不正确！";
                    //ViewBag.url = "/usercenter/login";
                    return Redirect("/app/login");
                    //return View("Error");
                }
                if (entity.IsLock) throw new CustomException("您的帐号已被冻结,请联系你的推荐人!");
                if (!entity.IsActivation) throw new CustomException("你的账号未激活，请联系你的推荐人!");
                var log = new ActLog();
                log.ActContent = "用户“" + username + "”登录成功！";
                log.CreateTime = DateTime.Now;
                log.IP = Request.UserHostAddress;
                if (Request.UrlReferrer != null)
                    log.Location = Request.UrlReferrer.ToString();
                log.Platform = "用户";
                log.Source = JN.Services.Tool.StringHelp.IPGetCity(Request.UserHostAddress);
                log.UID = entity.ID;
                log.UserName = entity.UserName;
                ActLogService.Add(log);

                //更新最后一次登录时间 
                entity.LastLoginTime = DateTime.Now;
                entity.LastLoginIP = Request.UserHostAddress;
                UserService.Update(entity);

                SysDBTool.Commit();

                //每天登录奖励
                //if (entity.IsActivation)
                //{
                //    if (MvcCore.Unity.Get<IBonusDetailService>().List(x => x.UID == entity.ID && x.BonusID == 1106 && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Count() <= 0)
                //    {
                //        decimal PARAM_SDJJ = MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 1106).Value.ToDecimal();
                //        Bonus.UpdateUserWallet(PARAM_SDJJ, 1106, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 1106).Name, "Fc生态系统活跃度生成奖金", entity.ID, "Addup1106", true);
                //    }
                //}

                result.Status = 200;
                //if (entity.IsActivation)
                result.Message = "/app/home";
                //else
                //result.Message = oldurl ?? "/usercenter/home/index?lang=" + lang;

                //如果勾选记住密码，则保存密码一个星期
                DateTime expiration = DateTime.Now.AddMinutes(20);
                //if (rp == "1")
                //    expiration = DateTime.Now.AddDays(7);

                // 设置Ticket信息
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    entity.ID.ToString(),
                    DateTime.Now,
                    expiration,
                    false, LoginInfoType.User.ToString());

                // 加密验证票据
                string strTicket = FormsAuthentication.Encrypt(ticket);

                // 使用新userdata保存cookie
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strTicket);
                cookie.Expires = ticket.Expiration;
                this.Response.Cookies.Add(cookie);
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Redirect("/app/home");
            //return Json(result);
        }

        #endregion

        /// <summary>
        /// 登录验证码密码
        /// </summary>
        /// <returns></returns>
        public ActionResult SendMobileLogin()
        {
            Result result = new Result();
            string phone = Request["phone"];
            string username = Request["username"];
            string countrycode = Request["countrycode"];//国家区号
            result = SMSValidateCode.SendMobileLoginMsm(countrycode, phone, username, UserService, cacheSysParam);
            return Json(result);
        }
    }
}
