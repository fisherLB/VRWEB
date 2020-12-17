using JN.Data.Service;
using System;
using System.Linq;
using System.Web;
using JN.Services.Tool;
using JN.Data.Extensions;
using System.Collections.Generic;

namespace JN.Services.Manager
{
    ///<summary>
    /// time:2018年1月18日  短信验证码归于本类
    ///</summary>
    public partial class SMSValidateCode
    {
        //private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 6000).ToList();

        #region 发送注册短信验证码
        /// <summary>
        /// 发送注册短信验证码
        /// </summary>
        /// <param name="countrycode">国家区号</param>
        /// <param name="phone">手机号</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendRegMobileMsm(string phone, string countrycode, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (string.IsNullOrEmpty(phone)) throw new CustomException.CustomException("请您填写手机号码");
                if (!StringHelp.IsNumber(phone)) throw new CustomException.CustomException("请输入正确的手机号码");

                //if (HttpContext.Current.Session["SMSRegSendTime"] != null)
                //{
                //    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSRegSendTime"].ToString())))
                //        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                //}
                var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
                if (MvcCore.Unity.Get<IUserService>().List(x => x.Mobile == phone.Trim()).Count() > 0 && !string.IsNullOrEmpty(phone) && sysEntity.RegistOnlyOneItems.Contains(",Mobile,")) throw new JN.Services.CustomException.CustomException("手机号码已被使用");

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["SMSRegValidateCode"] = smscode;
                HttpContext.Current.Session["GetSMSRegUser"] = phone;
                HttpContext.Current.Session["SMSRegSendTime"] = DateTime.Now;

                //bool b = SMSHelper.WebChineseMSM(countrycode + phone, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));   

                var Param = cacheSysParam.Single(x => x.ID == 3510);
                string tempid = countrycode == "+86" ? Param.Value3 : Param.Value4;
                //string text = countrycode == "+86" ? Param.Value.Replace("#code#", smscode) : Param.Value2.Replace("#code#", smscode);
                string text = smscode;
                bool b = SMSHelper.CYmsm(countrycode + phone, text, tempid);


                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SMSRegValidateCode"] = null;
                    HttpContext.Current.Session["GetSMSRegUser"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record"; //短信发送失败,详情请查阅发送记录
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送获取密码验证码
        /// <summary>
        /// 发送获取密码验证码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendPwdMobileMsm(string phone, IUserService UserService, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (string.IsNullOrEmpty(phone)) throw new CustomException.CustomException("请您填写手机号码");
                if (!StringHelp.IsNumber(phone)) throw new CustomException.CustomException("请输入正确的手机号码");

                //判断账号是否存在
                var user = UserService.Single(x => x.Mobile == phone);
                if (user == null) throw new CustomException.CustomException("对不起，手机号不存在");

                if (HttpContext.Current.Session["SMSSendTimeGetPwd"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSSendTimeGetPwd"].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["SMSValidateGetPwd"] = smscode;
                HttpContext.Current.Session["GetSMSUserGetPwdphone"] = phone;
                HttpContext.Current.Session["SMSSendTimeGetPwd"] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(user.CountryCode + user.Mobile, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SMSValidateGetPwd"] = null;
                    HttpContext.Current.Session["GetSMSUserGetPwdphone"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }

            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion


        #region 修改交易密码发送

        /// <summary>
        /// 修改交易密码发送
        /// </summary>
        /// <returns></returns>
        public static Result SendMobileResetTran(JN.Data.User Umodel, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                string phone = Umodel.Mobile;
                if (!(Umodel.IsMobile ?? false)) throw new JN.Services.CustomException.CustomException("您还没有手机认证，请您手机验证后再操作");
                if (!StringHelp.IsNumber(phone)) throw new JN.Services.CustomException.CustomException("请输入正确的手机号码");


                if (HttpContext.Current.Session["SendMobileResetTranSMSSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SendMobileResetTranSMSSendTime"].ToString())))
                        throw new JN.Services.CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }
                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["SendMobileResetTranSMSCode"] = smscode;
                HttpContext.Current.Session["GetSendMobileResetTranSMSUser"] = phone;
                HttpContext.Current.Session["SendMobileResetTranSMSSendTime"] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(Umodel.CountryCode + phone, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SendMobileResetTranSMSCode"] = null;
                    HttpContext.Current.Session["GetSendMobileResetTranSMSUser"] = null;
                    result.Status = 500;
                }
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 修改登录密码发送

        /// <summary>
        /// 修改登录密码发送
        /// </summary>
        /// <returns></returns>
        public static Result SendMobileLoginPwd(JN.Data.User Umodel, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                string phone = Umodel.Mobile;
                if (!(Umodel.IsMobile ?? false)) throw new JN.Services.CustomException.CustomException("您还没有手机认证，请您手机验证后再操作");
                if (!StringHelp.IsNumber(phone)) throw new JN.Services.CustomException.CustomException("请输入正确的手机号码");

                if (HttpContext.Current.Session["LoginPwdSMSSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["LoginPwdSMSSendTime"].ToString())))
                        throw new JN.Services.CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["LoginPwdSMSCode"] = smscode;
                HttpContext.Current.Session["GetLoginPwdSMSUser"] = phone;
                HttpContext.Current.Session["LoginPwdSMSSendTime"] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(Umodel.CountryCode + phone, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["LoginPwdSMSCode"] = null;
                    HttpContext.Current.Session["GetLoginPwdSMSUser"] = null;
                    result.Status = 500;
                }
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送登录手机错误密码验证码
        /// <summary>
        /// 发送登录手机错误密码验证码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendMobileLoginMsm(string countrycode, string phone, string username, IUserService UserService, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (string.IsNullOrEmpty(username)) throw new CustomException.CustomException("请输入您的用户名");
                if (string.IsNullOrEmpty(countrycode)) throw new CustomException.CustomException("请您选择国家区号");
                if (string.IsNullOrEmpty(phone)) throw new CustomException.CustomException("请您填写手机号码");
                if (!StringHelp.IsNumber(phone)) throw new CustomException.CustomException("请输入正确的手机号码");

                //判断账号是否存在
                var user = UserService.Single(x => x.UserName == username);
                if (user == null) throw new CustomException.CustomException("用户不存在");

                if (user.Mobile != phone) throw new CustomException.CustomException("当前用户未与该手机绑定，发送失败！");

                if (HttpContext.Current.Session["SMSSendTimeLogin"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSSendTimeLogin"].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["SMSValidateLogin"] = smscode;
                HttpContext.Current.Session["GetSMSUserLoginphone"] = phone;
                HttpContext.Current.Session["SMSSendTimeLogin"] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(user.CountryCode + user.Mobile, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SMSValidateLogin"] = null;
                    HttpContext.Current.Session["GetSMSUserLoginphone"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }

            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 修改手机发送
        /// <summary>
        /// 修改手机发送
        /// </summary>
        /// <returns></returns>
        public static Result SendSetMobileMsm(string countrycode, string phone, IUserService UserService, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (string.IsNullOrEmpty(phone)) throw new JN.Services.CustomException.CustomException("请您填写手机号码");
                if (string.IsNullOrEmpty(countrycode)) throw new JN.Services.CustomException.CustomException("请您选择国家区号");
                if (!StringHelp.IsNumber(phone)) throw new JN.Services.CustomException.CustomException("请输入正确的手机号码");

                //手机唯一
                if (MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.Mobile == phone) != null) { throw new JN.Services.CustomException.CustomException("手机已经重复"); }

                if (HttpContext.Current.Session["PhoneSettingSMSSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["PhoneSettingSMSSendTime"].ToString())))
                        throw new JN.Services.CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["PhoneSettingSMSCode"] = smscode;
                HttpContext.Current.Session["GetPhoneSettingSMSUser"] = phone;
                HttpContext.Current.Session["PhoneSettingSMSSendTime"] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(countrycode + phone, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["PhoneSettingSMSCode"] = null;
                    HttpContext.Current.Session["GetPhoneSettingSMSUser"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送修改交易密码验证码
        /// <summary>
        /// 发送修改交易密码验证码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendResetMobileMsm(string phone, Data.User Umodel)
        {
            Result result = new Result();
            try
            {
                //string phone = Request["myphone"];
                if (string.IsNullOrEmpty(phone)) throw new CustomException.CustomException("请您填写手机号码");
                if (!StringHelp.IsNumber(phone)) throw new CustomException.CustomException("请输入正确的手机号码");

                if (phone != Umodel.Mobile) throw new CustomException.CustomException("手机号码与资料不符");
                if (HttpContext.Current.Session["SMSResetSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSResetSendTime"].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["SMSResetValidateCode"] = smscode;
                HttpContext.Current.Session["GetSMSResetUser"] = phone;
                HttpContext.Current.Session["SMSResetSendTime"] = DateTime.Now;

                var cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 6000).ToList();
                var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
                bool b = SMSHelper.WebChineseMSM(Umodel.CountryCode + phone, cacheSysParam.SingleAndInit(x => x.ID == 3508).Value2.Replace("{SYSNAME}", sysEntity.SysName).Replace("{VCODE}", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SMSResetValidateCode"] = null;
                    HttpContext.Current.Session["GetSMSResetUser"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送添加银行卡短信验证码
        /// <summary>
        /// 发送添加银行卡短信验证码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendBankMobileMsm(JN.Data.User Umodel, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                string phone = Umodel.Mobile;
                if (string.IsNullOrEmpty(phone)) throw new CustomException.CustomException("您尚未绑定手机号码");

                if (HttpContext.Current.Session["SMSSendTimebank"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSSendTimebank"].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["SMSbankCode"] = smscode;
                HttpContext.Current.Session["GetbankSMSUser"] = phone;
                HttpContext.Current.Session["SMSSendTimebank"] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(Umodel.CountryCode + phone, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));
                if (b)
                {
                    result.Status = 200;
                }
                else
                {
                    HttpContext.Current.Session["SMSbankCode"] = null;
                    HttpContext.Current.Session["GetbankSMSUser"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }

            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送绑定账户验证码
        /// <summary>
        /// 发送绑定账户验证码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendBindUserMobileMsm(Data.User Umodel, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (string.IsNullOrEmpty(Umodel.Mobile)) throw new CustomException.CustomException("您尚未绑定手机号码");
                //if (!StringHelp.IsNumber(Umodel.Mobile)) throw new CustomException("请输入正确的手机号码");

                //if (phone != Umodel.Mobile) throw new CustomException("手机号码与资料不符");
                if (HttpContext.Current.Session["SMSBindUserSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSBindUserSendTime"].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                //System.ValidateCode vCode = new System.ValidateCode();
                //string smscode = vCode.CreateRandomCode(4, "1,2,3,4,5,6,7,8,9,0");                

                HttpContext.Current.Session["SMSBindUserValidateCode"] = smscode;
                HttpContext.Current.Session["GetSMSBindUserUser"] = Umodel.Mobile;
                HttpContext.Current.Session["SMSBindUserSendTime"] = DateTime.Now;

                var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
                bool b = SMSHelper.WebChineseMSM(Umodel.CountryCode + Umodel.Mobile, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));

                //bool b = SMSHelper.WebChineseMSM(phone, cacheSysParam.SingleAndInit(x => x.ID == 3508).Value2.Replace("{SYSNAME}", sysEntity.SysName).Replace("{VCODE}", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SMSBindUserValidateCode"] = null;
                    HttpContext.Current.Session["GetSMSBindUserUser"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送绑定账户验证码
        /// <summary>
        /// 发送绑定账户验证码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendCancelBindUserMobileMsm(Data.User Umodel, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (string.IsNullOrEmpty(Umodel.Mobile)) throw new CustomException.CustomException("您尚未绑定手机号码");
                //if (!StringHelp.IsNumber(Umodel.Mobile)) throw new CustomException("请输入正确的手机号码");

                //if (phone != Umodel.Mobile) throw new CustomException("手机号码与资料不符");
                if (HttpContext.Current.Session["SMSCancelBindUserSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSCancelBindUserSendTime"].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                //System.ValidateCode vCode = new System.ValidateCode();
                //string smscode = vCode.CreateRandomCode(4, "1,2,3,4,5,6,7,8,9,0");                

                HttpContext.Current.Session["SMSCancelBindUserValidateCode"] = smscode;
                HttpContext.Current.Session["GetSMSCancelBindUserUser"] = Umodel.Mobile;
                HttpContext.Current.Session["SMSCancelBindUserSendTime"] = DateTime.Now;

                var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
                bool b = SMSHelper.WebChineseMSM(Umodel.CountryCode + Umodel.Mobile, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));

                //bool b = SMSHelper.WebChineseMSM(phone, cacheSysParam.SingleAndInit(x => x.ID == 3508).Value2.Replace("{SYSNAME}", sysEntity.SysName).Replace("{VCODE}", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SMSCancelBindUserValidateCode"] = null;
                    HttpContext.Current.Session["GetSMSCancelBindUserUser"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送绑定账户转账验证码
        /// <summary>
        /// 发送绑定账户转账验证码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendBindTransferMobileMsm(Data.User Umodel, List<JN.Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (string.IsNullOrEmpty(Umodel.Mobile)) throw new CustomException.CustomException("您尚未绑定手机号码");
                //if (!StringHelp.IsNumber(Umodel.Mobile)) throw new CustomException("请输入正确的手机号码");

                //if (phone != Umodel.Mobile) throw new CustomException("手机号码与资料不符");
                if (HttpContext.Current.Session["SMSBindTransferSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["SMSBindTransferSendTime"].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session["SMSBindTransferValidateCode"] = smscode;
                HttpContext.Current.Session["GetSMSBindTransferUser"] = Umodel.Mobile;
                HttpContext.Current.Session["SMSBindTransferSendTime"] = DateTime.Now;

                var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
                bool b = JN.Services.Tool.SMSHelper.WebChineseMSM(Umodel.CountryCode + Umodel.Mobile, cacheSysParam.SingleAndInit(x => x.ID == 3503).Value.Replace("#code#", smscode));
                //bool b = SMSHelper.WebChineseMSM(phone, cacheSysParam.SingleAndInit(x => x.ID == 3508).Value2.Replace("{SYSNAME}", sysEntity.SysName).Replace("{VCODE}", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session["SMSBindTransferValidateCode"] = null;
                    HttpContext.Current.Session["GetSMSBindTransferUser"] = null;
                    result.Message = "SMS sent failure, details please refer to the send record";
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion


    }


}