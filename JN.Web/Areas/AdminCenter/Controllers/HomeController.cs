using JN.Data.Service;
using JN.Services.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class HomeController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 6000).ToList();

        //
        // GET: /AdminCenter/Index/
        public ActionResult Index()
        {
            //var userlist = MvcCore.Unity.Get<IUserService>().List(x => x.R3001 == null || x.R3001 == "").ToList();
            //foreach (var item in userlist)
            //{
            //    var r3001 = Users.GetRandomString(33, true, true, true, false, "1");
            //    item.R3001 = r3001;
            //    MvcCore.Unity.Get<IUserService>().Update(item);
            //    MvcCore.Unity.Get<ISysDBTool>().Commit();
            //}

            return View();
        }

        public ActionResult NoAuthority()
        {
            return View();
        }

        //退出
        public ActionResult Logout()
        {
            ActMessage = "退出系统";
            Services.AdminLoginHelper.AdminUserLogout();
            return Redirect(Url.Action("Index", "Login"));
        }

        public ActionResult ClearAllCache()
        {
            List<String> caches = MvcCore.Extensions.CacheExtensions.GetAllCache();
            foreach (var cachename in caches)
                MvcCore.Extensions.CacheExtensions.ClearCache(cachename);
            List<String> caches2 = Services.Tool.DataCache.GetAllCache();
            foreach (var cachename in caches2)
                Services.Tool.DataCache.ClearCache(cachename);
            Services.AdminLoginHelper.AdminUserLogout();
            Stocks.ClearAllTradeCache();//清币种缓存
            Stocks.HeavyLoad();//清币种参数缓存
            Bonus.HeavyLoad();//清参数缓存
            Finance.HeavyLoad();//清参数缓存
            Users.HeavyLoad();//清参数缓存
            Wallets.HeavyLoad();//清参数缓存
            return Redirect(Url.Action("Index", "Login"));
        }

        public ActionResult ChangePassword()
        {
            ActMessage = "修改密码";
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            string oldpassword = form["oldpassword"];
            string newpassword = form["newpassword"];
            string connewpassword = form["newpassword2"];
            string strErr = "";

            if (oldpassword.Trim().Length == 0 || newpassword.Trim().Length == 0)
                strErr += "原密码不能为空 <br />";

            if (newpassword != connewpassword)
                strErr += "新密码与确认密码不相符 <br />";


            if (Amodel.Password != oldpassword.ToMD5().ToMD5())
                strErr += "原密码不正确 <br />";
            if (strErr != "")
            {
                ViewBag.ErrorMsg = "抱歉您填写的信息有误： <br />" + strErr + "请核实后重新提交！";
                return View("Error");
            }

            Amodel.Password = newpassword.ToMD5().ToMD5();
            MvcCore.Unity.Get<IAdminUserService>().Update(Amodel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
            ActMessage = "密码修改成功";
            return View("Success");
        }
    }
}