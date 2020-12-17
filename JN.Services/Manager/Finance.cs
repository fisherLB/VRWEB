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
using JN.Data;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace JN.Services.Manager
{
    ///<summary>
    /// time:2017年8月29日 18:24:16  name:lin    alt:转账提币，充值转换等业务逻辑统一归于本类
    ///</summary>
    public partial class Finance
    {
        private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        /// <summary>
        /// 清除缓存后重新加载
        /// </summary>
        public static void HeavyLoad()
        {
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        }

        #region 充值人民币
        /// <summary>
        /// 充值人民币
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">登录的实体</param>
        /// <param name="RemittanceService">充值表</param>
        /// <returns></returns>
        public static Result RMBRecharge(FormCollection fc, Data.User Umodel, JN.Data.Service.IRemittanceService RemittanceService)
        {
            Result result = new Result();
            try
            {
                string rechargeway = fc["rechargeway"];
                //string rechargedate = form["rechargedate"];
                string rechargeamount = fc["rechargeamount"];
                string remark = fc["remark"];

                if (rechargeamount.ToDecimal() <= 0) throw new JN.Services.CustomException.CustomException("请输入正确的金额");
                // if (!Services.Tool.StringHelp.IsDateString(rechargedate)) throw new CustomException("日期格式不正确");
                if (remark.Trim().Length > 100) throw new JN.Services.CustomException.CustomException("备注长度不能超过100个字节");

                var param1901 = cacheSysParam.Single(x => x.ID == 1901);

                decimal minMoney = param1901.Value.ToDecimal();
                if (rechargeamount.ToDecimal() < minMoney) throw new JN.Services.CustomException.CustomException("金额必须大于等于" + minMoney);

                decimal beisu = param1901.Value2.ToDecimal();
                if (rechargeamount.ToDecimal() % beisu != 0) throw new JN.Services.CustomException.CustomException("金额必须是" + beisu + "的倍数");

                //写入汇款表
                var model = new Data.Remittance
                {
                    CreateTime = DateTime.Now,
                    ChongNumber = Guid.NewGuid().ToString(),
                    PayOrderNumber = "",
                    Platform = 0,
                    RechargeAmount = rechargeamount.ToDecimal(),
                    ActualAmount = rechargeamount.ToDecimal(),
                    RechargeWay = rechargeway,
                    Remark = remark,
                    UID = Umodel.ID,
                    UserName = Umodel.UserName,
                    Status = (int)JN.Data.Enum.RechargeSatus.Wait,
                    RechargeDate = DateTime.Now //rechargedate.ToDateTime()
                };
                RemittanceService.Add(model);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
                result.Status = 200;
                result.Message = model.ID.ToString();
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

        #region 取消充值
        /// <summary>
        /// 取消充值
        /// </summary>
        /// <param name="id">充值ID号</param>
        /// <param name="RemittanceService">取消充值实体</param>
        /// <returns>返回状态码和信息</returns>
        public static Result CancelRemittance(int id, JN.Data.Service.IRemittanceService RemittanceService)
        {
            Result result = new Result();
            try
            {
                var model = RemittanceService.Single(id);
                if (model != null)
                {
                    if (model.Status == (int)JN.Data.Enum.RechargeSatus.Wait)
                    {
                        model.Status = (int)JN.Data.Enum.RechargeSatus.Fail;
                        RemittanceService.Update(model);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                        result.Status = 200;
                    }
                    else
                    {
                        throw new JN.Services.CustomException.CustomException("您的充值申请无法取消");
                    }
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

        #region 提币操作
        /// <summary>
        /// 提币操作
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">用户实体模型</param>
        /// <returns>返回状态码和信息</returns>
        public static Result WithdrawCash(FormCollection fc, Data.User Umodel, ICurrencyService CurrencyService)
        {

            Result result = new Result();
            try
            {
                int bankid = fc["bankid"].ToInt();
                decimal drawmoney = (fc["drawmoney"] ?? "0").ToDecimal();
                string remark = fc["remark"];
                string password2 = fc["password2"];
                string currencyAddress = fc["currencyAddress"];
                int curid = 3;// fc["curid"].ToInt();//币种

                if (string.IsNullOrEmpty(password2)) throw new CustomException.CustomException("请填写交易密码");
                if (Umodel.Password2 != password2.ToMD5().ToMD5()) throw new JN.Services.CustomException.CustomException("交易密码不正确");

                if (string.IsNullOrEmpty(currencyAddress)) throw new CustomException.CustomException("请填写提币地址");

                if (drawmoney <= 0) throw new JN.Services.CustomException.CustomException("请输入正确的提币金额");
                if (!string.IsNullOrWhiteSpace(remark) && remark.Trim().Length > 100) throw new JN.Services.CustomException.CustomException("备注长度不能超过100个字节");

                //if (MvcCore.Unity.Get<JN.Data.Service.IUserBankCardService>().List(x => x.UID == Umodel.ID && x.ID == bankid).Count() <= 0) throw new JN.Services.CustomException.CustomException("您没有选择银行卡，如没有添加银行卡，请到银行卡管理选项去添加！");

                var takeCoin = CurrencyService.Single(x => x.ID == curid);
                if (takeCoin == null) throw new JN.Services.CustomException.CustomException("错误的参数");

                //账户余额
                decimal balance = JN.Services.Manager.Users.WalletCur((int)takeCoin.WalletCurID, Umodel);

                if (balance < drawmoney) throw new JN.Services.CustomException.CustomException("余额不足，请重新填写");

                //单笔限制
                if ((takeCoin.TbMinLimit ?? 0) > 0 && (takeCoin.TbMinLimit ?? 0) > drawmoney) throw new CustomException.CustomException("低于单笔提币额：" + takeCoin.TbMinLimit);
                //单笔限制
                if ((takeCoin.TbMaxLimit ?? 0) > 0 && (takeCoin.TbMaxLimit ?? 0) < drawmoney) throw new CustomException.CustomException("超出单笔提币额：" + takeCoin.TbMaxLimit);

                //decimal beisu = cacheSysParam.Single(x => x.ID == 1905).Value.ToDecimal();
                //if (drawmoney.ToDecimal() % beisu != 0) throw new JN.Services.CustomException.CustomException("金额必须是" + beisu + "的倍数");

                Wallets.doTakeCash(Umodel, drawmoney.ToDecimal(), remark, bankid, takeCoin, currencyAddress);

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

        #region 取消提币
        /// <summary>
        /// 取消提币
        /// </summary>
        /// <param name="id">提币列表实体ID</param>
        /// <param name="TakeCashService">提币列表接口</param>
        /// <returns>返回状态码和信息</returns>
        public static Result CancelTakeCash(int id, JN.Data.Service.ITakeCashService TakeCashService)
        {
            Result result = new Result();
            try
            {
                var model = TakeCashService.Single(id);

                if (model.Status == (int)JN.Data.Enum.TakeCaseStatus.Wait)
                {
                    Wallets.changeWallet(model.UID, model.DrawMoney, (int)model.CurID, "取消提币");
                    model.Status = (int)JN.Data.Enum.TakeCaseStatus.Cancel;
                    TakeCashService.Update(model);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                    result.Status = 200;
                }
                else
                {
                    throw new JN.Services.CustomException.CustomException("成功取消提币申请");
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

        #region 用户转帐
        /// <summary>
        /// 用户转帐
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">登录的实体</param>
        /// <param name="RemittanceService">充值表</param>
        /// <returns></returns>
        public static Result UserTransfer(FormCollection form, Data.User Umodel, IUserService UserService, List<Data.SysParam> cacheSysParam)
        {
            Result result = new Result();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("UserTransfer" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("UserTransfer" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                string recusername = form["recuser"];
                string transfermoney = form["transfermoney"] ?? "0";
                string remark = form["remark"];
                string password2 = form["password2"];
                if (string.IsNullOrEmpty(form["coinid"]))
                {
                    throw new CustomException.CustomException("无效的参数");
                }
                int coinid = form["coinid"].ToInt();

                if (Umodel.IsLock) throw new CustomException.CustomException("您的帐号受限，无法进行相关操作");
                if (Umodel.Password2 != password2.ToMD5().ToMD5()) throw new CustomException.CustomException("交易密码不正确");
                if (transfermoney.ToDecimal() <= 0) throw new CustomException.CustomException("请输入正确的数量");

                var recUser = UserService.Single(x => x.UserName == recusername.Trim());
                if (recUser == null) throw new CustomException.CustomException("转入用户不存在");
                if (Umodel.UserName == recusername.Trim()) throw new CustomException.CustomException("不可以给自己转帐");

                decimal minMoney = cacheSysParam.Single(x => x.ID == 3102).Value.ToDecimal();
                //decimal maxMoney = cacheSysParam.Single(x => x.ID == 3102).Value2.ToDecimal();

                if (transfermoney.ToDecimal() < minMoney) throw new JN.Services.CustomException.CustomException("数值错误，金额不低于：" + minMoney);

                decimal beisu = cacheSysParam.Single(x => x.ID == 3102).Value2.ToDecimal();
                if (beisu != 0 && transfermoney.ToDecimal() % beisu != 0) throw new JN.Services.CustomException.CustomException("金额必须是" + beisu + "的倍数");


                decimal PARAM_POUNDAGEBL = 0;

                if (coinid == 1)
                    PARAM_POUNDAGEBL = cacheSysParam.Single(x => x.ID == 3102).Value3.ToDecimal(); //转帐矿工费

                decimal realtransfermoney = (transfermoney.ToDecimal() * (1 + PARAM_POUNDAGEBL));

                var cmodel = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().SingleAndInit(x => x.ID == coinid);
                if (cmodel == null || cmodel.ID < 0)
                {
                    throw new JN.Services.CustomException.CustomException("错误的参数");
                }

                decimal havemoney = Users.WalletCur(cmodel.WalletCurID ?? 0, Umodel);//钱包余额
                if (realtransfermoney > havemoney)
                {
                    throw new CustomException.CustomException("余额不足");
                }

                #region 事务操作
                //using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                //{
                Wallets.doTransfer(Umodel, recUser, transfermoney.ToDecimal(), cmodel.WalletCurID ?? 0, remark, cmodel);
                // ts.Complete();
                result.Status = 200;
                // }
                #endregion
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
            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("UserTransfer" + Umodel.ID);//清除缓存
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
        /// <returns></returns>
        public static Result BankManage(FormCollection fc, Data.User Umodel, JN.Data.Service.IUserBankCardService UserBankCardService)
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

                //查找当前记录（有？代表修改）
                var bankmodel = UserBankCardService.Single(x => x.ID == id && x.UID == Umodel.ID);

                #region 验证项
                if (string.IsNullOrEmpty(password2)) throw new JN.Services.CustomException.CustomException("请您填写交易密码");
                //验证二级密码
                if (Umodel.Password2 != password2.ToMD5().ToMD5()) throw new JN.Services.CustomException.CustomException("交易密码不正确");

                //验证其他选项
                if (string.IsNullOrEmpty(Umodel.RealName)) throw new JN.Services.CustomException.CustomException("您还没有实名验证");

                //验证手机
                if (cacheSysParam.SingleAndInit(x => x.ID == 3507).Value.ToInt() == 1)
                {
                    if (!(Umodel.IsMobile ?? false) && string.IsNullOrEmpty(Umodel.Mobile)) throw new JN.Services.CustomException.CustomException("您还没有验证手机！");
                    if (string.IsNullOrEmpty(smscode)) throw new JN.Services.CustomException.CustomException("手机验证码不能为空");
                    if (HttpContext.Current.Session["SMSbankCode"] == null || smscode.Trim().ToLower() != HttpContext.Current.Session["SMSbankCode"].ToString().ToLower()) throw new JN.Services.CustomException.CustomException("验证码不正确或已过期");
                    if (HttpContext.Current.Session["GetbankSMSUser"] == null || Umodel.Mobile != HttpContext.Current.Session["GetbankSMSUser"].ToString()) throw new JN.Services.CustomException.CustomException("手机号码与接收验证码的设备不相符");
                }

                //获取银行名称
                if (BankNameId <= 0) throw new JN.Services.CustomException.CustomException("请您选择银行");
                if (string.IsNullOrEmpty(BankOfDeposit)) throw new JN.Services.CustomException.CustomException("请您填写开户行");
                if (string.IsNullOrEmpty(BankCard) || string.IsNullOrEmpty(BankCard2)) throw new JN.Services.CustomException.CustomException("请您填写银行卡号");
                if (BankCard != BankCard2) throw new JN.Services.CustomException.CustomException("两次银行卡号填写不一致！");
                #endregion

                if (bankmodel != null)
                {
                    bankmodel.BankAddress = BankAddress;
                    bankmodel.BankOfDeposit = BankOfDeposit;
                    bankmodel.BankCard = BankCard;
                    bankmodel.BankName = "";
                    bankmodel.BankNameID = BankNameId;
                    bankmodel.BankUser = Umodel.RealName;
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

                    UserBankCardService.Add(new JN.Data.UserBankCard()
                    {
                        UID = Umodel.ID,
                        UserName = Umodel.UserName,
                        CreateTime = DateTime.Now,
                        BankUser = Umodel.RealName,
                        BankOfDeposit = BankOfDeposit,
                        BankName = "",
                        BankNameID = BankNameId,
                        BankCard = BankCard,
                        BankAddress = BankAddress,
                    });
                }
                MvcCore.Unity.Get<ISysDBTool>().Commit();
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

        #region 绑定用户转账
        /// <summary>
        /// 绑定用户转账
        /// </summary>
        /// <param name="fc">表单值</param>
        /// <param name="Umodel">登录的实体</param>
        /// <param name="RemittanceService">充值表</param>
        /// <returns></returns>
        public static Result BindTransferMethod(FormCollection form, JN.Data.User Umodel)
        {
            Result result = new Result();
            try
            {
                if (!Umodel.BindStatus) throw new CustomException.CustomException("账号未绑定");
                decimal money = form["money"].ToDecimal();  //金额
                string password2 = form["Password2"].ToString();  //登陆密码
                if (Umodel.Password2 != password2.ToMD5().ToMD5())
                {
                    throw new CustomException.CustomException("密码有误");
                }
                int curid = HttpContext.Current.Request["curid"].ToInt();
                //获取币种实体
                var cmodel = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(curid);
                if (cmodel == null) throw new CustomException.CustomException("操作失败，查无此币种");

                decimal walletBalance = JN.Services.Manager.Users.WalletCur(cmodel.WalletCurID ?? 0, Umodel);//钱包余额

                decimal poundage = money * cacheSysParam.SingleAndInit(x => x.ID == 3101).Value.ToDecimal();
                decimal actualMoney = money + poundage;  //实际金额

                if (actualMoney > walletBalance) throw new CustomException.CustomException(cmodel.CurrencyName + "余额不足");

                DataTable userTable = SqlDataHelper.GetDataTable("select * from [User] where UserName=@UserName and BindStatus=1 and BindUserName=@BindUserName", new SqlParameter[] { new SqlParameter("@BindUserName", Umodel.UserName), new SqlParameter("@BindUserId", Umodel.BindUserId), new SqlParameter("@UserName", Umodel.BindUserName) });

                //检测安全性
                if (userTable.Rows.Count <= 0)
                {
                    throw new CustomException.CustomException("绑定账号存在异常");
                }

                var newGuid = Guid.NewGuid().ToString("N");
                bool isCommit = false;
                decimal curprice = JN.Services.Manager.Stocks.getcurrentprice(cmodel);//当前价格
                //本系统扣除
                using (var ts = new System.Transactions.TransactionScope())
                {
                    StringBuilder sql = new StringBuilder();
                    System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                    sql.AppendFormat("insert into [OtherTransfer] (No,UID,UserName,ChangeMoney,CoinID,doType,Status,BindUserId,BindUserName,CreateTime,Price,Poundage,ActualMoney) values('{0}',{1},'{2}',{3},{4},{5},{6},{7},'{8}','{9}',{10},{11},{12})", newGuid, Umodel.ID, Umodel.UserName, money, 0, (int)JN.Data.Enum.OtherTransfersType.OUT, (int)JN.Data.Enum.OtherTransfersStatus.Sucess, Umodel.BindUserId, Umodel.BindUserName, DateTime.Now, curprice, poundage, actualMoney);

                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().ExecuteSQL(sql.ToString(), s);

                    Wallets.changeWallet(Umodel.ID, -actualMoney, cmodel.WalletCurID ?? 0, "[转出],价格:" + curprice + ",来自单号“" + newGuid + "”手续费：" + poundage, cmodel);

                    ts.Complete();
                    isCommit = true;
                }

                result.Message = "绑定过程中出现问题,请联系管理员!";

                //生成记录
                if (isCommit)
                {
                    //decimal actualprice = curprice * money;
                    string str = "来自[" + Umodel.UserName + "]" + newGuid;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("insert into [OtherTransfer] (No,UID,UserName,ChangeMoney,CoinID,doType,Status,BindUserId,BindUserName,CreateTime,Price) values('{0}',{1},'{2}',{3},{4},{5},{6},{7},'{8}','{9}',{10})", newGuid, Umodel.BindUserId, Umodel.BindUserName, money, 0, (int)JN.Data.Enum.OtherTransfersType.IN, (int)JN.Data.Enum.OtherTransfersStatus.Wait, Umodel.ID, Umodel.UserName, DateTime.Now, curprice);
                    int isSuceess = SqlDataHelper.ExecuteCommand(sb.ToString());

                    if (isSuceess > 0)
                    {
                        result.Message = "转账成功!";
                    }
                }

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

        #region 币种兑换
        public static Result ApplyExchange(FormCollection form, Data.User Umodel, ICurrencyService CurrencyService, IExchangeDetailService ExchangeDetailService, IExchangeCurrencyService ExchangeCurrencyService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("ApplyExchange" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("ApplyExchange" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

            int fromcoinid = (form["fromcoinid"] ?? "0").ToInt();
                int tocoinid = (form["tocoinid"] ?? "0").ToInt();
                int rulerid = (form["rulerid"] ?? "0").ToInt();
                string tradePassword = form["tradeinPassword"];
                decimal accout = (form["accout"] ?? "0").ToDecimal();

                if(rulerid<=0) throw new CustomException.CustomException("请选择兑换方式");
                var ExchangeModel = ExchangeCurrencyService.Single(rulerid);
                if(ExchangeModel == null) throw new CustomException.CustomException("兑换方式信息有误");
                if (!(ExchangeModel.IsUse??false)) throw new CustomException.CustomException("该兑换方式已关闭使用");
                if (accout <= 0) throw new CustomException.CustomException("交易数量不正确");
                if (string.IsNullOrEmpty(tradePassword) || Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException.CustomException("交易密码不正确");
                if (Umodel.IsLock) throw new CustomException.CustomException("您的帐号受限，无法进行相关操作");
                var FromCur = CurrencyService.Single(x => x.ID == ExchangeModel.FromCoinID);//兑换币种
                var ToCur = CurrencyService.Single(x => x.ID == ExchangeModel.ToCoinID);//目标币种

                if ((ExchangeModel.ISEchangeRate ?? false))
                {
                    //ExchangeModel.Rate = Stocks.GetEchangeRate(FromCur, ToCur);
                }

                if (accout > ExchangeModel.MaxNumber) throw new JN.Services.CustomException.CustomException("数值错误，金额已经超出上限：" + ExchangeModel.MaxNumber);

                if (ExchangeModel.MinNumber != 0 && accout % ExchangeModel.MinNumber != 0) throw new JN.Services.CustomException.CustomException("金额必须是" + ExchangeModel.MinNumber + "的倍数");

                decimal exchangeMoney = accout * ExchangeModel.Rate;

                decimal fromWalletBalance = JN.Services.Manager.Users.WalletCur((FromCur.WalletCurID??0), Umodel);
                if (fromWalletBalance < accout) throw new JN.Services.CustomException.CustomException("余额不足，请重新填写");

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {

                    //币种操作
                    Wallets.changeWalletNoCommit(Umodel.ID, 0 - accout, (int)FromCur.WalletCurID, "【" + FromCur.CurrencyName + "】兑换【" + ToCur.CurrencyName + "】扣除", FromCur);//余额减少

                    Wallets.changeWalletNoCommit(Umodel.ID, exchangeMoney, (int)ToCur.WalletCurID, "【" + FromCur.CurrencyName + "】兑换【" + ToCur.CurrencyName + "】获得", ToCur);//积分增加

                    //添加记录
                    var model = new Data.ExchangeDetail();
                    model.CreateTime = DateTime.Now;
                    model.UID = Umodel.ID;
                    model.UserName = Umodel.UserName;
                    model.FromCoinID = FromCur.ID;
                    model.FromCoinName = FromCur.CurrencyName;
                    model.ToCoinID = ToCur.ID;
                    model.ToCoinName = ToCur.CurrencyName;
                    model.Rate = ExchangeModel.Rate;
                    model.ApplyMoney = accout;
                    model.ExchangeMoney = exchangeMoney;
                    ExchangeDetailService.Add(model);

                    SysDBTool.Commit();


                    //Users.UpdateLevel(exchangeMoney,Umodel.ID);

                    //Bonus.Bonus1103(accout, Umodel.ID,ToCur ,FromCur);//直推奖

                    //Bonus.Bouns1104(accout, Umodel.ID, ToCur, FromCur);//见点奖只拿第一次

                    //Bonus.Bouns1105(accout, Umodel.ID, ToCur, FromCur);//团队复投奖

                    //Bonus.ExchangeUp(accout, Umodel, ToCur, FromCur);//兑换加速

                    //if ((int)ToCur.WalletCurID == 3002)
                    //{
                    //  Umodel.ReserveDecamal2 = (Umodel.ReserveDecamal2 ?? 0) + exchangeMoney;
                    //}
                    //Users.UpdateLevel(Umodel);//会员升级判断
                    ts.Complete();
                }
                //Bonus.AchievementUp(accout, Umodel.ID);//业绩提升
                //SysDBTool.Commit();

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
            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("ApplyExchange" + Umodel.ID);//清除缓存
            }
            return result;
        }
        #endregion

        #region 转入/转出
        public static Result TransferOutCheck(FormCollection form, Data.User Umodel)
        {
            Result result = new Result();
            try
            {
                string tousername = form["tousername"];
                if (Umodel.IsLock) throw new CustomException.CustomException("您的帐号受限，无法进行相关操作");

                var ToUser = MvcCore.Unity.Get<IUserService>().Single(x => x.UserName == tousername || x.Mobile == tousername);
                if (ToUser == null) throw new CustomException.CustomException("对方会员不存在");

                if (string.IsNullOrWhiteSpace(tousername)) throw new CustomException.CustomException("请输入对方会员编号");
                if (Umodel.UserName == tousername.Trim() || Umodel.Mobile == tousername.Trim()) throw new CustomException.CustomException("不能转给自己");

                result.Status = 200;
                result.Message = "/app/finance/TransferOutConfirm?username=" + tousername;
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), ex);
            }

            return result;
        }
        public static Result DoTransferOut(FormCollection form, Data.User Umodel, IUserService UserService, ICurrencyService CurrencyService, ITransferService TransferService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                int curid = (form["curid"] ?? "0").ToInt();//币种(余额账户)
                int touid = (form["touid"] ?? "0").ToInt();//对方会员id
                string tomobile = form["tomobile"];//对方会员手机号后面4位
                decimal money = (form["money"] ?? "0").ToDecimal();//金额
                string tradePassword = form["tradepassword"];
                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException.CustomException("交易密码不正确");
                var toUser = UserService.Single(touid);
                if (toUser == null) throw new CustomException.CustomException("对方会员不存在");
                if (Umodel.UserName == toUser.UserName) throw new CustomException.CustomException("不可以给自己转帐");
                if (string.IsNullOrWhiteSpace(tomobile)) throw new CustomException.CustomException("对方会员手机号手机末4位不能为空");
                
                if (tomobile != toUser.Mobile.Substring(toUser.Mobile.Length-4,4)) throw new CustomException.CustomException("对方会员手机号手机末4位数字不正确");

                if (money <= 0) throw new CustomException.CustomException("交易数量不正确");
                //if (string.IsNullOrEmpty(tradePassword) || Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException.CustomException("交易密码不正确");

                var yeCur = CurrencyService.Single(x => x.ID == curid);//转账币种
                //var jfCur = CurrencyService.Single(2);//积分账户
                if (yeCur == null) throw new CustomException.CustomException("币种不存在");
                //if (jfCur == null) throw new CustomException.CustomException("币种不存在");

                var param3102 = cacheSysParam.Single(x => x.ID == 3102);//转账参数
                decimal minMoney = param3102.Value.ToDecimal();
                //decimal maxMoney = cacheSysParam.Single(x => x.ID == 3102).Value2.ToDecimal();

                if (money < minMoney) throw new JN.Services.CustomException.CustomException("数值错误，金额不低于：" + minMoney);

                decimal beisu = param3102.Value2.ToDecimal();
                if (beisu != 0 && money % beisu != 0) throw new JN.Services.CustomException.CustomException("金额必须是" + beisu + "的倍数");


                decimal poundage = 0;
                if (curid == 1)
                    poundage = money * param3102.Value3.ToDecimal();
                decimal realtransfermoney = money + poundage;//实际金额

                decimal balance = JN.Services.Manager.Users.WalletCur((int)yeCur.WalletCurID, Umodel);
                if (balance < realtransfermoney) throw new JN.Services.CustomException.CustomException("余额不足，请重新填写");

                string description = "用户转帐：转给【" + toUser.UserName + "】";
                //写入转帐表
                TransferService.Add(new Data.Transfer
                {
                    ActualTransferMoney = realtransfermoney,
                    CreateTime = DateTime.Now,
                    Poundage = poundage,
                    Remark = "",
                    ToUID = toUser.ID,
                    ToUserName = toUser.UserName,
                    TransferMoney = money,
                    UID = Umodel.ID,
                    UserName = Umodel.UserName,
                    CurID = yeCur.ID,
                    CurName = yeCur.CurrencyName
                });

                //流通增值举例：A会员转账给B会员1000余额，那么B会员私下打1000现金给A会员，平台赠送800积分给A会员（A会员共获得1000现金800积分），B会员获得800余额+200积分

                //var paramOut = cacheSysParam.SingleAndInit(x => x.ID == 2201);
                //var paramIn = cacheSysParam.SingleAndInit(x => x.ID == 2202);

                //写入资金明细表（转帐方）
                Wallets.changeWalletNoCommit(Umodel.ID, 0 - realtransfermoney, (int)yeCur.WalletCurID, description, yeCur);
                //Wallets.changeWalletNoCommit(Umodel.ID, realtransfermoney * paramOut.Value.ToDecimal(), (int)jfCur.WalletCurID, description + "获得", jfCur);

                //写入资金明细表（接收方）
                description = "用户转帐：从【" + Umodel.UserName + "】转入";
                Wallets.changeWalletNoCommit(toUser.ID, money, (int)yeCur.WalletCurID, description, yeCur);
                //Wallets.changeWalletNoCommit(toUser.ID, money * paramIn.Value2.ToDecimal(), (int)jfCur.WalletCurID, description, jfCur);

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

    }


}