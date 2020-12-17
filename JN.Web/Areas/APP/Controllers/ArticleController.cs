using JN.Data.Service;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace JN.Web.Areas.APP.Controllers
{
    [ValidateInput(false)]
    public class ArticleController : Controller
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

        /// <summary>
        /// 文章列表
        /// </summary>
        /// <param name="cId">传过来的二级分类id</param>
        /// <returns></returns>
        public ActionResult ArticleList(int cId, int? page)
        {
            var classModel = ArticleClassService.Single(cId);
            if (classModel != null)
            {
                ViewBag.Title = "网站公告";
                //找到该大类下的所有文章
                var list = ArticleService.List(x => x.ClassID==classModel.ID).OrderByDescending(x => x.ID).ToList();
                ViewBag.ArticleTitle = classModel.ClassName;
                if (Request.IsAjaxRequest())
                    return View("_ArticleList", list.ToPagedList(page ?? 1, 20));
                return View(list.ToPagedList(page ?? 1, 20));
            }
            else
            {
                return Redirect("/home/index");
            }           
        }


        /// <summary>
        /// 阅读文章
        /// </summary>
        /// <param name="aId">传过来的文章id</param>
        /// <returns></returns>
        public ActionResult ReadOneArticle()
        {
            string sid = Request["sId"];
            string id = Request["Id"];
            string cId = Request["cId"];
            if (string.IsNullOrEmpty(sid)) return Content("<script>alert('没有数据');window.top.location.href ='/home/index'</script>");
            int counsid = int.Parse(sid);
            var articleModel_p = ArticleClassService.Single(x => x.ID == counsid);//主类

           
            if (!string.IsNullOrEmpty(id))
            {
                int aid = int.Parse(id);
                var article = ArticleService.Single(x => x.ID == aid);//如果是当前页面文章
                if (article != null && articleModel_p != null)
                {
                    article.ReadCount = article.ReadCount + 1;//浏览量增加
                    SysDBTool.Commit();
                    ViewBag.Data = articleModel_p;
                    ViewBag.Title = article.Title;
                    return View(article);
                }
                else {
                    return Content("<script>alert('没有数据');window.top.location.href ='/home/index'</script>");
                }
            }
            else if (!string.IsNullOrEmpty(cId) && articleModel_p != null)
            {
                int classid = int.Parse(cId);
                var articleModel = ArticleService.List(x => x.ClassID == classid).FirstOrDefault();//如果是首页进来，根据二级分类来查找第一个文章
                if (articleModel != null)
                {
                    articleModel.ReadCount = articleModel.ReadCount + 1;//浏览量增加
                    SysDBTool.Commit();
                    ViewBag.Data = articleModel_p;
                    ViewBag.Title = articleModel.Title;
                    return View(articleModel);
                }
                else {
                    return Content("<script>alert('没有数据');window.top.location.href ='/home/index'</script>");
                }
            }
            else
            {
                return Content("<script>alert('没有数据');window.top.location.href ='/home/index'</script>");
            }

        }
        /// <summary>
        /// 阅读文章
        /// </summary>
        /// <param name="cId">传过来的分类id</param>
        /// <returns></returns>
        public ActionResult ReadArticle(int cId)
        {
            var articleModel = ArticleService.List(x => x.ID == cId).FirstOrDefault();
            if (articleModel != null && articleModel.ID >0)
            {
                articleModel.ReadCount = articleModel.ReadCount + 1;//浏览量增加
                SysDBTool.Commit();
                ViewBag.Title = articleModel.Title;
                return View(articleModel);
            }
            else
            {
                ViewBag.ErrorMsg = "数据走丢啦，请稍后再试~~~";
                return View("Error");
            }

        }

    }
}
