using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using JN.Data.Extensions;
using JN.Services.Tool;
using JN.Services.Manager;
using System.Text.RegularExpressions;
using JN.Services.CustomException;
using System.Data.Entity.Validation;
using System.Collections;
using Webdiyer.WebControls.Mvc;
using System.IO;
using System.Data;
using JN.Data;
using System.Data.SqlClient;
using JN.Services;
using System.Web.Security;

namespace JN.Web.Areas.APP.Controllers
{
    public class UserController : BaseController
    {
        //cookies info
        static string cacheUserName { get { return typeof(Data.User).Name + "_"; } }
        static Data.User _adminloginuser = null;
        static string CookieName_User = (ConfigHelper.GetConfigString("DBName") + "_User").ToMD5();
        static string CookieName_Tonen = (ConfigHelper.GetConfigString("DBName") + "_Tonen").ToMD5();



        private static List<Data.SysParam> cacheSysParam = null;
        private static List<Data.Currency> cacheCurrency = null;
        private readonly IUserService UserService;
        private readonly IAccountRelationService AccountRelationService;

        private readonly IMachineService MachineService;

        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IUserBankCardService UserBankCardService;


        public UserController(ISysDBTool SysDBTool, IUserService UserService, IMachineService MachineService, IAccountRelationService AccountRelationService, IActLogService ActLogService, IUserBankCardService UserBankCardService)
        {
            this.UserService = UserService;
            this.AccountRelationService = AccountRelationService;
            this.SysDBTool = SysDBTool;
            this.MachineService = MachineService;
            this.ActLogService = ActLogService;
            this.UserBankCardService = UserBankCardService;
            cacheSysParam = Services.Manager.CacheHelp.GetSysParamsList();
            cacheCurrency = Services.Manager.CacheHelp.GetCurrencyList();
        }

        #region 我的会员列表
        /// <summary>
        /// 我的会员列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Index()
        {
            ActMessage = "已激活用户列表";
            ViewBag.Title = ActMessage;
            return View();
        }
        /// <summary>
        /// 获取转出记录
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult getMyUserList(int? page)
        {
            int pageSize = 10;
            IQueryable<JN.Data.User> list;
            string isactivation = Request["isactivation"];
            if (!string.IsNullOrEmpty(isactivation))
            {
                bool isactive = (isactivation == "1");
                list = UserService.List(x => x.IsActivation == isactive && ((x.CreateBy == Umodel.UserName && x.ID != Umodel.ID) || x.RefereeID == Umodel.ID));
            }
            else
            {
                list = UserService.List(x => ((x.CreateBy == Umodel.UserName && x.ID != Umodel.ID) || x.RefereeID == Umodel.ID));
            }

            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            return Json(new { data = list.OrderByDescending(x => x.CreateTime).ThenByDescending(x => x.ID).ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 删除用户（未激活状态下）
        /// <summary>
        /// 删除用户，未激活时才可以
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            //只允许删除您推荐的用户
            var onUser = UserService.Single(id);
            if (onUser != null)
            {
                if (Umodel.UserName == onUser.CreateBy || Umodel.AgentID == onUser.AgentID)
                {
                    if (!onUser.IsActivation)
                    {
                        //删除时上级子用户减少
                        if (onUser.ParentID > 0)
                        {
                            Data.User _parentUser = UserService.Single(onUser.ParentID);
                            _parentUser.Child = _parentUser.Child - 1;
                            UserService.Update(_parentUser);
                        }
                        UserService.Delete(id);
                        SysDBTool.Commit();
                        ViewBag.SuccessMsg = "用户已被删除！";
                        ActMessage = "删除用户";
                        return View("Success");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "该用户已被激活，无法删除。";
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "只允许删除您创建的用户。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }
        #endregion

        #region 激活用户
        //public ActionResult doPass()
        //{
        //    return View();
        //}

        [HttpPost]
        /// <summary>
        /// 激活用户，激活用户产生直推奖，见点奖
        /// </summary>
        /// <returns></returns>
        public ActionResult doPass(int uid)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var noUser = UserService.Single(uid);
                if (noUser == null) { throw new CustomException("对不起，用户不存在！"); }

                //    string pincode = fc["pincode"];
                //        if (MvcCore.Unity.Get<JN.Data.Service.IPINCodeService>().List(x => x.UID == Umodel.ID && x.IsUsed == false && x.KeyCode == pincode.Trim()).Count() <= 0)
                //            throw new CustomException("激活码无效！");

                if (Umodel.Wallet2004 < cacheSysParam.Single(x => x.ID == 1001).Value.ToInt()) throw new CustomException("您的激活币数量不足无法激活该用户！");

                //var trans = MvcCore.Unity.Get<IUserService>().Single(x => x.ID == Umodel.RefereeID);
                //if (trans.Count() <= 0) throw new CustomException("您需要通过推荐人来转帐一笔Fc才可以激活");


                if (!noUser.IsActivation)
                {
                    //激活所需要的金币
                    #region 事务操作
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //激活用户
                        Users.ActivationAccount(noUser, Umodel.ID);

                        //  var codeEntity = MvcCore.Unity.Get<JN.Data.Service.IPINCodeService>().Single(x => x.KeyCode == pincode);
                        //  codeEntity.IsUsed = true;
                        //  codeEntity.UseUID = Umodel.ID;
                        //  codeEntity.UseUserName = Umodel.UserName;
                        //  codeEntity.UseTime = DateTime.Now;
                        //  MvcCore.Unity.Get<JN.Data.Service.IPINCodeService>().Update(codeEntity);
                        //  SysDBTool.Commit();

                        ts.Complete();
                        result.Status = 200;
                    }
                    #endregion
                }
                else
                {
                    result.Message = "用户已经激活!";
                }
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 激活子帐号(未用)
        //public ActionResult doSubpass(int id)
        //{
        //    //更新激活标记
        //    TUser onUser = users.GetModel(id);
        //    if (onUser != null)
        //    {
        //        if (Umodel.ID == onUser.MainAccountID || Umodel.ID == onUser.ReserveInt1)
        //        {
        //            if (!onUser.IsActivation)
        //            {
        //                //需再推一个人才可以激活子帐户
        //                //int PARAM_TJRS = 1; // TypeConverter.StrToInt(sysparams.GetModel(2004).Value2);
        //                //if ((users.GetRecordCount("RefereeID=" + Umodel.ID + " and IsSubAccount=0") - onUser.ReserveInt1) >= PARAM_TJRS)

        //                onUser.IsActivation = true;
        //                onUser.ActivationTime = DateTime.Now;
        //                users.Update(onUser);
        //            }
        //            else
        //            {
        //                ViewBag.ErrorMsg = "该子帐号已被激活，请不要重复激活。";
        //                return View("Error");
        //            }
        //        }
        //        else
        //        {
        //            ViewBag.ErrorMsg = "只充许激活您自己的子帐号。";
        //            return View("Error");
        //        }
        //    }
        //    ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
        //    return View("Error");
        //}
        #endregion

        #region 登录日志
        /// <summary>
        /// 登录日志
        /// </summary>
        /// <returns></returns>
        public ActionResult loginlog(int? page)
        {
            ViewBag.Title = "登录日志";
            ActMessage = ViewBag.Title;
            var list = ActLogService.List(x => x.UID == Umodel.ID && x.ActContent.Contains("登录成功")).OrderByDescending(x => x.ID).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            if (Request.IsAjaxRequest())
                return View("_loginlog", list.ToPagedList(page ?? 1, 20));

            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 重置登录密码
        /// <summary>
        /// 重置登录密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ResetPassword()
        {
            ViewBag.Title = "重置登录密码";
            ActMessage = ViewBag.Title;
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(FormCollection fc)
        {
            Result result = new Result();
            result = JN.Services.Manager.Users.ResetPwdMethod(fc, Umodel, UserService, SysDBTool);
            return Json(result);
        }

        public ActionResult SendMobileResetTran()
        {
            Data.Extensions.Result result = new Data.Extensions.Result();
            result = SMSValidateCode.SendMobileResetTran(Umodel, cacheSysParam);
            return Json(result);
        }
        #endregion

        #region 设置交易密码
        /// <summary>
        /// 设置交易密码
        /// </summary>
        /// <returns></returns>
        public ActionResult SetTransactionPassword()
        {
            if (!string.IsNullOrEmpty(Umodel.Password2))
            {
                return Redirect("/App/home");
            }

            ViewBag.Title = "设置交易密码";
            ActMessage = ViewBag.Title;
            return View();
        }
        [HttpPost]
        public ActionResult SetTransactionPassword(FormCollection fc)
        {
            Data.Extensions.Result result = new Data.Extensions.Result();
            result = Users.SetPwd2Method(fc, Umodel, UserService, SysDBTool);
            return Json(result);
        }
        #endregion

        #region 重置交易密码
        /// <summary>
        /// 重置交易密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ResetTransactionPassword()
        {
            ViewBag.Title = "重置交易密码";
            ActMessage = ViewBag.Title;
            return View();
        }
        [HttpPost]
        public ActionResult ResetTransactionPassword(FormCollection fc)
        {
            Data.Extensions.Result result = new Data.Extensions.Result();
            result = Users.ResetPwd2Method(fc, Umodel, UserService, SysDBTool);
            return Json(result);
        }


        public ActionResult SendMobileLoginPwd()
        {
            Data.Extensions.Result result = new Data.Extensions.Result();
            result = SMSValidateCode.SendMobileLoginPwd(Umodel, cacheSysParam);
            return Json(result);
        }
        #endregion

        #region 银行卡管理
        /// <summary>
        /// 银行卡列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult BankList()
        {
            ActMessage = "银行卡列表";
            ActMessage = ViewBag.Title;
            return View();
        }

        /// <summary>
        /// 获取银行卡列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult GetBankList(int? page)
        {
            int pageSize = 10;
            var list = UserBankCardService.List(x => x.UID == Umodel.ID).OrderByDescending(x => x.ID);
            int totalPage = (int)System.Math.Ceiling((double)list.Count() / pageSize);//总页数
            var listdata = list.ToPagedList(page ?? 1, pageSize);//取数据

            return Json(new { data = listdata, pages = totalPage }, JsonRequestBehavior.AllowGet);
            //return Json(new { data = list.ToPagedList(page ?? 1, pageSize), pages = totalPage }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加/修改银行卡
        /// </summary>
        /// <returns></returns>
        public ActionResult BankSettings()
        {
            ActMessage = "添加/绑定银行卡";
            ViewBag.Title = ActMessage;
            int bankid = Request["bankid"].ToInt();
            JN.Data.UserBankCard bankmodel = UserBankCardService.Single(x => x.UID == Umodel.ID && x.ID == bankid);
            if (bankmodel == null)
            {
                bankmodel = new JN.Data.UserBankCard() { ID = 0 };
            }
            return View(bankmodel);
        }
        /// <summary>
        /// 添加/修改银行卡
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BankSubmit(FormCollection fc, string bankimg)
        {
            Result result = new Result();
            result = Users.DoBankSettings(fc, Umodel, UserBankCardService, SysDBTool, bankimg);
            return Json(result);
        }

        /// <summary>
        /// 添加银行卡验证手机
        /// </summary>
        /// <returns></returns>
        public ActionResult SendMobileBank()
        {
            Result result = new Result();
            result = Users.SendMobileMsm(Umodel, "Bank");
            return Json(result);
        }

        /// <summary>
        /// 设置默认银行卡
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetDefaultBankCard(int id)
        {
            ActMessage = "设置默认银行卡";
            ActMessage = ViewBag.Title;
            Result r = new Result();
            r = Users.DoSetDefaultBankCard(id, Umodel, UserBankCardService, SysDBTool);
            return Json(r);
        }

        /// <summary>
        /// 解绑银行卡
        /// </summary>
        /// <returns></returns>
        public ActionResult BankDeleteSubmit(int id)
        {
            ActMessage = "解绑银行卡";
            ActMessage = ViewBag.Title;
            Result r = new Result();
            r = Users.DeleteBank(id, Umodel, UserBankCardService, SysDBTool); //添加修改操作
            return Json(r);
        }

        #endregion

        #region 添加一个用户
        public ActionResult Add()
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sys.IsRegisterOpen)
            {
                ViewBag.ErrorMsg = sys.CloseRegisterHint;
                return View("Error");
            }
            ViewBag.NewUserName = "C" + Users.GetUserCode(7);

            ViewBag.Title = "添加一个新用户";
            ActMessage = ViewBag.Title;

            ViewBag.CurrentUser = Umodel.UserName;
            if (Umodel.IsAgent ?? false)
                ViewBag.AgentName = Umodel.AgentName;
            else
                ViewBag.AgentName = Umodel.AgentUser;

            int _parentid = Request["ParentID"].ToInt();

            if (UserService.List(x => x.ID == _parentid && x.IsActivation).Count() > 0)
                ViewBag.ParentUser = UserService.Single(_parentid).UserName;

            return View();
        }

        [HttpPost]
        public ActionResult Add(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();

            if (Umodel.IsLock) throw new CustomException("您的帐号受限，无法进行相关操作");
            Result r = new Result();
            var entity = new Data.User();
            TryUpdateModel(entity, fc.AllKeys);
            r = Users.AddUser(fc, UserService, entity);
            return Json(r);
        }
        #endregion

        #region 实名验证
        public ActionResult NameAuth()
        {
            ActMessage = "实名认证";
            ViewBag.Title = ActMessage;
            return View(Umodel);
        }
        [HttpPost]
        public ActionResult NameAuth(FormCollection fc)
        {
            Result result = new Result();
            string imgurl = fc["imagesurl"];
            result = JN.Services.Manager.Users.NameAuth(fc, Umodel, UserService, imgurl);
            return Json(result);
        }
        #endregion

        #region 邮箱验证
        public ActionResult EmailSettings()
        {
            ActMessage = "邮箱验证";
            ViewBag.Title = ActMessage;
            return View(Umodel);
        }

        [HttpPost]
        public ActionResult EmailSettings(FormCollection fc)
        {
            Data.Extensions.Result result = new Data.Extensions.Result();

            result = Users.EmailSettings(fc, UserService, Umodel);

            return Json(result);
        }

        public ActionResult SendEmail()
        {
            Data.Extensions.Result result = new Data.Extensions.Result();
            result = Users.SendEmail(Umodel);
            return Json(result);
        }
        #endregion

        #region 手机验证

        /// <summary>
        /// 验证手机号
        /// </summary>
        /// <returns></returns>
        public ActionResult vphone()
        {
            string phone = Request["phone"];
            if (Umodel.Mobile != phone)
            {
                return Json(new { Status = 500, Message = "对不起，你的手机号不存在！" });
            }
            else
            {
                return Json(new { Status = 200, Message = "验证成功！" });
            }

        }

        public ActionResult PhoneSettings()
        {
            ActMessage = "手机验证";
            ViewBag.Title = ActMessage;
            return View(Umodel);
        }

        [HttpPost]
        public ActionResult PhoneSettings(FormCollection fc)
        {
            Result result = new Result();
            result = JN.Services.Manager.Users.PhoneSettingsMethod(fc, Umodel, UserService, SysDBTool);
            return Json(result);
        }

        public ActionResult SendSetMobileMsm()
        {
            Result result = new Result();
            string phone = Request["phone"];
            string countrycode = Request["countrycode"];//国家区号
            result = SMSValidateCode.SendSetMobileMsm(countrycode, phone, UserService, cacheSysParam);
            return Json(result);
        }

        #endregion

        #region 个人信息
        public ActionResult MyInfo()
        {
            ViewBag.Title = "个人信息";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }

        /// <summary>
        /// 修改头像和基本信息
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MyInfo(FormCollection fc)
        {
            ViewBag.Title = "个人信息提交";
            ActMessage = ViewBag.Title;
            Result result = new Result();

            string info = Request["intro"];
            string RealName = Request["RealName"];
            string IDCard = Request["IDCard"];
            string AliPay = Request["AliPay"];
            string WeiXin = Request["WeiXin"];
            string BankName = Request["BankName"];
            string BankCard = Request["BankCard"];
            string BankOfDeposit = Request["BankOfDeposit"];
            string BankUser = Request["BankUser"];

            HttpPostedFileBase file = Request.Files["face"];
            string imgurl = "";
            try
            {
                //图片1
                if (((file != null) && (file.ContentLength > 0)))
                {
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
                        UserService.Update(Umodel);
                        SysDBTool.Commit();
                        throw new CustomException("试图上传木马文件，您的帐号已被冻结");
                    }
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                        throw new CustomException("非法上传，您只可以上传图片格式的文件！");
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();


                    if (!Directory.Exists(Request.MapPath("~/Upload/face")))
                        Directory.CreateDirectory(Request.MapPath("~/Upload/face"));

                    var fileName = Path.Combine(Request.MapPath("~/Upload/face"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        //判断是否是图片，排除木马 (用image对象判断是否为图片)
                        if (!JN.Services.Tool.FileValidation.IsImage(fileName))
                        {
                            System.IO.File.Delete(fileName); //删除原文件
                            throw new Exception("您上传的不是图片，如果你上传多次不是图片的文件，系统将识别为木马文件，系统将自动冻结您的账号");
                        }

                        //判断是否是图片，排除木马 (判断文件头，文件流)
                        if (!JN.Services.Tool.FileValidation.IsPicture(fileName))
                        {
                            System.IO.File.Delete(fileName); //删除原文件
                            throw new Exception("您上传的不是图片，如果你上传多次不是图片的文件，系统将识别为木马文件，系统将自动冻结您的账号");
                        }
                        imgurl += "/Upload/face/" + newfilename;
                        Umodel.HeadFace = imgurl;
                    }
                    catch (Exception ex)
                    {
                        throw new CustomException("上传失败：" + ex.Message);
                    }

                }
                if(UserService.Single(x=>x.IDCard== IDCard.Trim() && x.ID!=Umodel.ID)!=null) throw new CustomException("身份证号已被使用");
                Umodel.RealName = RealName;
                Umodel.IDCard = IDCard;
                //Umodel.AliPay = AliPay;
                //Umodel.WeiXin = WeiXin;
                //Umodel.BankName = BankName;
                //Umodel.BankCard = BankCard;
                //Umodel.BankOfDeposit = BankOfDeposit;
                //Umodel.BankUser = BankUser;

                Umodel.UserInfo = info;
                UserService.Update(Umodel);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 个人中心
        public ActionResult Personal()
        {
            ActMessage = "个人";

            var advertiseOrderList = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().List(x => System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) <= 30 && x.Status == (int)Data.Enum.AdvertiseOrderStatus.Completed).ToList();
            
            var refererList = UserService.List(x => (x.RefereePath.Contains("," + Umodel.ID + ",") || x.RefereeID == Umodel.ID) && x.IsActivation);  //获得用户所有子用户用户集合

            ViewBag.ReShowBuyTradeNum = refererList.Sum(x => x.ShowBuyTradeNum);
            ViewBag.ReShowSellTradeNum = refererList.Sum(x => x.ShowSellTradeNum);

            string ids = ",";
            foreach (var team in refererList)
            {
                ids += team.ID + ",";
            }

            decimal reNum = 0;
            //旗下会员释放记录
            var reAdvertiseOrde = advertiseOrderList.Where(x => ids.Contains("," + x.BuyUID.ToString() + ",")).ToList();
            if (reAdvertiseOrde.Count > 0 && reAdvertiseOrde != null)
            {
                reNum = reAdvertiseOrde.Sum(x => x.Quantity);
            }
            ViewBag.ReAdvertiseNum = reNum;

            return View();
        }
        #endregion

        #region 我的矿机
        public ActionResult Mymine()
        {
            ViewBag.Title = "我的矿机";
            return View();
        }
        #endregion

        #region 挖矿记录
        public ActionResult Miningrecord()
        {
            ViewBag.Title = "挖矿记录";
            return View();
        }
        #endregion

        #region 矿机数据
        public ActionResult Miningdata(int id)
        {
            ViewBag.Title = "矿机数据";

            return View();
        }
        #endregion

        #region 矿机加载进度
        public ActionResult Progress()
        {
            ViewBag.Title = "矿机加载进度";
            return View();
        }
        #endregion

        #region 币币信息
        public ActionResult Coinmarketcap()
        {
            ViewBag.Title = "币币信息";
            if (!MvcCore.Extensions.CacheExtensions.CheckCache("curPriceList"))
            {
                Wallets.GetPriceList();
            }
            return View();
        }
        #endregion

        #region 采矿
        public ActionResult Mining()
        {
            ViewBag.Title = "采矿";
            return View();
        }
        #endregion

        #region 采矿2
        public ActionResult Mining2()
        {
            ViewBag.Title = "采矿2";
            int moID = Request["moID"].ToInt();
            var model = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().SingleAndInit(x => x.ID == moID);
            if (model == null)
            {
                ViewBag.ErrorMsg = "矿机不存在。";
                return View("Error");
            }
            //var product = MvcCore.Unity.Get<IShopProductService>().Single(model.ProductID);
            //model.ImageUrl = product.ImageUrl;
            return View(model);
        }
        #endregion

        #region 买卖数据
        public ActionResult Business()
        {
            var advertiseOrderList = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().List(x => System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) <= 30 && x.Status == (int)Data.Enum.AdvertiseOrderStatus.Completed).ToList();



            var refererList = UserService.List(x => (x.RefereePath.Contains("," + Umodel.ID + ",") || x.RefereeID == Umodel.ID) && x.IsActivation);  //获得用户所有子用户用户集合

            ViewBag.ReShowBuyTradeNum = refererList.Sum(x => x.ShowBuyTradeNum);
            ViewBag.ReShowSellTradeNum = refererList.Sum(x => x.ShowSellTradeNum);

            string ids = ",";
            foreach (var team in refererList)
            {
                ids += team.ID + ",";
            }

            decimal reNum = 0;
            //旗下会员释放记录
            var reAdvertiseOrde = advertiseOrderList.Where(x => ids.Contains("," + x.BuyUID.ToString() + ",")).ToList();
            if (reAdvertiseOrde.Count > 0 && reAdvertiseOrde != null)
            {
                reNum = reAdvertiseOrde.Sum(x => x.Quantity);
            }
            ViewBag.ReAdvertiseNum = reNum;


            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List(x => System.Data.Entity.SqlServer.SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0 && x.UID==Umodel.ID).ToList();
            ViewBag.teamAchievement = teamAchievementList.Count() > 0 ? teamAchievementList.OrderByDescending(x => x.CreateTime).FirstOrDefault() : new TeamAchievement();

            return View();
        }
        #endregion


        #region 验证用户信息(返回JSON)
        /// <summary>
        /// 验证用户信息(返回JSON)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult CheckUserInfo(string username)
        {
            var model = UserService.Single(x => x.UserName.ToLower() == username.ToLower());
            if (model != null)
            {
                string okmsg = "";// "用户名：" + model.UserName + "";
                //if (model.RealName.Length > 0)
                //    okmsg += "，姓名：*" + model.RealName.Substring(1, model.RealName.Length - 1);

                // if (model.Mobile.Length > 7)
                //     okmsg += "，电话：" + model.Mobile.Substring(0,3) + "****" + model.Mobile.Substring(7, model.Mobile.Length - 7);

                return Json(new { result = "ok", refMsg = "用户验证成功，" + okmsg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "err", refMsg = "该用户不存在" }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult CheckUserInfo2(string username)
        {
            var ulist = Users.GetAllRefereeChild(Umodel);
            var model = ulist.Where(x => x.UserName.ToLower().Equals(username.ToLower())).FirstOrDefault();
            if (model != null)
            {
                string okmsg = "用户名：" + model.UserName + ", 邮箱：" + model.Email;
                //if (model.RealName.Length > 0)
                //    okmsg += "，姓名：*" + model.RealName.Substring(1, model.RealName.Length - 1);

                //if (model.Mobile.Length > 7)
                //    okmsg += "，电话：" + model.Mobile.Substring(0, 3) + "****" + model.Mobile.Substring(7, model.Mobile.Length - 7);

                return Json(new { result = "ok", refMsg = "用户验证成功，" + okmsg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "err", refMsg = "该用户不存在" }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult CheckUserName(string username)
        {
            var model = UserService.Single(x => x.UserName.ToLower() == username.ToLower());
            if (model != null)
            {
                string okmsg = "用户名已被使用";
                return Json(new { result = "err", refMsg = okmsg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "ok", refMsg = "恭喜您，该用户名可以使用" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region 树视图（太阳线）
        /// <summary>
        /// 树状视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult TreeView()
        {
            if (!Umodel.IsActivation) return RedirectToAction("doPass", "User");
            ViewBag.Title = "用户图谱";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }


        /// <summary>
        /// 获取树状用户数据json格式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetTreeJson(int id)
        {
            //int allchild;
            int child;
            IList<TreeNode> list = new List<TreeNode>();
            int tjrs = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.RefereeID == Umodel.ID && x.IsActivation).Count();
            tjrs = Math.Min(7, tjrs);
            //取所有父ID与参数相符数据封装到 List<TUser> 并以JSON格式返回
            var ulst = UserService.List(x => x.RefereeID == id).ToList();
            foreach (var mt in ulst)
            {
                child = UserService.List(x => x.RefereeID == mt.ID).Count();
                //allchild = users.GetRecordCount("ParentPath like '%," + mt.ID.ToString() + ",%' ") + child;
                TreeNode p = new TreeNode();
                p.id = mt.ID;
                if (mt.IsActivation)
                {
                    if (mt.IsAgent ?? false)
                        p.text = "<i style='color:#f00'>" + mt.UserName + "(经理" + mt.RealName + ",推荐" + child + "人)</i>";
                    else
                        p.text = "" + mt.UserName + "(" + mt.RealName + ",推荐" + child + "人,"+ typeof(JN.Data.Enum.UserLevel).GetEnumDesc(mt.UserLevel) + ")";
                }
                else
                    p.text = "<em style='color:#999'>" + mt.UserName + "(未激活)</em>";

                if (mt.RefereeID == 0)
                    p.type = "root";

                if (child > 0 && mt.RefereeDepth < (Umodel.RefereeDepth + tjrs))
                {
                    p.icon = "fa fa-users";
                    p.children = true;
                }
                else
                {
                    p.icon = "fa fa-user";
                    //p.state = "disabled=true";
                    p.children = false;
                }
                list.Add(p);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 列表视图（太阳线）
        /// <summary>
        /// 列表视图
        /// </summary>
        /// <param name="df">日期字段</param>
        /// <param name="dr">日期范围</param>
        /// <param name="kf">查询字段</param>
        /// <param name="kv">查询关键字</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public ActionResult ListView(int? page)
        {
            ActMessage = "自属业绩";
            var list = UserService.List(x => x.RefereeID == Umodel.ID && x.IsActivation).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 双轨视图
        [HttpPost]
        public ActionResult DoubleTrack(FormCollection form)
        {
            string kv = form["kv"];

            if (UserService.List(x => x.UserName == kv).Count() > 0)
            {
                return RedirectToAction("DoubleTrack", new { ID = UserService.Single(x => x.UserName == kv).ID });
            }
            else
            {
                ViewBag.ErrorMsg = "查询的用户不存在。";
                return View("Error");
            }
        }
        /// <summary>
        /// 双轨视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DoubleTrack(int parentid = 0)
        {

            var model = parentid == 0 ? Umodel : UserService.Single(parentid);

            ViewBag.Title = "用户双轨视图";
            ActMessage = ViewBag.Title;

            if (model == null)
            {
                ViewBag.ErrorMsg = "查询的用户不存在。";
                return View("Error");
            }

            if (model != Umodel && !(model.ParentPath + ",").Contains("," + Umodel.ID + ","))
            {
                ViewBag.ErrorMsg = "非法查询请求。";
                return View("Error");
            }

            var lst = new List<Data.User>();
            if (model != null)
            {
                lst.Add(model);

                //第一层
                var U11 = UserService.Single(x => x.ParentID == model.ID && x.ChildPlace == 1);
                if (U11 == null) U11 = new Data.User { ID = -1, ParentID = model.ID };
                lst.Add(U11);

                var U12 = UserService.Single(x => x.ParentID == model.ID && x.ChildPlace == 2);
                if (U12 == null) U12 = new Data.User { ID = -1, ParentID = model.ID };
                lst.Add(U12);


                //第二层
                var U21 = UserService.Single(x => x.ParentID == U11.ID && x.ChildPlace == 1);
                if (U21 == null) U21 = new Data.User { ID = -1, ParentID = U11.ID };
                lst.Add(U21);

                var U22 = UserService.Single(x => x.ParentID == U11.ID && x.ChildPlace == 2);
                if (U22 == null) U22 = new Data.User { ID = -1, ParentID = U11.ID };
                lst.Add(U22);

                var U23 = UserService.Single(x => x.ParentID == U12.ID && x.ChildPlace == 1);
                if (U23 == null) U23 = new Data.User { ID = -1, ParentID = U12.ID };
                lst.Add(U23);

                var U24 = UserService.Single(x => x.ParentID == U12.ID && x.ChildPlace == 2);
                if (U24 == null) U24 = new Data.User { ID = -1, ParentID = U12.ID };
                lst.Add(U24);

                //第三层
                var U31 = UserService.Single(x => x.ParentID == U21.ID && x.ChildPlace == 1);
                if (U31 == null) U31 = new Data.User { ID = -1, ParentID = U21.ID };
                lst.Add(U31);

                var U32 = UserService.Single(x => x.ParentID == U21.ID && x.ChildPlace == 2);
                if (U32 == null) U32 = new Data.User { ID = -1, ParentID = U21.ID };
                lst.Add(U32);

                var U33 = UserService.Single(x => x.ParentID == U22.ID && x.ChildPlace == 1);
                if (U33 == null) U33 = new Data.User { ID = -1, ParentID = U22.ID };
                lst.Add(U33);

                var U34 = UserService.Single(x => x.ParentID == U22.ID && x.ChildPlace == 2);
                if (U34 == null) U34 = new Data.User { ID = -1, ParentID = U22.ID };
                lst.Add(U34);

                var U35 = UserService.Single(x => x.ParentID == U23.ID && x.ChildPlace == 1);
                if (U35 == null) U35 = new Data.User { ID = -1, ParentID = U23.ID };
                lst.Add(U35);

                var U36 = UserService.Single(x => x.ParentID == U23.ID && x.ChildPlace == 2);
                if (U36 == null) U36 = new Data.User { ID = -1, ParentID = U23.ID };
                lst.Add(U36);

                var U37 = UserService.Single(x => x.ParentID == U24.ID && x.ChildPlace == 1);
                if (U37 == null) U37 = new Data.User { ID = -1, ParentID = U24.ID };
                lst.Add(U37);

                var U38 = UserService.Single(x => x.ParentID == U24.ID && x.ChildPlace == 2);
                if (U38 == null) U38 = new Data.User { ID = -1, ParentID = U24.ID };
                lst.Add(U38);

                //第四层
                var U41 = UserService.Single(x => x.ParentID == U31.ID && x.ChildPlace == 1);
                if (U41 == null) U41 = new Data.User { ID = -1, ParentID = U31.ID };
                lst.Add(U41);

                var U42 = UserService.Single(x => x.ParentID == U31.ID && x.ChildPlace == 2);
                if (U42 == null) U42 = new Data.User { ID = -1, ParentID = U31.ID };
                lst.Add(U42);

                var U43 = UserService.Single(x => x.ParentID == U32.ID && x.ChildPlace == 1);
                if (U43 == null) U43 = new Data.User { ID = -1, ParentID = U32.ID };
                lst.Add(U43);

                var U44 = UserService.Single(x => x.ParentID == U32.ID && x.ChildPlace == 2);
                if (U44 == null) U44 = new Data.User { ID = -1, ParentID = U32.ID };
                lst.Add(U44);

                var U45 = UserService.Single(x => x.ParentID == U33.ID && x.ChildPlace == 1);
                if (U45 == null) U45 = new Data.User { ID = -1, ParentID = U33.ID };
                lst.Add(U45);

                var U46 = UserService.Single(x => x.ParentID == U33.ID && x.ChildPlace == 2);
                if (U46 == null) U46 = new Data.User { ID = -1, ParentID = U33.ID };
                lst.Add(U46);

                var U47 = UserService.Single(x => x.ParentID == U34.ID && x.ChildPlace == 1);
                if (U47 == null) U47 = new Data.User { ID = -1, ParentID = U34.ID };
                lst.Add(U47);

                var U48 = UserService.Single(x => x.ParentID == U34.ID && x.ChildPlace == 2);
                if (U48 == null) U48 = new Data.User { ID = -1, ParentID = U34.ID };
                lst.Add(U48);

                var U49 = UserService.Single(x => x.ParentID == U35.ID && x.ChildPlace == 1);
                if (U49 == null) U49 = new Data.User { ID = -1, ParentID = U35.ID };
                lst.Add(U49);

                var U50 = UserService.Single(x => x.ParentID == U35.ID && x.ChildPlace == 2);
                if (U50 == null) U50 = new Data.User { ID = -1, ParentID = U35.ID };
                lst.Add(U50);

                var U51 = UserService.Single(x => x.ParentID == U36.ID && x.ChildPlace == 1);
                if (U51 == null) U51 = new Data.User { ID = -1, ParentID = U36.ID };
                lst.Add(U51);

                var U52 = UserService.Single(x => x.ParentID == U36.ID && x.ChildPlace == 2);
                if (U52 == null) U52 = new Data.User { ID = -1, ParentID = U36.ID };
                lst.Add(U52);

                var U53 = UserService.Single(x => x.ParentID == U37.ID && x.ChildPlace == 1);
                if (U53 == null) U53 = new Data.User { ID = -1, ParentID = U37.ID };
                lst.Add(U53);

                var U54 = UserService.Single(x => x.ParentID == U37.ID && x.ChildPlace == 2);
                if (U54 == null) U54 = new Data.User { ID = -1, ParentID = U37.ID };
                lst.Add(U54);

                var U55 = UserService.Single(x => x.ParentID == U38.ID && x.ChildPlace == 1);
                if (U55 == null) U55 = new Data.User { ID = -1, ParentID = U38.ID };
                lst.Add(U55);

                var U56 = UserService.Single(x => x.ParentID == U38.ID && x.ChildPlace == 2);
                if (U56 == null) U56 = new Data.User { ID = -1, ParentID = U38.ID };
                lst.Add(U56);

                return View(lst);
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
        #endregion

        #region 设置
        /// <summary>
        /// 设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting(int? page)
        {
            ViewBag.Title = "设置";
            return View();
        }
        #endregion

        #region 更换头像

        [HttpPost]
        public ActionResult headFace(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var onUser = UserService.Single(Umodel.ID);
                #region 上传图片
                HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (file != null)
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP })) throw new CustomException("非法上传，您只可以上传图片格式的文件！");
                    string filePath = string.Format("/Upload/HeadFace/{0}/", Umodel.ID);
                    //20160711安全更新 ---------------- start
                    var newfilename = "headFace_" + Umodel.UserName + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(Request.MapPath(filePath)))
                        Directory.CreateDirectory(Request.MapPath(filePath));

                    if (Path.GetExtension(file.FileName).ToLower().Contains("aspx"))
                    {
                        var wlog = new Data.WarningLog();
                        wlog.CreateTime = DateTime.Now;
                        wlog.IP = Request.UserHostAddress;
                        if (Request.UrlReferrer != null)
                            wlog.Location = Request.UrlReferrer.ToString();
                        wlog.Platform = "会员";
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
                        throw new Exception("上传失败：" + ex.Message);
                    }
                    finally
                    {
                        System.IO.File.Delete(fileName); //删除原文件
                    }
                    //20160711安全更新  --------------- end
                }
                #endregion
                onUser.HeadFace = imgurl;
                UserService.Update(onUser);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 身份证图片上传
        //身份证图片上传
        public ActionResult UpMainPic(string uid)
        {
            string strUid = uid;
            System.Collections.Hashtable hash = UpPic1(strUid);
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传身份证图片到文件夹
        /// <param name="dir">文件夹名称 "/Upload/IDCartInfo/" + userId + "/" + dir + "/";</param>
        /// <param name="uid">用户ID</param>
        /// <returns></returns>
        public Hashtable UpPic1(string uid)
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            try
            {
                logs.WriteLog("sid:" + uid);
                string oldLogo = "/Upload/IDCartInfo/" + uid + "/";
                string img = UploadPic.MvcUpload(imgFile, new string[] { ".png", ".gif", ".jpg" }, 1024 * 1024, System.Web.HttpContext.Current.Server.MapPath(oldLogo));
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
        #endregion

        #region 设置安全口令
        //申请成为社区代表
        public ActionResult SettingPassword3()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SettingPassword3(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string Password3 = form["Password3"];
                if (!String.IsNullOrEmpty(Umodel.Password3)) throw new CustomException("您已经设置过安全口令");
                if (String.IsNullOrEmpty(Password3)) throw new CustomException("请输入安全口令");
                if (Password3.Length < 6 || Password3.Length > 20) throw new CustomException("请设置6-20位数的安全口令");
                Umodel.Password3 = Password3.ToMD5().ToMD5();
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
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 修改手机号码
        //申请成为社区代表
        public ActionResult SettingMobile()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SettingMobile(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string Mobile = form["Mobile"];
                string securityPassword = form["securityPassword"];
                if (String.IsNullOrEmpty(Umodel.Password3)) throw new CustomException("您还未设置安全口令");
                if (String.IsNullOrEmpty(securityPassword)) throw new CustomException("请输入安全口令");
                if (Umodel.Password3 != securityPassword.ToMD5().ToMD5()) throw new CustomException("安全口令不正确");


                if (cacheSysParam.SingleAndInit(x => x.ID == 3505).Value.ToInt() == 1)  //开启注册手机短信验证
                {
                    string smscode = form["smscode"];
                    if (string.IsNullOrEmpty(smscode)) throw new JN.Services.CustomException.CustomException("请输入手机验证码");
                    if (Session["SMSRegValidateCode"] == null || smscode.Trim().ToLower() != Session["SMSRegValidateCode"].ToString().ToLower()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                    if (Session["GetSMSRegUser"] == null || Mobile != Session["GetSMSRegUser"].ToString()) throw new JN.Services.CustomException.CustomException("手机号码与接收验证码的设备不相符");
                }

                //更新申请状态
                Umodel.Mobile = Mobile;
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
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 我的资产
        //申请成为社区代表
        public ActionResult MyAssets()
        {
            return View();
        }
        #endregion

        #region 修改登陆密码
        //申请成为社区代表
        public ActionResult SettingPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SettingPassword(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string password = form["password"];
                string securityPassword = form["securityPassword"];
                if (String.IsNullOrEmpty(Umodel.Password3)) throw new CustomException("您还未设置安全口令");
                if (String.IsNullOrEmpty(password)) throw new CustomException("请输入新登录密码");
                if (String.IsNullOrEmpty(securityPassword)) throw new CustomException("请输入安全口令");
                if (Umodel.Password3 != securityPassword.ToMD5().ToMD5()) throw new CustomException("安全口令不正确");
                //更新申请状态
                Umodel.Password = password.Trim().ToMD5().ToMD5();
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
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 修改支付密码
        //申请成为社区代表
        public ActionResult SettingPassword2()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SettingPassword2(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string password2 = form["password2"];
                string securityPassword = form["securityPassword"];
                if (String.IsNullOrEmpty(Umodel.Password3)) throw new CustomException("您还未设置安全口令");
                if (String.IsNullOrEmpty(password2)) throw new CustomException("请输入新交易密码");
                if (String.IsNullOrEmpty(securityPassword)) throw new CustomException("请输入安全口令");
                if (Umodel.Password3 != securityPassword.ToMD5().ToMD5()) throw new CustomException("安全口令不正确");
                //更新申请状态
                Umodel.Password2 = password2.Trim().ToMD5().ToMD5();
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
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion

        #region 邀请好友

        /// <summary>
        /// 邀请好友
        /// </summary>
        /// <returns></returns>
        public ActionResult Invite()
        {
            ViewBag.Title = "邀请好友";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }
        #endregion

        #region 注册新会员
        public ActionResult Register()
        {
            ViewBag.Title = "注册新会员";
            ViewBag.username = Umodel.UserName;
            return View();
        }
        #endregion

        #region 礼品中心

        /// <summary>
        /// 礼品中心
        /// </summary>
        /// <returns></returns>
        public ActionResult Rewardcenter()
        {
            ViewBag.Title = "礼品中心";
            ActMessage = ViewBag.Title;
            return View();
        }
        #endregion

        #region 信任您的人和您屏蔽的人
        /// <summary>
        /// 信任您的人
        /// </summary>
        /// <returns></returns>
        public ActionResult UserTrusted(int? page)
        {
            ActMessage = "信任您的人";
            string[] ids = (Umodel.TrustPath ?? "").Split(',');
            var list = UserService.List(x => ids.Contains(x.ID.ToString())).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 您信任的人
        /// </summary>
        /// <returns></returns>
        public ActionResult UserTrusting(int? page)
        {
            ActMessage = "您信任的人";
            var list = UserService.List(x => (x.TrustPath + ",").Contains("," + Umodel.ID + ",")).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 您屏蔽的人
        /// </summary>
        /// <returns></returns>
        public ActionResult UserBlocking(int? page)
        {
            ActMessage = "您屏蔽的人";
            var list = UserService.List(x => (x.IgnorePath + ",").Contains("," + Umodel.ID + ",")).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 合成图片，生成有背景图的二维码

        /// <summary>
        /// 合成图片，生成有背景图的二维码
        /// </summary>
        /// <returns></returns>
        public ActionResult returnImage()
        {
            string yimages = "/theme/app/images/qr-img.jpg";  //获取图片
            string url = Request["m"];      //获取二维码数据
            //创建名字
            string name = Umodel.RealName;
            ValidateCode vCode = new ValidateCode();
            //byte[] bytes = vCode.CreateImage(name);
            //byte[] bytes = ValidateWhiteBlackImgCode.Img(name, 200,75);

            //CodeRend cd = new CodeRend();
            //string code = name;
            ////Session["AdminValidateCode"] = code.ToLowerInvariant();
            //System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //System.Drawing.Bitmap image = cd.CreatenameImageCode(code);
            //image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //return File(ms.GetBuffer(), "image/JPEG");
            // byte[] bytes = ms.GetBuffer();

            //生成名字图片大小          
            int fSize = 25;
            int Padding = 2;
            int fWidth = fSize + Padding + 6;
            int imageWidth = (int)(fWidth) + 10 + Padding * 2;
            //int imageHeight = fSize * 2 + Padding;
            int imageHeight = fSize + Padding;

            byte[] mimages = MvcCore.Extensions.QRCodeExtensions.ToQRCode(url, 5, 5);  //将数据转换成二维码数据流
            byte[] img = JN.Services.Manager.Users.CombinImage(yimages, mimages, null, imageWidth, imageHeight);  //将背景图片路径和二维码数据流和用户名数据流传入 得到合成图片的数据流
            return File(img, @"image/jpeg");//将得到的数据流转成图片（和验证码的用法一样）
        }
        #endregion

        #region 绑定用户 目标A系统
        public ActionResult BingUser()
        {
            ActMessage = "绑定账号";
            return View();
        }

        [HttpPost]
        public ActionResult BingUser(FormCollection form)
        {
            Result result = new Result();
            result = JN.Services.Manager.Users.BingUserMethod(form, Umodel, UserService, SysDBTool);
            return Json(result);
        }

        #region 绑定账户验证码
        public ActionResult SendBindUserMobileMsm()
        {
            Result result = new Result();
            result = SMSValidateCode.SendBindUserMobileMsm(Umodel, cacheSysParam);
            return Json(result);
        }
        #endregion

        #region 解绑账号
        public ActionResult BingUserCancel()
        {
            ActMessage = "解绑账号";
            return View(Umodel);
        }

        [HttpPost]
        public ActionResult BingUserCancel(FormCollection form)
        {
            Result result = new Result();
            result = JN.Services.Manager.Users.BingUserCancelMethod(form, Umodel, UserService, SysDBTool);
            return Json(result);
        }
        #endregion

        #endregion


        #region 异步登录入口   //与B系统对接登陆
        //[HttpPost]
        //public ActionResult LoginIndex(string username, string token)
        //{
        //    ReturnResult result = new ReturnResult();
        //    try
        //    {
        //        if (string.IsNullOrEmpty(username) | string.IsNullOrEmpty(token))
        //            throw new CustomException("操作超时");
        //        JN.Data.User model = MvcCore.Unity.Get<IUserService>().Single(x => x.BindUserName == username); //accountrelations.GetModel("RelationUserName='" + username + "' and Status=100");
        //        if (model == null) throw new CustomException("未绑定用户系统账号，请登陆后立即绑定！");
        //        //{
        //        //    ViewBag.Title = "未绑定账号！";
        //        //    ViewBag.msg = "未绑定用户系统账号，请登陆后立即绑定！";
        //        //    ViewBag.url = "/user/login";
        //        //    return View("msg");
        //        //}
        //        string bjpassword = (model.BindUserName + model.BindUserId + DateTime.Now.ToString("yyyy-MM-dd HH:mm")).ToMD5().ToMD5();
        //        //  logs.WriteLog("RelationUserName:" + model.RelationUserName + ",RelationUID:" + model.RelationUID+",bjpassword:"+bjpassword);
        //        if (token != bjpassword)// return Redirect("/User/Login");
        //        {
        //            ViewBag.Title = "登陆账号或密码不正确！";
        //            ViewBag.msg = "登陆账号或密码不正确！";
        //            ViewBag.url = "/usercenter/login";
        //            return View("Error");
        //        }

        //        var entity = UserLoginHelper.GetUserLoginBy_To(username, model.Password);
        //        if (entity == null) //return Redirect("/User/Login");
        //        {
        //            //  logs.WriteLog("onuser == null");
        //            ViewBag.Title = "登陆账号或密码不正确！";
        //            ViewBag.msg = "登陆账号或密码不正确！";
        //            ViewBag.url = "/usercenter/login";
        //            return View("Error");
        //        }
        //        if (entity.IsLock) throw new CustomException("您的帐号已被冻结,请联系你的推荐人!");
        //        if (!entity.IsActivation) throw new CustomException("你的账号未激活，请联系你的推荐人!");
        //        var log = new ActLog();
        //        log.ActContent = "用户“" + username + "”登录成功！";
        //        log.CreateTime = DateTime.Now;
        //        log.IP = Request.UserHostAddress;
        //        if (Request.UrlReferrer != null)
        //            log.Location = Request.UrlReferrer.ToString();
        //        log.Platform = "用户";
        //        log.Source = JN.Services.Tool.StringHelp.IPGetCity(Request.UserHostAddress);
        //        log.UID = entity.ID;
        //        log.UserName = entity.UserName;
        //        ActLogService.Add(log);

        //        //更新最后一次登录时间 
        //        entity.LastLoginTime = DateTime.Now;
        //        entity.LastLoginIP = Request.UserHostAddress;
        //        UserService.Update(entity);

        //        SysDBTool.Commit();

        //        //每天登录奖励
        //        //if (entity.IsActivation)
        //        //{
        //        //    if (MvcCore.Unity.Get<IBonusDetailService>().List(x => x.UID == entity.ID && x.BonusID == 1106 && SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0).Count() <= 0)
        //        //    {
        //        //        decimal PARAM_SDJJ = MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 1106).Value.ToDecimal();
        //        //        Bonus.UpdateUserWallet(PARAM_SDJJ, 1106, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 1106).Name, "Fc生态系统活跃度生成奖金", entity.ID, "Addup1106", true);
        //        //    }
        //        //}

        //        result.Status = 200;
        //        //if (entity.IsActivation)
        //        result.Message = "/usercenter/orepool/mining?lang=";
        //        //else
        //        //result.Message = oldurl ?? "/usercenter/home/index?lang=" + lang;

        //        //如果勾选记住密码，则保存密码一个星期
        //        DateTime expiration = DateTime.Now.AddMinutes(20);
        //        //if (rp == "1")
        //        //    expiration = DateTime.Now.AddDays(7);

        //        // 设置Ticket信息
        //        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
        //            1,
        //            entity.ID.ToString(),
        //            DateTime.Now,
        //            expiration,
        //            false, LoginInfoType.User.ToString());

        //        // 加密验证票据
        //        string strTicket = FormsAuthentication.Encrypt(ticket);

        //        // 使用新userdata保存cookie
        //        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strTicket);
        //        cookie.Expires = ticket.Expiration;
        //        this.Response.Cookies.Add(cookie);
        //    }
        //    catch (CustomException ex)
        //    {
        //        result.Message = ex.Message;
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        result.Message = "The network system is busy, please try again later!";
        //        logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Message = "The network system is busy, please try again later!";
        //        logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
        //    }
        //    return Json(result);
        //}

        #endregion

        #region 异步登录入口   //与A系统对接登陆

        //生成口令token  （用户名+用户ID+当前时间 加密）
        [HttpPost]
        public JsonResult gettoken()
        {
            string passe = "";
            if (Umodel != null)
            {
                passe = (Umodel.BindUserName + Umodel.BindUserId + DateTime.Now.ToString("yyyy-MM-dd HH:mm")).ToMD5().ToMD5();
                return Json(new { status = 200, messges = passe }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = 500, messges = passe }, JsonRequestBehavior.AllowGet);
        }




        #endregion


        #region 091401矿机部分

        #region 采矿
        public ActionResult Miner()
        {
            ViewBag.Title = "采矿";
            var list = MachineService.List(x => x.IsSales).OrderByDescending(x => x.ID).ToList();

            return View(list);
        }
        #endregion

        #region 购买支付
        private static object obj = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">矿机ID</param>
        /// <param name="number">购买数量</param>
        /// <param name="coinid">购买币种</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Pay(int id, int number, int coinid)
        {
            ReturnResult result = new ReturnResult();
            try
            {                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoPay" + Umodel.ID))//检测缓存是否存在，区分大小写
                {                    throw new JN.Services.CustomException.CustomException("请您稍后再试");                }
                MvcCore.Extensions.CacheExtensions.SetCache("DoPay" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                lock (obj)
                {
                    var onUser = UserService.SingleAndInit(Umodel.ID);
                    //if (!(onUser.IsAuthentication ?? false)) throw new CustomException("认证过才可以购买矿机");
                    var machine = MachineService.Single(id);
                    if (machine == null) throw new CustomException("记录不存在或已被删除");
                    if (number <= 0) throw new CustomException("请输入正确的购买数量");
                    // if (product.Stock < 1) throw new CustomException("库存不足");

                    // var numCount = MvcCore.Unity.Get<Data.Service.IMachineOrderService>().List(x => x.UID == onUser.ID && x.ProductID == machine.ID && x.IsBalance).Count();

                    var payCur = cacheCurrency.SingleAndInit(x => x.WalletCurID == coinid);

                    decimal balance3001 = Users.WalletCur(payCur.WalletCurID ?? 3001, onUser);

                    decimal realMoney = number * machine.RealPrice;// 数量*价格             实际要扣除的金额

                    Newtonsoft.Json.Linq.JArray prices = JN.Services.Tool.StringHelp.GetCurInfo("https://api.coinmarketcap.com/v1/ticker/");
                    decimal currePrice = prices == null ? 0 : Convert.ToDecimal(prices.FirstOrDefault(x => (string)x["id"] == payCur.English.ToLower())["price_usd"]);// 1;//市场价
                   
                    if(currePrice==0) throw new CustomException("获取市场价格失败");
                    //主流币按市场价折合扣除866，等于扣多少主流币
                    if (currePrice > 0) realMoney = realMoney / currePrice;

                    if (balance3001 < realMoney) throw new CustomException("账户余额不足");

                    #region 成功支付
                    //写入汇款表
                    var model = new Data.MachineOrder
                    {
                        InvestmentNo = GetOrderNumber(),
                        InvestmentType = 1,
                        UID = Umodel.ID,
                        UserName = Umodel.UserName,
                        ApplyInvestment = machine.RealPrice * number,
                        SettlementMoney = machine.RealPrice,
                        IsBalance = true,
                        BuyNum = 1,
                        HashnestNum = number,
                        Status = (int)Data.Enum.RechargeSatus.Sucess,
                        CreateTime = DateTime.Now,
                        AddupInterest = 0,//累计收益
                        ShopID = machine.MachineID,//ShopID
                        ProductID = machine.ID,//矿机ID
                        ProductName = machine.MachineName,//产品名称
                        TimesType = machine.TimesType,
                        TopBonus = machine.TopBonus,
                        PayWay = payCur.CurrencyName,
                        Duration = (machine.Duration ?? 1),
                        WaitExtractIncome = 0,
                        ImageUrl = machine.ImageUrl,
                        LastProfitTime = DateTime.Now,
                        ReserveBoolean1 = false,
                        ReserveDecamal1 = (machine.Performance ?? 0),
                        ReserveInt2 = machine.TimesType > 0 ? (int)(machine.TopBonus / machine.TimesType) : 0,
                        PayMoney = realMoney,
                        ActivationNums=1,
                        ActivationTime=DateTime.Now
                    };
                    MvcCore.Unity.Get<IMachineOrderService>().Add(model);

                    MachineService.Update(machine);

                    Wallets.changeWallet(Umodel.ID, -realMoney, payCur.WalletCurID ?? 0, "购买矿机" + machine.MachineName + "扣除,单号:" + model.InvestmentNo, payCur);//扣币

                    // product.Stock -= 1;
                    //Bonus.Bouns1103(model.ID);
                    //Bonus.Bouns1105(model.ID);
                    #endregion
                    // SysDBTool.Commit();

                    #region 增加业绩
                    //一条线上都累加业绩
                    List<string> sqlString = new List<string>();

                    Bonus.AchievementUp(number, onUser, cacheSysParam, ref sqlString);
                    var skip = 0;
                    System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                    while (skip < sqlString.Count)
                    {
                        var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                        skip += 2000;
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                    }
                    sqlString.Clear();
                    #endregion
                    result.Status = 200;
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
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("DoPay" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }
        //生成真实订单号
        public string GetOrderNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "M";
            result += dateTime.Year.ToString().Substring(2, 2) + dateTime.Month + dateTime.Day;//年月日
            int hour = dateTime.Hour;
            int minu = dateTime.Minute;
            int secon = dateTime.Second;
            int count = hour * 20 + minu * 10 + secon * 5;
            int lengh = count.ToString().Length;
            string temp = count.ToString();
            string bu = "";
            if (lengh < 5)//不足5位前面补0
            {
                for (int i = 1; i < 5 - lengh; i++)
                {
                    bu += "0";
                }
            }
            result += bu + temp;
            result += Services.Tool.StringHelp.GetRandomNumber(4);//4位随机数字
            if (IsHave(result))
            {
                return GetOrderNumber();
            }
            return result;
        }

        //检查订单号是否重复
        private bool IsHave(string number)
        {
            return MvcCore.Unity.Get<Data.Service.IMachineOrderService>().List(x => x.InvestmentNo == number).Count() > 0;
        }
        #endregion

        #region 激活矿机
        [HttpPost]
        public ActionResult ActivationPay(int id, int coinid)
        {
            ReturnResult result = new ReturnResult();
            try
            {                if (MvcCore.Extensions.CacheExtensions.CheckCache("ActivationPay" + Umodel.ID))//检测缓存是否存在，区分大小写
                {                    throw new JN.Services.CustomException.CustomException("请您稍后再试");                }
                MvcCore.Extensions.CacheExtensions.SetCache("ActivationPay" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                var onUser = UserService.SingleAndInit(Umodel.ID);
                var machineOrder = MvcCore.Unity.Get<IMachineOrderService>().Single(id);
                if (machineOrder == null) throw new CustomException("记录不存在或已被删除");

                var payCur = cacheCurrency.SingleAndInit(x => x.WalletCurID == coinid);
                decimal balance3001 = Users.WalletCur(payCur.WalletCurID ?? 3001, onUser);

                decimal realMoney = machineOrder.ApplyInvestment;// 数量*价格             实际要扣除的金额

                Newtonsoft.Json.Linq.JArray prices = JN.Services.Tool.StringHelp.GetCurInfo("https://api.coinmarketcap.com/v1/ticker/");
                decimal currePrice = prices == null ? 0 : Convert.ToDecimal(prices.FirstOrDefault(x => (string)x["id"] == payCur.English.ToLower())["price_usd"]);// 1;//市场价

                if (currePrice == 0) throw new CustomException("获取市场价格失败");
                //主流币按市场价折合扣除866，等于扣多少主流币
                if (currePrice > 0) realMoney = realMoney / currePrice;

                if (balance3001 < realMoney) throw new CustomException("激活失败！账户余额不足");

                #region 成功支付
                machineOrder.ActivationNums += 1;
                machineOrder.ActivationTime = DateTime.Now;
                machineOrder.IsBalance = true;
                MvcCore.Unity.Get<IMachineOrderService>().Update(machineOrder);

                Wallets.changeWallet(Umodel.ID, -realMoney, payCur.WalletCurID ?? 0, "激活矿机" + machineOrder.ProductName + "扣除,单号:" + machineOrder.InvestmentNo, payCur);//扣币

                #endregion

                #region 增加业绩
                //一条线上都累加业绩
                List<string> sqlString = new List<string>();

                Bonus.AchievementUp(machineOrder.HashnestNum, onUser, cacheSysParam, ref sqlString);
                var skip = 0;
                System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                while (skip < sqlString.Count)
                {
                    var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                    skip += 2000;
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                }
                sqlString.Clear();
                #endregion
                result.Status = 200;
            }
            catch (CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("ActivationPay" + Umodel.ID);//清除缓存
            }
            return Json(result);
        }
        #endregion

        #endregion

        #region G1矿机自动挖掘部分
        /// <summary>
        /// G1矿机自动挖掘部分
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddMachineMin()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                int id = Request["id"].ToInt();
                var moOrder = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Single(x => x.ID == id && x.ShopID == 1);
                if (moOrder == null) throw new CustomException("获取失败，查无此矿机");
                if (moOrder.IsBalance == false) throw new CustomException("获取失败，此矿机已停止分红");
                if (moOrder.TodayMiningTime == moOrder.Duration) throw new CustomException("获取失败，此矿机今日已达今日封顶");


                var minName = "MachineSendTime" + id;
                if (Session[minName] != null)
                {
                    if (!DateDiff_minuMachine(DateTime.Parse(Session[minName].ToString())))
                        throw new CustomException("挖掘间隔不能少于1分钟");
                }

                moOrder.TodayMiningTime += 1;
                if (moOrder.TodayMiningTime >= moOrder.Duration)
                    moOrder.TodayMiningTime = moOrder.Duration;
                Session[minName] = DateTime.Now;
                MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Update(moOrder);
                SysDBTool.Commit();
                result.Status = 200;

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

            return this.JsonResult(result);
        }

        /// <summary>
        ///判断是否于5分钟之前
        /// </summary>
        /// <param name="DateTimeOld">较早的日期和时间</param>
        /// <returns></returns>
        public static bool DateDiff_minuMachine(DateTime DateTimeOld)
        {
            TimeSpan ts1 = new TimeSpan(DateTimeOld.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            int minu = ts.Minutes;
            if (minu >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region G1矿机复投
        /// <summary>
        /// G1矿机复投
        /// </summary>
        /// <returns></returns>
        public ActionResult MinerInvestment()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                int id = Request["id"].ToInt();
                var moOrder = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Single(x => x.ID == id && x.ShopID == 1);
                if (moOrder == null) throw new CustomException("获取失败，查无此矿机");
                if (moOrder.IsBalance == true) throw new CustomException("此矿机无需复投");
                var mon = cacheSysParam.SingleAndInit(x => x.ID == 3110).Value.ToDecimal();

                var payCur = MvcCore.Unity.Get<Data.Service.ICurrencyService>().SingleAndInit(x => x.WalletCurID == 3001);
                decimal balance3001 = Users.WalletCur(3001, Umodel);
                if (balance3001 < mon)
                {
                    throw new CustomException("账户余额不足");
                }
                Wallets.changeWallet(Umodel.ID, -mon, payCur.WalletCurID ?? 0, "复投矿机" + moOrder.ProductName + "扣除,单号:" + moOrder.InvestmentNo, payCur);



                moOrder.IsBalance = true;
                moOrder.AddupInterest = 0;
                moOrder.ReserveInt1 = 0;


                MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Update(moOrder);
                SysDBTool.Commit();
                result.Status = 200;

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

            return this.JsonResult(result);
        }
        #endregion

    }
}
