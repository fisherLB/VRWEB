using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using JN.Services.Tool;
using System.IO;
using Webdiyer.WebControls.Mvc;
using JN.Services.CustomException;
using JN.Services.Manager;

namespace JN.Web.Areas.APP.Controllers
{
    [ValidateInput(false)]
    public class MailController : BaseController
    {
        private readonly IUserService UserService;
        private readonly IMessageService MessageService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public MailController(ISysDBTool SysDBTool,
            IMessageService MessageService,
            IUserService UserService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.MessageService = MessageService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
        }

        public ActionResult Inbox(string key, int? page)
        {
            ActMessage = "收件箱";
            var list = MessageService.List(x => x.UID == Umodel.ID && x.ToUID == Umodel.ID).OrderByDescending(x => x.ID);

            if (Request.IsAjaxRequest())
                return View("_Message", list.ToPagedList(page ?? 1, 20));

            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Sent(string key, int? page)
        {
            ActMessage = "发件箱";
            var list = MessageService.List(x => x.UID == Umodel.ID && x.FormUID == Umodel.ID && x.IsSendSuccessful).OrderByDescending(x => x.ID);

            if (Request.IsAjaxRequest())
                return View("_SentMessage", list.ToPagedList(page ?? 1, 20));

            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Draft(string key, int? page)
        {
            ActMessage = "草稿箱";
            var list = MessageService.List(x => x.UID == Umodel.ID && x.FormUID == Umodel.ID && !x.IsSendSuccessful).OrderByDescending(x => x.ID);
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult MessageList()
        {
            return View();
        }
        public ActionResult GetMessageList(int? page)
        {
            //var list = MessageService.List(x => x.UID == Umodel.ID && (x.FormUID == Umodel.ID || x.ToUID == Umodel.ID) && x.IsSendSuccessful).OrderByDescending(x => x.ID);
            var list = MessageService.List(x => x.UID == Umodel.ID && x.ToUID == Umodel.ID && x.IsSendSuccessful).OrderByDescending(x => x.ID);
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Write()
        {
            ViewBag.recipient = "Admin";
            if (!string.IsNullOrEmpty(Request["r"]))
            {
                var model = MessageService.Single(Request["r"].ToInt());
                if (model != null)
                {
                    ViewBag.recipient = model.FormUserName;
                    ViewBag.subject = "回复：" + model.Title;
                    ViewBag.content = "\n\n\n\n\n----------------------------原文---------------------------- \n" + model.Content;
                }
            }

            if (!string.IsNullOrEmpty(Request["f"]))
            {
                var model = MessageService.Single(Request["f"].ToInt());
                if (model != null)
                {
                    ViewBag.recipient = "";
                    ViewBag.subject = "转发：" + model.Title;
                    ViewBag.content = "\n\n\n\n\n----------------------------原文---------------------------- \n" + model.Content;
                }
            }

            ActMessage = "写邮件";
            return View();
        }

        [HttpPost]
        public ActionResult Write(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("MailWrite" + Umodel.ID))
                {
                    throw new CustomException("请稍后提交");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("MailWrite" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                string recipient = "Admin";// form["recipient"];
                string subject = form["messagetype"];// form["subject"];
                string content = form["content"];
                string messagetype = form["messagetype"];
                if (string.IsNullOrEmpty(subject)) throw new CustomException("请选择反馈问题类型");
                if (string.IsNullOrEmpty(content)) throw new CustomException("请描述一下您的问题，最少输入5个字");
                //if (string.IsNullOrEmpty(recipient) || string.IsNullOrEmpty(subject)) throw new CustomException("收件人、标题不能为空");
                int toUID = 0;
                if (recipient == "Admin") recipient = "管理员";
                if (recipient.Trim() != "管理员")
                {
                    if (UserService.List(x => x.UserName.Equals(recipient.Trim())).Count() <= 0)
                         throw new CustomException("收件人不存在");
                    else
                        toUID = UserService.Single(x => x.UserName.Equals(recipient.Trim())).ID;

                }
                subject = StringHelp.FilterSqlHtml(subject);//过滤敏感字符串
                content = StringHelp.FilterSqlHtml(content);//过滤敏感字符串

                HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (file != null)
                {
                    if (!string.IsNullOrEmpty(file.FileName))
                    {
                        if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                        {
                            throw new CustomException("非法上传，您只可以上传图片格式的文件！");
                        }

                        //20160711安全更新 ---------------- start
                        var newfilename = "MAIL_" + Umodel.UserName + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                        string filePath = string.Format("/Upload/mail/{0}/", Umodel.ID);
                        if (!Directory.Exists(Request.MapPath(filePath)))
                            Directory.CreateDirectory(Request.MapPath(filePath));

                        if (Path.GetExtension(file.FileName).ToLower().Contains("aspx"))
                        {
                            var wlog = new Data.WarningLog();
                            wlog.CreateTime = DateTime.Now;
                            wlog.IP = Request.UserHostAddress;
                            if (Request.UrlReferrer != null)
                                wlog.Location = Request.UrlReferrer.ToString();
                            wlog.Platform = "用户";
                            wlog.WarningMsg = "试图上传木马文件";
                            wlog.WarningLevel = "严重";
                            wlog.ResultMsg = "拒绝";
                            wlog.UserName = Umodel.UserName;
                            MvcCore.Unity.Get<IWarningLogService>().Add(wlog);

                            Umodel.IsLock = true;
                            Umodel.LockTime = DateTime.Now;
                            Umodel.LockReason = "试图上传木马文件";
                            MvcCore.Unity.Get<IUserService>().Update(Umodel);
                            MvcCore.Unity.Get<ISysDBTool>().Commit();
                            throw new Exception("试图上传木马文件，您的帐号已被冻结");
                        }

                        var fileName = Path.Combine(Request.MapPath(filePath), newfilename);
                        try
                        {
                            file.SaveAs(fileName);
                            var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath(filePath), 1024, 768, "EQU");
                            imgurl = filePath + thumbnailfilename;
                        }
                        catch (Exception ex)
                        {
                            throw new CustomException("图片上传失败：" + ex.Message);
                        }
                        finally
                        {
                            System.IO.File.Delete(fileName); //删除原文件
                        }
                        //20160711安全更新  --------------- end
                    }
                }
               
                var model = new Data.Message();
                model.Attachment = imgurl;
                model.MessageType = messagetype;
                model.Content = content;
                model.CreateTime = DateTime.Now;
                model.FormUID = Umodel.ID;
                model.FormUserName = Umodel.UserName;
                model.ForwardID = 0;
                model.IsFlag = false;
                model.IsForward = false;
                model.IsRead = false;
                model.IsReply = false;
                model.IsSendSuccessful = true;
                model.IsStar = false;
                model.ReplyID = 0;
                model.Title = subject;
                model.ToUID = toUID;
                model.ToUserName = recipient.Trim();
                model.UID = Umodel.ID;

                var model2 = new Data.Message();
                model2.Attachment = model.Attachment;
                model2.MessageType = model.MessageType;
                model2.Content = model.Content;
                model2.CreateTime = model.CreateTime;
                model2.FormUID = model.FormUID;
                model2.FormUserName = model.FormUserName;
                model2.ForwardID = model.ForwardID;
                model2.IsFlag = model.IsFlag;
                model2.IsForward = model.IsForward;
                model2.IsRead = model.IsRead;
                model2.IsReply = model.IsReply;
                model2.IsSendSuccessful = model.IsSendSuccessful;
                model2.IsStar = model.IsStar;
                model2.ReplyID = model.ReplyID;
                model2.Title = model.Title;
                model2.ToUID = model.ToUID;
                model2.ToUserName = model.ToUserName;
                model2.UID = model.ToUID;

                MessageService.Add(model);
                MessageService.Add(model2);
                SysDBTool.Commit();

                if (model.ID > 0 && model2.ID > 0)
                {
                    model.RelationID = model2.ID;
                    MessageService.Update(model);
                    SysDBTool.Commit();
                    model2.RelationID = model.ID;
                    MessageService.Update(model2);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "邮件发送成功！";
                    ActMessage = ViewBag.SuccessMsg;
                    ActPacket = model;
                    result.Status = 200;
                }
                else
                {
                    throw new CustomException("邮件发送失败");
                }
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("MailWrite" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }

        //加载邮件内容视图
        public ActionResult SubMailContent(int id)
        {
            var model = MessageService.Single(id);
            if (model == null)
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除！";
                return View("Error");
            }
            model.IsRead = true;
            MessageService.Update(model);
            SysDBTool.Commit();
            ActMessage = "查看邮件";
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var model = MessageService.Single(id);
            if (model != null && model.UID == Umodel.ID)
            {
                ActPacket = model;
                MessageService.Delete(id);
                SysDBTool.Commit();
                return RedirectToAction("inbox", "mail");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult doCommand(string ids, string commandtype)
        {
            Dictionary<string, string> dicCommand = new Dictionary<string, string>();
            if (commandtype.ToLower() == "makeread")
                dicCommand.Add("IsRead", "1");
            else if (commandtype.ToLower() == "makeunread")
                dicCommand.Add("IsRead", "0");
            else if (commandtype.ToLower() == "makeflag")
                dicCommand.Add("IsFlag", "1");
            else if (commandtype.ToLower() == "makeunflag")
                dicCommand.Add("IsFlag", "0");
            MessageService.Update(new Data.Message(), dicCommand, "id in (" + ids.TrimEnd(',').TrimStart(',') + ")");

            SysDBTool.Commit();
            return Content("ok");
        }


        public ActionResult MultiDelete(string ids)
        {
            MessageService.Delete(a => ids.TrimEnd(',').TrimStart(',').Contains(a.ID.ToString()) && a.UID == Umodel.ID);
            SysDBTool.Commit();
            return Content("ok");
        }


        //获取新消息
        public JsonResult GetNewMail()
        {
            var list = MessageService.List(x => x.IsRead == false && x.UID == Umodel.ID && x.ToUID == Umodel.ID).OrderByDescending(x => x.ID).ToList();
            int total = list.Count();//数据总数    

            if (list != null && total != 0)
            {
                return Json(new { result = "ok", data = list, total = total },JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "err" });
            }
        }
    }
}
