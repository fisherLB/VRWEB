using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JN.Data.Service;

namespace JN.Web.Areas.APP.Controllers
{
    public class QRCodeController : BaseController
    {   
        public ActionResult Android()
        {
            ActMessage = "扫码支付";
            ViewBag.Title = ActMessage;
            return View();
        }
    }
}
