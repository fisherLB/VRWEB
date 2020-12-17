

using JN.Data.Service;
using JN.Services.Tool;
/**
* Member.cs
*
* 功 能： N/A
* 类 名： Member
*
* Ver    变更日期             负责人  变更内容
* ───────────────────────────────────
* V0.01  2014/9/30            N/A      初版
*
* Copyright (c) 2012 GxBlessing Corporation. All rights reserved.
*┌──────────────────────────────────┐
*│　此技术信息为本公司机密信息，未经本公司书面同意禁止向第三方披露．　│
*│　版权所有：www.yinkee.net　　　　　　　　　　　　                　│
*└──────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data.Common;
using System.Web;
namespace JN.Services.Manager
{
    public partial class MMM
    {
        //此页面有线程调用，不能在此处做通用赋值放置参数调用等
        //20160708更新

        #region 利息发放过程（自动结算、手动结算）未用
        /// <summary>
        /// 利息发放
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void CalculateFLX(
            IUserService UserService,
            IWalletLogService WalletLogService,
            IBonusDetailService BonusDetailService,
            ISettlementService SettlementService,
            ISupplyHelpService SupplyHelpService,
            ISysDBTool SysDBTool,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
            int period = SettlementService.List().Count() > 0 ? SettlementService.List().Max(x => x.Period) + 1 : 1;  //分红总期数
            var param = cacheSysParam.SingleAndInit(x => x.ID == 1102);
            //统计所有未进行结算的投资
            int PARAM_LXDAY = param.Value.ToInt();
            var supplylist = SupplyHelpService.List(x => x.IsAccruaCount && x.SurplusAccrualDay > 0 && x.Status > 0 && x.OrderType == 0 && x.AreaType == 1).ToList();
            //supplyhelps.GetModelList("IsAccruaCount=1 and AccrualDay<" + PARAM_LXDAY + " and Status>0");
            DataCache.SetCache("TotalRow", supplylist.Count);
            decimal totalLX = 0;
            int j = 1;
            foreach (var item in supplylist)
            {
                var onUser = UserService.Single(item.UID);
                //float PARAM_LXRL = TypeConverter.StrToFloat(sysparams.GetModel(1102).Value2);
                DataCache.SetCache("CurrentRow", j);
                DataCache.SetCache("TransInfo", "正在结算“" + item.SupplyNo + "”提供单利息，用时：" + Tool.DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");
                //用户当期利息
                decimal bonusMoney = (item.OrderMoney ?? 0) * (item.AccruaRate ?? 0);
                //给当前用户的分红
                totalLX += bonusMoney;
                string bonusDesc = "来算订单【" + item.SupplyNo + "】【" + DateTime.Now.ToShortDateString() + "】的利息";
                bool isbalance = false;
                if (item.IsAccrualEffective) isbalance = true; //如果利息已经生效直接进帐户（提供单已经确认收款）
                if (bonusMoney > 0)
                {
                    //是否马上结算
                    if (isbalance)
                    {
                        //写入明细
                        WalletLogService.Add(new Data.WalletLog
                        {
                            ChangeMoney = bonusMoney,
                            Balance = onUser.Wallet2001 + bonusMoney,
                            CoinID = 2001,
                            CoinName = cacheSysParam.Single(x => x.ID == 2001).Name,
                            CreateTime = DateTime.Now,
                            Description = bonusDesc,
                            UID = onUser.ID,
                            UserName = onUser.UserName
                        });

                        //更新用户钱包
                        onUser.Wallet2001 = onUser.Wallet2001 + bonusMoney;
                        UserService.Update(onUser);
                    }
                    //写入奖金表
                    BonusDetailService.Add(new Data.BonusDetail
                    {
                        Period = 0,
                        BalanceTime = DateTime.Now,
                        BonusMoney = bonusMoney,
                        BonusID = 1102,
                        BonusName = cacheSysParam.Single(x => x.ID == 1102).Name,
                        CreateTime = DateTime.Now,
                        Description = bonusDesc,
                        IsBalance = isbalance,
                        UID = onUser.ID,
                        IsEffective = true,
                        EffectiveTime = DateTime.Now,
                        UserName = onUser.UserName
                    });
                }

                var updateEntity = SupplyHelpService.Single(item.ID);
                updateEntity.AccrualMoney = updateEntity.AccrualMoney + bonusMoney;
                updateEntity.AccrualDay = updateEntity.AccrualDay + 1;
                updateEntity.SurplusAccrualDay = updateEntity.SurplusAccrualDay - 1;
                updateEntity.TotalMoney = updateEntity.ExchangeAmount + updateEntity.AccrualMoney;
                SupplyHelpService.Update(updateEntity);
                SysDBTool.Commit();
                j++;
            }
            SettlementService.Add(new Data.Settlement { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = supplylist.Count });
            SysDBTool.Commit();
            DataCache.SetCache("TransInfo", "成功对" + supplylist.Count + "条提供帮助订单发利息，用时：" + Tool.DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");
        }
        #endregion


        #region 每日利息 wp18040101
        /// <summary>
        /// 每日分红利息发放手动
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void CalculateDfh(
            ISettlementService SettlementService,
             IUserService UserService,
            IWalletLogService WalletLogService,
            IBonusDetailService BonusDetailService,
            ISupplyHelpService SupplyHelpService,
            ISysDBTool SysDBTool,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
            int period = SettlementService.List().Count() > 0 ? SettlementService.List().Max(x => x.Period) + 1 : 1;  //每日分红总期数

            var param1102 = cacheSysParam.Single(x => x.ID == 1102);
            StringBuilder userSql = new StringBuilder();//用户表更新语句           

            List<Data.WalletLog> addWalletLogList = new List<Data.WalletLog>();
            List<Data.BonusDetail> addBonusDetailList = new List<Data.BonusDetail>();

            //持有现金币就可以分红
            //var userlist = UserService.List(x => x.Wallet2001 > 0 && x.IsLock == false).OrderBy(x => x.ID).ToList();
            var minDay = param1102.Value2.ToInt();
            var SupplyHelplist = SupplyHelpService.List(x => x.Status == (int)Data.Enum.HelpStatus.AllDeal && x.IsAccruaCount && x.AccrualDay < minDay).ToList();
            DataCache.SetCache("TotalRow", SupplyHelplist.Count);
            DataCache.SetCache("StartTime", DateTime.Now);

            decimal totalLX = 0;
            int j = 1;
            string bonusDesc;
            foreach (var item in SupplyHelplist)
            {
                DataCache.SetCache("CurrentRow", j);
                DataCache.SetCache("TransInfo", "正在结算“" + item.SupplyNo + "”的每日收益");
                decimal bonusMoney = 0;

                bonusMoney = item.ExchangeAmount * (item.AccruaRate ?? 0);

                bonusDesc = "来自订单【" + item.SupplyNo + "】的" + DateTime.Now + "的每日收益";

                if (bonusMoney > 0)
                {
                    //    addWalletLogList.Add(new Data.WalletLog
                    //    {
                    //        ChangeMoney = bonusMoney,
                    //        Balance = item.Wallet2001 + bonusMoney,
                    //        CoinID = param2001.ID,
                    //        CoinName = param2001.Name,
                    //        CreateTime = DateTime.Now,
                    //        Description = bonusDesc,
                    //        UID = item.ID,
                    //        UserName = item.UserName
                    //    });

                    //写入奖金表
                    addBonusDetailList.Add(new Data.BonusDetail
                    {
                        Period = period,
                        BalanceTime = DateTime.Now,
                        BonusMoney = bonusMoney,
                        BonusID = param1102.ID,
                        BonusName = param1102.Name,
                        CreateTime = DateTime.Now,
                        Description = bonusDesc,
                        IsBalance = false,
                        UID = item.UID,
                        UserName = item.UserName,
                        IsEffective = true,
                        EffectiveTime = DateTime.Now,
                        FromUID = item.UID,
                        FromUserName = item.UserName,
                        SupplyNo = item.SupplyNo
                    });

                    //更新用户钱包
                    item.AccrualDay += 1;
                    if (item.AccrualDay >= param1102.Value2.ToInt())
                    {
                        item.IsAccruaCount = false;
                        item.AccrualStopReason = "在【" + DateTime.Now + "】发放收益后达到" + param1102.Value2 + "天";
                    }
                    item.AccrualMoney += bonusMoney;
                    item.TotalMoney += bonusMoney;
                    userSql.AppendFormat(" update [SupplyHelp] set AccrualDay={0}, IsAccruaCount='{1}', AccrualMoney='{2}', TotalMoney='{3}' where ID={4}", item.AccrualDay, item.IsAccruaCount, item.AccrualMoney, item.TotalMoney, item.ID);

                }

                //给当前用户的分红
                totalLX += bonusMoney;
                j++;
            }

            if (addBonusDetailList.Count() > 0)
            {
                //批量处理用户的信息
                DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                //写入钱包表
                //WalletLogService.BulkInsert(addWalletLogList);
                //批量写入奖金记录
                BonusDetailService.BulkInsert(addBonusDetailList);
            }

            SettlementService.Add(new Data.Settlement { BalanceMode = balancemode, Bonus = 0, CreateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00")), Period = period, TotalBonus = totalLX, TotalUser = SupplyHelplist.Count });
            SysDBTool.Commit();
            SettlementBonus(UserService,WalletLogService,BonusDetailService,SupplyHelpService,SysDBTool,cacheSysParam);//结算奖金
            DataCache.SetCache("TransInfo", "成功对" + SupplyHelplist.Count + "条订单进行结算，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");

        }
        //结算奖金
        public static void SettlementBonus(
            IUserService UserService,
           IWalletLogService WalletLogService,
           IBonusDetailService BonusDetailService,
           ISupplyHelpService SupplyHelpService,
           ISysDBTool SysDBTool,
           List<Data.SysParam> cacheSysParam)
        {
            var param1102 = cacheSysParam.Single(x => x.ID == 1102);
            var param2001 = cacheSysParam.Single(x => x.ID == 2001);
            StringBuilder userSql = new StringBuilder();//用户表更新语句   
            List<Data.WalletLog> addWalletLogList = new List<Data.WalletLog>();
            //统计所有投资到达条件订单
            var SupplyHelplist = SupplyHelpService.List(x => x.Status == (int)Data.Enum.HelpStatus.AllDeal && !x.IsAccruaCount && !(x.IsTakeOut ?? false)).ToList();
            //相关会员id集合
            List<int> userIDList = SupplyHelplist.Select(d => d.UID).Distinct().ToList();
            //把相关会员放到内存
            Dictionary<int, Data.User> dicUser = UserService.List(d => userIDList.Contains(d.ID)).ToDictionary(d => d.ID, d => d);
            Data.User onUser = null;//用户当期利息
            foreach (var item in SupplyHelplist)
            {
                //没有会员则跳出
                if (!dicUser.ContainsKey(item.UID))
                {
                    continue;
                }
                onUser = dicUser[item.UID];//直接读取内存里的会员信息
                var BonusDetailList = BonusDetailService.List(x => x.BonusID == param1102.ID && x.SupplyNo == item.SupplyNo && !x.IsBalance).ToList();
                if (BonusDetailList.Count() > 0)
                {
                    var bonusMoney = BonusDetailList.Sum(x => x.BonusMoney);
                    addWalletLogList.Add(new Data.WalletLog
                    {
                        ChangeMoney = bonusMoney,
                        Balance = onUser.Wallet2001 + bonusMoney,
                        CoinID = param2001.ID,
                        CoinName = param2001.Name,
                        CreateTime = DateTime.Now,
                        Description = "订单【" + item.SupplyNo + "】的每日收益和本金结算：" + bonusMoney,
                        UID = onUser.ID,
                        UserName = onUser.UserName
                    });
                    dicUser[item.UID].Wallet2001 = onUser.Wallet2001 + bonusMoney;
                    userSql.AppendFormat(" update [BonusDetail] set IsBalance=1, BalanceTime=GETDATE() where SupplyNo='{0}' and BonusID in (1102) and IsBalance=0", item.SupplyNo);
                }
                //item.IsTakeOut = true;
                //item.Status = (int)Data.Enum.HelpStatus.receiveIncome;
                userSql.AppendFormat(" update [SupplyHelp] set IsTakeOut=1, Status=6 where ID={0}", item.ID);
            }

            if (SupplyHelplist.Count() > 0)
            {
                //批量处理用户的信息
                DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                foreach (var item in dicUser)
                {
                    userSql.AppendFormat(" update [User] set Wallet2001={0} where ID={1} ", item.Value.Wallet2001, item.Value.ID);
                }
                SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                //写入钱包表
                WalletLogService.BulkInsert(addWalletLogList);
            }
            SysDBTool.Commit();

        }
        #endregion

        #region 检查排队时间，排队期结束后进行重排处理
        /// <summary>
        /// 审核提供和接受帮助是否达到15天
        /// </summary>
        public static void CheckQueuing()
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 10000).ToList();

            int PARAM_QUEUINGDAY = cacheSysParam.SingleAndInit(x => x.ID == 3203).Value.ToInt(); //排队期参数（以计息周期算）
            //对未匹配的到期供单重新排队，利息归0
            Dictionary<string, string> updateParam = new Dictionary<string, string>();
            updateParam.Add("AccrualMoney", "0");
            updateParam.Add("AccrualDay", "0");
            updateParam.Add("IsRepeatQueuing", "1");
            updateParam.Add("RepeatQueuingTime", DateTime.Now.ToString());
            updateParam.Add("EndTime", DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString());
            MvcCore.Unity.Get<ISupplyHelpService>().Update(new Data.SupplyHelp(), updateParam, "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");
            MvcCore.Unity.Get<ISysDBTool>().Commit();
            //supplyhelps.Update("AccrualMoney=0,AccrualDay=0,IsRepeatQueuing=1,RepeatQueuingTime=getdate(),EndTime='" + DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString() + "'", "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");

            //对未匹配的到期受单重新排队
            Dictionary<string, string> updateParam2 = new Dictionary<string, string>();
            updateParam2.Add("IsRepeatQueuing", "1");
            updateParam2.Add("RepeatQueuingTime", DateTime.Now.ToString());
            updateParam2.Add("EndTime", DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString());
            MvcCore.Unity.Get<IAcceptHelpService>().Update(new Data.AcceptHelp(), updateParam2, "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");
            MvcCore.Unity.Get<ISysDBTool>().Commit();
            //accepthelps.Update("IsRepeatQueuing=1,RepeatQueuingTime=getdate(),EndTime='" + DateTime.Now.AddMinutes(PARAM_QUEUINGDAY).ToString() + "'", "Status=" + (int)Data.Enum.HelpStatus.NoMatching + " and EndTime<='" + DateTime.Now.ToString() + "'");
        }
        #endregion

        #region 检查匹配单是否在付款时限内付款
        /// <summary>
        /// 检查匹配单是否在付款时限内付款，超出侧进行处理
        /// </summary>
        public static void CheckPayEndTime()
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 10000).ToList();
            int PARAM_PAYENDHOUR = cacheSysParam.SingleAndInit(x => x.ID == 3206).Value.ToInt(); //付款时限参数 48小时
            int PARAM_PAYENDHOUR_DELAYED = cacheSysParam.SingleAndInit(x => x.ID == 3206).Value2.ToInt(); //付款延时参数 48小时
            //找出超时未付款配单(包括超时未付款及延时后超时未付款)
            var matchlist = MvcCore.Unity.Get<IMatchingService>().List(x => (x.Status == (int)Data.Enum.MatchingStatus.UnPaid && (x.PayEndTime ?? DateTime.Now) < DateTime.Now) || (x.Status == (int)Data.Enum.MatchingStatus.Delayed && SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) > (PARAM_PAYENDHOUR + PARAM_PAYENDHOUR_DELAYED))).ToList();
            foreach (var item in matchlist)
            {
                var onUser = MvcCore.Unity.Get<IUserService>().Single(item.SupplyUID);
                if (onUser != null)
                {
                    //bool isCancelSupply = true;//是否取消提供单
                    CancelMatching(item, "超时未进行付款", true);//提供单取消
                    //if (isCancelSupply)
                    //{
                    //    //删除提供单利息,奖金
                    //    MvcCore.Unity.Get<IBonusDetailService>().Delete(x => x.SupplyNo == item.SupplyNo && x.IsBalance == false);
                    //    MvcCore.Unity.Get<ISysDBTool>().Commit();
                    //}
                    //对供单用户帐号冻结处理
                    onUser.IsLock = true;
                    onUser.LockTime = DateTime.Now;
                    onUser.LockReason = "超时未付款触发冻结，单号：" + item.MatchingNo + "";
                    MvcCore.Unity.Get<IUserService>().Update(onUser);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();

                    //对推荐人扣除百分之5
                    //var ruser = MvcCore.Unity.Get<IUserService>().Single(onUser.RefereeID);
                    //if (ruser != null)
                    //{
                    //    decimal M = cacheSysParam.SingleAndInit(x => x.ID == 3209).Value.ToDecimal();
                    //    logs.WriteLog("应扣除金额：" + M + "当前用户果实篮子金额：" + ruser.Wallet2001);
                    //    if (M > ruser.Wallet2001)
                    //    {
                    //        M = ruser.Wallet2001;
                    //    }

                    //    Wallets.changeWallet(ruser.ID, 0 - M, 2001, "下属会员" + onUser.UserName + "供单“" + item.SupplyNo + "”未在限时内付款");
                    //}
                }

            }
        }

        #endregion

        #region 检查匹配单是否在收款时限内确认
        /// <summary>
        /// 检查匹配单是否在收款时限内确认
        /// </summary>
        public static void CheckVerifiedEndTime()
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 10000).ToList();
            int PARAM_VerifiedENDHOUR = cacheSysParam.SingleAndInit(x => x.ID == 3208).Value.ToInt(); //付款后确认时限参数 48小时
            var matchlist = MvcCore.Unity.Get<IMatchingService>().List(x => x.Status == (int)Data.Enum.MatchingStatus.Paid && SqlFunctions.DateDiff("minute", (x.PayEndTime ?? DateTime.Now), DateTime.Now) > PARAM_VerifiedENDHOUR).ToList();

            foreach (var item in matchlist)
            {
                Bonus.Settlement(item);
                var updateEnitity = MvcCore.Unity.Get<IMatchingService>().Single(item.ID);
                updateEnitity.Status = (int)Data.Enum.MatchingStatus.Verified;
                updateEnitity.AllDealTime = DateTime.Now;
                MvcCore.Unity.Get<IMatchingService>().Update(updateEnitity);

                var onUser = MvcCore.Unity.Get<IUserService>().Single(item.AcceptUID);
                if (onUser != null)
                {
                    //对供单用户帐号冻结处理
                    onUser.IsLock = true;
                    onUser.LockTime = DateTime.Now;
                    onUser.LockReason = "超时未进行确认收款，单号：" + item.MatchingNo + "";
                    MvcCore.Unity.Get<IUserService>().Update(onUser);
                }
            }
            MvcCore.Unity.Get<ISysDBTool>().Commit();
        }

        #endregion

        #region 匹配处理
        /// <summary>
        /// 匹配处理，ids和ida都为空时作自动匹配处理
        /// </summary>
        /// <param name="ids">提供订单ID集，“,”间隔</param>
        /// <param name="ida">接受订单ID集，“,”间隔</param>
        public static void Matching(string ids, string ida, ref string outMsg)
        {
            //if (MvcCore.Extensions.CacheExtensions.CheckCache("Matching"))//检测缓存是否存在，区分大小写
            //{
            //    return;
            //}
            //MvcCore.Extensions.CacheExtensions.SetCache("Matching", "", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
            int matchcount = 0;
            string errmsg = "";
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 10000).ToList();

            string supplymobile = ""; //群发手机号（提供方,","分隔）
            string acceptmobile = ""; //群发手机号（接受方,","分隔）

            //查找所有冻结用户并不匹配 by Annie
            var lockUserList = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.IsLock == true).ToList();
            string lockUsers = "";
            if (lockUserList.Count() > 0)
            {
                foreach (var item in lockUserList)
                {
                    string userId = item.ID.ToString();
                    lockUsers += "," + userId;
                }
            }
            string[] userIds = lockUsers.TrimEnd(',').TrimStart(',').Split(',');

            //循环读取所有符合的受单
            var acceptlist = MvcCore.Unity.Get<IAcceptHelpService>().List(x => !userIds.Contains(x.UID.ToString()) && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)Data.Enum.HelpStatus.AllDeal && x.Status > 0)
                .OrderByDescending(x => x.IsTop)
                .ThenByDescending(x => x.IsRepeatQueuing)
                .ThenBy(x => x.CreateTime).ToList();

            if (!string.IsNullOrEmpty(ida)) //指定ID(接受方)
            {
                string[] idas = ida.TrimEnd(',').TrimStart(',').Split(',');
                acceptlist = acceptlist.Where(x => idas.Contains(x.ID.ToString())).ToList();
            }
            if (acceptlist.Count() <= 0) errmsg += "没有符合条件的接受单记录";
            foreach (var acceptModel in acceptlist)
            {
                if (ids == "" && ida == "" && matchcount > cacheSysParam.SingleAndInit(x => x.ID == 3403).Value.ToInt()) break; //自动匹配每次20条

                //循环读取所有符合的供单
                var supplylist = MvcCore.Unity.Get<ISupplyHelpService>().List(x => !userIds.Contains(x.UID.ToString()) && x.HaveMatchingAmount < x.ExchangeAmount && x.Status < (int)Data.Enum.HelpStatus.AllDeal && x.Status > 0)
                    .OrderByDescending(x => x.IsTop)
                    .ThenByDescending(x => x.IsRepeatQueuing)
                    .ThenBy(x => x.CreateTime).ToList();
                //errmsg += "提取" + supplylist.Count() + "条提供单记录";
                if (string.IsNullOrEmpty(ids)) //自动匹配时
                {
                    int PARAM_PDZXSJ = cacheSysParam.SingleAndInit(x => x.ID == 3201).Value.ToInt();
                    if (PARAM_PDZXSJ > 0) //提供帮助入匹配列表的时间
                        supplylist = supplylist.Where(x => SqlFunctions.DateDiff("minute", x.CreateTime, DateTime.Now) >= PARAM_PDZXSJ).ToList();
                }

                if (!string.IsNullOrEmpty(ids)) //指定ID(提供方)
                {
                    //errmsg += "ids:" + ids.TrimEnd(',').TrimStart(',') + "";
                    //errmsg += "id集" + string.Join(",", supplylist.Select(x => x.ID).ToList());
                    string[] idss = ids.TrimEnd(',').TrimStart(',').Split(',');
                    supplylist = supplylist.Where(x => idss.Contains(x.ID.ToString())).ToList();
                }
                if (supplylist.Count() <= 0) errmsg += "没有符合条件的提供单记录";
                foreach (var supplyModel in supplylist)
                {
                    if (acceptModel.UID == supplyModel.UID)
                    {
                        errmsg += "提供单和匹配单是同一个用户“" + acceptModel.UserName + "”在订单号：" + supplyModel.SupplyNo;
                        break; //提供和接受同一个用户跳过
                    }
                    var supplyUser = MvcCore.Unity.Get<IUserService>().Single(supplyModel.UID);
                    supplymobile += "," + supplyUser.Mobile;

                    var acceptUser = MvcCore.Unity.Get<IUserService>().Single(acceptModel.UID);
                    acceptmobile += "," + acceptUser.Mobile;

                    if (supplyUser.IsLock || acceptUser.IsLock)  //检测是否有冻结的会员
                    {
                        errmsg += supplyUser.UserName + "或" + acceptUser.UserName + "已被冻结，无法继续";
                        break; //冻结的跳过
                    }

                    var newacceptModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(acceptModel.ID);  //重读受单数据，才可以取到最新匹配余量
                    decimal _acceptppamount = (newacceptModel.ExchangeAmount - newacceptModel.HaveMatchingAmount); //受单可匹配的金额
                    decimal _supplyppamount = (supplyModel.ExchangeAmount - supplyModel.HaveMatchingAmount); //供单可匹配的金额
                    decimal _matchamount2 = 0; //匹配量

                    if (_supplyppamount <= _acceptppamount)
                        _matchamount2 = _supplyppamount;  //提供单全部匹配,接受单全部或部分匹配
                    else
                        _matchamount2 = _acceptppamount;  //提供单部分匹配


                    DateTime _payendtime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3206).Value.ToInt()); //付款截止时间

                    //if (supplyModel.OrderType == 0)
                    //    _payendtime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3205).Value2.ToInt()); //付款截止时间

                    //wl17032701
                    //if (supplyModel.OrderOrigin == 2) _payendtime = DateTime.Now.AddMinutes(cacheSysParam.SingleAndInit(x => x.ID == 3305).Value.ToInt());

                    //增加匹配记录
                    MvcCore.Unity.Get<IMatchingService>().Add(new Data.Matching
                    {
                        AcceptNo = acceptModel.AcceptNo,  //接受单单号
                        MatchingNo = Data.Extensions.Matching.GetOrderNumber(),  //匹配单单号
                        AcceptUID = acceptModel.UID, //接受者
                        AcceptUserName = acceptModel.UserName,  //接受者
                        CreateTime = DateTime.Now,
                        MatchAmount = _matchamount2,  //匹配数量
                        PayEndTime = _payendtime, //付款截止时间
                        Status = (int)Data.Enum.MatchingStatus.UnPaid, //未付款
                        SupplyNo = supplyModel.SupplyNo, //提供单单号
                        SupplyUID = supplyModel.UID, //提供者
                        SupplyUserName = supplyModel.UserName,
                        City = supplyModel.City,
                        County = supplyModel.County,
                        Province = supplyModel.Province,
                        BankID = newacceptModel.BankID
                    });
                    matchcount++;



                    //更新提供单状态及匹配余量
                    var updateSModel = MvcCore.Unity.Get<ISupplyHelpService>().Single(supplyModel.ID);
                    if (supplyModel.HaveMatchingAmount + _matchamount2 >= supplyModel.ExchangeAmount)
                    {
                        updateSModel.HaveMatchingAmount = updateSModel.ExchangeAmount;
                        updateSModel.Status = (int)Data.Enum.HelpStatus.AllMatching;
                    }
                    else
                    {
                        updateSModel.HaveMatchingAmount = updateSModel.HaveMatchingAmount + _matchamount2;
                        updateSModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                    }
                    MvcCore.Unity.Get<ISupplyHelpService>().Update(updateSModel);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();

                    //更新接受单状态及匹配余量
                    if (newacceptModel.HaveMatchingAmount + _matchamount2 >= newacceptModel.ExchangeAmount)
                    {
                        var updateAModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(newacceptModel.ID);
                        updateAModel.HaveMatchingAmount = updateAModel.ExchangeAmount;
                        updateAModel.Status = (int)Data.Enum.HelpStatus.AllMatching;
                        MvcCore.Unity.Get<IAcceptHelpService>().Update(updateAModel);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                        break;  //全部匹配后跳出循环
                    }
                    else
                    {
                        var updateAModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(newacceptModel.ID);
                        updateAModel.HaveMatchingAmount = updateAModel.HaveMatchingAmount + _matchamount2;
                        updateAModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                        MvcCore.Unity.Get<IAcceptHelpService>().Update(updateAModel);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }

                }
            }

            if (matchcount > 0)
            {
                if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3521).Value == "1")
                {
                    string[] supplymobileList = supplymobile.TrimEnd(',').TrimStart(',').Split(',');
                    foreach (var item in supplymobileList)
                    {
                        SMSHelper.WebChineseMSM(item, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3521).Value2);
                    }                    
                }

                if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3522).Value == "1")
                {
                    string[] acceptmobileList = acceptmobile.TrimEnd(',').TrimStart(',').Split(',');
                    foreach (var item2 in acceptmobileList)
                    {
                        SMSHelper.WebChineseMSM(item2, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3522).Value2);
                    }
                }                    
                outMsg = "已匹配成功" + matchcount + "条提供单！";
            }
            else
                outMsg = "本次操作没有匹配任何记录.";
            //MvcCore.Extensions.CacheExtensions.ClearCache("Matching");//清除缓存
            if (!string.IsNullOrEmpty(errmsg))
                outMsg += "\n\r提示：" + errmsg;
        }
        #endregion

        #region 惩罚处理
        /// <summary>
        /// 惩罚处理（提供虚假汇款信息时）
        /// </summary>
        /// <param name="matchid">匹配单号</param>
        public static void Punish(int matchid)
        {
            var mModel = MvcCore.Unity.Get<IMatchingService>().Single(matchid);
            CancelMatching(mModel, "虚假汇款信息", true);

            var pUser = MvcCore.Unity.Get<IUserService>().Single(mModel.SupplyUID);
            pUser.IsLock = true;
            pUser.LockTime = DateTime.Now;
            pUser.LockReason = "虚假汇款信息封号处理，匹配订单：" + mModel.MatchingNo + "";
            MvcCore.Unity.Get<IUserService>().Update(pUser);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
            //var ruser = MvcCore.Unity.Get<IUserService>().Single(pUser.RefereeID);
            //if (ruser != null)
            //{
            //    decimal M = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().SingleAndInit(x => x.ID == 3209).Value.ToDecimal();
            //    logs.WriteLog("应扣除金额：" + M + "当前用户果实篮子金额：" + ruser.Wallet2001);
            //    if (M > ruser.Wallet2001)
            //        M = ruser.Wallet2001;
            //    Wallets.changeWallet(ruser.ID, 0 - M, 2001, "下属会员" + pUser.UserName + "供单“" + mModel.SupplyNo + "”虚假汇款信息");
            //}
        }
        #endregion

        #region 取消匹配
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">匹配单</param>
        /// <param name="Reason">取消原因</param>
        /// <param name="isSupplyCancel">虚假汇款时提供单直接消费</param>
        public static void CancelMatching(Data.Matching item, string Reason, Boolean isSupplyCancel)
        {
            //供单退回处理
            string alog = "";
            string slog = "";
            bool err = false;
            var sModel = MvcCore.Unity.Get<ISupplyHelpService>().Single(x => x.SupplyNo == item.SupplyNo);
            slog = Reason + "后修正，原匹配金额：" + sModel.HaveMatchingAmount;
            if (isSupplyCancel)
            {
                //删除提供单利息,奖金
                MvcCore.Unity.Get<IBonusDetailService>().Delete(x => x.SupplyNo == item.SupplyNo && x.IsBalance == false);
                MvcCore.Unity.Get<ISysDBTool>().Commit();


                sModel.HaveMatchingAmount = sModel.HaveMatchingAmount - item.MatchAmount;  //减掉已匹配的金额
                sModel.CancelTime = DateTime.Now;
                sModel.Status = (int)Data.Enum.HelpStatus.Cancel;
            }
            else
            {
                //找已匹配的总量
                decimal havesmatching = MvcCore.Unity.Get<IMatchingService>().List(x => x.SupplyNo == sModel.SupplyNo && x.Status > 0).Count() > 0 ? MvcCore.Unity.Get<IMatchingService>().List(x => x.SupplyNo == sModel.SupplyNo && x.Status > 0).Sum(x => x.MatchAmount) : 0;
                slog += "，新：" + havesmatching;
                sModel.HaveMatchingAmount = havesmatching - item.MatchAmount;
                //重新修正状态
                if (sModel.HaveMatchingAmount == 0)
                    sModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
                else if (sModel.HaveMatchingAmount > 0 && sModel.HaveMatchingAmount < sModel.ExchangeAmount)
                    sModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                else
                {
                    sModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                    slog += "已匹配金额异常，系统取消处理";
                }
            }
            slog += "，修复为：" + sModel.HaveMatchingAmount;
            sModel.ReserveStr2 = slog;
            MvcCore.Unity.Get<ISupplyHelpService>().Update(sModel);




            //受单退回处理
            var aModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(x => x.AcceptNo == item.AcceptNo);
            alog = "原：" + aModel.HaveMatchingAmount;
            //找已匹配的总量
            decimal havematching=0;//匹配金额
            var Mathlist = MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptNo == aModel.AcceptNo && x.Status > 0).ToList();
            if(Mathlist.Count()>0)
            {
                havematching = Mathlist.Sum(x => x.MatchAmount);               
            }
            //虚假订单时不扣除当前匹配单金额，因为havematching不包含标记为虚假订单的部分
            if (item.Status == (int)Data.Enum.MatchingStatus.Falsehood) 
            {
             aModel.HaveMatchingAmount = havematching;
            }              
            else
            {
             aModel.HaveMatchingAmount = havematching - item.MatchAmount;
            }

            //重新修正状态
            if (aModel.HaveMatchingAmount == 0)
            {
                aModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
            }            
            else if (aModel.HaveMatchingAmount > 0 && aModel.HaveMatchingAmount < aModel.ExchangeAmount)
            {
                aModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
            }
            else
            {
                err = true;
                alog += "已匹配金额异常，系统自动修复";
            }
            alog += "，修复为：" + aModel.HaveMatchingAmount;
            aModel.ReserveStr2 = alog;
            aModel.IsTop = true;
            MvcCore.Unity.Get<IAcceptHelpService>().Update(aModel);

            //配单取消
            var mModel = MvcCore.Unity.Get<IMatchingService>().Single(item.ID);
            mModel.Status = (int)Data.Enum.MatchingStatus.Cancel;
            mModel.CancelTime = DateTime.Now;
            mModel.CanceReason = Reason;
            MvcCore.Unity.Get<IMatchingService>().Update(mModel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();

            if (err) //系统自动修复
            {
                var _aModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(x => x.AcceptNo == item.AcceptNo);
                _aModel.HaveMatchingAmount = MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptNo == aModel.AcceptNo && x.Status > 0).Count() > 0 ? MvcCore.Unity.Get<IMatchingService>().List(x => x.AcceptNo == aModel.AcceptNo && x.Status > 0).Sum(x => x.MatchAmount) : 0;
                if (_aModel.HaveMatchingAmount <= 0)
                    _aModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
                else if (_aModel.HaveMatchingAmount > 0 && _aModel.HaveMatchingAmount < _aModel.ExchangeAmount)
                    _aModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                else
                    _aModel.Status = (int)Data.Enum.HelpStatus.AllMatching;
                MvcCore.Unity.Get<IAcceptHelpService>().Update(aModel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 注册48小时内必须提供帮助
        public static void MustBeRegisteredAfterSupplyHelp(Data.User onUser)
        {
           // List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 10000).ToList();
            //注册48小时内必须提供帮助，否侧冻结帐号
            int jctgqx = MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 3806).Value.ToInt();
            if (jctgqx > 0)
            {
                if ((onUser.AddupSupplyAmount ?? 0) > 0)
                {
                    var list = MvcCore.Unity.Get<ISupplyHelpService>().List(x => x.UID == onUser.ID && x.Status >= (int)JN.Data.Enum.HelpStatus.NoMatching).Count();
                    if (list <= 0 && ((DateTime)onUser.CreateTime).AddMinutes(jctgqx) < DateTime.Now)
                    {
                        var updateUserEntity = MvcCore.Unity.Get<IUserService>().Single(onUser.ID);
                        if ((onUser.LockReason ?? "abc").Contains("注册后未在规定时间内排单") && !onUser.IsLock)
                            updateUserEntity.LockReason = "";
                        else
                        {
                            updateUserEntity.IsLock = true;
                            updateUserEntity.LockTime = DateTime.Now;
                            updateUserEntity.LockReason = "注册后未在规定时间内排单";
                        }
                        MvcCore.Unity.Get<IUserService>().Update(updateUserEntity);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }
                }                      
            }
        }
        #endregion

        #region 提现完成后必须进行复投
        public static string MustBeReCastAfterAcceptHelp(Data.User onUser)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<ISysParamService>().List(x => x.ID < 10000).ToList();

            //接受订单完成后24小时必须进行复投
            int ftqx = cacheSysParam.SingleAndInit(x => x.ID == 3803).Value.ToInt();
            if (ftqx > 0)
            {
                var shelps = MvcCore.Unity.Get<JN.Data.Service.ISupplyHelpService>().List(x => x.UID == onUser.ID).OrderByDescending(x => x.CreateTime).ToList();
                var ahelps = MvcCore.Unity.Get<JN.Data.Service.IAcceptHelpService>().List(x => x.UID == onUser.ID && x.Status == (int)JN.Data.Enum.HelpStatus.AllDeal).OrderByDescending(x => x.AllDealTime).ToList();
                if (ahelps.Count > 0 && shelps.Count > 0)
                {
                    if ((ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now).AddMinutes(ftqx) < DateTime.Now && (ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now) > shelps.FirstOrDefault().CreateTime)
                    {
                        //封号处理，初始帐号除外
                        if (onUser.ParentID > 0)
                        {
                            var updateUserEntity = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(onUser.ID);
                            if ((onUser.LockReason ?? "abc").Contains("超时未进行复投") && !onUser.IsLock)
                                updateUserEntity.LockReason = "";
                            else
                            {
                                updateUserEntity.IsLock = true;
                                updateUserEntity.LockTime = DateTime.Now;
                                updateUserEntity.LockReason = "超时未进行复投，单号：" + ahelps.FirstOrDefault().AcceptNo;
                            }
                            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(updateUserEntity);
                            MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                            return "<p style=\"color:#f00\" > 注意，请马上进行复投，刷新此页面您的帐号将被冻结，来自已完成的接受订单“" + ahelps.FirstOrDefault().AcceptNo + "”需要马上进行复投！</p>";
                        }
                    }
                    else if ((ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now) > shelps.FirstOrDefault().CreateTime)
                    {
                        DateTime endtime = (ahelps.FirstOrDefault().AllDealTime ?? DateTime.Now).AddMinutes(ftqx);
                        return "<p style=\"color:#f00\">请在时限内进行提供帮助，来自已完成的接受订单“" + ahelps.FirstOrDefault().AcceptNo + "”<i class=\"fa pe-7s-clock\"></i> 剩余时间：<span class=\"time_countdown\" style=\"color:#f00\" data=\"" + endtime + "\"></span></p>";
                    }
                    else
                    {
                        return "<p style=\"color:#f00\" > 受订单全部成时，您需要在限时内进行复投，否则您的帐号将被冻结。</p>";
                    }
                }
            }
            return "";
        }
        #endregion

        #region 取消提供单
        /// <summary>
        /// 
        /// </summary>
        public static void CancelSupplyHelp(string SupplyNo, string Reason)
        {
            //删除利息,奖金
            MvcCore.Unity.Get<IBonusDetailService>().Delete(x => x.SupplyNo == SupplyNo && x.IsBalance == false);
            MvcCore.Unity.Get<ISysDBTool>().Commit();

            var sModel = MvcCore.Unity.Get<ISupplyHelpService>().Single(x => x.SupplyNo == SupplyNo);
            if (sModel != null)
            {
                sModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                sModel.CancelTime = DateTime.Now;
                sModel.CancelReason = Reason;
                sModel.IsAccruaCount = false;
                MvcCore.Unity.Get<ISupplyHelpService>().Update(sModel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 取消接受单
        /// <summary>
        /// 
        /// </summary>
        public static void CancelAcceptHelp(string AcceptNo, string Reason)
        {
            //删除利息,奖金
            var aModel = MvcCore.Unity.Get<IAcceptHelpService>().Single(x => x.AcceptNo == AcceptNo);
            if (aModel != null)
            {
                Wallets.changeWallet(aModel.UID, (aModel.ExchangeAmount - aModel.HaveMatchingAmount), aModel.CoinID, "取消接受帮助“" + aModel.AcceptNo + "”订单返还");
                aModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                aModel.CancelTime = DateTime.Now;
                aModel.CancelReason = Reason;
                MvcCore.Unity.Get<IAcceptHelpService>().Update(aModel);
                MvcCore.Unity.Get<ISysDBTool>().Commit();
            }
        }
        #endregion
    }
}
