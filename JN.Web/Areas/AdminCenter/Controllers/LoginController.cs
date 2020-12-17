using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using System.Web.Security;
using JN.Data.Enum;
using JN.Services;
using JN.Services.Tool;
using JN.Services.CustomException;
using JN.Services.Manager;
using System.Drawing;
using System.Collections.Generic;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class LoginController : Controller
    {
        private static List<Data.SysParam> cacheSysParam = null;
        private readonly IAdminUserService AdminUserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public LoginController(ISysDBTool SysDBTool, IAdminUserService AdminUserService, IActLogService ActLogService)
        {
            this.AdminUserService = AdminUserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
        }
        public ActionResult Index()
        {
            //网址
            //if (Request.Url.Host == "coinglobe.com")
            //{
            //    return Redirect("http://www.coinglobe.com/AdminCenter/Login");
            //}
            return View();
        }

        [HttpPost]
        public JsonResult Index(string username, string password, string code)
        {
            string oldurl = Request["oldUrl"];
            string vcode = (Session["AdminValidateCode"] ?? "").ToString();
            Session.Remove("AdminValidateCode");

            //手机验证码部分
            string vmobilecode = (Session["SMSAdminCode"] ?? "").ToString();
            Session.Remove("SMSAdminCode");
            string mobilecode = Request["smscode"];

            ReturnResult result = new ReturnResult();
            try
            {
                //username = "admin";
                //password = "111111";
                //if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                //    throw new CustomException("验证码错误");

                if (string.IsNullOrEmpty(username) | string.IsNullOrEmpty(password))
                    throw new CustomException("用户名或密码不能为空");
                if (cacheSysParam.SingleAndInit(x => x.ID == 3508).Value.ToInt() == 1)
                {
                    if (string.IsNullOrEmpty(vmobilecode) || string.IsNullOrEmpty(mobilecode) || !vmobilecode.Equals(mobilecode, StringComparison.InvariantCultureIgnoreCase))
                        throw new CustomException("手机验证码错误");
                }


                var entity = AdminLoginHelper.GetAdminLoginBy(username, password);
                if (entity != null)
                {
                    if (!entity.IsPassed) throw new CustomException("您的帐号已被冻结,请联系你的推荐人!");
                    var log = new ActLog();
                    log.ActContent = "管理员“" + username + "”登录成功！";
                    log.CreateTime = DateTime.Now;
                    log.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        log.Location = Request.UrlReferrer.ToString();
                    log.Platform = "后台";
                    log.Source = "";
                    log.UID = entity.ID;
                    log.UserName = entity.AdminName;
                    ActLogService.Add(log);
                    SysDBTool.Commit();

                    result.Status = 200;
                    result.Message = oldurl ?? "/AdminCenter/Home";

                    //如果勾选记住密码，则保存密码一个星期
                    DateTime expiration = DateTime.Now.AddMinutes(20);
                    //if (rp == "1")
                    //    expiration = DateTime.Now.AddDays(7);

                    // 设置Ticket信息
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                        1,
                        entity.ID.ToString(),
                        DateTime.Now,
                        expiration,
                        false, LoginInfoType.Manager.ToString());

                    // 加密验证票据
                    string strTicket = FormsAuthentication.Encrypt(ticket);

                    // 使用新userdata保存cookie
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName+"admin", strTicket);
                    //cookie.Expires = ticket.Expiration;
                    cookie.Path = "/";
                    this.Response.Cookies.Add(cookie);
                }
                else
                    throw new CustomException("用户名或密码错误");
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


        //public FileResult GetCodeImage(int? width, int? height)
        //{
        //    string code = ValidateWhiteBlackImgCode.RandemCode(4);
        //    Session["AdminValidateCode"] = code;
        //    //return File(ValidateWhiteBlackImgCode.Img(code, 200, 75), "image/png");
        //    // change by chenzw :改为如果前台请求有指定尺寸的，则生成指定尺寸的验证码
        //    return File(ValidateWhiteBlackImgCode.Img(code, width ?? 200, height ?? 75), "image/png");
        //}





        #region  生成验证码

        /// <summary>
        /// 生成验证码（注册）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCodeImage()
        {
            CodeRend cd = new CodeRend();
            string code = cd.CreateVerifyCode(4);
            Session["AdminValidateCode"] = code.ToLowerInvariant();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Bitmap image = cd.CreateImageCode(code);
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return File(ms.GetBuffer(), "image/JPEG");
        }
        #endregion

        #region 管理员登陆发送手机验证码
        /// <summary>
        /// 管理员登陆发送手机验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult SendMobileMsm()
        {
            ReturnResult result = new ReturnResult();
            try
            {
              
                string username = Request["username"];

                if (string.IsNullOrEmpty(username)) throw new CustomException("请您填写账号");
                var admin = AdminUserService.Single(x => x.AdminName == username.Trim());

                if (admin == null) throw new CustomException("账号不存在");
                string phone = admin.Phone; //ConfigHelper.GetConfigString("AdminPhone");
                //if (string.IsNullOrEmpty(phone)) throw new CustomException("请您填写手机号码");
                //if (!StringHelp.IsNumber(phone)) throw new CustomException("请输入正确的手机号码");

                //if (admin.Phone != phone.Trim()) throw new CustomException("手机号与资料不符");

                if (Session["SMSAdminSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(Session["SMSAdminSendTime"].ToString())))
                        throw new CustomException("每次发送短信间隔不能少于1分钟");
                }

                ValidateCode vEmailCode = new ValidateCode();
                string smscode = vEmailCode.CreateRandomCode(4, "1,2,3,4,5,6,7,8,9,0");
                Session["SMSAdminCode"] = smscode;
                Session["GetSMSAdmin"] = phone;
                Session["SMSAdminSendTime"] = DateTime.Now;
                var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);

               string text = cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode) + "。如非本人操作，请忽略本短信";
                //sysEntity.SysName + "验证码" + smscode + "，5分钟内使用有效！"
                bool b = SMSHelper.SMSYunPian(phone, text);
                if (b)
                    result.Status = 200;
                else
                {
                    Session["SMSAdminCode"] = null;
                    Session["GetSMSAdmin"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }
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


        public ActionResult AutoLoginUser(string url)
        {

            ViewBag.Url = url;
            return View();
        }



    }
}
