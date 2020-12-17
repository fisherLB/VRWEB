using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using JN.Services.Tool;
using System.Text;
using System.Data.Entity.SqlServer;
using JN.Services.Manager;
using JN.Data.Extensions;
using JN.Services.CustomException;
using System.Data.Entity.Validation;
using System.IO;

namespace JN.Web.Areas.APP.Controllers
{
 
    public class HomeController : BaseController
    {
        #region 初始化，构造函数
        private readonly IUserService UserService;
        private readonly ISysSettingService SysSettingService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        public HomeController(ISysDBTool SysDBTool, 
            IUserService UserService,
            ISysSettingService SysSettingService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysSettingService = SysSettingService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
        }

        #endregion
        #region 网站首页，修改登录密码，交易密码，网站退出

        #region 网站首页
        /// <summary>
        /// 网站首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = "首页";
            ActMessage = ViewBag.Title;
            return View();
        }
        #endregion

        #region 视频
        public ActionResult Video()
        {
            ViewBag.Title = "视频";
            logs.WriteLog("视频");
            return View();
        }
        #endregion

        #region 安全退出


        //退出
        public ActionResult Logout()
        {
            ActMessage = "用户退出";
            Services.UserLoginHelper.UserLogout();
            return Redirect("/App/Login");
        }
        #endregion

        #region 修改一级密码

        /// <summary>
        /// 修改一级密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            ActMessage = "修改一级密码";
            ViewBag.Title = ActMessage;
            return View();
        }

        /// <summary>
        /// 修改登录密码提交
        /// </summary>
        /// <param name="form">表单</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            ActMessage = "修改登录密码提交";
            ViewBag.Title = ActMessage;
            ReturnResult result = new ReturnResult();
            try
            {
                string oldpassword = form["oldpassword"];
                string newpassword = form["newpassword"];
                string connewpassword = form["connewpassword"];
                // string password2 = form["password2"];
                if (oldpassword.Trim().Length == 0 || newpassword.Trim().Length == 0)
                    throw new CustomException("原登录密码、新登录密码不能为空");

                if (newpassword != connewpassword)
                    throw new CustomException("新登录密码与确认密码不相符");
                if (Umodel.Password != oldpassword.ToMD5().ToMD5())
                    throw new CustomException("原登录密码不正确");

                //if (Umodel.Password2 != password2.ToMD5().ToMD5())
                //    throw new CustomException("二级密码不正确");

                Umodel.Password = newpassword.ToMD5().ToMD5();
                UserService.Update(Umodel);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 修改二级密码

        /// <summary>
        /// 修改二级密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword2()
        {
            ActMessage = "修改交易密码";
            return View();
        }


        /// <summary>
        /// 修改交易密码提交
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangePassword2(FormCollection form)
        {
            ActMessage = "修改交易密码提交";
            ReturnResult result = new ReturnResult();
            try
            {
                string oldpassword2 = form["oldpassword2"];
                string newpassword2 = form["newpassword2"];
                string connewpassword2 = form["connewpassword2"];
                string password = form["password"];
                if (oldpassword2.Trim().Length == 0 || newpassword2.Trim().Length == 0)
                    throw new CustomException("原交易密码、新交易密码不能为空");

                if (newpassword2 != connewpassword2)
                    throw new CustomException("新交易密码与确认密码不相符");

                if (Umodel.Password2 != oldpassword2.ToMD5().ToMD5())
                    throw new CustomException("原交易密码不正确");
                if (Umodel.Password != password.ToMD5().ToMD5())
                    throw new CustomException("登录密码不正确");
                Umodel.Password2 = newpassword2.ToMD5().ToMD5();
                UserService.Update(Umodel);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion
        #endregion
    } 
 



}
