using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Controllers
{
    public class ServiceController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "客服中心";
           return View();
        }

        public ActionResult ServiceCenter()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PostMessage(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string recipient = "管理员";
                string formuser = form["formuser"];
                string email = form["email"];
                string phone = form["phone"];
                string content = form["content"];
                if (string.IsNullOrEmpty(email)) throw new Exception("请填写您的联系邮箱");
                var model = new Data.Message();
                model.Attachment = "";
                model.MessageType = "";
                model.Content = content;
                model.CreateTime = DateTime.Now;
                model.FormUID = -1;
                model.FormUserName = "游客：" + formuser;
                model.ForwardID = 0;
                model.IsFlag = false;
                model.IsForward = false;
                model.IsRead = false;
                model.IsReply = false;
                model.IsSendSuccessful = true;
                model.IsStar = false;
                model.ReplyID = 0;
                model.Title = "游客留言";
                model.ToUID = 0;
                model.ToUserName = recipient.Trim();
                model.UID = -1;
                MvcCore.Unity.Get<JN.Data.Service.IMessageService>().Add(model);
                MvcCore.Unity.Get<Data.Service.SysDBTool>().Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                 result.Message = StringHelp.FormatErrorString(ex.Message);
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
    }
}