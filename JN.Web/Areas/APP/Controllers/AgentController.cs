
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;
using JN.Services.Manager;
using JN.Services.Tool;
using JN.Services.CustomException;

namespace JN.Web.Areas.APP.Controllers
{
    public class AgentController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISysParamService SysParamService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public AgentController(ISysDBTool SysDBTool, IUserService UserService, ISysParamService SysParamService, IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SysParamService = SysParamService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();

        }

        public ActionResult Index(int? page)
        {
            ActMessage = "我管辖的用户";
            var list = UserService.List(x => x.AgentID == Umodel.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ApplyAgent()
        {
            ActMessage = "申请商代中心";
            //if (Umodel.Investment != cacheSysParam.SingleAndInit(x => x.ID == 1005).Value.ToDecimal())
            //{
            //    ViewBag.ErrorMsg = "你的用户级别无法申请商务中心。";
            //    return View("Error");
            //}

            //int ztrs = Users.GetAllChild(Umodel, 1).Count();
            //int tdrs = Users.GetAllChild(Umodel).Count();

            //if (ztrs < cacheSysParam.SingleAndInit(x => x.ID == 1801).Value.ToInt() || tdrs < cacheSysParam.SingleAndInit(x => x.ID == 1801).Value2.ToInt())
            //{
            //    ViewBag.ErrorMsg = "您还没有完成申请商务中心的所需的业绩条件。";
            //    return View("Error");
            //}
            return View(Umodel);
        }

        [HttpPost]
        public ActionResult ApplyAgent(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string refereename = form["refereename"];
                //string agentname = form["agentname"];
                string remark = form["agentremark"];

                if (string.IsNullOrEmpty(refereename.Trim())) throw new CustomException("商代中心推荐人用户名");
                //if (UserService.List(x => x.AgentName == agentname.Trim()).Count() > 0) throw new CustomException("商务中心编号已被使用");
                if (remark.Trim().Length > 100) throw new CustomException("备注长度不能超过100个字节");
                if ((Umodel.IsAgent ?? false)) throw new CustomException("您已是商代中心，无需要重复申请");
                if (!String.IsNullOrEmpty(Umodel.AgentName)) throw new CustomException("您已经提交过申请，请耐心等待系统审核");
                //var ids = onUser.RefereePath.Split(',');
                //if (UserService.List(x => ids.Contains(x.ID.ToString()) || x.ID == onUser.RefereeID).Count() <= 0) throw new CustomException("只充许给伞下推荐关系的用户申请商务中心");

                var referee = UserService.Single(x => x.UserName == refereename.Trim() && x.IsActivation && (x.IsLock == false));
                if (referee == null) throw new CustomException("推荐人用户名不存在");
                if (!(referee.IsAgent ?? false)) throw new CustomException("推荐人用户名不是商代中心");

                //更新申请状态
                Umodel.AgentName = Umodel.UserName;
                Umodel.AgentRefereeUser = refereename.Trim();
                Umodel.ApplyAgentRemark = "";
                Umodel.ApplyAgentTime = DateTime.Now;
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
    }
}
