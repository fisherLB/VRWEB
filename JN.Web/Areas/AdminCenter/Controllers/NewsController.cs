using JN.Data.Service;
using JN.Services.Manager;
using MvcCore.Controls;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using System.Collections;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class NewsController : BaseController
    {
        private readonly ISysDBTool SysDBTool;
        private readonly IShop_NewsService Shop_NewsService;
         public NewsController( 
            ISysDBTool SysDBTool,
            IShop_NewsService Shop_NewsService)
        {
            this.SysDBTool = SysDBTool;
            this.Shop_NewsService = Shop_NewsService;
        }

        /// <summary>
        /// 新闻列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int? page)
        {
            ActMessage = "新闻列表";
            DateTime time = DateTime.Now.AddMonths(-1);//一个月内的记录
            var list = Shop_NewsService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.Id).ToList();//.Where(x => x.CreateTime > time)
            string cateId = Request["CateId"];
            if (cateId != null && cateId.Length > 0)
            {
                int iCate = cateId.ToInt();
                return View(list.Where(x => x.CateId == iCate).ToList().ToPagedList(page ?? 1,20));
            }            
            return View(list.ToPagedList(page ?? 1, 20));  

        }


        #region 发布新闻
        /// <summary>
        /// 发布新闻
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public ActionResult AddNews()
        {
            ActMessage = "发布新闻";
            return View(new Data.Shop_News());
        }

        [HttpPost]
        [ValidateInput(false)]        
        public ActionResult AddNews(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();

                var entity = Shop_NewsService.SingleAndInit(fc["Id"].ToInt());
                string cateId = fc["cateId"];
                string title = fc["title"];
                string newsContent = fc["newsContent"];
                string TitleImageUrl = fc["TitleImageUrl"];
                if (string.IsNullOrEmpty(cateId))
                {
                    result.Message = "请选择分类！";
                    return Json(result);
                }
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(TitleImageUrl) || string.IsNullOrEmpty(fc["Desc"]))
                {
                    result.Message="新闻标题、描述、标题图片不能为空！";
                    return Json(result);
                }
                
                if (string.IsNullOrEmpty(newsContent))
                {
                    result.Message = "新闻内容不能为空！";
                    return Json(result);
                }
                
                entity.Title = title;
                entity.NewsContent = newsContent;
                entity.TitleImageUrl = TitleImageUrl;
                entity.CateId = cateId.ToInt();
                entity.CreateTime = DateTime.Now;
                entity.Hits = 0;
                entity.IsShow = true;
                entity.Status = 1;
                entity.Desc = fc["Desc"];
                Shop_NewsService.Add(entity);
                SysDBTool.Commit();

                result.Status = 200;
            
            return Json(result);
        }
        #endregion

        #region 删除新闻
        /// <summary>
        /// 删除新闻
        /// </summary>
        /// <returns></returns>
        public ActionResult DelNew(int id)
        {
            var model = Shop_NewsService.Single(id);
            if(model != null)
            {
                Shop_NewsService.Delete(id);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "“" + model.Title + "”已被删除！";
                return View("Success");                
            }
            ViewBag.ErrorMsg = "新闻不存在或已被删除！";
            return View("Error");
        }
        #endregion

        #region 编辑新闻
        /// <summary>
        /// 编辑新闻
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ModifyNew(int id)
        {
            ViewBag.Title = "编辑新闻";
            return View(Shop_NewsService.Single(id));
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ModifyNew(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string dd = fc["title"];
                var entity = Shop_NewsService.SingleAndInit(fc["Id"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                if (entity.Id > 0)
                {
                    string cateId = fc["cateId"];
                    if (string.IsNullOrEmpty(cateId)) throw new Exception("请选择分类！");
                    if (string.IsNullOrEmpty(entity.Title) || string.IsNullOrEmpty(entity.TitleImageUrl)) throw new Exception("新闻标题、标题图片不能为空！");
                    if (string.IsNullOrEmpty(entity.NewsContent)) throw new Exception("新闻内容不能为空！");
                    entity.CreateTime = DateTime.Now;
                    SysDBTool.Commit();
                    Shop_NewsService.Update(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 新闻历史记录
        /// <summary>
        /// 新闻历史记录
        /// </summary>
        /// <returns></returns>
        public ActionResult NewsHistory(int? page)
        {
            ViewBag.Title = "新闻历史记录";
            var list = Shop_NewsService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.Id).ToList();
            string cateId = Request["CateId"];
            if (cateId != null && cateId.Length > 0)
            {
                int iCate = cateId.ToInt();
                return View(list.Where(x => x.CateId == iCate).ToList().ToPagedList(page ?? 1, 20));
            }
            return View(list.ToPagedList(page ?? 1, 20)); 
        }
        #endregion

        #region 新闻关闭、显示
        /// <summary>
        /// 新闻关闭、显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsShow(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var newsIsShop = Shop_NewsService.SingleAndInit(id);
                if (newsIsShop != null)
                {
                    if (newsIsShop.IsShow == true) 
                    {
                        newsIsShop.IsShow = false;
                    }
                    else
                    {
                        newsIsShop.IsShow = true;
                    }
                    
                    Shop_NewsService.Update(newsIsShop);                    
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 图片上传

        //主图图片上传
        public ActionResult UpMainPic()
        {
            System.Collections.Hashtable hash = UpPic1();
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传主图到文件夹
        /// <param name="dir">文件夹名称 "/Upload//UserSpace/" + userId + "/" + dir + "/";</param>
        /// <param name="userId">订单从表ID</param>
        /// <returns></returns>
        public Hashtable UpPic1()
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                string oldLogo = "/Upload/Sys/News/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 2000, System.Web.HttpContext.Current.Server.MapPath(oldLogo));
                hash["error"] = 0;
                hash["url"] = oldLogo + img;
                return hash;
            }
            catch (Exception ex)
            {
                hash["error"] = 1;
                hash["message"] = ex.Message;
                return hash;
            }
        }
        #endregion



    }
}