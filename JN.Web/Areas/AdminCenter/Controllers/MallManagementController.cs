using JN.Data.Service;
using JN.Services.Manager;
using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using PagedList;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class MallManagementController : BaseController
    {
        private readonly ISysDBTool SysDBTool;
        private readonly IShop_Product_CategoryService Shop_Product_CategoryService;
        private readonly IUserService UserService;
        private readonly IShop_ProductService Shop_ProductService;
        private readonly IShop_FloorService Shop_FloorService;
        private readonly IShop_CommentsService Shop_CommentsService;
        private readonly IShop_Tmp_Pro_ImgService Shop_Tmp_Pro_ImgService;
        private readonly IShop_Order_DetailsService Shop_Order_DetailsService;
        private readonly IShop_Home_GroupService Shop_Home_GroupService;
        private readonly IShop_SPECService Shop_SPECService;
        private readonly IShop_Product_SKUService Shop_Product_SKUService;


        public MallManagementController(ISysDBTool SysDBTool,
            IShop_Product_CategoryService Shop_Product_CategoryService,
            IUserService UserService,
            IShop_ProductService Shop_ProductService,
            IShop_FloorService Shop_FloorService,
            IShop_CommentsService Shop_CommentsService,
            IShop_Tmp_Pro_ImgService Shop_Tmp_Pro_ImgService,
            IShop_Order_DetailsService Shop_Order_DetailsService,
            IShop_Home_GroupService Shop_Home_GroupService,
            IShop_SPECService Shop_SPECService,
            IShop_Product_SKUService Shop_Product_SKUService
            )
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.Shop_Product_CategoryService = Shop_Product_CategoryService;
            this.Shop_ProductService = Shop_ProductService;
            this.Shop_FloorService = Shop_FloorService;
            this.Shop_CommentsService = Shop_CommentsService;
            this.Shop_Tmp_Pro_ImgService = Shop_Tmp_Pro_ImgService;
            this.Shop_Order_DetailsService = Shop_Order_DetailsService;
            this.Shop_Home_GroupService = Shop_Home_GroupService;
            this.Shop_SPECService = Shop_SPECService;
            this.Shop_Product_SKUService = Shop_Product_SKUService;
        }

        #region
        public ActionResult HomeTitle(int? page)
        {
            return View();
        }
        #endregion 

        #region 商品分类
        /// <summary>
        /// 商品分类管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductCategory(int? page)
        {
            ActMessage = "商品分类";
            //var list = Shop_Product_CategoryService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.Id).ToList();

            var allCategory = Shop_Product_CategoryService.List().ToList();
            //List<JN.Data.Shop_Product_Category> allCategory = new List<Data.Shop_Product_Category>();
            if (allCategory.Count() == 0)
            {
                return View(allCategory.ToPagedList(page ?? 1, 20));
            }

            List<JN.Data.Shop_Product_Category> categoryList = new List<Data.Shop_Product_Category>();

            //1.先找最顶层
            var vfirstCategory = allCategory.Where(x => x.ParentId == 0);
            if (vfirstCategory.Count() > 0)
            {
                var firstCategory = vfirstCategory.ToList();
                for (int i = 0; i < firstCategory.Count(); i++)
                {
                    categoryList.Add(firstCategory[i]);
                    //2.找第二层
                    var vSecondCategory = allCategory.Where(x => x.ParentId == firstCategory[i].Id);
                    if (vSecondCategory.Count() > 0)
                    {
                        var SecondCategory = vSecondCategory.ToList();
                        for (int j = 0; j < SecondCategory.Count(); j++)
                        {
                            categoryList.Add(SecondCategory[j]);
                            //3.找第三层
                            var vThiredCategory = allCategory.Where(x => x.ParentId == SecondCategory[j].Id);
                            if (vThiredCategory.Count() > 0)
                            {
                                var ThiredCategory = vThiredCategory.ToList();
                                for (int n = 0; n < ThiredCategory.Count(); n++)
                                {
                                    categoryList.Add(ThiredCategory[n]);
                                }
                            }
                        }
                    }
                }
            }

            return View(categoryList.ToPagedList(page ?? 1, 20));
        }

        #region 分类编辑

        /// <summary>
        /// 分类编辑
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public ActionResult ModifyCategory(int? id)
        {
            ActMessage = "修改分类";
            ViewBag.Title = ActMessage;
            return View(Shop_Product_CategoryService.Single(id));

        }
        [HttpPost]
        public ActionResult ModifyCategory(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_Product_CategoryService.SingleAndInit(fc["id"].ToInt());
                string name = fc["name"];
                string color = fc["color"];
                string cateImg = fc["cateImg"];
                string sort = fc["sort"];

                if (string.IsNullOrEmpty(entity.Name)) throw new Exception("分类名称不能为空！");
                if (Shop_Product_CategoryService.List(x => x.Name == name.Trim()).Count() > 0 && entity.Name != name) throw new Exception("分类名称已被使用！");

                if (string.IsNullOrEmpty(sort)) throw new Exception("分类排序不能为空！");
                if (!StringHelp.IsNumber(sort)) throw new Exception("请输入正确的分类排序！");
                #region 父级分类
                string cateid = fc["cateid"];
                int cId;
                int.TryParse(cateid, out cId);
                var model = Shop_Product_CategoryService.Single(x => x.Id == cId);
                if (model != null)
                {
                    string[] pPacth = model.Ppacth.Split(',');
                    if (pPacth.Length > 3 && entity.Id != cateid.ToInt()) throw new Exception("请选择该项的上级分类！");//分类最多三层，修改时根据路径判断是否选择前两层

                    entity.Ppacth = model.Ppacth + model.Id + ",";
                    entity.parentName = model.Name;
                }
                else
                {
                    entity.Ppacth = "0,";//选择顶级分类
                    entity.parentName = null;
                }
                entity.ParentId = cId;
                #endregion

                entity.Name = name;
                entity.Color = color;
                entity.CateImg = cateImg;
                entity.Sort = sort.ToInt();
                Shop_Product_CategoryService.Update(entity);

                SysDBTool.Commit();
                result.Status = 200;

                //JN.Services.Manager.Category.SaveCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));
                //this.SaveCategoryHtml();//生成分类HTML
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);

        }
        #endregion

        #region 生成分类HTML
        ///// <summary>
        ///// 生成分类HTML
        ///// </summary>
        //public void SaveCategoryHtml()
        //{
        //    string str = "";

        //    int a = 0;
        //    var vProCategory = Shop_Product_CategoryService.List();
        //    //var vProCategory = MvcCore.Unity.Get<JN.Data.Service.IShop_Product_CategoryService>().List(x => x.ParentId == 0);

        //    if (vProCategory != null)
        //    {
        //        string strUrl = "/ShopCenter/QueryDataByCategory?cat=";
        //        foreach (Data.Shop_Product_Category cate in vProCategory.ToList())
        //        {
        //            if (a > 9)
        //            {
        //                break;
        //            }

        //            if (a % 2 > 0)
        //            {
        //                str += "<li class=\"mod_cate alt\">";
        //            }
        //            else
        //            {
        //                str += "<li class=\"mod_cate\">";
        //            }

        //            str += "<h2><i class=\"arrow_dot fr\"></i><a href=\"" + strUrl + cate.Id + "\">" + cate.Name + "</a></h2>";

        //            //str += "<p class=\"mod_cate_r\"><a href=\"" + strUrl + cate.Id"\">黄豆</a><a href="#">绿豆</a><a href="#">芸豆</a><a href="#">蚕豆</a><a href="#">荞麦</a></p>";

        //            str += "<div class=\"mod_subcate\">";
        //            str += "<div class=\"mod_subcate_main\" style=\"width:100%\">";
        //            str += "<dl>";

        //            str += "<dt>" + cate.Name + "</dt>";
        //            str += "<dd>";

        //            int id = cate.Id;
        //            var vChildList = Shop_Product_CategoryService.List(x => x.ParentId == id); //SandMem.BLL.ProductCategory.SelectList("ParentId=" + cate.Id);
        //            //var vChildList = vProCategory.Where(x => x.ParentId == id);
        //            if (vChildList != null)
        //            {
        //                foreach (Data.Shop_Product_Category childcate in vChildList.ToList())
        //                {
        //                    str += "<a href=\"" + strUrl + childcate.Id + "\" class=\"" + childcate.Color + "\">" + childcate.Name + "</a>";
        //                    //str += "<a href=\"/GridList/Index?CategoryId=" + childcate.Id + "\">" + childcate.Name + "</a>";
        //                }
        //            }

        //            str += "</dd>";
        //            str += "</dl>";
        //            str += "</div>";
        //            str += "</div>";
        //            str += "</li>";
        //            a++;
        //        }
        //    }
        //    JN.Services.Manager.Category.SaveCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));
        //    //JN.Services.Tool.HtmlHelp.SaveCategoryHtml("<ul>" + str + "</ul>");
        //}

        #endregion

        #region 添加分类
        /// <summary>
        /// 添加分类
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCategory()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string name = Request["name"];
                string id = Request["id"];

                var entity = new Data.Shop_Product_Category();
                if (string.IsNullOrEmpty(name)) throw new Exception("分类名称不能为空");
                if (Shop_Product_CategoryService.List(x => x.Name == name.Trim()).Count() > 0) throw new Exception("分类名称已被使用！");

                int cId;
                int.TryParse(id, out cId);

                var model = Shop_Product_CategoryService.Single(x => x.Id == cId);
                if (model != null)
                {
                    string[] pPacth = model.Ppacth.Split(',');
                    if (pPacth.Length > 3) throw new Exception("请选择该项的上级分类！");//分类最多三层，修改时根据路径判断是否选择前两层

                    entity.Ppacth = model.Ppacth + model.Id + ",";
                    entity.parentName = model.Name;
                }
                else
                {
                    entity.Ppacth = "0,";//选择顶级分类
                    entity.parentName = null;
                }
                entity.ParentId = cId;
                entity.Name = name;
                entity.CreateTime = DateTime.Now;
                entity.IsNavTop = false;
                entity.IsShow = true;
                int sort = 1;
                var categoryList = Shop_Product_CategoryService.List().ToList();
                if (categoryList != null && categoryList.Count() > 0)
                {
                    sort = categoryList.Max(x => x.Sort) + 1;
                }
                entity.Sort = sort;
                Shop_Product_CategoryService.Add(entity);
                SysDBTool.Commit();

                result.Status = 200;

                //JN.Services.Manager.Category.SaveCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));
                //this.SaveCategoryHtml();//生成分类HTML
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        #endregion

        #region 删除分类 未用
        public ActionResult DelNew(int id)
        {
            var model = Shop_Product_CategoryService.Single(id);
            var admin = Amodel;
            if (admin != null)
            {
                int childCount = Shop_Product_CategoryService.List(x => x.ParentId == id).Count();//判断是否有子类
                if (childCount > 0)
                {
                    ViewBag.ErrorMsg = "此类含" + childCount + "个子类，删除失败！";
                    return View("Error");
                }
                if (model != null)
                {
                    Shop_Product_CategoryService.Delete(id);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "“" + model.Name + "”已被删除！";
                    return View("Success");
                }
            }
            ViewBag.ErrorMsg = "分类不存在或已被删除！";
            return View("Error");
        }

        #endregion

        #region 是否显示
        public ActionResult HomeShow(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var productCategory = Shop_Product_CategoryService.SingleAndInit(id);
                if (productCategory != null)
                {
                    if (productCategory.IsShow == true)
                    {
                        productCategory.IsShow = false;
                    }
                    else
                    {
                        productCategory.IsShow = true;
                    }

                    Shop_Product_CategoryService.Update(productCategory);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            //JN.Services.Manager.Category.SaveCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));
            return Json(result);
        }

        #endregion

        #region 首页置顶
        public ActionResult HomeTop(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var productCategory = Shop_Product_CategoryService.SingleAndInit(id);
                if (productCategory != null)
                {
                    if (productCategory.IsNavTop == true)
                    {
                        productCategory.IsNavTop = false;
                    }
                    else
                    {
                        productCategory.IsNavTop = true;
                    }

                    Shop_Product_CategoryService.Update(productCategory);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
           // JN.Services.Manager.Category.SaveCategoryHtml(Shop_Product_CategoryService, JN.Services.Tool.ConfigHelper.GetConfigString("ShopTheme"));
            return Json(result);
        }

        #endregion

        #region 分类图片上传

        //分类主图图片上传
        public ActionResult CateUpMainPic()
        {
            System.Collections.Hashtable hash = CateUpPic1();
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传产品主图到文件夹
        /// <param name="dir">文件夹名称 "/Upload//UserSpace/" + userId + "/" + dir + "/";</param>
        /// <param name="userId">订单从表ID</param>
        /// <returns></returns>
        public Hashtable CateUpPic1()
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                string oldLogo = "/Upload/ProClass/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 100, System.Web.HttpContext.Current.Server.MapPath(oldLogo));
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

        #endregion

        #region 首页商品管理

        #region 首页商品管理列表
        /// <summary>
        /// 首页商品管理
        /// </summary>
        /// <returns></returns>
        public ActionResult HomeIndex()
        {
            ActMessage = "首页商品管理";
            string type = Request["type"];
            var list = Shop_FloorService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.Id).ToList();
            if (type != null)
            {
                list = list.Where(x => x.type == type.ToInt()).ToList();
            }
            return View(list);
        }
        #endregion

        #region 删除首页商品
        /// <summary>
        /// 删除首页商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelProduct(int id, int type)
        {
            var model = Shop_FloorService.Single(x => x.Id == id && x.type == type);
            if (model != null)
            {
                ActPacket = model;
                Shop_FloorService.Delete(id);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "删除成功";
            }

            return Redirect("/AdminCenter/MallManagement/HomeIndex?type=" + type);// View("");
        }
        #endregion

        #region 首页商品关闭、显示
        /// <summary>
        /// 新闻关闭、显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsUse(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var floor = Shop_FloorService.SingleAndInit(id);
                if (floor != null)
                {
                    if (floor.IsUse == true)
                    {
                        floor.IsUse = false;
                    }
                    else
                    {
                        floor.IsUse = true;
                    }

                    Shop_FloorService.Update(floor);
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

        #region 选择商品
        /// <summary>
        /// 选择商品
        /// </summary>
        /// <returns></returns>
        public ActionResult _SelectProduct()
        {
            string proName = Request["ProName"];
            string ShopName = Request["ShopName"];

            int type = Request["type"].ToInt();

            //List<JN.Data.Shop_Product> list = new List<Data.Shop_Product>();
            var parameters = new[]{
                    new SqlParameter(){ParameterName="type", Value=type}
                };
            var list = SysDBTool.Execute<JN.Data.Shop_Product>("SELECT * FROM [Shop_Product] where id not in(select pid from [Shop_Floor] where type = @type) and status=1 ", parameters);

            if (proName != null && proName.Length > 0 && ShopName != null && ShopName.Length == 0 && list != null && list.Count() > 0)
            {
                var pList = list.Where(x => x.ProductName.Contains(proName)); //Shop_ProductService.List(x => x.ProductName.Contains(proName) && x.Status == true);

                if (pList != null && pList.Count() > 0)
                {
                    list.AddRange(pList.ToList());
                }
            }
            else
            {
                if (proName != null && proName.Length > 0)
                {
                    //var proList = Shop_ProductService.List(x => x.ProductName.Contains(proName) && x.Status == true);
                    var pList = list.Where(x => x.ProductName.Contains(proName));

                    if (pList != null && pList.Count() > 0)
                    {
                        list.AddRange(pList.ToList());
                    }
                }
                if (ShopName != null && ShopName.Length > 0)
                {
                    var shopList = list.Where(x => x.ShopName.Contains(ShopName)); //Shop_ProductService.List(x => x.ShopName.Contains(ShopName) && x.Status == true);
                    if (shopList != null && shopList.Count() > 0)
                    {
                        list.AddRange(shopList.ToList());
                    }
                }
            }

            if (list.Count() >= 100)
            {
                //查找最新的100个
                list = list.Take(100).ToList();
                //list = Shop_ProductService.List(x => x.Status == true).Take(100).ToList();
            }
            return View(list.OrderByDescending(x => x.CreateTime).ToList());
        }

        /// <summary>
        /// 添加首页商品
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SelectProduct(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string proIds = fc["selId"];
                if (string.IsNullOrEmpty(proIds)) throw new Exception("请您选择记录！");

                int type = fc["type"].ToInt();

                string[] strPids = proIds.Split(',');
                for (int i = 0; i < strPids.Count(); i++)
                {
                    if (String.IsNullOrEmpty(strPids[i]))
                        continue;
                    int id = strPids[i].ToInt();
                    JN.Data.Shop_Product product = Shop_ProductService.Single(x => x.Id == id && x.Status == true);
                    if (product == null)
                    {
                        throw new Exception("添加失败，商品不存在或已下架");
                    }
                    JN.Data.Shop_Floor floor = Shop_FloorService.Single(x => x.Pid == id && x.type == type);
                    if (floor != null)
                    {
                        throw new Exception("不可添加重复的商品");
                    }

                    Shop_FloorService.Add(new Data.Shop_Floor
                    {
                        Pid = id,
                        ProductName = product.ProductName,
                        SId = product.SId,
                        ShopName = product.ShopName,
                        Tag = type,
                        type = type,
                        IsUse = true,
                        CreateTime = DateTime.Now
                    });
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

        #endregion

        #region 首页分组管理

        #region 首页分组管理
        /// <summary>
        /// 首页分组管理
        /// </summary>
        /// <returns></returns>
        public ActionResult HomeGroup(int? page)
        {
            ActMessage = "首页分组管理";
            string name = Request["Name"];

            var list = Shop_Home_GroupService.List().OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(name) && list != null && list.Count() > 0)
            {
                var data = list.Where(x => x.Name.ToString().Contains(name)).ToList();
                return View(data.ToPagedList(page ?? 1, 20));
            }

            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 添加、编辑首页分组
        /// <summary>
        /// 添加、编辑首页分组
        /// </summary>
        /// /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeGroup(int? id)
        {

            ActMessage = "添加首页分组";
            var model = new Data.Shop_Home_Group();
            if (id.HasValue)
            {
                ActMessage = "修改首页分组";
                model = Shop_Home_GroupService.Single(id);
            }
            ViewBag.Title = ActMessage;
            return View(model);
        }
        [HttpPost]

        public ActionResult AddHomeGroup(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = Services.AdminLoginHelper.CurrentAdmin();//校验管理员是否已经登录
                if (model == null) throw new Exception("您还没有登录，请登录后再重试！");

                var entity = Shop_Home_GroupService.SingleAndInit(fc["id"].ToInt());
                string name = fc["Name"];
                string title = fc["Title"];
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(title)) throw new Exception("分组名称、分组标题不能为空！");

                if (entity.Id > 0)
                {
                    if (Shop_Home_GroupService.List(x => x.Name == name.Trim()).Count() > 0 && entity.Name != name) throw new Exception("分组名已被使用");
                    entity.Name = name;
                    entity.Title = title;
                    Shop_Home_GroupService.Update(entity);
                }
                else
                {
                    if (Shop_Home_GroupService.List(x => x.Name == name.Trim()).Count() > 0) throw new Exception("分组名已被使用");
                    entity.Name = name;
                    entity.Title = title;
                    entity.CreateTime = DateTime.Now;
                    entity.Uid = Amodel.ID;
                    entity.UserName = Amodel.AdminName;
                    Shop_Home_GroupService.Add(entity);
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

        #endregion

        #region 商品评论

        #region 评论列表
        /// <summary>
        /// 评论列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Comments(int? page)
        {

            string keyfiled = Request["keyfiled"];
            string keyword = Request["keyword"];
            string dateform = Request["dateform"];
            string dateto = Request["dateto"];

            var list = new List<JN.Data.Shop_Comments>();
            if (keyfiled == "pid" && keyword != null && keyword.Length > 0)//根据商品名称查询
            {
                var parameters = new[]{
                     new SqlParameter(){ParameterName="GoodsName", Value=keyword}
                    //new SqlParameter(){ParameterName="GoodsName", Value="%"+keyword+"%"}
                };
                list = SysDBTool.Execute<JN.Data.Shop_Comments>("SELECT * FROM Shop_Comments where pid  in (select GoodsId from Shop_Order_Details where GoodsName like @GoodsName) ", parameters);
                if (dateform != null && dateform.Length > 0)
                {
                    DateTime dateform1 = dateform.ToDateTime();
                    list = list.Where(x => x.CreateTime >= dateform1).ToList();

                }
                if (dateto != null && dateto.Length > 0)
                {
                    DateTime dateto1 = dateto.ToDateTime();
                    list = list.Where(x => x.CreateTime <= dateto1).ToList();
                }

            }
            else if (keyfiled == "sid" && keyword != null && keyword.Length > 0)//根据店铺名称查询
            {
                var parameters = new[]{
                    new SqlParameter(){ParameterName="ShopName", Value=keyword}
                    //new SqlParameter(){ParameterName="ShopName", Value="%"+keyword+"%"}
                };
                list = SysDBTool.Execute<JN.Data.Shop_Comments>("SELECT * FROM Shop_Comments where sid  in (select ShopId from Shop_Order_Details where ShopName like @ShopName) ", parameters);
                if (dateform != null && dateform.Length > 0)
                {
                    DateTime dateform1 = dateform.ToDateTime();
                    list = list.Where(x => x.CreateTime >= dateform1).ToList();

                }
                if (dateto != null && dateto.Length > 0)
                {
                    DateTime dateto1 = dateto.ToDateTime();
                    list = list.Where(x => x.CreateTime <= dateto1).ToList();
                }
            }
            else
            {
                list = Shop_CommentsService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        #endregion

        #region 显示/关闭评论
        /// <summary>
        /// 显示/关闭评论
        /// </summary>
        /// /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowOrCloseComment(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_CommentsService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsLock == true)
                    {
                        entity.IsLock = false;
                    }

                    Shop_CommentsService.Update(entity);
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

        #endregion

        #region 商品列表/下架商品列表

        #region 商品列表

        public ActionResult ProductList(int? page)
        {
            ViewBag.Title = "商品列表";
            var list = Shop_ProductService.List(x => x.Status == true && x.IsPass == true).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.Id).ToList();
            int cateId = Request["cateid"].ToInt();
            if (cateId > 0)
            {
                return View(list.Where(x => x.GoodsClassId == cateId).ToList().ToPagedList(page ?? 1, 20));
            }
            return View(list.ToPagedList(page ?? 1, 10));
        }
        #endregion

        #region 下架商品列表
        /// <summary>
        /// 下架商品列表
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult OffProductIndex(int? page)
        {
            ViewBag.Title = "下架商品列表";
            var list = Shop_ProductService.List(x => x.Status == false && x.IsPass == true).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.Id).ToList();
            int cateId = Request["cateid"].ToInt();
            if (cateId > 0)
            {
                return View(list.Where(x => x.GoodsClassId == cateId).ToList().ToPagedList(page ?? 1, 20));
            }
            return View(list.ToPagedList(page ?? 1, 10));
        }
        #endregion

        #region 未审核商品列表

        public ActionResult NoPassProductList(int? page)
        {
            ViewBag.Title = "未审核商品列表";
            var list = Shop_ProductService.List(x => x.IsPass == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.Id).ToList();
            int cateId = Request["cateid"].ToInt();
            if (cateId > 0)
            {
                return View(list.Where(x => x.GoodsClassId == cateId).ToList().ToPagedList(page ?? 1, 20));
            }
            return View(list.ToPagedList(page ?? 1, 10));
        }
        #endregion

        #region 商品审核通过
        /// <summary>
        /// 商品审核通过
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductPass(int pid)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var productModel = Shop_ProductService.Single(pid.ToInt());
                if (productModel.IsPass == false)
                {
                    productModel.IsPass = true;//产品通过
                    productModel.Status = true;//产品上架
                    Shop_ProductService.Update(productModel);
                    SysDBTool.Commit();

                    result.Message = "商品审核通过！";
                    result.Status = 200;
                }
                else
                {
                    result.Message = "没有可操作的记录！";
                    result.Status = 500;
                }

            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }

            return Json(result);
        }
        #endregion

        #region 商品编辑
        /// <summary>
        /// 商品编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifyProduct(int? id)
        {         
            if (id.HasValue)
            {
                ViewBag.Title = "商品编辑";
                return View(Shop_ProductService.Single(id));
            }
            else
            {
                ActMessage = "新增初始会员";
                return View(new Data.Shop_Product());
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ModifyProduct(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(fc["ID"].ToInt());
                var entity2 = new JN.Data.Shop_SPEC();//商品规格表
                //var myShop = ShopInfoService.Single(x => x.UID == Umodel.ID);
                var shopClass = Shop_Product_CategoryService.Single(fc["GoodsClassId"].ToInt());
                if (shopClass == null)
                {
                    result.Message = "请选择商品分类";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                string Status = fc["Status"];

                string Categroy1 = fc["Categroy1"];
                string Categroy2 = fc["Categroy2"];
                string Categroy3 = fc["Categroy3"];
                string Categroy4 = fc["Categroy4"];
                string Categroy5 = fc["Categroy5"];

                string Ppacth = "0,";
                if (!string.IsNullOrEmpty(Categroy1))
                {
                    Ppacth += Categroy1 + ",";
                    entity.GoodsClassId = int.Parse(Categroy1);
                }
                if (!string.IsNullOrEmpty(Categroy2))
                {
                    Ppacth += Categroy2 + ",";
                    entity.GoodsClassId = int.Parse(Categroy2);
                }
                if (!string.IsNullOrEmpty(Categroy3))
                {
                    Ppacth += Categroy3 + ",";
                    entity.GoodsClassId = int.Parse(Categroy3);
                }
                if (!string.IsNullOrEmpty(Categroy4))
                {
                    Ppacth += Categroy4 + ",";
                    entity.GoodsClassId = int.Parse(Categroy4);
                }
                if (!string.IsNullOrEmpty(Categroy5))
                {
                    Ppacth += Categroy5 + ",";
                    entity.GoodsClassId = int.Parse(Categroy5);
                }

                TryUpdateModel(entity, fc.AllKeys);
                if (string.IsNullOrEmpty(Ppacth))
                {
                    result.Message = "请选择商品分类";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(entity.ProductName))
                {
                    result.Message = "请输入商品名称";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(fc["ImageUrl"]))
                {
                    result.Message = "请上传商品图片";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(fc["Specifications"]))
                {
                    result.Message = "请输入商品规格";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (entity.RealPrice <= 0)
                {
                    result.Message = "请填写销售价格";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                if (entity.Id > 0)
                {
                    int puid = fc["ID"].ToInt();
                    //entity.SId = myShop.ID;
                    //entity.ShopName = myShop.ShopName;
                    //entity.ClassPath = Ppacth;//shopClass.Ppacth;
                    entity.CreateTime = DateTime.Now;
                    entity.ProductName = fc["ProductName"];
                    entity.ImageUrl = entity.ImageUrl.Replace(";;", ";");
                    //entity.GoodsClassId = fc["gClass"].ToInt();
                    //entity.AfterSsales = fc["AfterSsales"];
                    entity.RealPrice = fc["RealPrice"].ToDecimal();
                    //entity.CostPrice = fc["CostPrice"].ToDecimal();
                    entity.Info = fc["Info"];//简介
                    entity.InfoMation = fc["txtContent"];//详细信息
                    entity.Inclueding = false;
                    entity.Stock = fc["Stock"].ToInt();
                    //entity.Status = Status.ToBool();//是否上架  
                    //entity.IsPass = true;//已审核
                    Shop_ProductService.Update(entity);

                    entity2 = Shop_SPECService.Single(x => x.PID == entity.Id);
                    if (entity2 != null && entity2.Value != fc["Specifications"])
                    {
                        entity2.Value = fc["Specifications"];
                        Shop_SPECService.Update(entity2);
                    }                  
                }
                else
                {
                    entity.SId = 0;// myShop.ID;
                    entity.ShopName = "";// myShop.ShopName;
                    entity.ClassPath = Ppacth; //shopClass.Ppacth;
                    entity.CreateTime = DateTime.Now;
                    //entity.ProductName = fc["ProductName"];
                    entity.ImageUrl = entity.ImageUrl.Replace(";;", ";");
                    //entity.GoodsClassId = fc["gClass"].ToInt();
                    //entity.AfterSsales = fc["AfterSsales"];
                    //entity.RealPrice = fc["RealPrice"].ToDecimal();
                    //entity.CostPrice = fc["CostPrice"].ToDecimal();
                    entity.Info = fc["Info"];//简介
                    entity.InfoMation = fc["txtContent"];//详细信息
                    entity.Inclueding = false;
                    //entity.Stock = fc["Stock"].ToInt();//通用库存
                    entity.Status = Status.ToBool();//是否上架            
                    entity.IsPass = true;//已审核
                    Shop_ProductService.Add(entity);
                    SysDBTool.Commit();

                    int pid = int.Parse(entity.Id.ToString());
                    var action = new JN.Data.Shop_Product_SKU();
                    //var action = Shop_Product_SKUService.SingleAndInit();
                    action.SKU_Name = "通用";
                    //action.ImgUrl = fc["hidMainimgSrc"];
                    action.Stock = fc["Stock"].ToInt();
                    action.Pid = entity.Id.ToInt();
                    action.CreateTime = DateTime.Now;
                    Shop_Product_SKUService.Add(action);
                    SysDBTool.Commit();

                    //添加商品规格
                    entity2.CreateTime = DateTime.Now;
                    entity2.PID = int.Parse(entity.Id.ToString());
                    entity2.Price = decimal.Parse(fc["RealPrice"]);
                    entity2.Value = fc["Specifications"];
                    Shop_SPECService.Add(entity2);
                }
                SysDBTool.Commit();

                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = "出错了，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 图片上传
        //产品主图图片上传
        public ActionResult UpMainPic(string sid)
        {
            string strSid = sid;
            System.Collections.Hashtable hash = UpPic1(strSid);
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传产品主图到文件夹
        /// <param name="dir">文件夹名称 "/Upload//UserSpace/" + userId + "/" + dir + "/";</param>
        /// <param name="userId">订单从表ID</param>
        /// <returns></returns>
        public Hashtable UpPic1(string sid)
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                logs.WriteLog("sid:" + sid);
                string oldLogo = "/Upload/Shop/" + sid + "/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 400, System.Web.HttpContext.Current.Server.MapPath(oldLogo));
                hash["error"] = 0;
                hash["url"] = oldLogo + img;
                logs.WriteLog("上传成功：" + oldLogo + img);
                return hash;
            }
            catch (Exception ex)
            {
                hash["error"] = 1;
                hash["message"] = ex.Message;
                logs.WriteErrorLog("上传失败2：", ex);
                return hash;
            }
        }

        //商品临时图片上传
        public ActionResult UpInfoPic()
        {
            System.Collections.Hashtable hash = UpPic2("Temp");
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传菜品临时图片到文件夹
        /// <param name="dir">文件夹名称 "/Upload//UserSpace/" + userId + "/" + dir + "/";</param>
        /// <param name="userId">订单从表ID</param>
        /// <returns></returns>
        public Hashtable UpPic2(string dir)
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                string oldLogo = "/Upload/Shop/" + dir + "/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 800, System.Web.HttpContext.Current.Server.MapPath(oldLogo));
                hash["error"] = 0;
                hash["url"] = oldLogo + img;
                JN.Data.Shop_Tmp_Pro_Img imgInfo = new JN.Data.Shop_Tmp_Pro_Img();
                imgInfo.CreateTime = DateTime.Now;
                imgInfo.ProId = 0;
                imgInfo.ImgUrl = oldLogo + img;

                Shop_Tmp_Pro_ImgService.Add(imgInfo);
                SysDBTool.Commit();
                return hash;
            }
            catch (Exception ex)
            {
                hash["error"] = 1;
                hash["message"] = ex.Message;
                return hash;
            }
        }

        //商品临时图片列表
        public ActionResult TempPic()
        {
            var refundImages = Shop_Tmp_Pro_ImgService.List(x => x.ProId == 0);
            if (refundImages != null && refundImages.Count() > 0)
            {
                return View(refundImages.ToList());
            }
            return View();
        }

        //插入临时图片到编辑器
        public ActionResult InserTempImg()
        {
            string hidId = Request["hidId"];
            string url = Request["url"];
            string saveUrl = "/Upload/Shop/" + hidId + "/";
            string reUrl = IOHelper.FileCopyNewName(url, saveUrl);
            if (string.IsNullOrEmpty(reUrl))
            {
                return Json(new { result = "插入内容失败！" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "ok", url = saveUrl + reUrl }, JsonRequestBehavior.AllowGet);
            }
        }

        //删除临时内容图片
        public ActionResult DelTempImg()
        {
            string imgId = Request["imgId"];
            int id;
            int.TryParse(imgId, out id);
            JN.Data.Shop_Tmp_Pro_Img model = Shop_Tmp_Pro_ImgService.Single(id);
            if (model != null)
            {
                Shop_Tmp_Pro_ImgService.Delete(id);
                SysDBTool.Commit();
                //IOHelper.DelByMapPath(model.ImgUrl);
                IOHelper.DeleteFile(model.ImgUrl);
            }
            return Content("ok");
        }

        #endregion

        #region 商品上架
        /// <summary>
        /// 商品上架
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult OnProduct(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var product = Shop_ProductService.SingleAndInit(id);
                if (product != null)
                {
                    //商品未审核无法上架
                    if (product.IsPass == false)
                    {
                        result.Message = "商品审核后才能上架！";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    product.Status = true;
                    Shop_ProductService.Update(product);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }

            return Json(result);
        }
        #endregion

        #region 商品下架
        /// <summary>
        /// 商品下架
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult OffProduct(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var product = Shop_ProductService.SingleAndInit(id.ToInt());
                if (product != null)
                {
                    string name = product.ShopName;
                    product.Status = false;
                    Shop_ProductService.Update(product);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 新品
        /// <summary>
        /// 新品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsNew(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsNew == true)
                    {
                        entity.IsNew = false;
                    }
                    else
                    {
                        entity.IsNew = true;
                    }

                    Shop_ProductService.Update(entity);
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

        #region 特价
        /// <summary>
        /// 特价
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsDiscount(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsDiscount == true)
                    {
                        entity.IsDiscount = false;
                    }
                    else
                    {
                        entity.IsDiscount = true;
                    }

                    Shop_ProductService.Update(entity);
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

        #region 明星产品
        /// <summary>
        /// 明星产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsStar(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsStar == true)
                    {
                        entity.IsStar = false;
                    }
                    else
                    {
                        entity.IsStar = true;
                    }

                    Shop_ProductService.Update(entity);
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

        #region 网友喜欢
        /// <summary>
        /// 网友喜欢
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsLike(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsLike == true)
                    {
                        entity.IsLike = false;
                    }
                    else
                    {
                        entity.IsLike = true;
                    }

                    Shop_ProductService.Update(entity);
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

        #region 热评商品
        /// <summary>
        /// 热评商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsF5(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsF5 == true)
                    {
                        entity.IsF5 = false;
                    }
                    else
                    {
                        entity.IsF5 = true;
                    }

                    Shop_ProductService.Update(entity);
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

        #region 爱看图书
        /// <summary>
        /// 爱看图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsF6(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsF6 == true)
                    {
                        entity.IsF6 = false;
                    }
                    else
                    {
                        entity.IsF6 = true;
                    }

                    Shop_ProductService.Update(entity);
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

        #region 皇辰乐题
        /// <summary>
        /// 皇辰乐题
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IsF7(int id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = Shop_ProductService.SingleAndInit(id);
                if (entity != null)
                {
                    if (entity.IsF7 == true)
                    {
                        entity.IsF7 = false;
                    }
                    else
                    {
                        entity.IsF7 = true;
                    }

                    Shop_ProductService.Update(entity);
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

        #region 删除商品 未用
        ///// <summary>
        ///// 删除商品
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public ActionResult Del(int id)
        //{
        //    var model = Shop_ProductService.Single(id);
        //    if (model != null)
        //    {
        //        ActPacket = model;
        //        Shop_ProductService.Delete(id);
        //        SysDBTool.Commit();
        //        ViewBag.SuccessMsg = "商品“" + model.ProductName + "”已被删除！";
        //    }

        //    return Redirect("/AdminCenter/MallManagement/ProductList");
        //}
        #endregion

        #endregion


        #region 获取商品分类
        public ActionResult GetCategroy(int ParentId)
        {
            var childList = MvcCore.Unity.Get<JN.Data.Service.IShop_Product_CategoryService>().List(x => x.ParentId == ParentId);
            if (childList != null && childList.Count() > 0)
            {
                return Content(childList.ToList().ToJson());
            }
            return Content(new JN.Data.Shop_Product_Category().ToJson());
        } 
        #endregion

    }
}