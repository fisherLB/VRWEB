using JN.Data.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Data.Extensions;
using JN.Data.Enum;
using JN.Services.Manager;
using System.Text.RegularExpressions;
using JN.Services.Tool;
using JN.Services.CustomException;
using System.Data.Entity.Validation;
using System.Collections;
namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IUserBankCardService UserBankCardService;
        private static List<Data.SysParam> cacheSysParam = null;
        public UserController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService, IUserBankCardService UserBankCardService)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.UserBankCardService = UserBankCardService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        }



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _AddUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult _AddUser(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = new Data.User();

                //20160711安全更新 ---------------- start
                if (!string.IsNullOrEmpty(fc["wallet2001"]) || !string.IsNullOrEmpty(fc["wallet2002"]) || !string.IsNullOrEmpty(fc["wallet2003"]))
                {
                    var wlog = new Data.WarningLog();
                    wlog.CreateTime = DateTime.Now;
                    wlog.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        wlog.Location = Request.UrlReferrer.ToString();
                    wlog.Platform = "用户";
                    wlog.WarningMsg = "试图在修改或添加用户时篡改数据(试图篡改钱包等敏感数据)";
                    wlog.WarningLevel = "严重";
                    wlog.ResultMsg = "拒绝";
                    wlog.UserName = Amodel.AdminName;
                    MvcCore.Unity.Get<IWarningLogService>().Add(wlog);
                    SysDBTool.Commit();
                    throw new CustomException("非法数据请求");
                }
                //20160711安全更新  --------------- end


                TryUpdateModel(entity, fc.AllKeys);
                if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new CustomException("用户名只能为字母、数字和下划线");
                //if (fc["password"] != fc["passwordconfirm"]) throw new CustomException("一级密码与确认密码不相符");
                //if (fc["password2"] != fc["password2confirm"]) throw new CustomException("二级密码与确认密码不相符");
                if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.RealName) || string.IsNullOrEmpty(entity.Mobile)) throw new CustomException("用户名、真实姓名、手机号码不能为空");
                if (string.IsNullOrEmpty(entity.Email)) throw new CustomException("电子邮箱不能为空");

                if (string.IsNullOrEmpty(entity.Password) || string.IsNullOrEmpty(entity.Password2)) throw new CustomException("一级密码、二级密码不能为空");
                if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0) throw new CustomException("用户名已被使用");
                //if (string.IsNullOrEmpty(entity.BankCard) || string.IsNullOrEmpty(entity.BankUser) || string.IsNullOrEmpty(entity.BankOfDeposit)) throw new CustomException("请输入银行卡信息");
                //if (entity.RealName != entity.BankUser) throw new CustomException("真实姓名与银行卡户名不相符");
                if (UserService.List(x => x.UserName == entity.RefereeUser && x.IsActivation).Count() <= 0) throw new CustomException("推荐人不存在或未激活");

                if (ConfigHelper.GetConfigString("MemberAtlas") != "sun")
                {
                    if (UserService.List(x => x.UserName == entity.ParentUser && x.IsActivation).Count() <= 0) throw new CustomException("安置人不存在或未激活");
                }
                if (ConfigHelper.GetConfigBool("HaveAgent"))
                {
                    if (UserService.List(x => x.AgentName == entity.AgentUser && x.IsActivation).Count() <= 0) throw new CustomException("商务中心不存在");
                    var agentUser = UserService.Single(x => x.AgentName == entity.AgentUser);
                    entity.AgentID = agentUser.ID;
                }

                Data.User _parentUser = null;
                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    if (_parentUser != null)
                    {
                        if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 2) throw new CustomException("安置人安置名额已满");
                    }
                    if (entity.ChildPlace > 2 || entity.ChildPlace < 1) throw new CustomException("安置参数不正确");
                    if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new CustomException("当前位置无法安置");
                }
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    if (_parentUser != null)
                    {
                        if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 3) throw new CustomException("安置人安置名额已满");
                    }

                    if (entity.ChildPlace > 3 || entity.ChildPlace < 1) throw new CustomException("安置参数不正确");
                    if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new CustomException("当前位置无法安置");
                }
                else
                    _parentUser = UserService.Single(x => x.UserName == entity.RefereeUser);

                entity.IsActivation = false;
                entity.IsAgent = false;
                entity.IsLock = false;

                //entity.Investment = cacheSysParam.SingleAndInit(x => x.ID == 1001).Value.ToDecimal();

                //用户部分
                var _refereeUser = UserService.Single(x => x.UserName == entity.RefereeUser);

                //if (_parentUser.RootID != _refereeUser.RootID || Umodel.RootID != _parentUser.RootID) throw new CustomException("推荐人和安置人以及您自己必须同一网内用户");
                entity.RefereeDepth = _refereeUser.RefereeDepth + 1;
                entity.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
                entity.RefereeID = _refereeUser.ID;
                entity.RefereeUser = _refereeUser.UserName;
                entity.MainAccountID = 0;

                //用户部分
                entity.ParentID = _parentUser.ID;
                entity.ParentUser = _parentUser.UserName;
                entity.RootID = _parentUser.RootID;
                entity.Depth = _parentUser.Depth + 1;
                entity.ParentPath = _parentUser.ParentPath + "," + _parentUser.ID;
                entity.Child = 0;

                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password2 = entity.Password2.ToMD5().ToMD5();
                entity.CreateTime = DateTime.Now;
                entity.CreateBy = "Admin";
                entity.ReserveDecamal1 = entity.Investment;
                entity.IsAuthentication = true;
                entity.IsMobile = true;
                entity.IsEmail = true;
                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 2 + entity.ChildPlace;
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 3 + entity.ChildPlace;
                else
                {
                    entity.DepthSort = 0;
                    entity.ChildPlace = UserService.List(x => x.ParentID == _parentUser.ID).Count() > 0 ? UserService.List(x => x.ParentID == _parentUser.ID).Max(x => x.ChildPlace) + 1 : 1;
                }
                //var r3001 = Users.GetRandomString(33, true, true, true, false, "1");
                //entity.R3001 = r3001;


                UserService.Add(entity);
                _parentUser.Child = _parentUser.Child + 1;
                UserService.Update(_parentUser);
                _refereeUser.RefereeCount = _refereeUser.RefereeCount + 1;
                UserService.Update(_refereeUser);
                SysDBTool.Commit();
                //Wallets.changeWallet(entity.ID, entity.Investment, 2002, "后台注册赠送");

                if (cacheSysParam.SingleAndInit(x => x.ID == 1010).Value.ToInt() == 1)
                    Bonus.RegRewardMachineExperience(entity.ID, 1, "");//注册成功后赠送   

                if (!entity.IsActivation) Users.ActivationAccount(entity, _refereeUser.ID);
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
        public ActionResult List(int? page)
        {
            ActMessage = "用户管理";
            //动态构建查询
            var list = UserService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            string isactivation = Request["isactivation"];
            if (!string.IsNullOrEmpty(isactivation))
            {
                bool isactive = (isactivation == "1");
                list = list.Where(x => x.IsActivation == isactive && x.IsLock == false);
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }


        public ActionResult ZtList(int? page)
        {
            ActMessage = "直推用户管理";
            //动态构建查询
            var list = UserService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            //if (Request["OrderFiled"] == "RefereeCount")
            //    list = list.OrderByDescending(x => x.RefereeCount);
            //if (Request["OrderFiled"] == "Investment")
            //    list = list.OrderByDescending(x => x.Investment);

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.RefereeCount).ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 被冻结的用户
        /// </summary>
        /// <returns></returns>
        public ActionResult Lock(int? page)
        {
            ActMessage = "被冻结的用户";
            var list = UserService.List(x => x.IsLock).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        #region 实名认证

        #region 用户信息认证列表
        /// <summary>
        /// 用户信息认证列表
        /// </summary>
        /// by Annie：2017.07.05
        /// <returns></returns>
        public ActionResult AuthenticationList(int? page)
        {
            ActMessage = "用户信息认证";
            //动态构建查询
            var list = UserService.List(x => (x.AuthenticationStatus ?? 0) == 1).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            string isauthentication = Request["isauthentication"];
            if (!string.IsNullOrEmpty(isauthentication))
            {
                bool isactive = (isauthentication == "1");
                list = list.Where(x => (x.IsAuthentication ?? false) == isactive && x.IsLock == false);
            }

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 未通过的实名认证
        /// <summary>
        /// 未通过的实名认证
        /// </summary>
        /// by Annie：2017.08.22
        /// <returns></returns>
        public ActionResult NoAuthenticationList(int? page)
        {
            ActMessage = "未通过的实名认证";
            //动态构建查询
            var list = UserService.List(x => (x.AuthenticationStatus ?? 0) == -1 && x.IsLock == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 审核实名认证
        /// <summary>
        /// 审核实名认证
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ActionResult Authentication(int id)
        {
            var model = UserService.Single(id);
            if ((model.IsAuthentication ?? false) == false && (model.AuthenticationStatus ?? 0) == 1)
            {
                model.IsAuthentication = true;
                model.AuthenticationTime = DateTime.Now;
                UserService.Update(model);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "操作成功！";
                return View("Success");
            }
            else
            {
                ViewBag.SuccessMsg = "没有可操作的数据！";
                return View("Error");
            }

        }
        #endregion

        #region 拒绝实名认证
        /// <summary>
        /// 拒绝实名认证 by Annie：2017.08.22
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ActionResult NoAuthentication(int id, string reason)
        {
            var model = UserService.Single(id);
            if (model != null && !string.IsNullOrEmpty(reason))
            {
                model.IsAuthentication = false;
                model.NoAuthenticationReason = reason;//拒绝理由
                model.AuthenticationStatus = -1;//拒绝认证
                model.NoAuthenticationTime = DateTime.Now;//拒绝时间
                UserService.Update(model);
                SysDBTool.Commit();
                return Content("ok");
            }
            return Content("Error");
        }
        #endregion

        #endregion

        public ActionResult Modify(int? id)
        {
            if (id.HasValue)
            {
                ActMessage = "编辑用户资料";
                //ViewBag.UserName = UserService.Single(id).UserName.ToString();
                return View(UserService.Single(id));
            }
            else
            {
                ActMessage = "新增初始用户";
                //ViewBag.UserName = Users.GetUserName().ToString();
                return View(new Data.User());
            }
        }

        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("AddUser" + Amodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new CustomException("请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("AddUser" + Amodel.ID, Amodel.AdminName, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                var entity = UserService.SingleAndInit(fc["ID"].ToInt());
                var onUser = entity.ToModel<Data.User>();
                TryUpdateModel(entity, fc.AllKeys);
                entity.UserName = entity.Mobile;//
                //20160711安全更新 ---------------- start
                if (!string.IsNullOrEmpty(fc["wallet2001"]) || !string.IsNullOrEmpty(fc["wallet2002"]) || !string.IsNullOrEmpty(fc["wallet2003"]))
                {
                    var wlog = new Data.WarningLog();
                    wlog.CreateTime = DateTime.Now;
                    wlog.IP = Request.UserHostAddress;

                    if (Request.UrlReferrer != null)
                        wlog.Location = Request.UrlReferrer.ToString();
                    wlog.Platform = "用户";
                    wlog.WarningMsg = "试图在修改或添加用户时篡改数据(试图篡改钱包等敏感数据)";
                    wlog.WarningLevel = "严重";
                    wlog.ResultMsg = "拒绝";
                    wlog.UserName = Amodel.AdminName;
                    MvcCore.Unity.Get<IWarningLogService>().Add(wlog);
                    SysDBTool.Commit();
                    throw new CustomException("非法数据请求");
                }
                //20160711安全更新  --------------- end

                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().ListCache("sysSet").FirstOrDefault();
                #region 资料验证项（不为空）
                //if (string.IsNullOrEmpty(entity.NickName) && sysEntity.RegistNotNullItems.Contains(",NickName,")) throw new JN.Services.CustomException.CustomException("昵称不能为空");
                if (string.IsNullOrEmpty(entity.RealName) && sysEntity.RegistNotNullItems.Contains(",RealName,")) throw new JN.Services.CustomException.CustomException("真实姓名不能为空");
                if (string.IsNullOrEmpty(entity.Mobile) && sysEntity.RegistNotNullItems.Contains(",Mobile,")) throw new JN.Services.CustomException.CustomException("手机号码不能为空");
                if (string.IsNullOrEmpty(entity.Email) && sysEntity.RegistNotNullItems.Contains(",Email,")) throw new JN.Services.CustomException.CustomException("邮箱不能为空");
                if (string.IsNullOrEmpty(entity.IDCard) && sysEntity.RegistNotNullItems.Contains(",IDCard,")) throw new JN.Services.CustomException.CustomException("身份证号码不能为空");
                if (string.IsNullOrEmpty(entity.AliPay) && sysEntity.RegistNotNullItems.Contains(",AliPay,")) throw new JN.Services.CustomException.CustomException("支付宝账号不能为空");
                if (string.IsNullOrEmpty(entity.WeiXin) && sysEntity.RegistNotNullItems.Contains(",WeiXin,")) throw new JN.Services.CustomException.CustomException("微信账号不能为空");
                if (string.IsNullOrEmpty(entity.QQ) && sysEntity.RegistNotNullItems.Contains(",QQ,")) throw new JN.Services.CustomException.CustomException("QQ账号不能为空");
                if (string.IsNullOrEmpty(entity.BankName) && sysEntity.RegistNotNullItems.Contains(",BankName,")) throw new JN.Services.CustomException.CustomException("银行名称不能为空");
                if (string.IsNullOrEmpty(entity.BankCard) && sysEntity.RegistNotNullItems.Contains(",BankCard,")) throw new JN.Services.CustomException.CustomException("银行卡卡号不能为空");
                if (string.IsNullOrEmpty(entity.BankUser) && sysEntity.RegistNotNullItems.Contains(",BankUser,")) throw new JN.Services.CustomException.CustomException("银行卡户名不能为空");
                if (string.IsNullOrEmpty(entity.BankOfDeposit) && sysEntity.RegistNotNullItems.Contains(",BankOfDeposit,")) throw new JN.Services.CustomException.CustomException("银行卡开户行不能为空");
                if (string.IsNullOrEmpty(entity.Question) && sysEntity.RegistNotNullItems.Contains(",Question,")) throw new JN.Services.CustomException.CustomException("取回密码问题不能为空");
                if (string.IsNullOrEmpty(entity.Answer) && sysEntity.RegistNotNullItems.Contains(",Answer,")) throw new JN.Services.CustomException.CustomException("取回密码答案不能为空");
                #endregion

                if (entity.ID > 0)
                {
                    string msg = "";
                    if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.Mobile)) throw new CustomException("手机号不能为空");//用户名、联系电话不能为空
                    //if (string.IsNullOrEmpty(entity.BankCard) || string.IsNullOrEmpty(entity.BankUser) || string.IsNullOrEmpty(entity.BankOfDeposit)) throw new CustomException("请输入银行卡信息");
                    //if (entity.RealName != entity.BankUser) throw new CustomException("真实姓名与银行卡户名不相符");
                    if(UserService.List(x => (x.UserName == entity.UserName.Trim() || x.Mobile == entity.Mobile.Trim()) && onUser.ID != entity.ID).Count() > 0) throw new CustomException("手机号码已被使用");

                    #region 修改资料唯一验证项
                    //if (!string.IsNullOrEmpty(entity.NickName) && sysEntity.RegistOnlyOneItems.Contains(",NickName,") && UserService.List(x => x.NickName == entity.NickName.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("昵称已被使用");
                    if (!string.IsNullOrEmpty(entity.RealName) && sysEntity.RegistOnlyOneItems.Contains(",RealName,") && UserService.List(x => x.RealName == entity.RealName.Trim() && onUser.RealName != entity.RealName).Count() > 0) throw new JN.Services.CustomException.CustomException("真实姓名已被使用");
                    if (!string.IsNullOrEmpty(entity.Mobile) && sysEntity.RegistOnlyOneItems.Contains(",Mobile,") && UserService.List(x => x.Mobile == entity.Mobile.Trim() && onUser.Mobile != entity.Mobile).Count() > 0) throw new JN.Services.CustomException.CustomException("手机号码已被使用");
                    if (!string.IsNullOrEmpty(entity.Email) && sysEntity.RegistOnlyOneItems.Contains(",Email,") && UserService.List(x => x.Email == entity.Email.Trim() && onUser.Email != entity.Email).Count() > 0) throw new JN.Services.CustomException.CustomException("邮箱已被使用");
                    if (!string.IsNullOrEmpty(entity.IDCard) && sysEntity.RegistOnlyOneItems.Contains(",IDCard,") && UserService.List(x => x.IDCard == entity.IDCard.Trim() && onUser.IDCard != entity.IDCard).Count() > 0) throw new JN.Services.CustomException.CustomException("身份证号码已被使用");
                    if (!string.IsNullOrEmpty(entity.AliPay) && sysEntity.RegistOnlyOneItems.Contains(",AliPay,") && UserService.List(x => x.AliPay == entity.AliPay.Trim() && onUser.AliPay != entity.AliPay).Count() > 0) throw new JN.Services.CustomException.CustomException("支付宝帐号已被使用");
                    if (!string.IsNullOrEmpty(entity.WeiXin) && sysEntity.RegistOnlyOneItems.Contains(",WeiXin,") && UserService.List(x => x.WeiXin == entity.WeiXin.Trim() && onUser.WeiXin != entity.WeiXin).Count() > 0) throw new JN.Services.CustomException.CustomException("微信帐号已被使用");
                    if (!string.IsNullOrEmpty(entity.BankCard) && sysEntity.RegistOnlyOneItems.Contains(",BankCard,") && UserService.List(x => x.BankCard == entity.BankCard.Trim() && onUser.BankCard != entity.BankCard).Count() > 0) throw new JN.Services.CustomException.CustomException("银行卡号已被使用");
                    #endregion

                    string resetpwd2 = fc["resetpassowrd2"];
                    string resetpwd = fc["resetpassowrd"];
                    string resetpwd3 = fc["resetpassowrd3"];
                    if (!string.IsNullOrEmpty(resetpwd))
                    {
                        entity.Password = resetpwd.ToMD5().ToMD5();
                        msg += " 修改登录密码";
                    }
                    if (!string.IsNullOrEmpty(resetpwd2))
                    {
                        entity.Password2 = resetpwd2.ToMD5().ToMD5();
                        msg += " 修改二级密码";
                    }
                    if (!string.IsNullOrEmpty(resetpwd3))
                    {
                        entity.Password3 = resetpwd3.ToMD5().ToMD5();
                        msg += " 修改二级密码";
                    }

                    if (onUser.Mobile != entity.Mobile) msg += " 手机变更：" + onUser.Mobile + " => " + entity.Mobile;
                    if (onUser.RealName != entity.RealName) msg += "　姓名变更：" + onUser.RealName + " => " + entity.RealName;
                    if (onUser.AliPay != entity.AliPay) msg += "　支付宝变更：" + onUser.AliPay + " => " + entity.AliPay;
                    if (onUser.BankCard != entity.BankCard) msg += "　银行卡变更：" + onUser.BankCard + " => " + entity.BankCard;

                    var wlog2 = new Data.WarningLog();
                    wlog2.CreateTime = DateTime.Now;
                    wlog2.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        wlog2.Location = Request.UrlReferrer.ToString();
                    wlog2.Platform = "后台";
                    wlog2.WarningMsg = "由管理员“" + Amodel.AdminName + "”修改用户资料" + (!string.IsNullOrEmpty(msg) ? ",涉及改动信息：" + msg : "");
                    wlog2.WarningLevel = "一般";
                    wlog2.ResultMsg = "通过";
                    wlog2.UserName = entity.UserName;
                    MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                    SysDBTool.Commit();

                    UserService.Update(entity);
                    SysDBTool.Commit();
                    //if (cacheSysParam.SingleAndInit(x => x.ID == 1010).Value.ToInt() == 1)
                    //    Bonus.RegRewardMachineExperience(entity.ID, 1, "");//注册成功后赠送  
                }
                else
                {

                    if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.Mobile)) throw new CustomException("手机号不能为空");//真实姓名、联系电话不能为空
                    //if (!Regex.IsMatch(entity.UserName, @"^[0-9_]+$")) throw new CustomException("会员ID只能为数字");
                    //if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.RealName) || string.IsNullOrEmpty(entity.Mobile)) throw new CustomException("用户名、真实姓名、联系电话不能为空");
                    //if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new CustomException("用户名只能为字母、数字和下划线");
                    //if (entity.UserName.Length > 20) throw new JN.Services.CustomException.CustomException("用户名长度超过20个字符");
                    if (UserService.List(x => x.UserName == entity.UserName.Trim() || x.Mobile == entity.Mobile.Trim()).Count() > 0) throw new CustomException("手机号已被使用");//会员ID已被使用
                    if (string.IsNullOrEmpty(entity.Password) || string.IsNullOrEmpty(entity.Password2) || string.IsNullOrEmpty(entity.Password3)) throw new CustomException("登录密码、交易密码、安全口令不能为空");

                    #region 注册唯一验证项
                    //if (!string.IsNullOrEmpty(entity.NickName) && sysEntity.RegistOnlyOneItems.Contains(",NickName,") && UserService.List(x => x.NickName == entity.NickName.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("昵称已被使用");
                    if (!string.IsNullOrEmpty(entity.RealName) && sysEntity.RegistOnlyOneItems.Contains(",RealName,") && UserService.List(x => x.RealName == entity.RealName.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("真实姓名已被使用");
                    if (!string.IsNullOrEmpty(entity.Mobile) && sysEntity.RegistOnlyOneItems.Contains(",Mobile,") && UserService.List(x => x.Mobile == entity.Mobile.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("手机号码已被使用");
                    if (!string.IsNullOrEmpty(entity.Email) && sysEntity.RegistOnlyOneItems.Contains(",Email,") && UserService.List(x => x.Email == entity.Email.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("邮箱已被使用");
                    if (!string.IsNullOrEmpty(entity.IDCard) && sysEntity.RegistOnlyOneItems.Contains(",IDCard,") && UserService.List(x => x.IDCard == entity.IDCard.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("身份证号码已被使用");
                    if (!string.IsNullOrEmpty(entity.AliPay) && sysEntity.RegistOnlyOneItems.Contains(",AliPay,") && UserService.List(x => x.AliPay == entity.AliPay.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("支付宝帐号已被使用");
                    if (!string.IsNullOrEmpty(entity.WeiXin) && sysEntity.RegistOnlyOneItems.Contains(",WeiXin,") && UserService.List(x => x.WeiXin == entity.WeiXin.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("微信帐号已被使用");
                    if (!string.IsNullOrEmpty(entity.BankCard) && sysEntity.RegistOnlyOneItems.Contains(",BankCard,") && UserService.List(x => x.BankCard == entity.BankCard.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("银行卡号已被使用");
                    #endregion

                    //entity.DisplayID = Users.GetDisplayID();
                    entity.IsActivation = true;
                    entity.IsAgent = true;
                    entity.AgentName = entity.UserName;
                    entity.IsLock = false;
                    entity.ActivationTime = DateTime.Now;
                    //用户部分
                    entity.RootID = 0;
                    entity.ParentID = 0;
                    entity.ParentUser = "";
                    entity.ParentPath = ",0";
                    entity.Depth = 0;
                    entity.DepthSort = 1;
                    entity.Child = 0;
                    entity.ChildPlace = 1;
                    entity.IsMobile = true;
                    entity.IsEmail = true;
                    entity.IsAuthentication = true;
                    entity.RefereeID = 0;
                    entity.RefereeUser = "";
                    entity.RefereeDepth = 0;
                    entity.RefereePath = ",0";

                    //添加绑定状态 by：Ann,time：2017/12/04
                    entity.BindStatus = false;//绑定状态
                    entity.BindUserId = 0;//绑定用户id
                    entity.BindUserName = "";//绑定用户名
                    entity.BindUserPath = "";//绑定用路径

                    entity.CreditValue = 100;//信用值（100）只扣不涨
                    entity.GoodScore = 0;//好评分（0）只涨不扣

                    entity.Investment = 0;// cacheSysParam.SingleAndInit(x => x.ID == 1005).Value.ToDecimal();
                    entity.UserLevel = (int)Data.Enum.UserLevel.Level0;
                    entity.Password = entity.Password.ToMD5().ToMD5();
                    entity.Password2 = entity.Password2.ToMD5().ToMD5();
                    entity.Password3 = entity.Password3.ToMD5().ToMD5();
                    entity.CreateTime = DateTime.Now;
                    entity.ReachingTime = DateTime.Now.AddMonths(-3);

                    //var r3001 = Users.GetRandomString(33, true, true, true, false, "1");
                    //entity.R3001 = r3001;

                    UserService.Add(entity);
                    SysDBTool.Commit();
                    
                    if (cacheSysParam.SingleAndInit(x => x.ID == 1010).Value.ToInt() == 1)
                        Bonus.RegRewardMachineExperience(entity.ID, 1, "");//注册成功后赠送  
                    //Users.ChangeParam3106();
                }
                //SysDBTool.Commit();
                //if (cacheSysParam.SingleAndInit(x => x.ID == 1010).Value.ToInt() == 1)
                //    Bonus.RegRewardMachineExperience(entity.ID, 1, "");//注册成功后赠送  
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
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("AddUser" + Amodel.ID);//清除缓存
            }
            return Json(result);
        }

        /// <summary>
        /// 删除用户，未激活时才可以
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                if (!model.IsActivation && model.Child == 0)
                {
                    //删除时上级子用户减少
                    if (model.ParentID > 0)
                    {
                        var _parentUser = UserService.Single(model.ParentID);
                        if (_parentUser != null)
                        {
                            _parentUser.Child = _parentUser.Child - 1;
                            UserService.Update(_parentUser);
                        }
                    }
                    UserService.Delete(id);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "删除成功！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "该用户已被激活或伞下还有用户，无法删除。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "ID不存在或已被删除。";
            return View("Error");
        }

        public ActionResult doCommand(int id, string commandtype)
        {
            var model = UserService.Single(id);
            if (commandtype.ToLower() == "lock")
            {
                model.IsLock = true;
                model.LockReason = Request["reason"];

            }
            else if (commandtype.ToLower() == "unlock")
            {
                model.IsLock = false;
                if (model.LockReason == "到期未确认收货冻结" || model.LockReason == "到期未付款冻结")
                {
                    model.LockReason = "";
                    model.CreditValue = 100;//重新来
                }
            }
            else if (commandtype.ToLower() == "resetinputwrong")
            {
                model.InputWrongPwdTimes = 0;
            }
            else if (commandtype.ToLower() == "walletlock")
            {
                model.WalletLock = true;
            }
            else if (commandtype.ToLower() == "unwalletlock")
            {
                model.WalletLock = false;
            }
            else if (commandtype.ToLower() == "unlocksell")
            {
                model.SellSwitch = true;
            }
            else if (commandtype.ToLower() == "locksell")
            {
                model.SellSwitch = false;
            }
            else if (commandtype.ToLower() == "unlockbuy")
            {
                model.BuySwitch = true;
            }
            else if (commandtype.ToLower() == "lockbuy")
            {
                model.BuySwitch = false;
            }

            UserService.Update(model);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }

        /// <summary>
        /// 用户帐号冻结
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ActionResult MakeLock(int id, string reason)
        {
            var model = UserService.Single(id);
            if (model != null)
            {
                model.IsLock = true;
                model.LockReason = reason;
                model.LockTime = DateTime.Now;
                UserService.Update(model);
                SysDBTool.Commit();
                return Content("ok");
            }
            return Content("Error");
        }

        /// <summary>
        /// 设置用户级别
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MakeLevel(int id, string level)
        {
            var model = UserService.Single(id);
            if (model != null && !string.IsNullOrWhiteSpace(level))
            {
                //int uplevel = 1000 + level.ToInt();
                //model.Investment = cacheSysParam.SingleAndInit(x => x.ID == uplevel).Value.ToDecimal();
                model.UserLevel = level.ToInt();
                UserService.Update(model);
                SysDBTool.Commit();
                ActPacket = model;
                return Content("ok");
            }
            return Content("Error");
        }
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
        public ActionResult DoubleTrack(int id)
        {
            ActMessage = "用户双轨视图";
            var model = UserService.Single(id);
            string keyword = Request["kv"];
            if (Request["kf"] == "username" && !string.IsNullOrEmpty(keyword))
            {
                var sUser = UserService.Single(x => x.UserName == keyword);
                if (sUser != null) return Redirect(Url.Action("DoubleTrack", new { ID = sUser.ID }));
            }
            else if (Request["kf"] == "mobile" && !string.IsNullOrEmpty(keyword))
            {
                var sUser = UserService.Single(x => x.Mobile == keyword);
                if (sUser != null) return Redirect(Url.Action("DoubleTrack", new { ID = sUser.ID }));
            }
            ViewBag.Title = "“" + model.UserName + "”用户双轨视图";

            var lst = new List<Data.User>();
            if (model != null)
            {
                lst.Add(model);

                //第一层
                var U11 = UserService.Single(x => x.ParentID == model.ID && x.ChildPlace == 1);
                if (U11 == null) U11 = new Data.User { ID = -1, ParentID = model.ID, ParentUser = model.UserName, AgentUser = model.AgentUser };
                lst.Add(U11);

                var U12 = UserService.Single(x => x.ParentID == model.ID && x.ChildPlace == 2);
                if (U12 == null) U12 = new Data.User { ID = -1, ParentID = model.ID, ParentUser = model.UserName, AgentUser = model.AgentUser };
                lst.Add(U12);


                //第二层
                var U21 = UserService.Single(x => x.ParentID == U11.ID && x.ChildPlace == 1);
                if (U21 == null) U21 = new Data.User { ID = -1, ParentID = U11.ID, ParentUser = U11.UserName, AgentUser = U11.AgentUser };
                lst.Add(U21);

                var U22 = UserService.Single(x => x.ParentID == U11.ID && x.ChildPlace == 2);
                if (U22 == null) U22 = new Data.User { ID = -1, ParentID = U11.ID, ParentUser = U11.UserName, AgentUser = U11.AgentUser };
                lst.Add(U22);

                var U23 = UserService.Single(x => x.ParentID == U12.ID && x.ChildPlace == 1);
                if (U23 == null) U23 = new Data.User { ID = -1, ParentID = U12.ID, ParentUser = U12.UserName, AgentUser = U12.AgentUser };
                lst.Add(U23);

                var U24 = UserService.Single(x => x.ParentID == U12.ID && x.ChildPlace == 2);
                if (U24 == null) U24 = new Data.User { ID = -1, ParentID = U12.ID, ParentUser = U12.UserName, AgentUser = U12.AgentUser };
                lst.Add(U24);

                //第三层
                var U31 = UserService.Single(x => x.ParentID == U21.ID && x.ChildPlace == 1);
                if (U31 == null) U31 = new Data.User { ID = -1, ParentID = U21.ID, ParentUser = U21.UserName, AgentUser = U21.AgentUser };
                lst.Add(U31);

                var U32 = UserService.Single(x => x.ParentID == U21.ID && x.ChildPlace == 2);
                if (U32 == null) U32 = new Data.User { ID = -1, ParentID = U21.ID, ParentUser = U21.UserName, AgentUser = U21.AgentUser };
                lst.Add(U32);

                var U33 = UserService.Single(x => x.ParentID == U22.ID && x.ChildPlace == 1);
                if (U33 == null) U33 = new Data.User { ID = -1, ParentID = U22.ID, ParentUser = U22.UserName, AgentUser = U22.AgentUser };
                lst.Add(U33);

                var U34 = UserService.Single(x => x.ParentID == U22.ID && x.ChildPlace == 2);
                if (U34 == null) U34 = new Data.User { ID = -1, ParentID = U22.ID, ParentUser = U22.UserName, AgentUser = U22.AgentUser };
                lst.Add(U34);

                var U35 = UserService.Single(x => x.ParentID == U23.ID && x.ChildPlace == 1);
                if (U35 == null) U35 = new Data.User { ID = -1, ParentID = U23.ID, ParentUser = U23.UserName, AgentUser = U23.AgentUser };
                lst.Add(U35);

                var U36 = UserService.Single(x => x.ParentID == U23.ID && x.ChildPlace == 2);
                if (U36 == null) U36 = new Data.User { ID = -1, ParentID = U23.ID, ParentUser = U23.UserName, AgentUser = U23.AgentUser };
                lst.Add(U36);

                var U37 = UserService.Single(x => x.ParentID == U24.ID && x.ChildPlace == 1);
                if (U37 == null) U37 = new Data.User { ID = -1, ParentID = U24.ID, ParentUser = U24.UserName, AgentUser = U24.AgentUser };
                lst.Add(U37);

                var U38 = UserService.Single(x => x.ParentID == U24.ID && x.ChildPlace == 2);
                if (U38 == null) U38 = new Data.User { ID = -1, ParentID = U24.ID, ParentUser = U24.UserName, AgentUser = U24.AgentUser };
                lst.Add(U38);

                //第四层
                var U41 = UserService.Single(x => x.ParentID == U31.ID && x.ChildPlace == 1);
                if (U41 == null) U41 = new Data.User { ID = -1, ParentID = U31.ID, ParentUser = U31.UserName, AgentUser = U31.AgentUser };
                lst.Add(U41);

                var U42 = UserService.Single(x => x.ParentID == U31.ID && x.ChildPlace == 2);
                if (U42 == null) U42 = new Data.User { ID = -1, ParentID = U31.ID, ParentUser = U31.UserName, AgentUser = U31.AgentUser };
                lst.Add(U42);

                var U43 = UserService.Single(x => x.ParentID == U32.ID && x.ChildPlace == 1);
                if (U43 == null) U43 = new Data.User { ID = -1, ParentID = U32.ID, ParentUser = U32.UserName, AgentUser = U32.AgentUser };
                lst.Add(U43);

                var U44 = UserService.Single(x => x.ParentID == U32.ID && x.ChildPlace == 2);
                if (U44 == null) U44 = new Data.User { ID = -1, ParentID = U32.ID, ParentUser = U32.UserName, AgentUser = U32.AgentUser };
                lst.Add(U44);

                var U45 = UserService.Single(x => x.ParentID == U33.ID && x.ChildPlace == 1);
                if (U45 == null) U45 = new Data.User { ID = -1, ParentID = U33.ID, ParentUser = U33.UserName, AgentUser = U33.AgentUser };
                lst.Add(U45);

                var U46 = UserService.Single(x => x.ParentID == U33.ID && x.ChildPlace == 2);
                if (U46 == null) U46 = new Data.User { ID = -1, ParentID = U33.ID, ParentUser = U33.UserName, AgentUser = U33.AgentUser };
                lst.Add(U46);

                var U47 = UserService.Single(x => x.ParentID == U34.ID && x.ChildPlace == 1);
                if (U47 == null) U47 = new Data.User { ID = -1, ParentID = U34.ID, ParentUser = U34.UserName, AgentUser = U34.AgentUser };
                lst.Add(U47);

                var U48 = UserService.Single(x => x.ParentID == U34.ID && x.ChildPlace == 2);
                if (U48 == null) U48 = new Data.User { ID = -1, ParentID = U34.ID, ParentUser = U34.UserName, AgentUser = U34.AgentUser };
                lst.Add(U48);

                var U49 = UserService.Single(x => x.ParentID == U35.ID && x.ChildPlace == 1);
                if (U49 == null) U49 = new Data.User { ID = -1, ParentID = U35.ID, ParentUser = U35.UserName, AgentUser = U35.AgentUser };
                lst.Add(U49);

                var U50 = UserService.Single(x => x.ParentID == U35.ID && x.ChildPlace == 2);
                if (U50 == null) U50 = new Data.User { ID = -1, ParentID = U35.ID, ParentUser = U35.UserName, AgentUser = U35.AgentUser };
                lst.Add(U50);

                var U51 = UserService.Single(x => x.ParentID == U36.ID && x.ChildPlace == 1);
                if (U51 == null) U51 = new Data.User { ID = -1, ParentID = U36.ID, ParentUser = U36.UserName, AgentUser = U36.AgentUser };
                lst.Add(U51);

                var U52 = UserService.Single(x => x.ParentID == U36.ID && x.ChildPlace == 2);
                if (U52 == null) U52 = new Data.User { ID = -1, ParentID = U36.ID, ParentUser = U36.UserName, AgentUser = U36.AgentUser };
                lst.Add(U52);

                var U53 = UserService.Single(x => x.ParentID == U37.ID && x.ChildPlace == 1);
                if (U53 == null) U53 = new Data.User { ID = -1, ParentID = U37.ID, ParentUser = U37.UserName, AgentUser = U37.AgentUser };
                lst.Add(U53);

                var U54 = UserService.Single(x => x.ParentID == U37.ID && x.ChildPlace == 2);
                if (U54 == null) U54 = new Data.User { ID = -1, ParentID = U37.ID, ParentUser = U37.UserName, AgentUser = U37.AgentUser };
                lst.Add(U54);

                var U55 = UserService.Single(x => x.ParentID == U38.ID && x.ChildPlace == 1);
                if (U55 == null) U55 = new Data.User { ID = -1, ParentID = U38.ID, ParentUser = U38.UserName, AgentUser = U38.AgentUser };
                lst.Add(U55);

                var U56 = UserService.Single(x => x.ParentID == U38.ID && x.ChildPlace == 2);
                if (U56 == null) U56 = new Data.User { ID = -1, ParentID = U38.ID, ParentUser = U38.UserName, AgentUser = U38.AgentUser };
                lst.Add(U56);

                return View(lst);
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
        #endregion

        #region 推荐树视图
        /// <summary>
        /// 树状视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult TreeView(int id)
        {
            var model = UserService.Single(id);
            ActMessage = "用户推荐图谱";
            if (model == null)
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除！";
                return View("Error");
            }
            return View(model);
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

            //取所有父ID与参数相符数据封装到 List<TUser> 并以JSON格式返回
            var ulst = UserService.List(x => x.RefereeID == id).ToList();
            foreach (var mt in ulst)
            {
                child = UserService.List(x => x.RefereeID == mt.ID).Count();
                //allchild = users.GetRecordCount("ParentPath like '%," + mt.ID.ToString() + ",%' ") + child;<a href=\"javascript: void(0);\" onclick=\"javascript: winopen('注册用户', '" + Url.Action("_AddUser", new { parentuser = mt.UserName, refereeuser = mt.UserName, agentuser = mt.AgentUser, childplace = 1 }) + "', 500); \">注册</a> <a href=\"javascript: void(0);\" onclick=\"javascript: winopen('注册用户', '" + Url.Action("_AddUser", new { parentuser = mt.UserName, refereeuser = mt.UserName, agentuser = mt.AgentUser, childplace = 1}) + "', 500); \">注册</a>
                TreeNode p = new TreeNode();
                p.id = mt.ID;
                if (mt.IsActivation)
                {
                    if (mt.IsAgent ?? false)
                        //p.text = "<i style='color:#f00'>" + mt.UserName + "ID:" + mt.ID + "路径" + mt.RefereePath + "推荐" + child + "人)</i>  ";
                        p.text = "<i style='color:#f00'>" + mt.UserName + "推荐" + child + "人)</i>  ";
                    else
                        //p.text = "" + mt.UserName + "(" + mt.RealName + "ID" + mt.ID + "路径" + mt.RefereePath + ",推荐" + child + "人)";
                        p.text = "" + mt.UserName + "(" + mt.RealName + ",推荐" + child + "人," + typeof(JN.Data.Enum.UserLevel).GetEnumDesc(mt.UserLevel) + ")";
                }
                else
                    p.text = "<em style='color:#999'>" + mt.UserName + "(未激活)</em>";

                if (mt.RefereeID == 0)
                {
                    p.type = "root";
                }

                if (child > 0)
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
        #region 身份证图片上传
        //身份证图片上传
        public ActionResult UpMainPic(string uid)
        {
            string strUid = uid;
            System.Collections.Hashtable hash = UpPic1(strUid);
            return Json(hash, "text/html; charset=UTF-8", JsonRequestBehavior.AllowGet);
        }

        ///上传身份证图片到文件夹
        /// <param name="dir">文件夹名称 "/Upload//IDCartInfo/" + userId + "/" + dir + "/";</param>
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

        public ActionResult UserBankCardList(int? page)
        {
            ActMessage = "会员银行信息";
            ViewBag.Title = ActMessage;

            //动态构建查询
            var list = UserBankCardService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));

            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderBy(x => x.UID).ThenBy(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult UserTranSwitch(int? page)
        {
            ActMessage = "用户交易锁";
            //动态构建查询
            var list = UserService.List(x => x.IsActivation == true && x.IsLock == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/Upload/" + FileName + ".xls"));
                return File(Server.MapPath("/Upload/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        #region 批量卖出锁定
        /// <summary>
        /// 批量卖出锁定
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchSellLock()
        {
            ActMessage = "批量卖出锁定";

            string strIDValue = Request["StrID"];

            if (string.IsNullOrEmpty(strIDValue))
            {
                ViewBag.ErrorMsg = "请选择记录！";
                return View("Error");
            }
            string[] ids = strIDValue.TrimEnd(',').Split(',');
            var userList = UserService.List(x => ids.Contains(x.ID.ToString())).ToList();
            try
            {
                foreach (var item in userList)
                {
                    item.SellSwitch = false;
                    UserService.Update(item);
                }
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "成功批量卖出锁定！";
                return View("Success");
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "操作错误！";
                return View("Error");
            }
        }
        #endregion

        #region 批量卖出解锁
        /// <summary>
        /// 批量卖出解锁
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchSellUnLock()
        {
            ActMessage = "批量卖出解锁";

            string strIDValue = Request["StrID"];

            if (string.IsNullOrEmpty(strIDValue))
            {
                ViewBag.ErrorMsg = "请选择记录！";
                return View("Error");
            }
            string[] ids = strIDValue.TrimEnd(',').Split(',');
            var userList = UserService.List(x => ids.Contains(x.ID.ToString())).ToList();
            try
            {
                foreach (var item in userList)
                {
                    item.SellSwitch = true;
                    UserService.Update(item);
                }
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "成功批量卖出解锁！";
                return View("Success");
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "操作错误！";
                return View("Error");
            }
        }
        #endregion

        #region 批量买入锁定
        /// <summary>
        /// 批量卖出锁定
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchBuyLock()
        {
            ActMessage = "批量买入锁定";

            string strIDValue = Request["StrID"];

            if (string.IsNullOrEmpty(strIDValue))
            {
                ViewBag.ErrorMsg = "请选择记录！";
                return View("Error");
            }
            string[] ids = strIDValue.TrimEnd(',').Split(',');
            var userList = UserService.List(x => ids.Contains(x.ID.ToString())).ToList();
            try
            {
                foreach (var item in userList)
                {
                    item.BuySwitch = false;
                    UserService.Update(item);
                }
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "成功批量买入锁定！";
                return View("Success");
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "操作错误！";
                return View("Error");
            }
        }
        #endregion

        #region 批量买入解锁
        /// <summary>
        /// 批量卖出解锁
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchBuyUnLock()
        {
            ActMessage = "批量买入解锁";

            string strIDValue = Request["StrID"];

            if (string.IsNullOrEmpty(strIDValue))
            {
                ViewBag.ErrorMsg = "请选择记录！";
                return View("Error");
            }
            string[] ids = strIDValue.TrimEnd(',').Split(',');
            var userList = UserService.List(x => ids.Contains(x.ID.ToString())).ToList();
            try
            {
                foreach (var item in userList)
                {
                    item.BuySwitch = true;
                    UserService.Update(item);
                }
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "成功批量买入解锁！";
                return View("Success");
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "操作错误！";
                return View("Error");
            }
        }
        #endregion

        #region 添加推荐人
        public ActionResult addReferee()
        {
            return View();
        }

        [HttpPost]
        public ActionResult addReferee(FormCollection form)
        {
            string username = form["username"];
            string refereeUser = form["RefereeUser"];

            var result = new ReturnResult();
            try
            {
                var onUser = UserService.Single(x => x.UserName.Equals(username.Trim()));
                if (onUser == null) throw new CustomException("用户不存在");
                if (onUser.RefereeUser != null && onUser.RefereeUser != "") throw new CustomException("已有推荐人，无法更改");
                var _refereeUser = UserService.Single(x => x.UserName == refereeUser);//推荐人信息
                if (_refereeUser == null) throw new JN.Services.CustomException.CustomException("推荐人不存在或未激活");




                onUser.RefereeDepth = _refereeUser.RefereeDepth + 1;
                onUser.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
                onUser.RefereeID = _refereeUser.ID;//
                onUser.RefereeUser = _refereeUser.UserName;//
                onUser.MainAccountID = 0;

                //用户双轨三轨部分
                onUser.ParentID = _refereeUser.ID;
                onUser.ParentUser = _refereeUser.UserName;
                onUser.RootID = _refereeUser.RootID;
                onUser.Depth = _refereeUser.Depth + 1;
                onUser.ParentPath = _refereeUser.ParentPath + "," + _refereeUser.ID;

                onUser.ChildPlace = UserService.List(x => x.ParentID == _refereeUser.ID).Count() > 0 ? UserService.List(x => x.ParentID == _refereeUser.ID).Max(x => x.ChildPlace) + 1 : 1;
                onUser.DepthSort = 0;

                _refereeUser.Child = _refereeUser.Child + 1;
                UserService.Update(_refereeUser);
                UserService.Update(onUser);
                MvcCore.Unity.Get<ISysDBTool>().Commit();

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

        #endregion


        #region 添加推荐人
        public ActionResult updataReferee()
        {
            return View();
        }

        [HttpPost]
        public ActionResult updataReferee(FormCollection form)
        {

            var result = new ReturnResult();
            try
            {
                string username = form["username"];
                string refereeUser = form["RefereeUser"];
                string oldpassword = form["oldpassword"];
                if (oldpassword.Trim().Length == 0)
                    throw new CustomException("原密码不能为空");

                if (Amodel.Password != oldpassword.ToMD5().ToMD5())
                    throw new CustomException("密码不正确");

                if (username.Trim().Length == 0 || refereeUser.Trim().Length == 0)
                    throw new CustomException("原密码不能为空");

                var onUser = UserService.ListWithTracking().Single(x => x.UserName.Equals(username.Trim()));
                var _refereeUser = UserService.ListWithTracking().Single(x => x.UserName == refereeUser);//推荐人信息

                if (onUser == null) throw new CustomException("用户不存在");
                if (_refereeUser == null) throw new JN.Services.CustomException.CustomException("推荐人不存在或未激活");

                onUser.RefereeDepth = _refereeUser.RefereeDepth + 1;
                onUser.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
                onUser.RefereeID = _refereeUser.ID;//
                onUser.RefereeUser = _refereeUser.UserName;//
                onUser.MainAccountID = 0;

                //用户双轨三轨部分
                onUser.ParentID = _refereeUser.ID;
                onUser.ParentUser = _refereeUser.UserName;
                onUser.RootID = _refereeUser.RootID;
                onUser.Depth = _refereeUser.Depth + 1;
                onUser.ParentPath = _refereeUser.ParentPath + "," + _refereeUser.ID;

                onUser.ChildPlace = UserService.List(x => x.ParentID == _refereeUser.ID).Count() > 0 ? UserService.List(x => x.ParentID == _refereeUser.ID).Max(x => x.ChildPlace) + 1 : 1;
                onUser.DepthSort = 0;

                _refereeUser.Child = _refereeUser.Child + 1;
                UserService.Update(_refereeUser);
                UserService.Update(onUser);
                MvcCore.Unity.Get<ISysDBTool>().Commit();

                updateRelationship(onUser.ID);

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

        #endregion

        #region 更新关系
        private static void updateRelationship(int onUserID)
        {
            try
            {
                var refererList = MvcCore.Unity.Get<IUserService>().ListWithTracking(x => x.RefereeID == onUserID).OrderBy(x => x.ID).ToList();
                if (refererList.Count > 0)
                {
                    foreach (var item in refererList)
                    {
                        var onUser = MvcCore.Unity.Get<IUserService>().ListWithTracking().Single(x => x.UserName == item.UserName);
                        var _refereeUser = MvcCore.Unity.Get<IUserService>().ListWithTracking().Single(x => x.UserName == item.RefereeUser);//推荐人信息

                        #region 更新
                        onUser.RefereeDepth = _refereeUser.RefereeDepth + 1;
                        onUser.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
                        onUser.RefereeID = _refereeUser.ID;//
                        onUser.RefereeUser = _refereeUser.UserName;//
                        onUser.MainAccountID = 0;

                        //用户双轨三轨部分
                        onUser.ParentID = _refereeUser.ID;
                        onUser.ParentUser = _refereeUser.UserName;
                        onUser.RootID = _refereeUser.RootID;
                        onUser.Depth = _refereeUser.Depth + 1;
                        onUser.ParentPath = _refereeUser.ParentPath + "," + _refereeUser.ID;

                        onUser.ChildPlace = MvcCore.Unity.Get<IUserService>().List(x => x.ParentID == _refereeUser.ID).Count() > 0 ? MvcCore.Unity.Get<IUserService>().List(x => x.ParentID == _refereeUser.ID).Max(x => x.ChildPlace) + 1 : 1;
                        onUser.DepthSort = 0;

                        _refereeUser.Child = _refereeUser.Child + 1;
                        MvcCore.Unity.Get<IUserService>().Update(_refereeUser);
                        MvcCore.Unity.Get<IUserService>().Update(onUser);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                        #endregion

                        var refererList2 = MvcCore.Unity.Get<IUserService>().ListWithTracking(x => x.RefereeID == item.ID).ToList();
                        if (refererList2.Count > 0)
                            updateRelationship(item.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                logs.WriteLog(ex.ToString());
                throw;
            }
        }
        #endregion


    }

}