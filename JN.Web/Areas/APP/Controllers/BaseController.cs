using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System.Collections.Specialized;
using System.Web.Security;

namespace JN.Web.Areas.APP.Controllers
{
    public class BaseController : Controller
    {
        public Data.User Umodel { get; set; }
        public object ActPacket { get; set; }   
        public string ActMessage { get; set; }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sys.IsOpenUp)
            {
                //Response.Write(sys.CloseHint);
                //Response.End();
                Response.Redirect("/Home/NotFound?msg=" + HttpUtility.UrlEncode(sys.CloseHint.Replace("\r", "").Replace("\n", "").Trim(), System.Text.Encoding.UTF8));
                filterContext.Result = new EmptyResult();
                return;
            }

            //int UnOperateTime = ConfigHelper.GetConfigInt("UnOperateTime");
            //string StartUnOperateTime = ConfigHelper.GetConfigString("StartUnOperateTime");
            //DateTime fulltime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + StartUnOperateTime);
            //int diff = Math.Abs(DateTimeDiff.DateDiff_Sec(fulltime, DateTime.Now));

            //if (diff < UnOperateTime * 60)
            //{
            //    Response.Write("很抱歉，现在是系统结算时间，预计还需要" + (UnOperateTime - (diff / 60)) + "分钟，请稍候再访问！");
            //    Response.End();
            //}

            //设置语言版本
            string lang = Request["lang"];
            if (!string.IsNullOrEmpty(lang))
            {
                Services.Resource.ResourceProvider.Culture = lang;
            }

            //超管登录
            string adminloginname = Request["adminloginname"];
            string isLogined = Request["isLogined"];
            if (!string.IsNullOrEmpty(adminloginname)&&string.IsNullOrWhiteSpace(isLogined))
            {
                var adminEntity = Services.AdminLoginHelper.CurrentAdmin();
                if (adminEntity != null && adminEntity.IsPassed)
                {
                    var userEntity = MvcCore.Unity.Get<IUserService>().Single(x => x.UserName == adminloginname);
                    if (userEntity != null)
                    {
                        var newUserEntity = Services.UserLoginHelper.GetSysLoginBy(userEntity, adminEntity);               
                        Response.Redirect("/"+ JN.Services.Tool.ConfigHelper.GetConfigString("AdminPath") + "/login/AutoLoginUser?url="+Server.UrlEncode(Request.Url.ToString()+ "&isLogined=1"));
                        filterContext.Result = new EmptyResult();
                        return;
                    }
                }
                else
                {
                    Response.Redirect("/App/Login");
                    filterContext.Result = new EmptyResult();
                    return;
                }
            }
            //校验用户是否已经登录
            var model = Services.UserLoginHelper.CurrentUser();
            if (model != null)
            {
                Umodel = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(model.ID);

                string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
                string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();

                string[] needactivecontroller = { "trade", "cfb", "mail" };
                string[] needactiveaction = { "applytransfer" };

                if (!Umodel.IsActivation && (needactivecontroller.Contains(controllerName) || needactiveaction.Contains(actionName)))
                {
                    Response.Redirect("/App/User/doPass");
                    filterContext.Result = new EmptyResult();
                    return;
                }

                if (Umodel.IsLock)
                {
                    Services.UserLoginHelper.UserLogout();
                    Response.Redirect("/App/Login");
                    filterContext.Result = new EmptyResult();
                    return;
                }

                //    string loginmodel = System.Web.HttpContext.Current.Session[ConfigHelper.GetConfigString("DBName") + "LOGINMODE"] as String;
                //    if (loginmodel != "adminlogin")  //从后台登录用户时不需要验证
                //    {
                //        //需要验证的模块
                //        if (userverifys.Exists("ControllerName='" + controllerName + "' and ActionName='" + actionName + "' and VerifyLevel > 1"))
                //        {
                //            TUserVerify userVerify = userverifys.GetModel("ControllerName='" + controllerName + "' and ActionName='" + actionName + "'");

                //            if (userVerify.VerifyLevel == 2 && Session["enpwd2"] == null)
                //            {
                //                if (controllerName != "enterpassword")
                //                {
                //                    Session["enterpwd_pl"] = userVerify.VerifyLevel;
                //                    Session["enterpwd_url"] = Request.Url.PathAndQuery;
                //                    Response.Redirect("/User/EnterPassword");
                //                    filterContext.Result = new EmptyResult();
                //                }
                //            }

                //            if (userVerify.VerifyLevel == 3 && Session["enpwd3"] == null)
                //            {
                //                if (controllerName != "enterpassword")
                //                {
                //                    Session["enterpwd_pl"] = userVerify.VerifyLevel;
                //                    Session["enterpwd_url"] = Request.Url.PathAndQuery;
                //                    Response.Redirect("/User/EnterPassword");
                //                    filterContext.Result = new EmptyResult();
                //                }
                //            }
                //        }
                //    }

                //未认证的用户跳转到认证页面 by Annie ：2017.07.19
                //if (!(Umodel.IsAuthentication ?? false) && (controllerName != "user" && actionName != "nameauth") && controllerName != "icomanage" && controllerName!="home")
                //{
                //    Response.Write(@"<script>window.top.location.href ='/App/user/nameauth'</script>");
                //    filterContext.Result = new EmptyResult();
                //}

            }
            else
            {
                string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
                if (controllerName == "icomanage") { Response.Redirect("/ico/sign_in"); }
                else
                {
                    Response.Redirect("/App/Login");
                    filterContext.Result = new EmptyResult();
                    return;
                }
               
            }
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            ViewBag.Title = ActMessage;

            if (ActMessage != "修改一级密码提交" && ActMessage != "修改交易密码提交")
            {
                var model = Services.UserLoginHelper.CurrentUser();
                if (model != null)
                {
                    Umodel = model;
                    if (!string.IsNullOrEmpty(ActMessage))
                    {
                        string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
                        string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();

                        //if (Services.Tool.ConfigHelper.GetConfigBool("DevelopMode"))
                        //{
                        //    if (MvcCore.Unity.Get<JN.Data.Service.IUserVerifyService>().List(x => x.ControllerName == controllerName && x.ActionName == actionName).Count() <=0)
                        //        MvcCore.Unity.Get<JN.Data.Service.IUserVerifyService>().Add(new Data.UserVerify { ActionName = actionName, Remark = "", VerifyLevel = 1, ControllerName = controllerName, LastUpdateTime = DateTime.Now, Name = ActMessage });
                        //}

                        //记录操作日志，写进操作日志中
                        var log = new Data.ActLog();
                        log.ActContent = ActMessage;
                        log.CreateTime = DateTime.Now;
                        log.IP = Request.UserHostAddress;
                        if (Request.UrlReferrer != null)
                            log.Location = Request.UrlReferrer.ToString();
                        //if (ActPacket != null)
                        //    log.PacketFile = Newtonsoft.Json.JsonConvert.SerializeObject(ActPacket);
                        //else
                        //    log.PacketFile = "";
                        log.Platform = "用户";
                        log.Source = "";
                        log.UID = model.ID;
                        log.UserName = model.UserName;
                        MvcCore.Unity.Get<JN.Data.Service.IActLogService>().Add(log);

                        if (ViewBag.mFormUrl != null)
                            ViewBag.FormUrl = ViewBag.mFormUrl;
                        else
                        {
                            if (Request.UrlReferrer != null)
                                ViewBag.FormUrl = Request.UrlReferrer.ToString();
                        }
                    }
                }
                else
                {
                    Response.Redirect("/App/Login");
                    filterContext.Result = new EmptyResult();
                }
            }
        }

        protected NameValueCollection FormatQueryString(NameValueCollection nameValues)
        {
            NameValueCollection nvcollection = new NameValueCollection();
            foreach (var item in nameValues.AllKeys)
            {
                if (item.Equals("df"))
                {                   
                    if (!String.IsNullOrEmpty(nameValues["dateform"]) && !String.IsNullOrEmpty(nameValues["dateto"]))
                    {   
                        DateTime startDate = DateTime.Parse(nameValues["dateform"]).AddSeconds(-1);
                        DateTime endDate = DateTime.Parse(nameValues["dateto"]).ToDateTime().AddDays(1);
                        nvcollection.Add(nameValues[item] + ">", startDate.ToString());
                        nvcollection.Add(nameValues[item] + "<", endDate.ToString());
                    }
                }
                //else if (item.Equals("nf"))
                //{
                //    if (!String.IsNullOrEmpty(nameValues["nf"]) && !String.IsNullOrEmpty(nameValues["numberto"]))
                //    {
                //        int startNumber = nameValues["nf"].ToInt() - 1;
                //        int endNumber = nameValues["nt"].ToInt() + 1;
                //        nvcollection.Add(nameValues[item] + ">", startNumber.ToString());
                //        nvcollection.Add(nameValues[item] + "<", endNumber.ToString());
                //    }
                //}
                else if (item.Equals("kf"))
                {
                    if (!string.IsNullOrEmpty(nameValues["kv"]))
                        nvcollection.Add(nameValues[item], nameValues["kv"]);
                }
                else if (item.Equals("isasc") || !string.IsNullOrEmpty(nameValues["isasc"]))
                    nvcollection.Add(item, nameValues["isasc"]);
                else if (item.Equals("orderfiled") || !string.IsNullOrEmpty(nameValues["orderfiled"]))
                    nvcollection.Add(item, nameValues["orderfiled"]);
            }

            return nvcollection;
        }
        public ActionResult showmsg(string msg)
        {
            ViewBag.Title = "提示！";
            ViewBag.url = "/app/home";
            if (Request.UrlReferrer != null)
                ViewBag.url = Request.UrlReferrer.ToString();
            ViewBag.msg = msg;
            return View("msg");
        }
    }
}
