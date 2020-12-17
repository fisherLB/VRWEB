using JN.Data.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Services.CustomException;
using System.Data.Entity.Validation;
using JN.Services.Tool;
using JN.Services.Manager;
using System.Text.RegularExpressions;
using JN.Data.Extensions;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using JN.Data;
using System.Data.Common;

namespace JN.Services.Manager
{
    /// <summary>
    /// time:2017年8月29日 18:25:28  name：lin   alt:用户修改，添加，图谱查询，升级，认证等等业务逻辑
    /// </summary>
    public partial class Users
    {
        private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();

        /// <summary>
        /// 清除缓存后重新加载
        /// </summary>
        public static void HeavyLoad()
        {
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        }

        #region 获得团队人数集合，正向查询用户/反向查询用户

        /// <summary>
        /// 获得用户所有子用户用户集合（安置关系，按上至下左至右排序）
        /// </summary>
        /// <param name="onUser">用户实体</param>
        /// <param name="countDepth">层深（几层内）</param>
        /// <returns></returns>
        public static List<Data.User> GetAllChild(Data.User onUser, int countDepth = 0)
        {
            var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => (x.ParentPath.Contains("," + onUser.ID + ",") || x.ParentID == onUser.ID) && x.IsActivation);
            if (countDepth > 0)
                userlist = userlist.Where(x => x.Depth <= (onUser.Depth + countDepth));
            return userlist.OrderBy(x => x.RefereeDepth).ThenBy(x => x.DepthSort).ToList();
        }

        /// <summary>
        /// 获得用户所有子用户用户集合（推荐关系，按上至下左至右排序）
        /// </summary>
        /// <param name="onUser">用户实体</param>
        /// <param name="countDepth">几代内</param>
        /// <returns></returns>
        public static List<Data.User> GetAllRefereeChild(Data.User onUser, int countDepth = 0)
        {
            var userlist = MvcCore.Unity.Get<IUserService>().List(x => (x.RefereePath.Contains("," + onUser.ID + ",") || x.RefereeID == onUser.ID) && x.IsActivation);
            if (countDepth > 0)
                userlist = userlist.Where(x => x.Depth <= (onUser.RefereeDepth + countDepth));
            return userlist.OrderBy(x => x.RefereeDepth).ThenBy(x => x.DepthSort).ToList();
        }

        /// <summary>
        /// 获得用户所有父用户用户集合（安置关系，反向查询）
        /// </summary>
        /// <param name="onUser">用户实体</param>
        /// <param name="countDepth">反向查询几层</param>
        /// <returns></returns>
        public static List<Data.User> GetAllParent(Data.User onUser, int countDepth = 0)
        {
            if (!string.IsNullOrEmpty(onUser.ParentPath))
            {
                string[] ids = onUser.ParentPath.Split(',');
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString()));
                if (countDepth > 0)
                    userlist = userlist.Where(x => x.Depth >= (onUser.Depth - countDepth));
                return userlist.OrderByDescending(x => x.Depth).ToList();
            }
            return null;
        }

        /// <summary>
        /// 获得用户所有父用户用户集合（推荐关系，反向查询）
        /// </summary>
        /// <param name="onUser">用户实体</param>
        /// <param name="countDepth">反向查询几代</param>
        /// <returns></returns>
        public static List<Data.User> GetAllRefereeParent(Data.User onUser, int countDepth = 0)
        {
            if (!string.IsNullOrEmpty(onUser.RefereePath))
            {
                string[] ids = onUser.RefereePath.Split(',');
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString()));
                if (countDepth > 0)
                    userlist = userlist.Where(x => x.Depth >= (onUser.RefereeDepth - countDepth));
                return userlist.OrderByDescending(x => x.RefereeDepth).ToList();
            }
            return null;
        }

        #endregion

        #region 注册用户（录入信息）

        /// <summary>
        /// 注册用户（录入信息）
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="UserService">用户表接口</param>
        /// <param name="entity">用户模型</param>
        /// <param name="RegUser">注册的用户</param>
        /// <returns></returns>
        public static Result AddUser(FormCollection fc, IUserService UserService, Data.User entity, Data.User RegUser = null)
        {
            var username = fc["mobile"].ToString();
            Data.User _parentUser = new JN.Data.User();
            Result result = new Result();
            try
            {
                //if (username <= 0)
                //{
                //    throw new JN.Services.CustomException.CustomException("会员ID错误");
                //}
                if (MvcCore.Extensions.CacheExtensions.CheckCache("AddUser" + username))//检测缓存是否存在，区分大小写
                {
                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("AddUser" + username, username, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                string code = fc["code"];
                string isincite = "true";// fc["isincite"];//"true"; 
                //if (fc["RefereeUser"] != null && fc["RefereeUser"] != "")
                //{
                //    isincite = "true";
                //}
                string country_code = fc["countrycode"];//国家区号

                //if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                //    throw new CustomException("验证码错误");


                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);
                // var entity = new Data.User();
                // TryUpdateModel(entity, fc.AllKeys);

                if (cacheSysParam.SingleAndInit(x => x.ID == 3501).Value.ToInt() == 1)  //开启注册手机短信验证
                {
                    string smscode = fc["smscode"];
                    if (string.IsNullOrEmpty(smscode)) throw new JN.Services.CustomException.CustomException("请输入手机验证码");
                    if (HttpContext.Current.Session["SMSRegValidateCode"] == null || smscode.Trim().ToLower() != HttpContext.Current.Session["SMSRegValidateCode"].ToString().ToLower()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                    if (HttpContext.Current.Session["GetSMSRegUser"] == null || entity.Mobile != HttpContext.Current.Session["GetSMSRegUser"].ToString()) throw new JN.Services.CustomException.CustomException("手机号码与接收验证码的设备不相符");
                }

                if (RegUser != null)
                {
                    //20160711安全更新 ---------------- start
                    if (!string.IsNullOrEmpty(fc["wallet2001"]) || !string.IsNullOrEmpty(fc["wallet2002"]) || !string.IsNullOrEmpty(fc["wallet2003"]) || !string.IsNullOrEmpty(fc["wallet2004"]) || !string.IsNullOrEmpty(fc["wallet2005"]))
                    {
                        var wlog = new Data.WarningLog();
                        wlog.CreateTime = DateTime.Now;
                        wlog.IP = HttpContext.Current.Request.UserHostAddress;
                        if (HttpContext.Current.Request.UrlReferrer != null)
                            wlog.Location = HttpContext.Current.Request.UrlReferrer.ToString();
                        wlog.Platform = "用户";
                        wlog.WarningMsg = "试图在添加用户时篡改数据(试图篡改钱包等敏感数据)";
                        wlog.WarningLevel = "严重";
                        wlog.ResultMsg = "拒绝";
                        wlog.UserName = RegUser.UserName;
                        MvcCore.Unity.Get<IWarningLogService>().Add(wlog);

                        RegUser.IsLock = true;
                        RegUser.LockTime = DateTime.Now;
                        RegUser.LockReason = "试图在添加用户时篡改数据(详情查看日志)";
                        MvcCore.Unity.Get<IUserService>().Update(RegUser);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                        throw new JN.Services.CustomException.CustomException("非法数据请求，您的帐号已被冻结");
                    }
                    //20160711安全更新  --------------- end

                }
                //国家区号
                //if (string.IsNullOrEmpty(country_code)) throw new JN.Services.CustomException.CustomException("请选择国家区号");//
                #region 基础信息验证项
                entity.UserName = entity.Mobile;

                if (string.IsNullOrEmpty(entity.UserName)) throw new JN.Services.CustomException.CustomException("请输入手机号码");//请输入用户名，用于登录和找回密码
                //if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new JN.Services.CustomException.CustomException("用户名只能为字母、数字和下划线");
                //if (entity.UserName.Length > 20) throw new JN.Services.CustomException.CustomException("用户名长度超过20个字符");
                if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("手机号已被使用");//用户名已被使用

                //if (entity.Investment <= 0) throw new CustomException("请选择注册类型");

                string reppassword = fc["reppassword"];
                string reppassword2 = fc["reppassword2"];

                if (string.IsNullOrEmpty(entity.Password)) throw new JN.Services.CustomException.CustomException("登录密码不能为空");//
                if (string.IsNullOrEmpty(reppassword)) throw new JN.Services.CustomException.CustomException("确认登录密码不能为空");
                if (string.IsNullOrEmpty(entity.Password2)) throw new JN.Services.CustomException.CustomException("交易密码不能为空");//
                if (string.IsNullOrEmpty(reppassword2)) throw new JN.Services.CustomException.CustomException("确认交易密码不能为空");
                if (!Regex.IsMatch(entity.Password, @"^[a-zA-Z0-9]{6,20}$")) throw new JN.Services.CustomException.CustomException("登录密码必须为6位数以上，字母加数字加符号组合");
                if(reppassword!=entity.Password) throw new JN.Services.CustomException.CustomException("登录密码与确认登录密码不一致");
                if (!Regex.IsMatch(entity.Password2, @"^[a-zA-Z0-9]{6,20}$")) throw new JN.Services.CustomException.CustomException("交易密码必须为6位数以上，字母加数字加符号组合");
                if (reppassword2 != entity.Password2) throw new JN.Services.CustomException.CustomException("交易密码与确认交易密码不一致");

                //必须10位数，字母加数字加符号组合
                //if (!Regex.IsMatch(entity.Password, @"^(?![a-zA-Z0-9]+$)(?![^a-zA-Z/D]+$)(?![^0-9/D]+$).{10,30}$")) throw new JN.Services.CustomException.CustomException("登录密码必须为10位数以上，字母加数字加符号组合");
                //必须10位数，字母加数字加符号组合
                //if (!Regex.IsMatch(entity.Password2, @"^(?![a-zA-Z0-9]+$)(?![^a-zA-Z/D]+$)(?![^0-9/D]+$).{10,30}$")) throw new JN.Services.CustomException.CustomException("交易密码必须为10位数以上，字母加数字加符号组合");

                //if (fc["password"] != fc["passwordconfirm"]) throw new JN.Services.CustomException.CustomException("登录密码与确认登录密码不相符");
                //if (fc["password2"] != fc["password2confirm"]) throw new JN.Services.CustomException.CustomException("交易密码与确认交易密码不相符");//二级密码登录后完善
                if (entity.Password == entity.Password2) throw new JN.Services.CustomException.CustomException("登录密码与交易密码不能相同");

                //资料验证项（不为空）
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

                //注册唯一验证项
                //if (!string.IsNullOrEmpty(entity.NickName) && sysEntity.RegistOnlyOneItems.Contains(",NickName,") && UserService.List(x => x.NickName == entity.NickName.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("昵称已被使用");
                if (!string.IsNullOrEmpty(entity.RealName) && sysEntity.RegistOnlyOneItems.Contains(",RealName,") && UserService.List(x => x.RealName == entity.RealName.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("真实姓名已被使用");
                if (!string.IsNullOrEmpty(entity.Mobile) && sysEntity.RegistOnlyOneItems.Contains(",Mobile,") && UserService.List(x => x.Mobile == entity.Mobile.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("手机号码已被使用");
                if (!string.IsNullOrEmpty(entity.Email) && sysEntity.RegistOnlyOneItems.Contains(",Email,") && UserService.List(x => x.Email == entity.Email.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("邮箱已被使用");
                if (!string.IsNullOrEmpty(entity.IDCard) && sysEntity.RegistOnlyOneItems.Contains(",IDCard,") && UserService.List(x => x.IDCard == entity.IDCard.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("身份证号码已被使用");
                if (!string.IsNullOrEmpty(entity.AliPay) && sysEntity.RegistOnlyOneItems.Contains(",AliPay,") && UserService.List(x => x.AliPay == entity.AliPay.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("支付宝帐号已被使用");
                if (!string.IsNullOrEmpty(entity.WeiXin) && sysEntity.RegistOnlyOneItems.Contains(",WeiXin,") && UserService.List(x => x.WeiXin == entity.WeiXin.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("微信帐号已被使用");
                if (!string.IsNullOrEmpty(entity.BankCard) && sysEntity.RegistOnlyOneItems.Contains(",BankCard,") && UserService.List(x => x.BankCard == entity.BankCard.Trim()).Count() > 0) throw new JN.Services.CustomException.CustomException("银行卡号已被使用");
                if (UserService.List().Count() == 0) throw new JN.Services.CustomException.CustomException("暂未开放注册");
                #endregion

                if (Tool.ConfigHelper.GetConfigBool("IsRefereeUser") || !string.IsNullOrEmpty(isincite))//是否有推荐人
                {
                    if (UserService.List(x => x.UserName == entity.RefereeUser).Count() <= 0) throw new JN.Services.CustomException.CustomException("推荐人不存在或未激活");
                }

                //if (ConfigHelper.GetConfigString("MemberAtlas") != "sun")//如果不是太阳线
                //{
                //    if (UserService.List(x => x.UserName == entity.ParentUser).Count() <= 0) throw new JN.Services.CustomException.CustomException("安置人不存在或未激活");
                //}
                if (ConfigHelper.GetConfigBool("HaveAgent"))//是否有商务中心
                {
                    if (UserService.List(x => x.AgentName == entity.AgentUser).Count() <= 0) throw new JN.Services.CustomException.CustomException("商务中心不存在");
                }
                var _refereeUser = UserService.Single(x => x.UserName == entity.RefereeUser);//推荐人信息
                //Data.User _parentUser = null;
                //var _refereeUser = UserService.List().OrderBy(i => i.CreateTime).First();//获取最早注册的用户
                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")//如果是双轨
                {
                    _parentUser = new JN.Data.User();// UserService.Single(x => x.UserName == entity.ParentUser);
                    int childPlace = fc["childPlace"].ToInt();
                    if (childPlace != 1 && childPlace != 2)
                    {
                        throw new JN.Services.CustomException.CustomException("请选择区位");
                    }
                    if (UserService.List(x => x.ParentID == _refereeUser.ID && x.ChildPlace == childPlace).Count() > 0)//如果推荐人左区或者右区已经有会员
                    {
                        var refree = UserService.Single(x => x.ParentID == _refereeUser.ID && x.ChildPlace == childPlace);//推荐人的左区或者右区的会员
                        _parentUser = UserService.List(x => ((x.ParentPath + ",").Contains("," + refree.ID.ToString() + ",") || x.ParentID == refree.ID || x.ID == refree.ID) && x.Child < 2).OrderBy(x => x.Depth).ThenBy(x => x.DepthSort).ToList().FirstOrDefault();//根据会员refree进行小公排（从上至下从左至右）
                        if (_parentUser.Child == 0)
                        {
                            entity.ChildPlace = 1;
                        }
                        else if (_parentUser.Child == 1)
                        {
                            entity.ChildPlace = 2;
                            if (UserService.List(x => x.ParentID == _parentUser.ID && x.ChildPlace == 2).Count() > 0)//如果右边已经有会员。这种排网造成右边可能已经有会员注册，所以要加上这个条件
                            {
                                entity.ChildPlace = 1;
                            }

                        }
                        else
                        {
                            throw new JN.Services.CustomException.CustomException("安置参数错误");
                        }

                    }
                    else
                    {
                        _parentUser = _refereeUser;
                        entity.ChildPlace = childPlace;
                    }
                    //if (MvcCore.Extensions.CacheExtensions.CheckCache("AddUser" + _parentUser.ID))//检测缓存是否存在，区分大小写
                    //{
                    //    throw new JN.Services.CustomException.CustomException("请您稍后再试");
                    //}
                    //MvcCore.Extensions.CacheExtensions.SetCache("AddUser" + _parentUser.ID, _parentUser.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                    //_parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    //if (_parentUser != null)
                    //{
                    //    if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 2) throw new JN.Services.CustomException.CustomException("安置人安置名额已满");
                    //}
                    //if (entity.ChildPlace > 2 || entity.ChildPlace < 1) throw new JN.Services.CustomException.CustomException("安置参数不正确");
                    //if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new JN.Services.CustomException.CustomException("当前位置无法安置");
                }
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")//如果是三轨
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.ParentUser);
                    if (_parentUser != null)
                    {
                        if (UserService.List(x => x.ParentID == _parentUser.ID).Count() >= 3) throw new JN.Services.CustomException.CustomException("安置人安置名额已满");
                    }

                    if (entity.ChildPlace > 3 || entity.ChildPlace < 1) throw new JN.Services.CustomException.CustomException("安置参数不正确");
                    if (UserService.List(x => x.ParentUser == entity.ParentUser && x.ChildPlace == entity.ChildPlace).Count() > 0) throw new JN.Services.CustomException.CustomException("当前位置无法安置");
                }
                else  //否则是太阳线
                {
                    _parentUser = UserService.Single(x => x.UserName == entity.RefereeUser);
                }

                entity.IsActivation = true;
                entity.IsAgent = false;
                entity.IsLock = false;
                if (cacheSysParam.SingleAndInit(x => x.ID == 3501).Value.ToInt() == 1)  //开启注册手机短信验证
                {
                    entity.IsMobile = true;
                }
                entity.Investment = 0;//cacheSysParam.SingleAndInit(x => x.ID == 1001).Value.ToDecimal();


                if (Tool.ConfigHelper.GetConfigBool("IsRefereeUser") || !string.IsNullOrEmpty(isincite))//是否有推荐人
                {
                    //用户太阳线部分
                    // var _refereeUser = UserService.Single(x => x.UserName == entity.RefereeUser);
                    entity.RefereeDepth = _refereeUser.RefereeDepth + 1;
                    entity.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
                    entity.RefereeID = _refereeUser.ID;//
                    entity.RefereeUser = _refereeUser.UserName;//
                    entity.MainAccountID = 0;

                    //用户双轨三轨部分
                    entity.ParentID = _parentUser.ID;
                    entity.ParentUser = _parentUser.UserName;
                    entity.RootID = _parentUser.RootID;
                    entity.Depth = _parentUser.Depth + 1;
                    entity.ParentPath = _parentUser.ParentPath + "," + _parentUser.ID;
                }
                else
                {
                    //用户太阳线部分
                    entity.RefereeDepth = 0;
                    entity.RefereePath = "";
                    entity.RefereeID = 0;
                    entity.RefereeUser = "";
                    entity.MainAccountID = 0;

                    //用户双轨三轨部分
                    entity.ParentID = 0;
                    entity.ParentUser = "";
                    entity.RootID = 0;
                    entity.Depth = 0;
                    entity.ParentPath = "";
                }

                entity.UserLevel = (int)JN.Data.Enum.UserLevel.Level0;//默认试用用户

                entity.Child = 0;
                entity.Mobile = entity.Mobile;
                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password2 = entity.Password2.ToMD5().ToMD5();
                entity.CreateTime = DateTime.Now;
                entity.CreateBy = entity.UserName;
                entity.DepthSort = 0;

                if (ConfigHelper.GetConfigString("MemberAtlas") == "double")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 2 + entity.ChildPlace;
                else if (ConfigHelper.GetConfigString("MemberAtlas") == "three")
                    entity.DepthSort = (_parentUser.DepthSort - 1) * 3 + entity.ChildPlace;
                else if (Tool.ConfigHelper.GetConfigBool("IsRefereeUser") || !string.IsNullOrEmpty(isincite))//是否有推荐人
                {
                    entity.ChildPlace = UserService.List(x => x.ParentID == _parentUser.ID).Count() > 0 ? UserService.List(x => x.ParentID == _parentUser.ID).Max(x => x.ChildPlace) + 1 : 1;
                    entity.DepthSort = 0;
                }

                entity.DisplayID = Users.GetDisplayID();
                entity.Wallet2001 = 0;
                entity.Wallet2002 = 0;
                entity.Wallet2003 = 0;
                entity.Wallet2004 = 0;
                entity.Wallet2005 = 0;
                entity.Wallet2006 = 0;

                //添加绑定状态 by：Ann,time：2017/12/04
                entity.BindStatus = false;//绑定状态
                entity.BindUserId = 0;//绑定用户id
                entity.BindUserName = "";//绑定用户名             
                entity.BindUserPath = "";//绑定用路径

                entity.CreditValue = 100;//信用值（100）只扣不涨
                entity.GoodScore = 0;//好评分（0）只涨不扣
                entity.CountryCode = country_code;
                entity.ReachingTime = DateTime.Now.AddMonths(-3);

                //var r3001 = GetRandomString(33, true, true, true, false, "1");
                //entity.R3001 = r3001;
                //Users.ChangeParam3106();
                UserService.Add(entity);

                if (Tool.ConfigHelper.GetConfigBool("IsRefereeUser") || !string.IsNullOrEmpty(isincite))//是否有推荐人
                {
                    _parentUser.Child = _parentUser.Child + 1;
                    UserService.Update(_parentUser);
                }
                MvcCore.Unity.Get<ISysDBTool>().Commit();

                //Bonus.RegReward(entity.ID);//注册成功后赠送         
                if (cacheSysParam.SingleAndInit(x => x.ID == 1010).Value.ToInt() == 1)
                    Bonus.RegRewardMachineExperience(entity.ID, 1, "");//注册成功后赠送      

                if (cacheSysParam.SingleAndInit(x => x.ID == 3502).Value == "1")//发送短信                  
                    SMSHelper.SMSYunPian(entity.CountryCode + entity.Mobile, cacheSysParam.SingleAndInit(x => x.ID == 3502).Value2.Replace("#USERNAME#", entity.UserName));

                result.Status = 200;
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("AddUser" + username);//清除缓存
            }
            return result;
        }

        #endregion

        #region 实名验证
        /// <summary>
        /// 实名验证
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体</param>
        /// <param name="UserService">用户接口</param>
        /// <returns>返回状态信息</returns>
        public static Result NameAuth(FormCollection fc, JN.Data.User Umodel, IUserService UserService, string imgurl)
        {
            Result result = new Result();
            try
            {
                string realname = fc["realname"];
                string idcard = fc["idcard"];
                if (Umodel.IsLock) throw new JN.Services.CustomException.CustomException("您的帐号受限，无法进行相关操作");
                if (string.IsNullOrEmpty(realname)) throw new JN.Services.CustomException.CustomException("请您填写姓名");
                if (string.IsNullOrEmpty(idcard)) throw new JN.Services.CustomException.CustomException("请您填写身份证号码");
                //if (!JN.Services.Tool.StringHelp.IsIDCard(idcard)) throw new JN.Services.CustomException.CustomException("您填写的身份证号码不正确！");
                //string imgurl = fc["imagesurl"];
                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);

                //判断唯一
                if (UserService.List(x => x.IDCard == idcard.Trim()).Count() > 0 && !string.IsNullOrEmpty(idcard) && sysEntity.RegistOnlyOneItems.Contains(",IDCard,")) throw new JN.Services.CustomException.CustomException("身份证号码已被使用");

                string[] newimgurl = imgurl.Split('|');
                if (newimgurl.Length < 4) throw new JN.Services.CustomException.CustomException("请您上传3张凭证！");

                Umodel.RealName = realname;
                Umodel.IDCard = idcard;
                Umodel.IDCardPic1 = newimgurl[1];
                Umodel.IDCardPic2 = newimgurl[2];
                Umodel.IDCardPic3 = newimgurl[3];
                Umodel.LastUpdateTime = DateTime.Now;
                Umodel.AuthenticationStatus = 1;//申请认证标识
                UserService.Update(Umodel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
                result.Status = 200;
                result.Message = "已提交，等待后台审核！";
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 手机验证
        /// <summary>
        /// 手机验证
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体</param>
        /// <param name="UserService">用户接口</param>
        /// <returns>返回状态信息</returns>
        public static Result PhoneSettingsMethod(FormCollection fc, JN.Data.User Umodel, IUserService UserService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                var entity = UserService.SingleAndInit(Umodel.ID);
                if (Umodel.IsLock) throw new CustomException.CustomException("您的帐号受限，无法进行相关操作");
                string mobile = fc["mobile"];
                string country_code = fc["country_code"];//国家区号
                if (string.IsNullOrEmpty(mobile)) throw new CustomException.CustomException("请您填写手机号码");
                if (string.IsNullOrEmpty(country_code)) throw new CustomException.CustomException("请您选择国家区号");
                if (!StringHelp.IsNumber(mobile)) throw new CustomException.CustomException("请输入正确的手机号码");

                //手机唯一
                if (UserService.Single(x => x.Mobile == mobile) != null) throw new CustomException.CustomException("手机已经重复");

                if (cacheSysParam.SingleAndInit(x => x.ID == 3506).Value.ToInt() == 1)  //开启注册手机短信验证
                {
                    string smscode = fc["mobilecode"];
                    if (string.IsNullOrEmpty(smscode)) throw new CustomException.CustomException("手机验证码不能为空");
                    if (HttpContext.Current.Session["PhoneSettingSMSCode"] == null || smscode.Trim() != HttpContext.Current.Session["PhoneSettingSMSCode"].ToString()) throw new CustomException.CustomException("验证码不正确或已过期");
                    if (HttpContext.Current.Session["GetPhoneSettingSMSUser"] == null || mobile != HttpContext.Current.Session["GetPhoneSettingSMSUser"].ToString()) throw new CustomException.CustomException("手机号码与接收验证码的设备不相符");
                }
                entity.IsMobile = true;
                entity.CountryCode = country_code;//国家区号
                entity.Mobile = mobile;
                UserService.Update(entity);
                SysDBTool.Commit();

                string msg = "手机修改：" + entity.Mobile;
                var wlog2 = new Data.WarningLog();
                wlog2.CreateTime = DateTime.Now;
                wlog2.IP = HttpContext.Current.Request.UserHostAddress;
                if (HttpContext.Current.Request.UrlReferrer != null)
                    wlog2.Location = HttpContext.Current.Request.UrlReferrer.ToString();
                wlog2.Platform = "用户";
                wlog2.WarningMsg = "用户自主认证修改资料，修改成功" + (!string.IsNullOrEmpty(msg) ? ",涉及改动信息：" + msg : "");
                wlog2.WarningLevel = "一般";
                wlog2.ResultMsg = "通过";
                wlog2.UserName = entity.UserName;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                SysDBTool.Commit();

                result.Status = 200; ;
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 用户星级
        public static string GetUserLevelImages(decimal investment)
        {
            string imagetext = "";
            if (investment == cacheSysParam.SingleAndInit(x => x.ID == 1001).Value.ToDecimal())
                imagetext = "<img src=\"/images/xing.png\" style=\"border:0\">";
            else if (investment == cacheSysParam.SingleAndInit(x => x.ID == 1002).Value.ToDecimal())
                imagetext = "<img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\">";
            else if (investment == cacheSysParam.SingleAndInit(x => x.ID == 1003).Value.ToDecimal())
                imagetext = "<img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\">";
            else if (investment == cacheSysParam.SingleAndInit(x => x.ID == 1004).Value.ToDecimal())
                imagetext = "<img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\">";
            else if (investment == cacheSysParam.SingleAndInit(x => x.ID == 1005).Value.ToDecimal())
                imagetext = "<img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\"><img src=\"/images/xing.png\" style=\"border:0\">";
            return imagetext;
        }
        #endregion

        #region 用户激活操作
        public static void ActivationAccount(Data.User onUser, int OpenID)
        {
            var cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparam", MvcCore.Extensions.CacheTimeType.ByHours, 24, x => x.ID < 6000);
            //更新激活标记
            onUser.IsActivation = true;
            onUser.ActivationTime = DateTime.Now;

            //更新推荐关系
            //用户部分
            var OpenUser = MvcCore.Unity.Get<IUserService>().Single(OpenID);
            //var _parentUser = _refereeUser;
            ////if (_parentUser.RootID != _refereeUser.RootID || Umodel.RootID != _parentUser.RootID) throw new CustomException("推荐人和安置人以及您自己必须同一网内用户");
            //onUser.RefereeDepth = _refereeUser.RefereeDepth + 1;
            //onUser.RefereePath = _refereeUser.RefereePath + "," + _refereeUser.ID;
            //onUser.RefereeID = _refereeUser.ID;
            //onUser.RefereeUser = _refereeUser.UserName;
            //onUser.MainAccountID = 0;

            //用户部分
            //onUser.ParentID = _parentUser.ID;
            //onUser.ParentUser = _parentUser.UserName;
            //onUser.RootID = _parentUser.RootID;
            //onUser.Depth = _parentUser.Depth + 1;
            //onUser.ParentPath = _parentUser.ParentPath + "," + _parentUser.ID;
            //onUser.Child = 0;
            //onUser.DepthSort = 0;
            //onUser.ChildPlace = MvcCore.Unity.Get<IUserService>().List(x => x.ParentID == _parentUser.ID).Count() > 0 ? MvcCore.Unity.Get<IUserService>().List(x => x.ParentID == _parentUser.ID).Max(x => x.ChildPlace) + 1 : 1;

            MvcCore.Unity.Get<IUserService>().Update(onUser);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
            //激活用户扣除激活币
            // Wallets.changeWallet(OpenUser.ID, 0 - cacheSysParam.Single(x => x.ID == 1001).Value.ToInt(), 2004, "激活新用户[" + onUser.UserName + "]帐号扣除");

            ////更新整线用户的对碰余量（双轨时）
            //if (!string.IsNullOrEmpty(onUser.ParentPath))
            //{
            //    string[] ids_dp = onUser.ParentPath.Split(',');
            //    var lst_DPUser = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids_dp.Contains(x.ID.ToString())).OrderBy(x => x.Depth).ThenBy(x => x.ChildPlace).ToList();
            //    foreach (var dpUser in lst_DPUser)
            //    {
            //        Data.User updateUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(dpUser.ID);
            //        if (onUser.Depth - dpUser.Depth == 1)
            //        {
            //            if (onUser.ChildPlace == 1)
            //            {
            //                updateUser.LeftDpMargin = (updateUser.LeftDpMargin ?? 0) + onUser.Investment;
            //                updateUser.LeftAchievement = (updateUser.LeftAchievement ?? 0) + onUser.Investment;
            //            }
            //            else
            //            {
            //                updateUser.RightDpMargin = (updateUser.RightDpMargin ?? 0) + onUser.Investment;
            //                updateUser.RightAchievement = (updateUser.RightAchievement ?? 0) + onUser.Investment;
            //            }
            //        }
            //        else
            //        {
            //            //左区安置点
            //            var leftchild = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x => x.ParentID == dpUser.ID && x.ChildPlace == 1);
            //            //如果出现在左区安置点
            //            if (leftchild != null && (onUser.ParentPath + ",").Contains("," + leftchild.ID + ","))
            //            {
            //                updateUser.LeftDpMargin = (updateUser.LeftDpMargin ?? 0) + onUser.Investment;
            //                updateUser.LeftAchievement = (updateUser.LeftAchievement ?? 0) + onUser.Investment;
            //            }
            //            else
            //            {
            //                updateUser.RightDpMargin = (updateUser.RightDpMargin ?? 0) + onUser.Investment;
            //                updateUser.RightAchievement = (updateUser.RightAchievement ?? 0) + onUser.Investment;
            //            }
            //        }
            //        MvcCore.Unity.Get<Data.Service.IUserService>().Update(updateUser);
            //        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //    }
            //}

            //计算奖金
            //Bonus.CalculateBonus(onUser.ID);

            //激活用户50%作为终身一次性开户费，剩下的购买X币
            //decimal xjbbl = 0;

            //if (onUser.Investment == cacheSysParam.SingleAndInit(x => x.ID == 1002).Value.ToDecimal())
            //    xjbbl = cacheSysParam.SingleAndInit(x => x.ID == 1002).Value2.ToDecimal();
            //else if (onUser.Investment == cacheSysParam.SingleAndInit(x => x.ID == 1003).Value.ToDecimal())
            //    xjbbl = cacheSysParam.SingleAndInit(x => x.ID == 1003).Value2.ToDecimal();
            //else if (onUser.Investment == cacheSysParam.SingleAndInit(x => x.ID == 1004).Value.ToDecimal())
            //    xjbbl = cacheSysParam.SingleAndInit(x => x.ID == 1004).Value2.ToDecimal();
            //else if (onUser.Investment == cacheSysParam.SingleAndInit(x => x.ID == 1005).Value.ToDecimal())
            //    xjbbl = cacheSysParam.SingleAndInit(x => x.ID == 1005).Value2.ToDecimal();
            //else if (onUser.Investment == cacheSysParam.SingleAndInit(x => x.ID == 1001).Value.ToDecimal())
            //    xjbbl = cacheSysParam.SingleAndInit(x => x.ID == 1001).Value2.ToDecimal();

            //decimal xjb = onUser.Investment * xjbbl;

        }
        #endregion

        #region 用户激活操作 wp18080102
        public static void ActivationUser(decimal money, int onUserID)
        {
            var onUser = MvcCore.Unity.Get<IUserService>().Single(onUserID);
            onUser.TransferInto = (onUser.TransferInto ?? 0) + money;
            if (!onUser.IsActivation)
            {
                var param3105 = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3105);
                if (onUser.TransferInto >= param3105.Value.ToDecimal())
                {
                    //更新激活标记
                    onUser.IsActivation = true;
                    onUser.ActivationTime = DateTime.Now;
                }
            }
            MvcCore.Unity.Get<IUserService>().Update(onUser);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
        }
        #endregion

        #region 用户升级
        /// <summary>
        /// 用户升级 计入累计积分并升级
        /// 1.币种兑换（余额兑换积分）1个
        /// 2.后台派币1个
        /// 3.用户转账2个
        /// 4.商城购买商品
        /// </summary>
        /// <param name="uid">用户id</param>
        public static void UpdateLevel(Data.User onUser)
        {
            // bool isupdate = false;
            ////var onUser = MvcCore.Unity.Get<IUserService>().Single(uid);
            ////Data.User onUser = MvcCore.Unity.Get<IUserService>().List().FirstOrDefault(x => x.ID == uid);
            // if (onUser != null && onUser.UserLevel <= (int)JN.Data.Enum.UserLevel.Level3)//没到最高级别
            // {
            //     //decimal tz1001 = cacheSysParam.SingleAndInit(x => x.ID >= 1001).Value.ToDecimal();
            //     //decimal tz1002 = cacheSysParam.SingleAndInit(x => x.ID >= 1002).Value.ToDecimal();
            //     //decimal tz1003 = cacheSysParam.SingleAndInit(x => x.ID >= 1003).Value.ToDecimal();

            //     if (onUser.UserLevel <= (int)JN.Data.Enum.UserLevel.Level2 && (onUser.ReserveDecamal2 ?? 0) >= cacheSysParam.SingleAndInit(x => x.ID >= 1003).Value.ToDecimal())
            //     {
            //         isupdate = true;
            //         onUser.UserLevel = (int)JN.Data.Enum.UserLevel.Level3;
            //         //MvcCore.Unity.Get<IUserService>().Update(onUser);
            //         //MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //     }
            //     else if (onUser.UserLevel <= (int)JN.Data.Enum.UserLevel.Level1 && (onUser.ReserveDecamal2 ?? 0) >= cacheSysParam.SingleAndInit(x => x.ID >= 1002).Value.ToDecimal())
            //     {
            //         isupdate = true;
            //         onUser.UserLevel = (int)JN.Data.Enum.UserLevel.Level2;
            //         //MvcCore.Unity.Get<IUserService>().Update(onUser);
            //         //MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //     }           
            // }

            // if (isupdate)
            // {
            //     //用户表更新语句   
            //     string sql = string.Format(" update [User] set UserLevel={0} where ID={1}", onUser.UserLevel, onUser.ID);
            //     DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };        
            //     MvcCore.Unity.Get<Data.Service.ISysDBTool>().ExecuteSQL(sql, dbparam);
            // }
        }
        #endregion

        #region 生成随机编号
        //生成随机编号
        public static string GetUserCode(int num)
        {
            DateTime dateTime = DateTime.Now;
            string result = Tool.StringHelp.GetRandomNumber(num);//7位随机数字
            if (IsHave(result))
            {
                return GetUserCode(num);
            }
            return result;
        }
        #endregion

        #region 检查订单号是否重复

        /// <summary>
        /// 检查订单号是否重复
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static bool IsHave(string number)
        {
            return MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.UserName == "C" + number).Count() > 0;
        }
        #endregion

        #region 选择币种余额主钱包（15个预留）
        /// <summary>
        /// 选择币种余额（10个预留）
        /// </summary>
        /// <param name="CurID"></param>
        /// <returns></returns>
        public static decimal WalletCur(int WalletCurID, JN.Data.User Umodel)
        {
            decimal d = 0;
            //15个币种（预留）
            switch (WalletCurID)
            {
                case 3001:
                    d = Umodel.Cur3001 ?? 0;
                    break;
                case 3002:
                    d = Umodel.Cur3002 ?? 0;
                    break;
                case 3003:
                    d = Umodel.Cur3003 ?? 0;
                    break;
                case 3004:
                    d = Umodel.Cur3004 ?? 0;
                    break;
                case 3005:
                    d = Umodel.Cur3005 ?? 0;
                    break;
                case 3006:
                    d = Umodel.Cur3006 ?? 0;
                    break;
                case 3007:
                    d = Umodel.Cur3007 ?? 0;
                    break;
                case 3008:
                    d = Umodel.Cur3008 ?? 0;
                    break;
                case 3009:
                    d = Umodel.Cur3009 ?? 0;
                    break;
                case 3010:
                    d = Umodel.Cur3010 ?? 0;
                    break;
                case 3011:
                    d = Umodel.Cur3011 ?? 0;
                    break;
                case 3012:
                    d = Umodel.Cur3012 ?? 0;
                    break;
                case 3013:
                    d = Umodel.Cur3013 ?? 0;
                    break;
                case 3014:
                    d = Umodel.Cur3014 ?? 0;
                    break;
                case 3015:
                    d = Umodel.Cur3015 ?? 0;
                    break;
                case 2001:
                    d = Umodel.Wallet2001;
                    break;
            }
            return d;
        }
        #endregion

        #region 选择币种余额冻结钱包（15个预留）
        /// <summary>
        /// 选择币种余额冻结钱包
        /// </summary>
        /// <param name="CurID">币种id号</param>
        /// <param name="Umodel">用户实体</param>
        /// <returns>返回余额</returns>
        public static decimal WalletCurFro(int CurID, int UserID)
        {
            var Umodel = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.ID == UserID);
            decimal d = 0;
            //15个币种（预留）
            switch (CurID)
            {
                case 3001:
                    d = Umodel.CurFro3001 ?? 0;
                    break;
                case 3002:
                    d = Umodel.CurFro3002 ?? 0;
                    break;
                case 3003:
                    d = Umodel.CurFro3003 ?? 0;
                    break;
                case 3004:
                    d = Umodel.CurFro3004 ?? 0;
                    break;
                case 3005:
                    d = Umodel.CurFro3005 ?? 0;
                    break;
                case 3006:
                    d = Umodel.CurFro3006 ?? 0;
                    break;
                case 3007:
                    d = Umodel.CurFro3007 ?? 0;
                    break;
                case 3008:
                    d = Umodel.CurFro3008 ?? 0;
                    break;
                case 3009:
                    d = Umodel.CurFro3009 ?? 0;
                    break;
                case 3010:
                    d = Umodel.CurFro3010 ?? 0;
                    break;
                case 3011:
                    d = Umodel.CurFro3011 ?? 0;
                    break;
                case 3012:
                    d = Umodel.CurFro3012 ?? 0;
                    break;
                case 3013:
                    d = Umodel.CurFro3013 ?? 0;
                    break;
                case 3014:
                    d = Umodel.CurFro3014 ?? 0;
                    break;
                case 3015:
                    d = Umodel.CurFro3015 ?? 0;
                    break;
                case 2001:
                    d = Umodel.Wallet2001;
                    break;
            }
            return d;
        }
        #endregion

        #region 选择币种余额冻结钱包+主钱包（15个预留）
        /// <summary>
        /// 选择币种余额冻结钱包
        /// </summary>
        /// <param name="CurID">币种id号</param>
        /// <param name="Umodel">用户实体</param>
        /// <returns>返回余额</returns>
        public static decimal WalletCurFroCount(int CurID, int UserID)
        {
            var Umodel = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.ID == UserID);
            decimal d = 0;
            //15个币种（预留）
            switch (CurID)
            {
                case 3001:
                    d = (Umodel.Cur3001 ?? 0) + (Umodel.CurFro3001 ?? 0);
                    break;
                case 3002:
                    d = (Umodel.Cur3002 ?? 0) + (Umodel.CurFro3002 ?? 0);
                    break;
                case 3003:
                    d = (Umodel.Cur3003 ?? 0) + (Umodel.CurFro3003 ?? 0);
                    break;
                case 3004:
                    d = (Umodel.Cur3004 ?? 0) + (Umodel.CurFro3004 ?? 0);
                    break;
                case 3005:
                    d = (Umodel.Cur3005 ?? 0) + (Umodel.CurFro3005 ?? 0);
                    break;
                case 3006:
                    d = (Umodel.Cur3006 ?? 0) + (Umodel.CurFro3006 ?? 0);
                    break;
                case 3007:
                    d = (Umodel.Cur3007 ?? 0) + (Umodel.CurFro3007 ?? 0);
                    break;
                case 3008:
                    d = (Umodel.Cur3008 ?? 0) + (Umodel.CurFro3008 ?? 0);
                    break;
                case 3009:
                    d = (Umodel.Cur3009 ?? 0) + (Umodel.CurFro3009 ?? 0);
                    break;
                case 3010:
                    d = (Umodel.Cur3010 ?? 0) + (Umodel.CurFro3010 ?? 0);
                    break;
                case 3011:
                    d = (Umodel.Cur3011 ?? 0) + (Umodel.CurFro3011 ?? 0);
                    break;
                case 3012:
                    d = (Umodel.Cur3012 ?? 0) + (Umodel.CurFro3012 ?? 0);
                    break;
                case 3013:
                    d = (Umodel.Cur3013 ?? 0) + (Umodel.CurFro3013 ?? 0);
                    break;
                case 3014:
                    d = (Umodel.Cur3014 ?? 0) + (Umodel.CurFro3014 ?? 0);
                    break;
                case 3015:
                    d = (Umodel.Cur3015 ?? 0) + (Umodel.CurFro3015 ?? 0);
                    break;

            }
            return d;
        }
        #endregion

        #region 获取币种地址（15个预留）
        /// <summary>
        /// 获取币种地址（15个预留）
        /// </summary>
        /// <param name="CurID"></param>
        /// <returns></returns>
        public static string GetWalletAddress(int CurID, JN.Data.User Umodel)
        {
            string Address = "";
            //十个币种（预留）
            switch (CurID)
            {
                case 3001:
                    Address = Umodel.R3001;
                    break;
                case 3002:
                    Address = Umodel.R3002;
                    break;
                case 3003:
                    Address = Umodel.R3003;
                    break;
                case 3004:
                    Address = Umodel.R3004;
                    break;
                case 3005:
                    Address = Umodel.R3005;
                    break;
                case 3006:
                    Address = Umodel.R3006;
                    break;
                case 3007:
                    Address = Umodel.R3007;
                    break;
                case 3008:
                    Address = Umodel.R3008;
                    break;
                case 3009:
                    Address = Umodel.R3009;
                    break;
                case 3010:
                    Address = Umodel.R3010;
                    break;
                case 3011:
                    Address = Umodel.R3011;
                    break;
                case 3012:
                    Address = Umodel.R3012;
                    break;
                case 3013:
                    Address = Umodel.R3013;
                    break;
                case 3014:
                    Address = Umodel.R3014;
                    break;
                case 3015:
                    Address = Umodel.R3015;
                    break;

            }
            return Address;
        }
        #endregion

        #region 写入币种地址（15个预留）
        /// <summary>
        /// 写入币种地址（15个预留）
        /// </summary>
        /// <param name="CurID"></param>
        /// <returns></returns>
        public static void SetWalletAddress(int CurID, JN.Data.User Umodel, string Address)
        {
            //十个币种（预留）
            switch (CurID)
            {
                case 3001:
                    Umodel.R3001 = Address;
                    break;
                case 3002:
                    Umodel.R3002 = Address;
                    break;
                case 3003:
                    Umodel.R3003 = Address;
                    break;
                case 3004:
                    Umodel.R3004 = Address;
                    break;
                case 3005:
                    Umodel.R3005 = Address;
                    break;
                case 3006:
                    Umodel.R3006 = Address;
                    break;
                case 3007:
                    Umodel.R3007 = Address;
                    break;
                case 3008:
                    Umodel.R3008 = Address;
                    break;
                case 3009:
                    Umodel.R3009 = Address;
                    break;
                case 3010:
                    Umodel.R3010 = Address;
                    break;
                case 3011:
                    Umodel.R3011 = Address;
                    break;
                case 3012:
                    Umodel.R3012 = Address;
                    break;
                case 3013:
                    Umodel.R3013 = Address;
                    break;
                case 3014:
                    Umodel.R3014 = Address;
                    break;
                case 3015:
                    Umodel.R3015 = Address;
                    break;

            }
            MvcCore.Unity.Get<Data.Service.IUserService>().Update(Umodel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
        }
        #endregion

        #region 写入币种地址 一个钱包两个币种共用
        /// <summary>
        /// 一个钱包两个币种共用
        /// </summary>
        /// <param name="CurID"></param>
        /// <returns></returns>
        public static void SetTwoWalletAddress(JN.Data.User Umodel, string Address)
        {
            var c1 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(9);
            var c2 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(10);

            #region 两个币种共用
            //十个币种（预留）
            switch (c1.WalletCurID ?? 0)
            {
                case 3001:
                    Umodel.R3001 = Address;
                    break;
                case 3002:
                    Umodel.R3002 = Address;
                    break;
                case 3003:
                    Umodel.R3003 = Address;
                    break;
                case 3004:
                    Umodel.R3004 = Address;
                    break;
                case 3005:
                    Umodel.R3005 = Address;
                    break;
                case 3006:
                    Umodel.R3006 = Address;
                    break;
                case 3007:
                    Umodel.R3007 = Address;
                    break;
                case 3008:
                    Umodel.R3008 = Address;
                    break;
                case 3009:
                    Umodel.R3009 = Address;
                    break;
                case 3010:
                    Umodel.R3010 = Address;
                    break;
                case 3011:
                    Umodel.R3011 = Address;
                    break;
                case 3012:
                    Umodel.R3012 = Address;
                    break;
                case 3013:
                    Umodel.R3013 = Address;
                    break;
                case 3014:
                    Umodel.R3014 = Address;
                    break;
                case 3015:
                    Umodel.R3015 = Address;
                    break;

            }
            switch (c2.WalletCurID ?? 0)
            {
                case 3001:
                    Umodel.R3001 = Address;
                    break;
                case 3002:
                    Umodel.R3002 = Address;
                    break;
                case 3003:
                    Umodel.R3003 = Address;
                    break;
                case 3004:
                    Umodel.R3004 = Address;
                    break;
                case 3005:
                    Umodel.R3005 = Address;
                    break;
                case 3006:
                    Umodel.R3006 = Address;
                    break;
                case 3007:
                    Umodel.R3007 = Address;
                    break;
                case 3008:
                    Umodel.R3008 = Address;
                    break;
                case 3009:
                    Umodel.R3009 = Address;
                    break;
                case 3010:
                    Umodel.R3010 = Address;
                    break;
                case 3011:
                    Umodel.R3011 = Address;
                    break;
                case 3012:
                    Umodel.R3012 = Address;
                    break;
                case 3013:
                    Umodel.R3013 = Address;
                    break;
                case 3014:
                    Umodel.R3014 = Address;
                    break;
                case 3015:
                    Umodel.R3015 = Address;
                    break;

            }
            #endregion

            MvcCore.Unity.Get<Data.Service.IUserService>().Update(Umodel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
        }
        #endregion

        #region 选择币种平台总额(包含已挂单未成交的)（15个预留）
        /// <summary>
        /// 选择币种平台总余额(包含已挂单未成交的)
        /// </summary>
        /// <param name="c">该币种的模型实体</param>
        /// <param name="UserService">用户接口</param>
        /// <returns>返回总额</returns>
        public static decimal CurrencyCount(JN.Data.Currency c, JN.Data.Service.IUserService UserService)
        {
            decimal d = 0;
            //该币种的余额{在挂单委托中和部分成交的}
            decimal StockQuantityCount = (MvcCore.Unity.Get<JN.Data.Service.IStockEntrustsTradeService>().List(x => x.Status <= 1 && x.Status > 0 && x.CurID == c.ID).Count() > 0 ? MvcCore.Unity.Get<JN.Data.Service.IStockEntrustsTradeService>().List(x => x.Status <= 1 && x.Status > 0 && x.CurID == c.ID).Sum(x => x.Quantity) : 0);
            //十个币种（预留）
            switch (c.WalletCurID)
            {
                case 3001:
                    d = UserService.List().Sum((x => x.Cur3001 ?? 0)) + UserService.List().Sum((x => x.CurFro3001 ?? 0));
                    break;
                case 3002:
                    d = UserService.List().Sum((x => x.Cur3002 ?? 0)) + UserService.List().Sum((x => x.CurFro3002 ?? 0));
                    break;
                case 3003:
                    d = UserService.List().Sum((x => x.Cur3003 ?? 0)) + UserService.List().Sum((x => x.CurFro3003 ?? 0));
                    break;
                case 3004:
                    d = UserService.List().Sum((x => x.Cur3004 ?? 0)) + UserService.List().Sum((x => x.CurFro3004 ?? 0));
                    break;
                case 3005:
                    d = UserService.List().Sum((x => x.Cur3005 ?? 0)) + UserService.List().Sum((x => x.CurFro3005 ?? 0));
                    break;
                case 3006:
                    d = UserService.List().Sum((x => x.Cur3006 ?? 0)) + UserService.List().Sum((x => x.CurFro3006 ?? 0));
                    break;
                case 3007:
                    d = UserService.List().Sum((x => x.Cur3007 ?? 0)) + UserService.List().Sum((x => x.CurFro3007 ?? 0));
                    break;
                case 3008:
                    d = UserService.List().Sum((x => x.Cur3008 ?? 0)) + UserService.List().Sum((x => x.CurFro3008 ?? 0));
                    break;
                case 3009:
                    d = UserService.List().Sum((x => x.Cur3009 ?? 0)) + UserService.List().Sum((x => x.CurFro3009 ?? 0));
                    break;
                case 3010:
                    d = UserService.List().Sum((x => x.Cur3010 ?? 0)) + UserService.List().Sum((x => x.CurFro3010 ?? 0));
                    break;
                case 3011:
                    d = UserService.List().Sum((x => x.Cur3011 ?? 0)) + UserService.List().Sum((x => x.CurFro3011 ?? 0));
                    break;
                case 3012:
                    d = UserService.List().Sum((x => x.Cur3012 ?? 0)) + UserService.List().Sum((x => x.CurFro3012 ?? 0));
                    break;
                case 3013:
                    d = UserService.List().Sum((x => x.Cur3013 ?? 0)) + UserService.List().Sum((x => x.CurFro3013 ?? 0));
                    break;
                case 3014:
                    d = UserService.List().Sum((x => x.Cur3014 ?? 0)) + UserService.List().Sum((x => x.CurFro3014 ?? 0));
                    break;
                case 3015:
                    d = UserService.List().Sum((x => x.Cur3015 ?? 0)) + UserService.List().Sum((x => x.CurFro3015 ?? 0));
                    break;

            }
            return (d + StockQuantityCount);
        }
        #endregion

        #region 计算净资产
        /// <summary>
        /// 计算净资产
        /// </summary>
        /// <param name="Umodel">该用户</param>
        /// <returns></returns>
        public static decimal WalletTotalAssets(JN.Data.User Umodel)
        {
            decimal totalAssets = 0;
            totalAssets = Umodel.Wallet2001 + Umodel.Wallet2002 + Umodel.Wallet2003 + Umodel.Wallet2004 + Umodel.Wallet2005 + Umodel.Wallet2006 + WalletCurBalance(3001, Umodel) + WalletCurBalance(3002, Umodel) + WalletCurBalance(3003, Umodel) + WalletCurBalance(3004, Umodel) + WalletCurBalance(3005, Umodel) + WalletCurBalance(3006, Umodel) + WalletCurBalance(3007, Umodel) + WalletCurBalance(3008, Umodel) + WalletCurBalance(3009, Umodel) + WalletCurBalance(3010, Umodel) + WalletCurBalance(3011, Umodel) + WalletCurBalance(3012, Umodel) + WalletCurBalance(3013, Umodel) + WalletCurBalance(3014, Umodel) + WalletCurBalance(3015, Umodel);
            return totalAssets;
        }
        #endregion

        #region 计算总资产
        /// <summary>
        /// 计算总资产
        /// </summary>
        /// <param name="Umodel">该用户</param>
        /// <returns></returns>
        public static decimal WalletTotalCount(JN.Data.User Umodel)
        {
            decimal totalAssets = 0;
            totalAssets = Umodel.Wallet2001 + Umodel.Wallet2002 + Umodel.Wallet2003 + Umodel.Wallet2004 + Umodel.Wallet2005 + Umodel.Wallet2006 + WalletCurBalance(3001, Umodel) + WalletCurBalance(3002, Umodel) + WalletCurBalance(3003, Umodel) + WalletCurBalance(3004, Umodel) + WalletCurBalance(3005, Umodel) + WalletCurBalance(3006, Umodel) + WalletCurBalance(3007, Umodel) + WalletCurBalance(3008, Umodel) + WalletCurBalance(3009, Umodel) + WalletCurBalance(3010, Umodel) + WalletCurBalance(3011, Umodel) + WalletCurBalance(3012, Umodel) + WalletCurBalance(3013, Umodel) + WalletCurBalance(3014, Umodel) + WalletCurBalance(3015, Umodel) + WalletCurBalance(3001, Umodel, true) + WalletCurBalance(3002, Umodel, true) + WalletCurBalance(3003, Umodel, true) + WalletCurBalance(3004, Umodel, true) + WalletCurBalance(3005, Umodel, true) + WalletCurBalance(3006, Umodel, true) + WalletCurBalance(3007, Umodel, true) + WalletCurBalance(3008, Umodel, true) + WalletCurBalance(3009, Umodel, true) + WalletCurBalance(3010, Umodel, true) + WalletCurBalance(3011, Umodel, true) + WalletCurBalance(3012, Umodel, true) + WalletCurBalance(3013, Umodel, true) + WalletCurBalance(3014, Umodel, true) + WalletCurBalance(3015, Umodel, true);
            return totalAssets;
        }
        #endregion

        #region 钱包余额

        /// <summary>
        /// 计算钱包余额
        /// </summary>
        /// <param name="CurID">钱包id号</param>
        /// <param name="onUser">当前用户</param>
        /// <param name="isFro">是否计算冻结钱包</param>
        /// <returns>返回余额</returns>
        public static decimal WalletCurBalance(int CurID, JN.Data.User onUser, bool isFro = false)
        {
            decimal balanceWallet = 0;//余额
            if (isFro)//是否计算冻结
            {
                switch (CurID)
                {
                    case 3001:
                        balanceWallet = onUser.CurFro3001 ?? 0;
                        break;
                    case 3002:
                        balanceWallet = onUser.CurFro3002 ?? 0;
                        break;
                    case 3003:
                        balanceWallet = onUser.CurFro3003 ?? 0;
                        break;
                    case 3004:
                        balanceWallet = onUser.CurFro3004 ?? 0;
                        break;
                    case 3005:
                        balanceWallet = onUser.CurFro3005 ?? 0;
                        break;
                    case 3006:
                        balanceWallet = onUser.CurFro3006 ?? 0;
                        break;
                    case 3007:
                        balanceWallet = onUser.CurFro3007 ?? 0;
                        break;
                    case 3008:
                        balanceWallet = onUser.CurFro3008 ?? 0;
                        break;
                    case 3009:
                        balanceWallet = onUser.CurFro3009 ?? 0;
                        break;
                    case 3010:
                        balanceWallet = onUser.CurFro3010 ?? 0;
                        break;
                    case 3011:
                        balanceWallet = onUser.CurFro3011 ?? 0;
                        break;
                    case 3012:
                        balanceWallet = onUser.CurFro3012 ?? 0;
                        break;
                    case 3013:
                        balanceWallet = onUser.CurFro3013 ?? 0;
                        break;
                    case 3014:
                        balanceWallet = onUser.CurFro3014 ?? 0;
                        break;
                    case 3015:
                        balanceWallet = onUser.CurFro3015 ?? 0;
                        break;
                }
            }
            else
            {
                switch (CurID)
                {
                    case 3001:
                        balanceWallet = onUser.Cur3001 ?? 0;
                        break;
                    case 3002:
                        balanceWallet = onUser.Cur3002 ?? 0;
                        break;
                    case 3003:
                        balanceWallet = onUser.Cur3003 ?? 0;
                        break;
                    case 3004:
                        balanceWallet = onUser.Cur3004 ?? 0;
                        break;
                    case 3005:
                        balanceWallet = onUser.Cur3005 ?? 0;
                        break;
                    case 3006:
                        balanceWallet = onUser.Cur3006 ?? 0;
                        break;
                    case 3007:
                        balanceWallet = onUser.Cur3007 ?? 0;
                        break;
                    case 3008:
                        balanceWallet = onUser.Cur3008 ?? 0;
                        break;
                    case 3009:
                        balanceWallet = onUser.Cur3009 ?? 0;
                        break;
                    case 3010:
                        balanceWallet = onUser.Cur3010 ?? 0;
                        break;
                    case 3011:
                        balanceWallet = onUser.Cur3011 ?? 0;
                        break;
                    case 3012:
                        balanceWallet = onUser.Cur3012 ?? 0;
                        break;
                    case 3013:
                        balanceWallet = onUser.Cur3013 ?? 0;
                        break;
                    case 3014:
                        balanceWallet = onUser.Cur3014 ?? 0;
                        break;
                    case 3015:
                        balanceWallet = onUser.Cur3015 ?? 0;
                        break;
                }
            }
            if (balanceWallet > 0)
            {
                //var currency = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == CurID);
                //if (currency != null)
                //{
                //    string curprice = Tool.StringHelp.GetPrice("https://api.coinmarketcap.com/v1/ticker/" + currency.English + "/?convert=USD", "USD");//当前价格"0";//
                //    if (!JN.Services.Tool.StringHelp.IsNumber(curprice)) curprice = "0";
                //    balanceWallet = balanceWallet * curprice.ToDecimal();
                //}
            }
            return balanceWallet;
        }
        #endregion

        #region 绑定邮箱
        /// <summary>
        /// 绑定邮箱
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="UserService">用户接口</param>
        /// <param name="Umodel">用户实体</param>
        /// <returns>返回状态信息</returns>
        [HttpPost]
        public static Result EmailSettings(FormCollection fc, IUserService UserService, Data.User Umodel)
        {
            Result result = new Result();
            try
            {
                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);

                string code = fc["codeEmail"];
                string Email = fc["Email"].ToString();
                if (!Tool.StringHelp.IsEmail(Email)) throw new JN.Services.CustomException.CustomException("你的邮箱格式错误！");

                string vcode = (HttpContext.Current.Session["EmailValidateCode"] ?? "").ToString();//邮箱验证码
                string vEmail = (HttpContext.Current.Session["GetEmail"] ?? "").ToString();//邮箱

                if (string.IsNullOrEmpty(code)) throw new JN.Services.CustomException.CustomException("邮箱验证码不能为空");
                if (vcode == null || code.Trim() != vcode.ToString()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                if (vEmail == null || Email != vEmail.ToString()) throw new JN.Services.CustomException.CustomException("邮箱与接收验证码的邮箱不相符");

                if (UserService.List(x => x.Email == Email.Trim()).Count() > 0 && !string.IsNullOrEmpty(Email) && sysEntity.RegistOnlyOneItems.Contains(",Email,")) throw new JN.Services.CustomException.CustomException("邮箱已被使用");

                var model = UserService.Single(x => x.ID == Umodel.ID);
                model.Email = Email;
                System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().ExecuteSQL("update [User] set Email='" + model.Email + "' where ID= " + model.ID, s);
                MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().ExecuteSQL("update [User] set IsEmail=1 where ID= " + model.ID, s);
                //UserService.Update(model);
                //SysDBTool.Commit();
                result.Status = 200;
                result.Message = "绑定邮箱成功！";
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送邮箱验证码（绑定邮箱）
        /// <summary>
        /// 发送邮箱验证码（绑定邮箱）
        /// </summary>
        /// <param name="Umodel">用户实体</param>
        /// <returns>返回状态信息</returns>
        public static Result SendEmail(Data.User Umodel)
        {
            Result r = new Result();
            try
            {
                string Email = HttpContext.Current.Request["Email"];
                if (string.IsNullOrEmpty(Email)) throw new CustomException.CustomException("请设置邮箱账号！");
                if (!JN.Services.Tool.StringHelp.IsEmail(Email)) throw new CustomException.CustomException("对不起，你的邮箱格式不正确!");

                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);
                if (MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.Email == Email.Trim()).Count() > 0 && !string.IsNullOrEmpty(Email) && sysEntity.RegistOnlyOneItems.Contains(",Email,")) throw new JN.Services.CustomException.CustomException("邮箱已被使用");

                if (HttpContext.Current.Session["emailSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session["emailSendTime"].ToString())))
                        throw new JN.Services.CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                ValidateCode vEmailCode = new ValidateCode();
                var seting = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().List(x => x.ID > 0).FirstOrDefault();
                string code = RandUserCode(5);
                HttpContext.Current.Session["emailSendTime"] = DateTime.Now;

                //string body = "<p>来自" + seting.SysName + "验证邮箱，你的验证码是：" + code + "</p><p>如果此活动不是您本人所为，请尽快联系" + seting.SysName + "在线客服。<p>具体交易详情请登录" + seting.SysName + "进行查询。请注意" + seting.SysName + "绝不会以任何形式询问您的帐户密码和验证码。</p><p>本邮件由系统自动生成，无需授权签名，请勿直接回复本邮件。</p>";

                string body = "<p>This is  a verification mail from" + seting.SiteUrl + ",your verification code is：" + code + ".</p><p>If this activity is not your own, please contact online service of" + seting.SiteUrl + " as soon as possible.<p>For detail of transaction, please login " + seting.SysName + ".Please note that " + seting.SysName + "will never ask your account password and verification code in any form.</p><p>This mail is automatically generated by the system without authorizing the signature. Please do not reply to this email directly.</p>";


                //发送邮件
                bool b = JN.Services.Tool.SMSHelper.SendMail(seting.SysName + "Please verify your mailbox", body, Email, seting.SmtpEmailAddress, seting.SmtpUserName, seting.SmtpPassword, seting.SmtpServer, seting.SmtpPort.ToInt());
                if (b)
                {
                    HttpContext.Current.Session["EmailValidateCode"] = code;
                    HttpContext.Current.Session["GetEmail"] = Email;
                    HttpContext.Current.Session["SMSSendTime"] = DateTime.Now;
                    r.Message = "Success!";
                    r.Status = 200;
                }
                else
                {
                    HttpContext.Current.Session["EmailValidateCode"] = null;
                    HttpContext.Current.Session["GetEmail"] = null;
                    r.Message = "Mailbox verification code failed to send, please refer to the sending record for details.";
                    r.Status = 500;
                    //r.Message = "Error!";
                }
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                r.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                r.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                r.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return r;
        }
        #endregion

        #region 判断等级并写入等级

        /// <summary>
        /// 判断等级并写入等级
        /// </summary>
        public static void UserLevel(int UserID)
        {
            //获取用户实体
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(UserID);

            var one = cacheSysParam.Single(x => x.ID == 1301);
            var tow = cacheSysParam.Single(x => x.ID == 1302);

            //循环整条线
            string[] ids = onUser.RefereePath.Split(',');
            var userlist = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString())).OrderByDescending(x => x.ID).ToList();//查询一条线下的人
            foreach (var user in userlist)
            {
                var usermodel = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(user.ID);
                //查找推荐人
                var userRelist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.RefereeID == usermodel.ID);

                if (usermodel.UserLevel == 0)
                {
                    int reCount = 0;//累计达到条件的直推数
                    foreach (var item in userRelist)
                    {
                        //查找本推荐人达到n人
                        int numner = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.RefereePath.Contains("," + item.ID)).Count();//加上自己
                        if (one.Value3.ToInt() <= numner)
                        {
                            reCount += 1;
                        }
                    }
                    if (userRelist.Count() >= one.Value2.ToInt() && one.Value2.ToInt() <= reCount)
                    {
                        usermodel.UserLevel = 1;
                    }
                }
                else if (usermodel.UserLevel == 1)
                {
                    //查找满足3个1等级的可升级
                    int Count = 0;//累计达到条件的直推数
                    foreach (var item in userRelist)
                    {
                        //查找本推荐人达到n人
                        int numner = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.RefereePath.Contains("," + item.ID) && x.UserLevel == 1).Count();//加上自己
                        if (tow.Value3.ToInt() <= numner)
                        {
                            Count += 1;
                        }
                    }
                    if (userRelist.Count() >= tow.Value2.ToInt() && Count >= tow.Value2.ToInt())
                    {
                        usermodel.UserLevel = 2;
                    }
                }

                MvcCore.Unity.Get<Data.Service.IUserService>().Update(usermodel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 汇总手续费，并计算推广佣金奖
        /// <summary>
        /// 汇总手续费，并计算推广奖
        /// </summary>
        public static void SetPoundage(decimal money, int UserID, string descinfo)
        {
            //计算奖
            //Bonus.Bouns1103(UserID, money, descinfo);

            //获取用户实体
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(UserID);
            onUser.Poundage = (onUser.Poundage ?? 0) + money;
            MvcCore.Unity.Get<Data.Service.IUserService>().Update(onUser);
            MvcCore.Unity.Get<ISysDBTool>().Commit();

        }
        #endregion

        #region 随机生成用户名
        /// <summary>
        /// 随机生成用户名
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string RandUserCode(int n)
        {
            char[] arrChar = new char[]{
             '1','2','3','4','5','6','7','8','9','0','1','2','3','4','5','6','7','8','9','0','1','2','3','4','5',
             '0','1','2','3','4','5','6','7','8','9',
             '1','2','3','4','5','6','7','8','9','0','1','2','3','4','5','6','7','8','9','0','1','2','3','4','5'
            };
            //char[] arrstr = new char[]{
            //'Q','W','E','R','T','Y','U','I','O','P','A','S','D','F','G','H','J','K','L','Z','X','C','V','B','N','M',
            //'L','K','J','H','G','F','D','S','A','M','N','B','V','C','X','Z','P','O','I','U','Y','T','R','E','W','Q'
            //};
            StringBuilder num = new StringBuilder();
            Random rnd = new Random(DateTime.Now.Millisecond);
            //string strName = "";
            for (int j = 1; j < 10; ++j)
            {
                num.Clear();
                //for (int i = 0; i < 2; i++)
                //{
                //    num.Append(arrstr[rnd.Next(0, arrstr.Length)].ToString());
                //}
                //num.Append("-");
                for (int i = 0; i < n; i++)
                {
                    num.Append(arrChar[rnd.Next(0, arrChar.Length)].ToString());
                }
                // strName = "CF-" + num.ToString();

            }

            return num.ToString();
        }
        #endregion

        #region 清空全部缓存
        /// <summary>
        /// 清空全部缓存
        /// </summary>
        public static void ClearCacheAll()
        {
            List<String> caches = MvcCore.Extensions.CacheExtensions.GetAllCache();
            foreach (var cachename in caches)
                MvcCore.Extensions.CacheExtensions.ClearCache(cachename);
            List<String> caches2 = Services.Tool.DataCache.GetAllCache();
            foreach (var cachename in caches2)
                Services.Tool.DataCache.ClearCache(cachename);


        }
        #endregion

        #region 合成图

        /// <summary>
        /// 调用此函数后使此两种图片合并，类似相册，有个
        /// 背景图，中间贴自己的目标图片
        /// </summary>
        /// <param name="sourceImg">相框图片</param>
        /// <param name="destImg">照片图片</param>
        public static byte[] CombinImage(string sourceImg, byte[] destImg, byte[] name, int namewidth, int nameheight)
        {

            string path = System.Web.HttpContext.Current.Server.MapPath(sourceImg);//通过相对路径找到绝对路径
            System.Drawing.Image imgBack = System.Drawing.Image.FromFile(path);     //读取相框图片  

            System.Drawing.Image img = ReturnPhoto(destImg);        //照片图片（将二维码数据流转换为图片作为照片图片）

            //System.Drawing.Image userimg = ReturnPhoto(name);

            //从指定的System.Drawing.Image创建新的System.Drawing.Graphics        
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(imgBack);

            //g.DrawImage(imgBack, 0, 0, 500, 715);      // g.DrawImage(imgBack, 0, 0, 相框宽, 相框高); 

            //g.FillRectangle(System.Drawing.Brushes.Black, 16, 16, (int)112 + 2, ((int)73 + 2));//相片四周刷一层黑色边框

            //g.DrawImage(img, 照片与相框的左边距, 照片与相框的上边距, 照片宽, 照片高);
            g.DrawImage(img, 270, 510, 180, 180);

            //画用户名
            // g.DrawImage(userimg, 220, 136, namewidth, nameheight);

            GC.Collect();

            return PhotoImageInsert(imgBack);        //将合成的图片转为数据流
        }

        //将byte[] 转换成Image 
        public static System.Drawing.Image ReturnPhoto(byte[] streamByte)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(streamByte);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return img;
        }

        //将Image转换成流数据，并保存为byte[]   
        public static byte[] PhotoImageInsert(System.Drawing.Image imgPhoto)
        {
            MemoryStream mstream = new MemoryStream();
            imgPhoto.Save(mstream, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] byData = new Byte[mstream.Length];
            mstream.Position = 0;
            mstream.Read(byData, 0, byData.Length); mstream.Close();
            return byData;
        }
        #endregion

        #region 绑定用户
        /// <summary>
        /// 绑定用户
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="UserService">用户表接口</param>
        /// <param name="entity">用户模型</param>
        /// <param name="RegUser">注册的用户</param>
        /// <returns></returns>
        public static Result BingUserMethod(FormCollection form, JN.Data.User Umodel, IUserService UserService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                if (Umodel.BindStatus) throw new CustomException.CustomException("账号已绑定" + Umodel.BindUserName);

                string b_username = form["UserName"].ToString();  //用户
                string b_pwd = form["Password"].ToString();  //登陆密码
                string b_pwdMD5 = b_pwd.ToMD5().ToMD5();  //加密后

                if (cacheSysParam.SingleAndInit(x => x.ID == 3509).Value.ToInt() == 1)  //开启注册手机短信验证
                {
                    string smscode = form["mobilecode"];
                    if (string.IsNullOrEmpty(smscode)) throw new JN.Services.CustomException.CustomException("手机验证码不能为空");
                    if (HttpContext.Current.Session["SMSCancelBindUserValidateCode"] == null || smscode.Trim().ToLower() != HttpContext.Current.Session["SMSCancelBindUserValidateCode"].ToString().ToLower()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                    if (HttpContext.Current.Session["GetSMSCancelBindUserUser"] == null || Umodel.Mobile != HttpContext.Current.Session["GetSMSCancelBindUserUser"].ToString()) throw new JN.Services.CustomException.CustomException("手机号码与接收验证码的设备不相符");
                }

                DataTable userTable = SqlDataHelper.GetDataTable("select * from [User] where username=@username and Password=@Password", new SqlParameter[] { new SqlParameter("@username", b_username), new SqlParameter("@Password", b_pwdMD5) });

                //检测安全性
                if (userTable.Rows.Count <= 0)
                {
                    throw new CustomException.CustomException("账号或密码有误,无法绑定");
                }

                //检测B系统
                bool BindStatus = userTable.Rows[0]["BindStatus"].ToBool();//是否绑定
                if (BindStatus) throw new CustomException.CustomException("账号已被绑定,无法继续");

                int b_userId = userTable.Rows[0]["ID"].ToInt();//绑定用户id

                //系统内检测
                string bing_username = userTable.Rows[0]["UserName"].ToString();
                //string bing_binduserpath = userTable.Rows[0]["BindUserPath"] == null ? "": userTable.Rows[0]["BindUserPath"].ToString();//绑定路径
                if (UserService.List(x => x.BindUserName == bing_username).ToList().Any()) throw new CustomException.CustomException("账号已绑定其他账号,无法继续");

                var user = UserService.SingleAndInit(Umodel.ID);
                user.BindUserId = b_userId;
                user.BindStatus = true;
                user.BindUserName = b_username;
                //user.BindUserPath = user.BindUserPath + "," + b_userId;
                UserService.Update(user);
                SysDBTool.Commit();
                //成功返回1
                int isSuceess = SqlDataHelper.ExecuteCommand("update [User] set BindUserId = @BindUserId,BindUserName = @BindUserName,BindStatus=1 where UserName = @UserName", new SqlParameter[] { new SqlParameter("@BindUserId", user.ID), new SqlParameter("@BindUserName", user.UserName), new SqlParameter("@UserName", bing_username) });

                result.Status = 200;
                if (isSuceess > 0)
                {
                    result.Message = "绑定成功!";
                }
                else
                {
                    result.Message = "绑定过程中出现问题,请联系管理员!";
                }
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        #endregion

        #region 解除用户绑定
        /// <summary>
        /// 解除用户绑定
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="UserService">用户表接口</param>
        /// <param name="entity">用户模型</param>
        /// <param name="RegUser">注册的用户</param>
        /// <returns></returns>
        public static Result BingUserCancelMethod(FormCollection form, JN.Data.User Umodel, IUserService UserService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                if (cacheSysParam.SingleAndInit(x => x.ID == 3510).Value.ToInt() == 1)  //开启注册手机短信验证
                {
                    string smscode = form["mobilecode"];
                    if (string.IsNullOrEmpty(smscode)) throw new JN.Services.CustomException.CustomException("手机验证码不能为空");
                    if (HttpContext.Current.Session["SMSBindUserValidateCode"] == null || smscode.Trim().ToLower() != HttpContext.Current.Session["SMSBindUserValidateCode"].ToString().ToLower()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                    if (HttpContext.Current.Session["GetSMSBindUserUser"] == null || Umodel.Mobile != HttpContext.Current.Session["GetSMSBindUserUser"].ToString()) throw new JN.Services.CustomException.CustomException("手机号码与接收验证码的设备不相符");
                }


                if (!Umodel.BindStatus) throw new Exception("账号未绑定,无法解绑");

                string Password2 = form["password2"].ToString();  //本系统交易密码
                if (Password2.ToMD5().ToMD5() != Umodel.Password2) throw new Exception("交易密码不正确,无法继续!");

                DataTable userTable = SqlDataHelper.GetDataTable("select * from [User] where ID=@BindUserId and BindUserId=@ID", new SqlParameter[] { new SqlParameter("@BindUserId", Umodel.BindUserId), new SqlParameter("@ID", Umodel.ID) });

                //检测安全性
                if (userTable.Rows.Count <= 0)
                {
                    throw new Exception("账号绑定存在异常,无法解绑");
                }

                //检测B系统
                bool BindStatus = userTable.Rows[0]["BindStatus"].ToBool();//是否绑定
                if (!BindStatus) throw new Exception("对方账号已解绑,无法继续");


                //if (cacheSysParam.SingleAndInit(x => x.ID == 3805).Value == "1")
                //{
                //    if (MvcCore.Extensions.CacheExtensions.CheckCache(Umodel.UserName + "checkrepeat"))  //检查
                //    {
                //        throw new CustomException("系统正在处理相关数据,请等待60秒后再进行操作!");
                //    }
                //    MvcCore.Extensions.CacheExtensions.SetCache(Umodel.UserName + "checkrepeat", "0", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);  //创建
                //}

                var user = UserService.SingleAndInit(Umodel.ID);
                user.BindUserId = 0;
                user.BindStatus = false;

                string bing_username = user.BindUserName;

                user.BindUserName = "";
                UserService.Update(user);
                SysDBTool.Commit();

                int isSuceess = SqlDataHelper.ExecuteCommand("update [User] set BindUserId = 0,BindUserName = '',BindStatus=0  where UserName = @UserName", new SqlParameter[] { new SqlParameter("@UserName", bing_username) });

                result.Status = 200;
                if (isSuceess > 0)
                {
                    result.Message = "解绑成功!";
                }
                else
                {
                    result.Message = "解绑过程中出现问题,请联系管理员!";
                }

                //if (cacheSysParam.SingleAndInit(x => x.ID == 3805).Value == "1" && MvcCore.Extensions.CacheExtensions.CheckCache(Umodel.UserName + "checkrepeat"))
                //{
                //    if (MvcCore.Extensions.CacheExtensions.CheckCache(Umodel.UserName + "checkrepeat"))  //检查
                //    {
                //        MvcCore.Extensions.CacheExtensions.ClearCache(Umodel.UserName + "checkrepeat");  //清空
                //    }
                //}
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        #endregion

        #region 修改登录密码
        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">当前用户</param>
        /// <param name="UserService">用户表接口</param>
        /// <param name="SysDBTool">工具表接口</param>
        /// <returns></returns>
        public static Result ResetPwdMethod(FormCollection fc, JN.Data.User Umodel, IUserService UserService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                string password = fc["password"];
                string newpassword = fc["newpassword"];
                string confirmnewpassword = fc["confirmnewpassword"];

                var onUser = UserService.Single(Umodel.ID);

                if (onUser.IsLock) throw new CustomException.CustomException("您的帐号受限，无法进行相关操作");
                //手机验证
                string smscode = fc["mobilecode"];
                if (string.IsNullOrEmpty(smscode)) throw new CustomException.CustomException("手机验证码不能为空");
                if (HttpContext.Current.Session["LoginPwdSMSCode"] == null || smscode.Trim() != HttpContext.Current.Session["LoginPwdSMSCode"].ToString()) throw new CustomException.CustomException("验证码不正确或已过期");
                if (HttpContext.Current.Session["GetLoginPwdSMSUser"] == null || Umodel.Mobile != HttpContext.Current.Session["GetLoginPwdSMSUser"].ToString()) throw new CustomException.CustomException("手机号码与接收验证码的设备不相符");

                if (string.IsNullOrEmpty(newpassword.Trim()) || string.IsNullOrEmpty(confirmnewpassword.Trim())) throw new CustomException.CustomException("新登录密码、确认新密码不能为空");

                //必须10位数，字母加数字加符号组合
                //if (!Regex.IsMatch(newpassword.Trim(), @"^(?![a-zA-Z0-9]+$)(?![^a-zA-Z/D]+$)(?![^0-9/D]+$).{10,30}$")) throw new JN.Services.CustomException.CustomException("登录密码必须为10位数以上，字母加数字加符号组合");

                if (newpassword.Trim() != confirmnewpassword.Trim()) throw new CustomException.CustomException("两次输入的密码不一致");
                onUser.Password = newpassword.ToMD5().ToMD5();
                UserService.Update(onUser);
                SysDBTool.Commit();

                //重写cookie
                string CookieName_User = (ConfigHelper.GetConfigString("DBName") + "_User").ToMD5();
                string CookieName_Tonen = (ConfigHelper.GetConfigString("DBName") + "_Tonen").ToMD5();

                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_User, onUser.UserName, 60);
                MvcCore.Extensions.CookieExtensions.WriteCookie(CookieName_Tonen, onUser.UserName.ToMD5() + "" + onUser.Password, 60);

                result.Status = 200;
                result.Message = "操作成功,请重新登录！";
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        #endregion

        #region 设置交易密码
        /// <summary>
        /// 设置交易密码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">当前用户</param>
        /// <param name="UserService">用户表接口</param>
        /// <param name="SysDBTool">工具表接口</param>
        /// <returns></returns>
        public static Result SetPwd2Method(FormCollection fc, JN.Data.User Umodel, IUserService UserService, ISysDBTool SysDBTool)
        {

            Result result = new Result();
            try
            {
                string password = fc["password"];
                string password2 = fc["password2"];
                string newpassword2 = fc["newpassword2"];
                if (!string.IsNullOrEmpty(Umodel.Password2))
                {
                    throw new CustomException.CustomException("您已设置过交易密码，如需要修改，请到修改页面");
                }
                if (Umodel.IsLock) throw new CustomException.CustomException("您的帐号受限，无法进行相关操作");
                if (password.ToMD5().ToMD5() != Umodel.Password) throw new CustomException.CustomException("登录密码错误");
                if (string.IsNullOrEmpty(password2.Trim())) throw new CustomException.CustomException("交易密码不能为空");
                if (password2.Trim() != newpassword2.Trim()) throw new CustomException.CustomException("确认密码不一致");

                //必须10位数，字母加数字加符号组合
                //if (!Regex.IsMatch(password2.Trim(), @"^(?![a-zA-Z0-9]+$)(?![^a-zA-Z/D]+$)(?![^0-9/D]+$).{10,30}$")) throw new JN.Services.CustomException.CustomException("交易密码必须为10位数以上，字母加数字加符号组合");

                Umodel.Password2 = newpassword2.ToMD5().ToMD5();
                UserService.Update(Umodel);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        #endregion

        #region 重置交易密码
        /// <summary>
        /// 重置交易密码
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">当前用户</param>
        /// <param name="UserService">用户表接口</param>
        /// <param name="SysDBTool">工具表接口</param>
        /// <returns></returns>
        public static Result ResetPwd2Method(FormCollection fc, JN.Data.User Umodel, IUserService UserService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                string password2 = fc["password2"];
                string newpassword2 = fc["newpassword2"];
                string confirmnewpassword2 = fc["confirmnewpassword2"];

                if (Umodel.IsLock) throw new CustomException.CustomException("您的帐号受限，无法进行相关操作");

                //手机验证
                string smscode = fc["mobilecode"];
                if (string.IsNullOrEmpty(smscode)) throw new CustomException.CustomException("手机验证码不能为空");
                if (HttpContext.Current.Session["SendMobileResetTranSMSCode"] == null || smscode.Trim() != HttpContext.Current.Session["SendMobileResetTranSMSCode"].ToString()) throw new CustomException.CustomException("验证码不正确或已过期");
                if (HttpContext.Current.Session["GetSendMobileResetTranSMSUser"] == null || Umodel.Mobile != HttpContext.Current.Session["GetSendMobileResetTranSMSUser"].ToString()) throw new CustomException.CustomException("手机号码与接收验证码的设备不相符");

                if (string.IsNullOrEmpty(newpassword2.Trim()) || string.IsNullOrEmpty(confirmnewpassword2.Trim())) throw new CustomException.CustomException("新交易密码、确认新密码不能为空");

                if (newpassword2.Trim() != confirmnewpassword2.Trim()) throw new CustomException.CustomException("两次输入的密码不一致");

                //必须10位数，字母加数字加符号组合
                //if (!Regex.IsMatch(newpassword2.Trim(), @"^(?![a-zA-Z0-9]+$)(?![^a-zA-Z/D]+$)(?![^0-9/D]+$).{10,30}$")) throw new JN.Services.CustomException.CustomException("交易密码必须为10位数以上，字母加数字加符号组合");

                Umodel.Password2 = newpassword2.ToMD5().ToMD5();
                UserService.Update(Umodel);
                SysDBTool.Commit();

                result.Status = 200;
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "The network system is busy, please try again later!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        #endregion

        #region 找回登录密码
        /// <summary>
        /// 找回登录密码 第一步
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="UserService">会员接口</param>
        /// <param name="SysDBTool">工具接口</param>
        /// <param name="isApp">是否app，决定返回路径</param>
        /// <returns>返回状态信息</returns>
        public static Result DoGetPwd(FormCollection fc, IUserService UserService, ISysDBTool SysDBTool, bool isApp)
        {
            Result result = new Result();
            try
            {
                string mobile = fc["mobile"];
                string smscode = fc["smscode"];
                //string code = fc["code"];
                if (string.IsNullOrEmpty(mobile)) throw new CustomException.CustomException("取回手机号码不能为空！");

                mobile = mobile.Trim();

                var onUser = UserService.Single(x => x.Mobile == mobile);
                if (onUser == null) throw new CustomException.CustomException("手机号码不存在！");

                string vcode = (HttpContext.Current.Session["UserValidateCode"] ?? "").ToString();

                //if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                //throw new CustomException.CustomException("图形验证码错误，请重新输入！");

                if (cacheSysParam.SingleAndInit(x => x.ID == 3504).Value.ToInt() == 1)
                {
                    //if (!(Umodel.IsMobile ?? false)) throw new JN.Services.CustomException.CustomException("您还没有验证手机！");
                    if (string.IsNullOrEmpty(smscode)) throw new JN.Services.CustomException.CustomException("手机验证码不能为空");
                    if (HttpContext.Current.Session["GetPwdSMSCode"] == null || smscode.Trim().ToLower() != HttpContext.Current.Session["GetPwdSMSCode"].ToString().ToLower()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                    if (HttpContext.Current.Session["GetPwdSMSMobile"] == null || onUser.Mobile != HttpContext.Current.Session["GetPwdSMSMobile"].ToString()) throw new JN.Services.CustomException.CustomException("手机号码与接收验证码的设备不相符");
                }


                //if (string.IsNullOrEmpty(onUser.Mobile))
                //{
                //    throw new CustomException.CustomException("用户未绑定手机号码，无法取回密码，请联系管理员重置密码。");
                //}
                //else
                //{
                if ((onUser.ReserveInt2 ?? 0) >= 3 && (onUser.ReserveDate2 ?? DateTime.Now).Date == DateTime.Now.Date)
                {
                    throw new CustomException.CustomException("每个账号每天最多可以重置3次密码！");
                }
                else
                {
                    HttpContext.Current.Session["GetPwdUser"] = onUser.UserName;
                    if ((onUser.ReserveDate2 ?? DateTime.Now).Date != DateTime.Now.Date)
                        onUser.ReserveInt2 = 1;
                    else
                        onUser.ReserveInt2 = (onUser.ReserveInt2 ?? 0) + 1;
                    onUser.ReserveDate2 = DateTime.Now;
                    UserService.Update(onUser);
                    SysDBTool.Commit();

                    result.Status = 200;
                    if (isApp)
                    {
                        result.Message = "/app/Login/GetPwd2";
                    }
                    else
                    {
                        result.Message = "/UserCenter/Login/GetPwd2";
                    }
                }

                //}
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        /// <summary>
        /// 找回登录密码 第一步
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="UserService">会员接口</param>
        /// <param name="SysDBTool">工具接口</param>
        /// <param name="isApp">是否app，决定返回路径</param>
        /// <returns>返回状态信息</returns>
        public static Result DoGetPwd2(FormCollection fc, IUserService UserService, ISysDBTool SysDBTool, bool isApp)
        {
            Result result = new Result();
            try
            {
                string password = fc["password"];
                string passwordcim = fc["passwordcim"];
                //string password2 = fc["password2"];
                //string password2cim = fc["password2cim"];

                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordcim))// || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password2cim)
                {
                    throw new CustomException.CustomException("以上必填");
                }
                password = password.Trim();
                passwordcim = passwordcim.Trim();
                //password2 = password2.Trim();
                //password2cim = password2cim.Trim();

                if (password != passwordcim) throw new CustomException.CustomException("确认密码不一致");// || password2 != password2cim

                string username = (HttpContext.Current.Session["GetPwdUser"] ?? "").ToString();
                if (string.IsNullOrWhiteSpace(username)) throw new CustomException.CustomException("用户信息已丢失，请重新找回密码");

                var chUser = UserService.Single(x => x.UserName == username);
                if (chUser == null) throw new CustomException.CustomException("用户信息已丢失，请重新找回密码");

                chUser.Password = password.ToMD5().ToMD5();
                //chUser.Password2 = password2.ToMD5().ToMD5();
                //chUser.Password3 = password3.ToMD5().ToMD5();
                UserService.Update(chUser);

                var wlog = new Data.WarningLog();
                wlog.CreateTime = DateTime.Now;
                wlog.IP = HttpContext.Current.Request.UserHostAddress;
                if (HttpContext.Current.Request.UrlReferrer != null)
                    wlog.Location = HttpContext.Current.Request.UrlReferrer.ToString();
                wlog.Platform = "会员";
                wlog.WarningMsg = "通过忘记密码修改密码，验证成功";
                wlog.WarningLevel = "一般";
                wlog.ResultMsg = "通过";
                wlog.UserName = username;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog);
                SysDBTool.Commit();

                HttpContext.Current.Session["GetPwdUser"] = null;

                result.Status = 200;
                if (isApp)
                {
                    result.Message = "/app/Login/index";
                }
                else
                {
                    result.Message = "/UserCenter/Login/index";
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 银行卡添加修改管理
        /// <summary>
        /// 银行卡添加修改管理
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">登录模型</param>
        /// <param name="UserBankCardService">银行卡表接口</param>
        /// <param name="SysDBTool">工具类接口</param>
        /// <returns></returns>
        public static Result DoBankSettings(FormCollection fc, Data.User Umodel, IUserBankCardService UserBankCardService, ISysDBTool SysDBTool, string bankimg)
        {
            Result result = new Result();
            try
            {
                int BankNameId = fc["BankNameId"].ToInt();
                string BankAddress = fc["BankAddress"];
                string BankOfDeposit = fc["BankOfDeposit"];
                string BankUser = fc["BankUser"];
                string BankCard = fc["BankCard"];
                string BankCard2 = fc["BankCard2"];
                string smscode = fc["smscode"];
                string password2 = fc["password2"];
                int id = fc["bankid"].ToInt();


                bool IsDefault = fc["IsDefault"].ToBool();

                //查找当前记录（有？代表修改）
                var bankmodel = UserBankCardService.Single(x => x.ID == id && x.UID == Umodel.ID);


                bankimg = bankimg.Replace("|", "");


                #region 验证项
                //if (string.IsNullOrEmpty(password2)) throw new JN.Services.CustomException.CustomException("请您填写交易密码");
                //验证交易密码
                //if (Umodel.Password2 != password2.ToMD5().ToMD5()) throw new JN.Services.CustomException.CustomException("交易密码不正确");

                //验证其他选项
                //if (string.IsNullOrEmpty(Umodel.RealName)) throw new JN.Services.CustomException.CustomException("您还没有实名验证");

                //验证手机
                //if (cacheSysParam.SingleAndInit(x => x.ID == 3507).Value.ToInt() == 1)
                //{
                //    if (!(Umodel.IsMobile ?? false)) throw new JN.Services.CustomException.CustomException("您还没有验证手机！");
                //    if (string.IsNullOrEmpty(smscode)) throw new JN.Services.CustomException.CustomException("手机验证码不能为空");
                //    if (HttpContext.Current.Session["SMSbankCode"] == null || smscode.Trim().ToLower() != HttpContext.Current.Session["SMSbankCode"].ToString().ToLower()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                //    if (HttpContext.Current.Session["GetbankSMSUser"] == null || Umodel.Mobile != HttpContext.Current.Session["GetbankSMSUser"].ToString()) throw new JN.Services.CustomException.CustomException("手机号码与接收验证码的设备不相符");
                //}

                var paramBank = cacheSysParam.Single(x => x.ID == BankNameId);

                //获取银行名称
                if (BankNameId <= 0 || paramBank == null) throw new JN.Services.CustomException.CustomException("请您选择银行");

                if (paramBank.ID != 5001 && paramBank.ID != 5002)
                    if (string.IsNullOrEmpty(BankOfDeposit)) throw new JN.Services.CustomException.CustomException("请您填写开户行");
                if (paramBank.ID == 5001 || paramBank.ID == 5002)
                    BankOfDeposit = paramBank.Value;

                if (string.IsNullOrEmpty(BankCard) || string.IsNullOrEmpty(BankCard2)) throw new JN.Services.CustomException.CustomException("请您填写账号");
                if (BankCard != BankCard2) throw new JN.Services.CustomException.CustomException("两次账号填写不一致！");
                if (string.IsNullOrEmpty(BankUser)) throw new JN.Services.CustomException.CustomException("请您填写持卡人姓名");

               
                #endregion

                if (string.IsNullOrEmpty(Umodel.RealName))
                {
                    Umodel.RealName = BankUser;
                    MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(Umodel);
                }

                if (bankmodel != null)
                {
                    bankmodel.BankAddress = BankAddress;
                    bankmodel.BankOfDeposit = BankOfDeposit;
                    bankmodel.BankCard = BankCard;
                    bankmodel.BankNameID = paramBank.ID;//银行id
                    bankmodel.BankName = paramBank.Value;//银行名称
                    bankmodel.BankIcon = paramBank.Value2;//银行图标
                    bankmodel.BankUser = Umodel.RealName;
                    bankmodel.IsDefault = IsDefault;

                    if (!string.IsNullOrWhiteSpace(bankimg))
                    {
                        bankmodel.BankImgUrl = bankimg;
                    }

                    UserBankCardService.Update(bankmodel);//直接更新
                }
                else
                {

                    //查看有多少已经有多少个，只能添加10条
                    int count = UserBankCardService.List(x => x.UID == Umodel.ID) == null ? 0 : UserBankCardService.List(x => x.UID == Umodel.ID).Count();
                    if ((count + 1) > 10)
                    {
                        throw new JN.Services.CustomException.CustomException("您已经添加超过10个银行卡，不能再进行添加！");
                    }
                    if ((paramBank.ID == 5001 || paramBank.ID == 5002) && string.IsNullOrWhiteSpace(bankimg)) throw new JN.Services.CustomException.CustomException("请您上传收款二维码");

                    UserBankCardService.Add(new JN.Data.UserBankCard()
                    {
                        UID = Umodel.ID,
                        UserName = Umodel.UserName,
                        CreateTime = DateTime.Now,
                        BankUser = Umodel.RealName,
                        BankOfDeposit = BankOfDeposit,
                        BankNameID = paramBank.ID,//银行id
                        BankName = paramBank.Value,//银行名称
                        BankIcon = paramBank.Value2,//银行图标
                        IsDefault = IsDefault,//不是默认银行卡
                        BankCard = BankCard,
                        BankAddress = BankAddress,
                        BankImgUrl = bankimg,
                    });
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }

            return result;
        }

        /// <summary>
        /// 设置默认银行卡
        /// </summary>
        /// <param name="id">id值</param>
        /// <param name="Umodel">登录模型</param>
        /// <param name="UserBankCardService">银行卡表接口</param>
        /// <param name="SysDBTool">工具类接口</param>
        /// <returns></returns>
        public static Result DoSetDefaultBankCard(int id, Data.User Umodel, IUserBankCardService UserBankCardService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                var model = UserBankCardService.Single(x => x.ID == id && x.UID == Umodel.ID);
                if (model != null)
                {
                    model.IsDefault = true;//设置为默认
                    UserBankCardService.Update(model);

                    //更新除该条记录其他状态非默认
                    UserBankCardService.Update(new JN.Data.UserBankCard(), new Dictionary<string, string>() { { "IsDefault", "0" } }, "UID=" + Umodel.ID + " AND  ID != " + id);
                    SysDBTool.Commit();
                    result.Status = 200;
                    result.Message = "设置成功！";
                }
                else
                {
                    throw new CustomException.CustomException("银行卡不存在或已删除");
                }
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }


        /// <summary>
        /// 解绑银行卡
        /// </summary>
        /// <param name="id">id值</param>
        /// <param name="Umodel">登录模型</param>
        /// <param name="UserBankCardService">银行卡表接口</param>
        /// <param name="SysDBTool">工具类接口</param>
        /// <returns></returns>
        public static Result DeleteBank(int id, Data.User Umodel, IUserBankCardService UserBankCardService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                var model = UserBankCardService.Single(id);
                if (model == null) throw new JN.Services.CustomException.CustomException("该银行卡已解绑");
                if (model.UID != Umodel.ID) throw new JN.Services.CustomException.CustomException("您没有该权限");

                string msg = "会员解绑银行卡，验证成功，解绑卡号：" + model.BankCard;

                UserBankCardService.Delete(id);

                var wlog2 = new Data.WarningLog();
                wlog2.CreateTime = DateTime.Now;
                wlog2.IP = HttpContext.Current.Request.UserHostAddress;
                if (HttpContext.Current.Request.UrlReferrer != null)
                    wlog2.Location = HttpContext.Current.Request.UrlReferrer.ToString();
                wlog2.Platform = "会员";
                wlog2.WarningMsg = msg;
                wlog2.WarningLevel = "一般";
                wlog2.ResultMsg = "通过";
                wlog2.UserName = Umodel.UserName;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                SysDBTool.Commit();

                result.Status = 200;
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 发送短信验证码(通用)
        /// <summary>
        /// 发送短信验证码(通用，需手机验证)
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="Umodel">会员实体模型</param>
        /// <param name="msmFormat">验证码格式</param>
        /// <param name="isModify">是否修改手机</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendMobileMsm(string mobile, JN.Data.User Umodel, string msmFormat, bool isModify = false)
        {
            Result result = new Result();
            try
            {
                string smsTime = msmFormat + "SMSTime";//验证时间
                string smsMobile = msmFormat + "SMSMobile";//验证手机
                string smsCode = msmFormat + "SMSCode";//验证码

                if (string.IsNullOrEmpty(mobile)) throw new CustomException.CustomException("请您填写手机号码");
                if (!StringHelp.IsPhone(mobile)) throw new CustomException.CustomException("请输入正确的手机号码");

                if (isModify)//修改手机
                {
                    //手机唯一
                    if (MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.Mobile == mobile && (x.IsMobile ?? true)) != null) { throw new JN.Services.CustomException.CustomException("手机已经重复"); }
                }
                else
                {
                    if (Umodel.Mobile != mobile) throw new CustomException.CustomException("手机号码与会员资料不符");
                }

                if (HttpContext.Current.Session[smsTime] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session[smsTime].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }
                //System.ValidateCode vEmailCode = new System.ValidateCode();
                //string smscode = vEmailCode.CreateRandomCode(4, "1,2,3,4,5,6,7,8,9,0");

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session[smsCode] = smscode;
                HttpContext.Current.Session[smsMobile] = mobile;
                HttpContext.Current.Session[smsTime] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(mobile, cacheSysParam.Single(x => x.ID == 3519).Value.Replace("#code#", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session[smsCode] = null;
                    HttpContext.Current.Session[smsMobile] = null;
                    result.Message = "短信发送失败,详情请查阅发送记录";
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        /// <summary>
        /// 发送短信验证码(通用，无需手机验证)
        /// </summary>
        /// <param name="Umodel">会员实体模型</param>
        /// <param name="msmFormat">验证码格式</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendMobileMsm(JN.Data.User Umodel, string msmFormat)
        {
            Result result = new Result();
            try
            {
                string smsTime = msmFormat + "SMSTime";//验证时间
                string smsMobile = msmFormat + "SMSMobile";//验证手机
                string smsCode = msmFormat + "SMSCode";//验证码

                string mobile = Umodel.Mobile;//验证码
                                              //if (!(Umodel.IsMobile ?? false)) throw new JN.Services.CustomException.CustomException("您还没有手机认证，请您手机验证后再操作");
                if (string.IsNullOrEmpty(mobile)) throw new CustomException.CustomException("请您在个人资料中设置手机号码");
                if (!StringHelp.IsPhone(mobile)) throw new CustomException.CustomException("您资料中设置的手机号码格式不正确");

                if (HttpContext.Current.Session[smsTime] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session[smsTime].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }
                //System.ValidateCode vEmailCode = new System.ValidateCode();
                //string smscode = vEmailCode.CreateRandomCode(4, "1,2,3,4,5,6,7,8,9,0");

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session[smsCode] = smscode;
                HttpContext.Current.Session[smsMobile] = mobile;
                HttpContext.Current.Session[smsTime] = DateTime.Now;

                bool b = SMSHelper.WebChineseMSM(mobile, cacheSysParam.Single(x => x.ID == 3519).Value.Replace("#code#", smscode));
                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session[smsCode] = null;
                    HttpContext.Current.Session[smsMobile] = null;
                    result.Message = "短信发送失败,详情请查阅发送记录";
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }

        /// <summary>
        /// 发送注册/忘记密码短信验证码(通用)
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="msmFormat">验证码格式</param>
        /// <param name="isGetPw">是否找回密码</param>
        /// <returns>返回状态码和信息</returns>
        public static Result SendRegOrPWMobileMsm(string mobile, string msmFormat, bool isGetPw = false)
        {
            Result result = new Result();
            try
            {
                string smsTime = msmFormat + "SMSTime";//验证时间
                string smsMobile = msmFormat + "SMSMobile";//验证手机
                string smsCode = msmFormat + "SMSCode";//验证码

                var countrycode = "+86";

                if (string.IsNullOrEmpty(mobile)) throw new CustomException.CustomException("请您填写手机号码");
                //if (!StringHelp.IsPhone(mobile)) throw new CustomException.CustomException("请输入正确的手机号码");

                if (HttpContext.Current.Session[smsTime] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(HttpContext.Current.Session[smsTime].ToString())))
                        throw new CustomException.CustomException("每次获取验证码间隔不能少于1分钟");
                }

                if (isGetPw)
                {
                    //string vcode = (HttpContext.Current.Session["UserValidateCode"] ?? "").ToString();
                    //string code = (HttpContext.Current.Request["code"] ?? "").ToString();
                    //if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                    //    throw new CustomException.CustomException("图形验证码错误，请重新输入！");

                    //判断账号是否存在
                    var user = MvcCore.Unity.Get<IUserService>().Single(x => x.Mobile == mobile);
                    if (user == null) throw new CustomException.CustomException("对不起，手机号不存在");
                    countrycode = user.CountryCode;
                }
                else
                {
                    var sysEntity = MvcCore.Unity.Get<ISysSettingService>().ListCache("sysSet").FirstOrDefault();
                    if (MvcCore.Unity.Get<IUserService>().List(x => x.Mobile == mobile.Trim()).Count() > 0 && !string.IsNullOrEmpty(mobile) && sysEntity.RegistOnlyOneItems.Contains(",Mobile,")) throw new JN.Services.CustomException.CustomException("手机号码已被使用");
                }

                #region 过于频繁获取手机验证码限制
                var param = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).SingleAndInit(x => x.ID == 3520);
                var paraValue = param.Value.ToInt(); //时间内
                var SMSLogEntity = MvcCore.Unity.Get<ISMSLogService>().List(x => x.Mobile == mobile && x.SMSContent.Contains("验证码") && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) <= paraValue).OrderByDescending(x => x.ID).ToList();

                if (SMSLogEntity.Count() >= param.Value2.ToInt())  //同个手机号 获取短信 限制
                {
                    throw new CustomException.CustomException("获取手机验证码过于频繁！");
                }

                SMSLogEntity = MvcCore.Unity.Get<ISMSLogService>().List(x => x.Mobile == mobile && x.SMSContent.Contains("验证码") && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) <= 30).OrderByDescending(x => x.ID).ToList();
                if (SMSLogEntity.Count() >= 10)  //同个手机号30分钟内限制10条 获取短信 限制
                {
                    throw new CustomException.CustomException("获取手机验证码过于频繁！");
                }

                string getIP = HttpContext.Current.Request.UserHostAddress;
                //var smsLogList = ;
                if (MvcCore.Unity.Get<ISMSLogService>().List(x => x.IP == getIP && x.SMSContent.Contains("验证码") && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) <= paraValue).OrderByDescending(x => x.ID).ToList().Count() >= param.Value2.ToInt())  //同个IP 获取短信 限制
                {
                    throw new CustomException.CustomException("获取手机验证码过于频繁！");
                }
                #endregion

                Random random = new Random();
                int r = random.Next(1000, 10000);//随机数
                string smscode = r.ToString();

                HttpContext.Current.Session[smsCode] = smscode;
                HttpContext.Current.Session[smsMobile] = mobile;
                HttpContext.Current.Session[smsTime] = DateTime.Now;

                //bool b = SMSHelper.WebChineseMSM(mobile, cacheSysParam.Single(x => x.ID == 3519).Value.Replace("#code#", smscode));

                var Param = cacheSysParam.Single(x => x.ID == 3510);
                string tempid = countrycode == "+86" ? Param.Value3 : Param.Value4;
                //string text = countrycode == "+86" ? Param.Value.Replace("#code#", smscode) : Param.Value2.Replace("#code#", smscode);
                string text = smscode;
                bool b = SMSHelper.CYmsm(countrycode + mobile, text, tempid);


                if (b)
                    result.Status = 200;
                else
                {
                    HttpContext.Current.Session[smsCode] = null;
                    HttpContext.Current.Session[smsMobile] = null;
                    result.Message = "短信发送失败,详情请查阅发送记录";
                    result.Status = 500;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }
            return result;
        }
        #endregion

        #region 生成会员显示ID
        public static int GetUserName()
        {
            var param3106 = MvcCore.Unity.Get<Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3106);
            //int maxUserName = MvcCore.Unity.Get<Data.Service.IUserService>().List().Count() > 0 ? (MvcCore.Unity.Get<Data.Service.IUserService>().List().Max(x => x.UserName.ToInt()).ToInt() + 1) : 1;
            var Userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List().OrderByDescending(x => x.CreateTime).Take(200).ToList();
            int maxUserName = Userlist.Count() > 0 ? (Userlist.Max(x => x.UserName.ToInt()) + 1) : 1;
            if (param3106.Value2 == "1")
            {
                maxUserName = maxUserName + param3106.Value.ToInt();
            }
            if (IsHaveUserName(maxUserName.ToString()))
            {
                return GetUserName();
            }
            return maxUserName;
        }
        //检查订单号是否重复
        private static bool IsHaveUserName(string number)
        {
            return MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.UserName == number).Count() > 0;
        }
        public static void ChangeParam3106()
        {
            var param3106 = MvcCore.Unity.Get<Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3106);
            param3106.Value2 = "0";
            MvcCore.Unity.Get<Data.Service.ISysParamService>().Update(param3106);
        }
        public static int GetDisplayID()
        {
            //DateTime dateTime = DateTime.Now;
            Random random = new Random();
            int r = random.Next(1, 100);//随机数
            int result = 0;
            int maxid = MvcCore.Unity.Get<Data.Service.IUserService>().List().Count() > 0 ? MvcCore.Unity.Get<Data.Service.IUserService>().List().Max(x => x.DisplayID).ToInt() : 0;
            if (maxid < 1000) maxid = 1000;
            result += (maxid + r);
            if (IsHaveDisplayID(result))
            {
                return GetDisplayID();
            }
            return result;
        }

        //检查订单号是否重复
        private static bool IsHaveDisplayID(int number)
        {
            return MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.DisplayID == number).Count() > 0;
        }
        #endregion


        #region 生成随机地址
        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            if (IsHave(s))
            {
                return GetRandomString(33, true, true, true, false, "1");
            }
            return s;
        }

        #region 检查订单号是否重复
        private static bool IsHaveUserAdd(string number)
        {
            var pro = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.R3001 == number);
            if (pro == null)
                return false;
            return true;
        }
        #endregion

        #endregion
    }
}