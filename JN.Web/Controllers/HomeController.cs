using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace JN.Web.Controllers
{
    public class HomeController : Controller
    {
      
        public ActionResult Index()
        {           
            //设置语言版本
            string lang = Request["lang"];
            if (!string.IsNullOrEmpty(lang))
            {
                Services.Resource.ResourceProvider.Culture = lang;
            }
            ViewBag.Title = "网站首页";
            return Redirect("/AdminCenter/Login");
            //  return Redirect("/App/home");
            //return Redirect("/APP/Home/Video");
            //return View("Video");
        }
     

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }
    }
}