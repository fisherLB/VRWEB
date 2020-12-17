using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JN.Data.Service;
using JN.Services.Tool;
using System.Text.RegularExpressions;
using JN.Services.Manager;
using System.Data.Entity.Validation;
using JN.Services.CustomException;
using JN.Data.Extensions;

namespace JN.Web.Areas.APP.Controllers
{
    public class RegController : Controller
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public RegController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
        }
        public ActionResult Index()
        {
            ViewBag.Title = "注册用户";
            string username = Request["rename"];
            var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sysEntity.IsRegisterOpen)
            {
                Response.Write(sysEntity.CloseRegisterHint);
                Response.End();
            }
            //ViewBag.ID = Users.GetUserName().ToString();
            if (!string.IsNullOrEmpty(username))
                ViewBag.username = username;
            return View();
        }


        /// <summary>
        /// 邀请用户
        /// </summary>
        /// <returns></returns>
        public ActionResult invite()
        {
            string reusername = Request["rename"];
            if(string.IsNullOrEmpty(reusername))
            {
                return Redirect("/home/index");
            }
           var  rename= UserService.Single(x => x.UserName == reusername);
           if (rename != null)
           {
               return View();
           }

           return Redirect("/home/index");
        }

        #region 添加用户

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        
        [HttpPost]
        public ActionResult index(FormCollection fc)
        {
            string vcode = (Session["UserValidateCode"] ?? "").ToString();
            Session.Remove("UserValidateCode");
             Result r = new Result();
            var entity = new Data.User();
             TryUpdateModel(entity, fc.AllKeys);
            //var user = UserService.List().OrderBy(i => i.CreateTime).First();//获取最早注册的用户
            //entity.RefereeID = user.ID;
            //entity.RefereeUser = user.UserName;
            r = Users.AddUser(fc, UserService, entity);
            
              return Json(r);
        }

        #endregion

        #region 发送注册账户手机验证码
        /// <summary>
        /// 发送注册账户手机验证码
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public ActionResult SendRegMobileMsm()
        {
            Result result = new Result();
            string phone = Request["myphone"];
            string countrycode = Request["country_code"];//国家区号
            result = SMSValidateCode.SendRegMobileMsm(phone, countrycode, cacheSysParam);
            return Json(result);
        }
        #endregion

        #region 下载app页面
        public ActionResult Download()
        {
            ViewBag.Title = "下载app页面";
            return View();
        }
        #endregion

    }
}
