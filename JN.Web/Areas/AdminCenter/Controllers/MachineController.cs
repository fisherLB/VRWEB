using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using System.IO;
using JN.Services.Manager;
using System.Data.Entity.Validation;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    //矿机
    [ValidateInput(false)]
    public class MachineController : BaseController
    {
        private readonly IUserService UserService;
        private readonly IMachineOrderService MachineOrderService;
        private readonly IMachineService MachineService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public MachineController(ISysDBTool SysDBTool,
            IUserService UserService,
            IMachineOrderService MachineOrderService,
            IMachineService MachineService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.MachineOrderService = MachineOrderService;
            this.MachineService = MachineService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
        }

        public ActionResult Modify(int? id)
        {
            ActMessage = "编辑矿机";
            if (id.HasValue)
                return View(MachineService.Single(id));
            else
            {
                ActMessage = "发布矿机";
                return View(new Data.Machine());
            }
        }

        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            try
            {
                var entity = MachineService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                //if (ClassId.Trim().Length == 0)
                //    strErr += "请选择商品分类 <br />";

                //HttpPostedFileBase file = Request.Files["imgurl"];
                //string imgurl = "";
                //if (!string.IsNullOrEmpty(file.FileName))
                //{
                //    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                //    {
                //        ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                //        return View("Error");
                //    }
                //    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                //    //20160709更新缩略图 ------------开始
                //    var fileName = Path.Combine(Request.MapPath("~/Upload"), newfilename);
                //    try
                //    {
                //        file.SaveAs(fileName);
                //        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/Upload/"), 1024, 768, "EQU");
                //        System.IO.File.Delete(fileName); //删除原文件
                //        imgurl = "/Upload/" + thumbnailfilename;
                //    }
                //    catch (Exception ex)
                //    {
                //        throw new Exception("上传失败：" + ex.Message);
                //    }
                //    System.IO.File.Delete(fileName); //删除原文件
                //    //20160709更新 ------------结束
                //}
                //entity.ImageUrl = imgurl;
                if (entity.ID > 0)
                    MachineService.Update(entity);
                else
                {
                    entity.IsSales = true;
                    entity.SaleCount = 0;
                    entity.CreateTime = DateTime.Now;
                    MachineService.Add(entity);
                }
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "矿机修改/发布成功！";
                return View("Success");
            }
            catch (DbEntityValidationException ex)
            {
                //foreach (var item in ex.EntityValidationErrors)
                //{
                //    foreach (var item2 in item.ValidationErrors)
                //        error += string.Format("{0}:{1}\r\n", item2.PropertyName, item2.ErrorMessage);
                //}
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
                return View("Error");
            }
        }

        public ActionResult MachineList(int? page)
        {
            ActMessage = "矿机管理";
            var list = MachineService.List(x => x.IsSales).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult OffSales(int? page)
        {
            ActMessage = "下架矿机管理";
            var list = MachineService.List(x => x.IsSales == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View("MachineList", list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult MachineCommand(int id, string commandtype)
        {
            Data.Machine model = MachineService.Single(id);

            if (commandtype.ToLower() == "onsales")
                model.IsSales = true;
            else if (commandtype.ToLower() == "offsales")
                model.IsSales = false;
            MachineService.Update(model);
            SysDBTool.Commit();
            return RedirectToAction("MachineList", "Machine");
        }

        public ActionResult Delete(int id)
        {
            var model = MachineService.Single(id);
            if (model != null)
            {
                ActPacket = model;
                MachineService.Delete(id);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "“" + model.MachineName + "”已被删除！";
                ActMessage = ViewBag.SuccessMsg;
                return View("Success");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
        public ActionResult MachineOrder(int? page, int status)
        {
            ActMessage = "订单管理";
            var list = MachineOrderService.List(x => x.Status == status).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }
    }
}
