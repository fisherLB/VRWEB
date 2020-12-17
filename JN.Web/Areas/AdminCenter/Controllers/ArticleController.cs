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
using System.Data.Common;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    [ValidateInput(false)]
    public class ArticleController : BaseController
    {
        private readonly IArticleService ArticleService;
        private readonly ISysDBTool SysDBTool;
        private readonly IArticleClassService ArticleClassService;

        public ArticleController(ISysDBTool SysDBTool,
            IArticleService ArticleService,
            IArticleClassService ArticleClassService)
        {
            this.ArticleService = ArticleService;
            this.SysDBTool = SysDBTool;
            this.ArticleClassService = ArticleClassService;
        }

        public ActionResult ArticleList(int? page)
        {
            ActMessage = "文章列表";
            var list = ArticleService.List().OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ArticleClassList(int? page)
        {
            ActMessage = "文章分类列表";
            var list = ArticleClassService.List(x=>x.Pid==0).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ArticleAddOrModify(int? id)
        {
            ActMessage = "文章发布";
            var model = new Data.Article();
            if (id.HasValue)
            {
                ActMessage = "修改文章";
                model = ArticleService.Single(id);
            }
            return View(model);
        }

        public ActionResult ArticleClassAddOrModify(int? id)
        {
            ActMessage = "添加文章分类";
            var model = new Data.ArticleClass();
            if (id.HasValue)
            {
                ActMessage = "修改文章分类";
                model = ArticleClassService.Single(id);
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult ArticleClassAddOrModify(FormCollection fc)
        {
            var result = new ReturnResult();
            try
            {
                var entity = ArticleClassService.SingleAndInit(fc["ID"].ToInt());
               string classname= fc["ClassName"];             

                TryUpdateModel(entity, fc.AllKeys);
                
                if (entity.ID > 0)
                {                  
                    ArticleClassService.Update(entity);
                }
                else
                {
                    if (ArticleClassService.List(x => x.ClassName == classname).Count() > 0)
                    {
                        throw new CustomException("分类名称不能重名");
                    }
                    if (entity.Pid == 0)
                    {
                        //if (ArticleClassService.List(x => x.Pid == 0).Count() > 0)
                        //{
                        //    throw new CustomException("父分类只能有一个");
                        //}
                        //entity.Pid = 0;//选择顶级分类
                        entity.Ppach = "0,";//选择顶级分类

                        //是否单一
                        string IsAlone = fc["IsAlone"];
                        if (IsAlone == "1")
                        {
                            entity.IsAlone = true;
                        }
                        else
                        {
                            entity.IsAlone = false;
                        }
                    }
                    else
                    {
                        var pModel = ArticleClassService.Single(x => x.ID == entity.Pid);
                        entity.IsNotice = pModel.IsNotice;
                        //entity.Pid = pModel.ID;
                        entity.Ppach = pModel.Ppach + pModel.ID + ",";
                        entity.IsAlone = pModel.IsAlone;//继承父类的单一属性
                    }
                    int sort = 1;
                    var categoryList = ArticleClassService.List().ToList();
                    if (categoryList != null && categoryList.Count() > 0)
                    {
                        sort = categoryList.Max(x => x.Sort) + 1;
                    }
                    entity.Sort = sort;
                    entity.CreateTime = DateTime.Now;
                    ArticleClassService.Add(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = StringHelp.FormatErrorString(ex.Message);
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }


        public ActionResult ArticleClassDelet(int id)
        {
            var model = ArticleClassService.Single(id);
            if (model.Pid == 0)
            {
                if (ArticleClassService.List(x => x.Pid == model.ID).Count() > 0)
                {
                    ViewBag.ErrorMsg = "对不起，请先删除子分类！";
                    return View("Error");
                }
            }
            ArticleClassService.Delete(model.ID);
            SysDBTool.Commit();
            return RedirectToAction("ArticleClassList", "Article");
        }


        
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ArticleAddOrModify(FormCollection fc)
        {
            var result = new ReturnResult();
            try
            {
                var entity = ArticleService.SingleAndInit(fc["ID"].ToInt());
                int oldClassID = entity.ClassID;

                TryUpdateModel(entity, fc.AllKeys);

                if (string.IsNullOrEmpty(entity.Title)) throw new Exception("文章标题不能为空");
                if (string.IsNullOrEmpty(entity.Content)) throw new Exception("文章内容不能为空");
                if (string.IsNullOrEmpty(entity.Author)) throw new Exception("文章作者不能为空");
                if (string.IsNullOrEmpty(entity.Source)) throw new Exception("文章来源不能为空");

                int classID = fc["ClassID"].ToInt();
                var classModel = ArticleClassService.Single(x => x.ID == classID);
                entity.ClassPath = ",";// classModel.Ppach + classModel.ID + 

                if (entity.ID > 0)
                {
                    //分类已更换
                    if (entity.ClassID != oldClassID && classModel.IsAlone == true)
                    {
                        int count = ArticleService.List(x => x.ClassID == classModel.ID).Count();
                        if (count > 0)
                        {
                            throw new Exception("改分类下已有文章，无法重复添加");
                        }
                    }

                    ArticleService.Update(entity);
                }
                else
                {
                    if (classModel.IsAlone)
                    {
                        int count = ArticleService.List(x => x.ClassID == classModel.ID).Count();
                        if (count > 0)
                        {
                            throw new Exception("改分类下已有文章，无法重复添加");
                        }
                    }

                    if ((classModel.IsNotice ?? false))
                    {
                        entity.IsNotice = true;
                    }

                    entity.ReadCount = 0;
                    entity.ClassID = classID;
                  
                    entity.CreateTime = DateTime.Now;
                    ArticleService.Add(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = StringHelp.FormatErrorString(ex.Message);
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }


        public ActionResult ArticleDelet(int id)
        {
            var model = ArticleService.Single(id);
            ArticleService.Delete(model.ID);
            SysDBTool.Commit();
            return RedirectToAction("ArticleList", "Article");
        }

        #region 文章推广
        /// <summary>
        /// 文章推广
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsNotice(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = ArticleClassService.SingleAndInit(id);
                if (entity != null)
                {
                    if ((entity.IsNotice ?? false) == true)
                    {
                        entity.IsNotice = false;
                        //修改子类
                        if (ArticleClassService.List(x => x.Pid == entity.ID).Count() > 0)
                        {
                            string selluserSql = "UPDATE [ArticleClass] set IsNotice=0  where Pid=" + entity.ID;
                            DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                            SysDBTool.ExecuteSQL(selluserSql.ToString(), dbparam);
                           
                        }
                    }
                    else
                    {

                        //修改子类
                        if (ArticleClassService.List(x => x.Pid == entity.ID).Count() > 0)
                        {
                            string selluserSql = "UPDATE [ArticleClass] set IsNotice=1  where Pid=" + entity.ID;
                            DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                            SysDBTool.ExecuteSQL(selluserSql.ToString(), dbparam);

                        }
                        entity.IsNotice = true;
                    }

                    ArticleClassService.Update(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 底部显示
        /// <summary>
        /// 底部显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsShowBottom(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = ArticleClassService.SingleAndInit(id);
                if (entity != null)
                {
                    if ((entity.IsShowBottom ?? false) == true)
                    {
                        entity.IsShowBottom = false;
                    }
                    else
                    {
                        //如果不是单一类不能在底部显示
                        if (entity.IsAlone == false)
                        {
                            throw new Exception("不符合更改条件！");
                        }
                        entity.IsShowBottom = true;
                    }

                    ArticleClassService.Update(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 首页文章显示
        /// <summary>
        /// 首页文章显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsShowHome(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = ArticleService.SingleAndInit(id);
                if (entity != null)
                {
                    if ((entity.IsShowHome ?? false) == true)
                    {
                        entity.IsShowHome = false;
                    }
                    else
                    {
                        entity.IsShowHome = true;
                    }

                    ArticleService.Update(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 首页文章置顶
        /// <summary>
        /// 首页文章置顶
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsTop(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = ArticleService.SingleAndInit(id);
                if (entity != null)
                {
                    if ((entity.IsTop ?? false) == true)
                    {
                        entity.IsTop = false;
                    }
                    else
                    {
                        entity.IsTop = true;
                    }

                    ArticleService.Update(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        //public ActionResult NoticeCommand(int id, string commandtype)
        //{
        //    var model = ArticleService.Single(id);
        //    if (commandtype.ToLower() == "ontop")
        //        model.IsTop = true;
        //    else if (commandtype.ToLower() == "untop")
        //        model.IsTop = false;
        //    ArticleService.Update(model);
        //    SysDBTool.Commit();
        //    return RedirectToAction("Index", "Notice");
        //}

        //public ActionResult Delete(int id)
        //{
        //    var model = NoticeService.Single(id);
        //    if (model != null)
        //    {
        //        ActPacket = model;
        //        NoticeService.Delete(id);
        //        SysDBTool.Commit();
        //        ViewBag.SuccessMsg = "“" + model.Title + "”已被删除！";
        //        return View("Success");
        //    }
        //    ViewBag.ErrorMsg = "记录不存在或已被删除！";
        //    return View("Error");
        //}


        //[HttpPost]
        //public ActionResult Modify(FormCollection fc)
        //{
        //    var result = new ReturnResult();
        //    try
        //    {
        //        var entity = NoticeService.SingleAndInit(fc["ID"].ToInt());
        //        TryUpdateModel(entity, fc.AllKeys);
        //        if (entity.ID > 0)
        //        {
        //            NoticeService.Update(entity);
        //        }
        //        else
        //        {
        //            entity.CreateTime = DateTime.Now;
        //            NoticeService.Add(entity);
        //        }
        //        SysDBTool.Commit();
        //        result.Status = 200;
        //    }
        //    catch (Exception ex)
        //    {
        //         result.Message = StringHelp.FormatErrorString(ex.Message);
        //        Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
        //    }
        //    return Json(result);
        //}
    }
}
