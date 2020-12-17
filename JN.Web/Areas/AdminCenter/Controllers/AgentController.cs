using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using JN.Services.CustomException;
using JN.Services.Manager;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class AgentController : BaseController
    {
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public AgentController(ISysDBTool SysDBTool,
            IUserService UserService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
        }
        public ActionResult Delivery()
        {
            ActMessage = "指定一个商务中心";
            return View();
        }

        [HttpPost]
        public ActionResult Delivery(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string username = form["username"];
                string agentname = form["agentname"];
                string remark = form["remark"];

                var onUser = UserService.Single(x => x.UserName == username.Trim());
                if (onUser == null) throw new CustomException("用户不存在");
                if (string.IsNullOrEmpty(agentname)) throw new CustomException("商务中心编号不能为空");
                if (UserService.List(x => x.AgentName == agentname.Trim()).Count() > 0) throw new CustomException("商务中心编号已被使用");
                if (remark.Trim().Length > 100) throw new CustomException("备注长度不能超过100个字节");
                if ((onUser.IsAgent ?? false)) throw new CustomException("该用户已是商务中心，无需要重复申请");

                onUser.AgentName = agentname;
                onUser.IsAgent = true;
                onUser.ApplyAgentTime = DateTime.Now;
                onUser.ApplyAgentRemark = remark;
                UserService.Update(onUser);
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
        public ActionResult Index(int? page)
        {
            ActMessage = "商务中心列表";
            var list = UserService.List(x => (x.IsAgent ?? false)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult UnPassed(int? page)
        {
            ActMessage = "待审核的商务中心";
            var list = UserService.List(x => !(x.IsAgent ?? false) && !string.IsNullOrEmpty(x.AgentName)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult doCancel(int id)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.IsAgent = false;
                model.AgentName = "";
                model.ApplyAgentRemark = "商务中心被取消";
                UserService.Update(model);

                //其它指定该用户的商务中心也需要一同取消
                var ulist = UserService.ListWithTracking(x => x.AgentUser == model.UserName).OrderBy(x => x.ID).ToList();
                foreach (var item in ulist)
                {
                    item.AgentID = 0;
                    item.AgentUser = "";
                    UserService.Update(item);
                }
                SysDBTool.Commit();

                //向商务中心提币的订单取消
                var tlist = MvcCore.Unity.Get<ITakeCashService>().ListWithTracking(x => x.FromAgent == model.UserName && x.Status < (int)Data.Enum.TakeCaseStatus.Payed && x.Status >= 0).OrderBy(x => x.ID).ToList();
                foreach (var item in tlist)
                {
                    item.Status = (int)JN.Data.Enum.TakeCaseStatus.Cancel;
                    MvcCore.Unity.Get<ITakeCashService>().Update(item);
                    SysDBTool.Commit();
                    Wallets.changeWallet(item.UID, item.DrawMoney, 2002, "商代中心取消，提币无法完成");
                }
               
                ViewBag.SuccessMsg = "用户“" + model.UserName + "”商务中心资格已被取消！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }

        public ActionResult doPass(int id)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.IsAgent = true;
                UserService.Update(model);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "审核通过用户“" + model.UserName + "”成为商务中心！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }

        public ActionResult doNoPass(int id)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.IsAgent = false;
                model.AgentName = "";
                UserService.Update(model);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "拒绝用户“" + model.UserName + "”成为商务中心！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }
    }
}
