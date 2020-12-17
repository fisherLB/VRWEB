using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;

namespace JN.Web.Areas.APP.Controllers
{
    public class NoticeController : Controller
    {
        private readonly INoticeService NoticeService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public NoticeController(ISysDBTool SysDBTool,
            INoticeService NoticeService,
            IActLogService ActLogService)
        {
            this.NoticeService = NoticeService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
        }

        //public ActionResult Index(int? page)
        //{
        //    ViewBag.Title = "公告列表";
        //    var list = NoticeService.List().OrderByDescending(x => x.ID);
        //    return View(list.ToPagedList(page ?? 1, 20));
        //}
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetNoticeList(int? page)
        {
            var list = NoticeService.List().OrderByDescending(x => x.ID);
            int pageSize = 10;
            var listdata = list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize);//取数据
            var countrow = list.Count();//取总条数
            int totalPage = (countrow + pageSize - 1) / pageSize;//取总页数
            return Json(new { data = listdata, pages = totalPage, count = listdata.Count() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail(int id)
        {

            ViewBag.Title = "公告详情";
            var model = NoticeService.Single(id);
            if (model != null)
                return View(model);
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }
    }
}
