using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.SqlServer;
using System.Linq;

namespace JN.Services.Manager
{
    /// <summary>
    ///奖金结算
    /// </summary>
    public partial class Bonus
    {
        private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        /// <summary>
        /// 清除缓存后重新加载
        /// </summary>
        public static void HeavyLoad()
        {
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        }

        #region 写入奖金明细表并更新用户钱包
        /// <summary>
        /// 写入资金明细表并更新用户钱包（未结算只写入奖金表不进钱包及资金明细）
        /// </summary>
        /// <param name="bonusmoney">奖金金额</param>
        /// <param name="period">期数（发放利息时）</param>
        /// <param name="bonusid">奖金ID（对应参数表SysParam的ID）</param>
        /// <param name="bonusname">奖金名称（对应参数表SysParam的Name）</param>
        /// <param name="bonusdesc">获奖描述来源</param>
        /// <param name="onUserID">用户ID</param>
        /// <param name="addupfield">累计奖金字段（对应用户表User的Addup1101-1107/1802）</param>
        /// <param name="isbalance">是否结算,Ture时写入钱包明细表WalletDetails及更新User表中用户钱包余额Wallet2001-2005，Falsh时只记入奖金明细表BonusDetails</param>
        public static void UpdateUserWallet(decimal bonusmoney, string supplyno, int bonusid, string bonusname, string bonusdesc, int onUserID, int formUserID, string addupfield, bool isbalance, bool isEffective, DateTime effectiveTime, string MatchNo)
        {
            var onUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(onUserID);
            var fromUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(formUserID);
            if (bonusmoney > 0)
            {
                var param2005 = cacheSysParam.SingleAndInit(x => x.ID == 2005);//未来钱包
                //是否马上结算
                if (isbalance)
                {
                    switch (bonusid)
                    {
                        case 1103:
                            Wallets.changeWallet(onUser.ID, bonusmoney, 2001, bonusdesc);
                            break;
                        case 1102:
                            Wallets.changeWallet(onUser.ID, bonusmoney, 2001, bonusdesc);
                            break;
                    }
                }
                //写入奖金表
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Add(new Data.BonusDetail
                {
                    Period = 0,
                    BalanceTime = DateTime.Now,
                    BonusMoney = bonusmoney,
                    BonusID = bonusid,
                    BonusName = bonusname,
                    CreateTime = DateTime.Now,
                    Description = bonusdesc,
                    BonusMoneyCF = 0,
                    BonusMoneyCFBA = 0,
                    IsBalance = isbalance,
                    UID = onUser.ID,
                    UserName = onUser.UserName,
                    SupplyNo = supplyno,
                    IsEffective = isEffective,
                    EffectiveTime = effectiveTime,
                    FromUID = fromUser.ID,
                    FromUserName = fromUser.UserName,
                    MatchNo = MatchNo
                });
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }
        }

        /// <summary>
        /// 写入资金明细表并更新用户钱包（未结算只写入奖金表不进钱包及资金明细）
        /// </summary>
        /// <param name="bonusmoney">奖金金额</param>
        /// <param name="bonusid">奖金ID（对应参数表SysParam的ID）</param>
        /// <param name="bonusname">奖金名称（对应参数表SysParam的Name）</param>
        /// <param name="bonusdesc">获奖描述来源</param>
        /// <param name="onUser">用户</param>
        /// <param name="fromUser">来源用户</param>
        /// <param name="isbalance">是否结算,Ture时写入钱包明细表WalletDetails及更新User表中用户钱包余额，Falsh时只记入奖金明细表BonusDetails</param>
        /// <param name="effectiveTime">结算时间</param>
        /// <param name="jfCur">结算币种</param>
        public static void UpdateUserWallet(decimal bonusmoney, int bonusid, string bonusname, string bonusdesc, Data.User onUser, Data.User fromUser, bool isbalance, bool isEffective, DateTime effectiveTime, JN.Data.Currency jfCur, JN.Data.Currency yeCur)
        {
            if (bonusmoney > 0)
            {
                //是否马上结算
                if (isbalance)
                {
                    switch (bonusid)
                    {
                        case 1103:
                            Wallets.changeWalletNoCommitAddup(onUser.ID, -bonusmoney, bonusid, bonusdesc, jfCur);//积分减少
                            Wallets.changeWalletNoCommitAddup(onUser.ID, bonusmoney, bonusid, bonusdesc, yeCur);//余额增加
                            break;
                        case 1104:
                            Wallets.changeWalletNoCommitAddup(onUser.ID, -bonusmoney, bonusid, bonusdesc, jfCur);//积分减少
                            Wallets.changeWalletNoCommitAddup(onUser.ID, bonusmoney, bonusid, bonusdesc, yeCur);//余额增加
                            break;
                        case 1105:
                            Wallets.changeWalletNoCommitAddup(onUser.ID, -bonusmoney, bonusid, bonusdesc, jfCur);//积分减少
                            Wallets.changeWalletNoCommitAddup(onUser.ID, bonusmoney, bonusid, bonusdesc, yeCur);//余额增加
                            break;
                    }
                }
                //写入奖金表
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Add(new Data.BonusDetail
                {
                    Period = 0,
                    BalanceTime = DateTime.Now,
                    BonusMoney = bonusmoney,
                    BonusID = bonusid,
                    BonusName = bonusname,
                    CreateTime = DateTime.Now,
                    Description = bonusdesc,
                    BonusMoneyCF = 0,
                    BonusMoneyCFBA = 0,
                    IsBalance = isbalance,
                    UID = onUser.ID,
                    UserName = onUser.UserName,
                    SupplyNo = "",
                    IsEffective = isEffective,
                    EffectiveTime = effectiveTime,
                    FromUID = fromUser.ID,
                    FromUserName = fromUser.UserName,
                    MatchNo = ""
                });
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }
        }


        /// <summary>
        /// 写入资金明细表并更新用户钱包（未结算只写入奖金表不进钱包及资金明细）
        /// </summary>
        /// <param name="bonusmoney">奖金金额</param>
        /// <param name="bonusid">奖金ID（对应参数表SysParam的ID）</param>
        /// <param name="bonusname">奖金名称（对应参数表SysParam的Name）</param>
        /// <param name="bonusdesc">获奖描述来源</param>
        /// <param name="onUser">用户</param>
        /// <param name="fromUser">来源用户</param>
        /// <param name="isbalance">是否结算,Ture时写入钱包明细表WalletDetails及更新User表中用户钱包余额，Falsh时只记入奖金明细表BonusDetails</param>
        /// <param name="effectiveTime">结算时间</param>
        /// <param name="jfCur">结算币种</param>
        public static void UpdateUserWallet(decimal bonusmoney, int bonusid, string bonusname,int conid, string bonusdesc, Data.User onUser, Data.User fromUser, bool isbalance, bool isEffective, DateTime effectiveTime, JN.Data.Currency curmodel, ref List<Data.BonusDetail> BonusDetailList, ref List<Data.WalletLog> WalletLogList,ref Dictionary<int, Data.User> dicUser)
        {
            if (bonusmoney > 0)
            {
                //是否马上结算
                if (isbalance)
                {
                    decimal changeWallet = 0;
                    switch (curmodel.WalletCurID)
                    {
                        case 3001:
                            changeWallet = (dicUser[onUser.ID].Cur3001 ?? 0);
                            dicUser[onUser.ID].Cur3001 = (dicUser[onUser.ID].Cur3001 ?? 0) + bonusmoney;
                            break;
                        case 3002:
                            changeWallet = (dicUser[onUser.ID].Cur3002 ?? 0);
                            dicUser[onUser.ID].Cur3002 = (dicUser[onUser.ID].Cur3002 ?? 0) + bonusmoney;
                            if (bonusmoney > 0)
                            {
                                dicUser[onUser.ID].ReserveDecamal2 = (dicUser[onUser.ID].ReserveDecamal2 ?? 0) + bonusmoney;
                            }
                            break;
                        case 3003:
                            changeWallet = (dicUser[onUser.ID].Cur3003 ?? 0);
                            dicUser[onUser.ID].Cur3003 = (dicUser[onUser.ID].Cur3003 ?? 0) + bonusmoney;

                            break;
                        case 3004:
                            changeWallet = (dicUser[onUser.ID].Cur3004 ?? 0);
                            dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusmoney;

                            break;

                        case 3005:
                            changeWallet = (dicUser[onUser.ID].Cur3005 ?? 0);
                            dicUser[onUser.ID].Cur3005 = (dicUser[onUser.ID].Cur3005 ?? 0) + bonusmoney;

                            break;
                        case 3006:
                            changeWallet = (dicUser[onUser.ID].Cur3006 ?? 0);
                            dicUser[onUser.ID].Cur3006 = (dicUser[onUser.ID].Cur3006 ?? 0) + bonusmoney;

                            break;
                        case 3007:
                            changeWallet = (dicUser[onUser.ID].Cur3007 ?? 0);
                            dicUser[onUser.ID].Cur3007 = (dicUser[onUser.ID].Cur3007 ?? 0) + bonusmoney;
                            break;
                        case 3008:
                            changeWallet = (dicUser[onUser.ID].Cur3008 ?? 0);
                            dicUser[onUser.ID].Cur3008 = (dicUser[onUser.ID].Cur3008 ?? 0) + bonusmoney;

                            break;
                        case 3009:
                            changeWallet = (dicUser[onUser.ID].Cur3009 ?? 0);
                            dicUser[onUser.ID].Cur3009 = (dicUser[onUser.ID].Cur3009 ?? 0) + bonusmoney;

                            break;
                        case 3010:
                            changeWallet = (dicUser[onUser.ID].Cur3010 ?? 0);
                            dicUser[onUser.ID].Cur3010 = (dicUser[onUser.ID].Cur3010 ?? 0) + bonusmoney;

                            break;

                        case 3011:
                            changeWallet = (dicUser[onUser.ID].Cur3011 ?? 0);
                            dicUser[onUser.ID].Cur3011 = (dicUser[onUser.ID].Cur3011 ?? 0) + bonusmoney;

                            break;

                        case 3012:
                            changeWallet = (dicUser[onUser.ID].Cur3012 ?? 0);
                            dicUser[onUser.ID].Cur3012 = (dicUser[onUser.ID].Cur3012 ?? 0) + bonusmoney;
                            break;

                        case 3013:
                            changeWallet = (dicUser[onUser.ID].Cur3013 ?? 0);
                            dicUser[onUser.ID].Cur3013 = (dicUser[onUser.ID].Cur3013 ?? 0) + bonusmoney;

                            break;

                        case 3014:
                            changeWallet = (dicUser[onUser.ID].Cur3014 ?? 0);
                            dicUser[onUser.ID].Cur3014 = (dicUser[onUser.ID].Cur3014 ?? 0) + bonusmoney;

                            break;
                        case 3015:
                            changeWallet = (dicUser[onUser.ID].Cur3015 ?? 0);
                            dicUser[onUser.ID].Cur3015 = (dicUser[onUser.ID].Cur3015 ?? 0) + bonusmoney;
                            break;
                    }

                    switch (bonusid)
                    {
                        case 1101:
                            dicUser[onUser.ID].Addup1101 = (dicUser[onUser.ID].Addup1101 ?? 0) + bonusmoney;
                            break;
                        case 1102:
                            dicUser[onUser.ID].Addup1102 = (dicUser[onUser.ID].Addup1102 ?? 0) + bonusmoney;
                            break;
                        case 1103:
                            dicUser[onUser.ID].Addup1103 = (dicUser[onUser.ID].Addup1103 ?? 0) + bonusmoney;
                            break;
                        case 1104:
                            dicUser[onUser.ID].Addup1104 = (dicUser[onUser.ID].Addup1104 ?? 0) + bonusmoney;
                            break;
                        case 1105:
                            dicUser[onUser.ID].Addup1105 = (dicUser[onUser.ID].Addup1105 ?? 0) + bonusmoney;
                            break;
                        case 1106:
                            dicUser[onUser.ID].Addup1106 = (dicUser[onUser.ID].Addup1106 ?? 0) + bonusmoney;
                            break;
                        case 1107:
                            dicUser[onUser.ID].Addup1107 = (dicUser[onUser.ID].Addup1107 ?? 0) + bonusmoney;
                            break;
                        case 1108:
                            dicUser[onUser.ID].Addup1802 = (dicUser[onUser.ID].Addup1802 ?? 0) + bonusmoney;
                            break;
                    }


                    dicUser[onUser.ID].ReserveInt1 = 123456;//更改标记
                    //写入明细
                    WalletLogList.Add(new Data.WalletLog
                    {
                        ChangeMoney = bonusmoney,
                        Balance = changeWallet + bonusmoney,
                        CoinID = curmodel.WalletCurID ?? 0,
                        CoinName = curmodel.CurrencyName,
                        CreateTime = DateTime.Now,
                        Description = bonusdesc,
                        UID = onUser.ID,
                        UserName = onUser.UserName
                    });
                }
                //写入奖金表
                BonusDetailList.Add(new Data.BonusDetail
                {
                    Period = 0,
                    BalanceTime = DateTime.Now,
                    BonusMoney = bonusmoney,
                    BonusID = bonusid,
                    BonusName = bonusname,
                    CreateTime = DateTime.Now,
                    Description = bonusdesc,
                    BonusMoneyCF = 0,
                    BonusMoneyCFBA = 0,
                    IsBalance = isbalance,
                    UID = onUser.ID,
                    UserName = onUser.UserName,
                    SupplyNo = "",
                    IsEffective = isEffective,
                    EffectiveTime = effectiveTime,
                    FromUID = fromUser.ID,
                    FromUserName = fromUser.UserName,
                    MatchNo = ""
                });
            }
        }

        public static void UpdateUserWallet(decimal bonusmoney, int bonusid, string bonusname, decimal Balance, string bonusdesc, Data.User onUser, Data.User fromUser,JN.Data.Currency curmodel, ref List<Data.BonusDetail> BonusDetailList, ref List<Data.WalletLog> WalletLogList)
        {
            if (bonusmoney > 0)
            {
                //写入明细
                WalletLogList.Add(new Data.WalletLog
                {
                    ChangeMoney = bonusmoney,
                    Balance = Balance,
                    CoinID = curmodel.WalletCurID ?? 0,
                    CoinName = curmodel.CurrencyName,
                    CreateTime = DateTime.Now,
                    Description = bonusdesc,
                    UID = onUser.ID,
                    UserName = onUser.UserName
                });
                //写入奖金表
                BonusDetailList.Add(new Data.BonusDetail
                {
                    Period = 0,
                    BalanceTime = DateTime.Now,
                    BonusMoney = bonusmoney,
                    BonusID = bonusid,
                    BonusName = bonusname,
                    CreateTime = DateTime.Now,
                    Description = bonusdesc,
                    BonusMoneyCF = 0,
                    BonusMoneyCFBA = 0,
                    IsBalance = true,
                    UID = onUser.ID,
                    UserName = onUser.UserName,
                    SupplyNo = "",
                    IsEffective = true,
                    EffectiveTime = DateTime.Now,
                    FromUID = fromUser.ID,
                    FromUserName = fromUser.UserName,
                    MatchNo = ""
                });
            }
        }

        #endregion

        #region 直推奖
        /// <summary>
        /// 直推奖 wp18070301
        /// </summary>
        /// 享受直推1代直接释放 互助排单和余额兑换积分的5%
        /// <param name="money">兑换金额</param>
        /// <param name="onUserId">会员ID</param>
        /// <param name="jfCur">扣除币种,积分</param>
        /// <param name="yeCur">增加币种,余额</param>
        public static void Bonus1103(decimal money, int onUserId, Data.Currency jfCur, Data.Currency yeCur)
        {
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x => x.ID == onUserId);//会员实体
            var refereeUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x => x.ID == onUser.RefereeID);//推荐人

            if (refereeUser != null && refereeUser.UserLevel >= (int)JN.Data.Enum.UserLevel.Level2)//自己也得是普通会员
            {
                var param = cacheSysParam.Single(x => x.ID == 1103);//推荐奖金参数
                decimal bonusMoney = money * param.Value.ToDecimal();

                //小于余额则等于余额账户的金额
                bonusMoney = bonusMoney < (refereeUser.Cur3002 ?? 0) ? bonusMoney : (refereeUser.Cur3002 ?? 0);
                if (bonusMoney > 0)
                {
                    string bonusDesc = "来自会员【" + onUser.UserName + "】的" + param.Name + "(" + money.ToDouble() + "*" + param.Value + ")";

                    UpdateUserWallet(bonusMoney, param.ID, param.Name, bonusDesc, refereeUser, onUser, true, false, DateTime.Now, jfCur, yeCur);
                }
            }
        }
        #endregion

        #region 见点奖
        /// <summary>
        /// 见点奖   wp18070301
        /// </summary>
        ///  必须是普通会员 1%、拿团队15代
        /// <param name="onUserId">投资会员</param>
        /// <param name="money">投资金额</param>
        /// <param name="jfCur">扣除币种,积分</param>
        /// <param name="yeCur">增加币种,余额</param>
        public static void Bouns1104(decimal money, int onUserId, Data.Currency jfCur, Data.Currency yeCur)
        {
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x => x.ID == onUserId);
            //var userlist = Users.GetAllRefereeParent(onUser, 10);//MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString())).OrderByDescending(x => x.ID);//查询一条线上的人
            if ((onUser.ReserveInt1 ?? 0) <= 0 && !string.IsNullOrEmpty(onUser.RefereePath))//没拿过并且有推荐人
            {
                var param = cacheSysParam.Single(x => x.ID == 1104);//见点奖金参数

                var param1401 = cacheSysParam.SingleAndInit(x => x.ID == 1401);
                var param1402 = cacheSysParam.SingleAndInit(x => x.ID == 1402);
                var param1403 = cacheSysParam.SingleAndInit(x => x.ID == 1403);

                string[] ids = onUser.RefereePath.Split(',');
                int maxDepth = param1403.Value2.ToInt();
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString()) && x.RefereeDepth >= (onUser.RefereeDepth - maxDepth)).ToList();

                if (userlist.Count() > 0)
                {
                    int depth = 0;//代数
                    decimal PARAM_JDJBL = 0;  //见点奖可拿比例
                    decimal bonusMoney = 0;//奖金金额
                    int refereeCount = 0;//有效推荐人数   
                    string bonusDesc = "";//奖金描述

                    foreach (var tjjUser in userlist)
                    {
                        if (tjjUser.UserLevel < (int)JN.Data.Enum.UserLevel.Level2)//自己也得是普通会员
                        {
                            continue;
                        }
                        var username = tjjUser.UserName;
                        depth = onUser.RefereeDepth - tjjUser.RefereeDepth;
                        refereeCount = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.RefereeID == tjjUser.ID && x.IsActivation && !x.IsLock && x.UserLevel >= (int)JN.Data.Enum.UserLevel.Level2).Count();//必须是普通会员

                        if (refereeCount >= param1403.Value.ToInt() && depth <= param1403.Value2.ToInt()) /*depth >= param1403.Value2.ToInt()*/
                        {
                            PARAM_JDJBL = param1403.Value3.ToDecimal();
                        }
                        else if (refereeCount >= param1402.Value.ToInt() && depth <= param1402.Value2.ToInt())
                        {
                            PARAM_JDJBL = param1402.Value3.ToDecimal();
                        }
                        else if (refereeCount >= param1401.Value.ToInt() && depth <= param1401.Value2.ToInt())
                        {
                            PARAM_JDJBL = param1401.Value3.ToDecimal();
                        }
                        else
                        {
                            PARAM_JDJBL = 0;
                        }
                        bonusMoney = money * PARAM_JDJBL;
                        //小于余额则等于余额账户的金额
                        bonusMoney = bonusMoney < (tjjUser.Cur3002 ?? 0) ? bonusMoney : (tjjUser.Cur3002 ?? 0);
                        if (bonusMoney > 0)
                        {
                            bonusDesc = "来自会员【" + onUser.UserName + "】的" + param.Name + "(" + money.ToDouble() + "*" + PARAM_JDJBL + ")";
                            UpdateUserWallet(bonusMoney, param.ID, param.Name, bonusDesc, tjjUser, onUser, true, false, DateTime.Now, jfCur, yeCur);
                        }
                    }
                }
                string sql = "update [User] set ReserveInt1=ISNULL(ReserveInt1,0)+1 where ID=" + onUser.ID;
                DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().ExecuteSQL(sql.ToString(), dbparam);
            }
        }
        #endregion

        #region 团队复投奖
        /// <summary>
        /// 团队复投奖   wp18070301
        /// </summary>
        ///  拿团队15代 指的是团队内余额兑换积分
        /// <param name="onUserId">会员ID</param>
        /// <param name="money">投资金额</param>
        /// <param name="jfCur">扣除币种,积分</param>
        /// <param name="yeCur">增加币种,余额</param>
        public static void Bouns1105(decimal money, int onUserId, Data.Currency jfCur, JN.Data.Currency yeCur)
        {
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x => x.ID == onUserId);//会员实体
            var param = cacheSysParam.Single(x => x.ID == 1105);//团队复投奖参数
            //var userlist = Users.GetAllRefereeParent(onUser, 10);//MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString())).OrderByDescending(x => x.ID);//查询一条线上的人

            if (!string.IsNullOrEmpty(onUser.RefereePath))
            {
                var param1501 = cacheSysParam.SingleAndInit(x => x.ID == 1501);
                var param1502 = cacheSysParam.SingleAndInit(x => x.ID == 1502);
                var param1503 = cacheSysParam.SingleAndInit(x => x.ID == 1503);

                string[] ids = onUser.RefereePath.Split(',');
                int maxDepth = param1503.Value2.ToInt();
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString()) && x.RefereeDepth >= (onUser.RefereeDepth - maxDepth)).ToList();

                if (userlist.Count() > 0)
                {
                    int depth = 0;//代数
                    decimal PARAM_JDJBL = 0;  //见点奖可拿比例
                    decimal bonusMoney = 0;//奖金金额
                    int refereeCount = 0;//有效推荐人数   
                    string bonusDesc = "";//奖金描述

                    foreach (var tdjUser in userlist)
                    {
                        if (tdjUser.UserLevel < (int)JN.Data.Enum.UserLevel.Level2)//自己也得是普通会员
                        {
                            continue;
                        }

                        depth = onUser.RefereeDepth - tdjUser.RefereeDepth;
                        refereeCount = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.RefereeID == tdjUser.ID && x.IsActivation && !x.IsLock && x.UserLevel >= (int)JN.Data.Enum.UserLevel.Level2).Count();//必须是普通会员

                        if (refereeCount >= param1503.Value.ToInt() && depth <= param1503.Value2.ToInt()) /*depth >= param1403.Value2.ToInt()*/
                        {
                            PARAM_JDJBL = param1503.Value3.ToDecimal();
                        }
                        else if (refereeCount >= param1502.Value.ToInt() && depth <= param1502.Value2.ToInt())
                        {
                            PARAM_JDJBL = param1502.Value3.ToDecimal();
                        }
                        else if (refereeCount >= param1501.Value.ToInt() && depth <= param1501.Value2.ToInt())
                        {
                            PARAM_JDJBL = param1501.Value3.ToDecimal();
                        }
                        else
                        {
                            PARAM_JDJBL = 0;
                        }
                        bonusMoney = money * PARAM_JDJBL;
                        //小于余额则等于余额账户的金额
                        bonusMoney = bonusMoney < (tdjUser.Cur3002 ?? 0) ? bonusMoney : (tdjUser.Cur3002 ?? 0);
                        if (bonusMoney > 0)
                        {
                            bonusDesc = "来自会员【" + onUser.UserName + "】的" + param.Name + "(" + money.ToDouble() + "*" + PARAM_JDJBL + ")";
                            UpdateUserWallet(bonusMoney, param.ID, param.Name, bonusDesc, tdjUser, onUser, true, false, DateTime.Now, jfCur, yeCur);
                        }
                    }
                }
            }
        }
        #endregion

        #region 兑换加速设置
        /// <summary>
        /// 兑换加速设置   wp18070301
        /// </summary>
        /// 余额兑换积分，加快积分释放（积分减少，增加到余额里）
        /// <param name="onUserId">投资会员</param>
        /// <param name="money">投资金额</param>
        /// <param name="jfCur">积分</param>
        /// <param name="yeCur">余额</param>
        public static void ExchangeUp(decimal money, Data.User onUser, Data.Currency jfCur, Data.Currency yeCur)
        {
            if (!string.IsNullOrEmpty(onUser.RefereePath))
            {
                var param2301 = cacheSysParam.SingleAndInit(x => x.ID == 2301);
                var param2302 = cacheSysParam.SingleAndInit(x => x.ID == 2302);

                string[] ids = onUser.RefereePath.Split(',');
                int maxDepth = param2302.Value2.ToInt();
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString()) && x.RefereeDepth >= (onUser.RefereeDepth - maxDepth)).ToList();

                if (userlist.Count() > 0)
                {
                    int depth = 0;//代数
                    decimal bonusMoney = 0;//奖金金额
                    int refereeCount = 0;//有效推荐人数   
                    string bonusDesc = "";//奖金描述

                    foreach (var tdjUser in userlist)
                    {
                        depth = onUser.RefereeDepth - tdjUser.RefereeDepth;
                        //refereeCount = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => (x.RefereePath.Contains("," + tdjUser.ID + ",") || x.RefereeID == tdjUser.ID) && x.IsActivation && !x.IsLock && x.UserLevel >= (int)JN.Data.Enum.UserLevel.Level2).Count();

                        refereeCount = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.RefereeID == tdjUser.ID && x.IsActivation && !x.IsLock && x.UserLevel >= (int)JN.Data.Enum.UserLevel.Level2).Count();

                        if (tdjUser.UserLevel < (int)JN.Data.Enum.UserLevel.Level2)//自己是普通会员
                        {
                            continue;
                        }

                        if (refereeCount < param2302.Value.ToInt() && depth > param2302.Value3.ToInt())
                        {
                            continue;
                        }
                        else if (refereeCount >= param2302.Value.ToInt() && depth > param2302.Value2.ToInt())
                        {
                            continue;
                        }

                        bonusMoney = money * param2301.Value.ToDecimal();

                        //小于余额则等于余额账户的金额
                        bonusMoney = bonusMoney < (tdjUser.Cur3002 ?? 0) ? bonusMoney : (tdjUser.Cur3002 ?? 0);

                        if (bonusMoney > 0)
                        {
                            //兑换奖励加速
                            var release = new Data.ReleaseDetail();
                            release.Period = 0;
                            release.UID = tdjUser.ID;
                            release.UserName = tdjUser.UserName;
                            release.CurID = jfCur.ID;
                            release.CurName = jfCur.CurrencyName;
                            release.Money = bonusMoney;
                            release.IsSign = true;   //已领取                   
                            release.CreateTime = DateTime.Now;//创建时间     
                            release.EndTime = DateTime.Now;//失效时间
                            MvcCore.Unity.Get<Data.Service.IReleaseDetailService>().Add(release);

                            //Wallets.changeWalletNoCommit(onUser.ID, -bonusMoney, (int)jfCur.WalletCurID, "释放加速", jfCur);//结算奖金并写入累计字段
                            //Wallets.changeWallet(onUser.ID, bonusMoney, (int)yeCur.WalletCurID, "释放加速", yeCur);//结算奖金并写入累计字段
                            Wallets.changeWalletNoCommit(tdjUser.ID, -bonusMoney, (int)jfCur.WalletCurID, "会员【" + onUser.UserName + "】兑换奖励加速", jfCur);//结算奖金并写入累计字段
                            Wallets.changeWallet(tdjUser.ID, bonusMoney, (int)yeCur.WalletCurID, "会员【" + onUser.UserName + "】兑换奖励加速", yeCur);//结算奖金并写入累计字段
                        }
                    }
                }
            }
        }
        #endregion

        #region 业绩提升
        /// <summary>
        /// 业绩提升   wp18080102
        /// </summary>
        /// 业绩提升
        /// <param name="onUserId">会员</param>
        /// <param name="money">金额</param>
        /// <param name="jfCur">积分</param>
        /// <param name="yeCur">余额</param>
        public static void AchievementUp(decimal money, int id)
        {
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().ListWithTracking(x => x.ID == id).FirstOrDefault();
            onUser.Investment = onUser.Investment + money;
            MvcCore.Unity.Get<Data.Service.IUserService>().Update(onUser);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            if (!string.IsNullOrEmpty(onUser.RefereePath))
            {
                //var param2301 = cacheSysParam.SingleAndInit(x => x.ID == 2301);
                //var param2302 = cacheSysParam.SingleAndInit(x => x.ID == 2302);

                string[] ids = onUser.ParentPath.Split(',');
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString())).OrderByDescending(x => x.ID).ToList();
                if (userlist.Count() > 0)
                {
                    int childPlace = onUser.ChildPlace;
                    foreach (var tdjUser in userlist)
                    {
                        var RefereeUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x => x.ID == tdjUser.ID);
                        if (childPlace == 1)
                        {
                            RefereeUser.LeftAchievement = (RefereeUser.LeftAchievement ?? 0) + money;
                            RefereeUser.LeftDpMargin = (RefereeUser.LeftDpMargin ?? 0) + money;
                        }
                        else
                        {
                            RefereeUser.RightAchievement = (RefereeUser.RightAchievement ?? 0) + money;
                            RefereeUser.RightDpMargin = (RefereeUser.RightDpMargin ?? 0) + money;
                        }
                        childPlace = tdjUser.ChildPlace;
                        MvcCore.Unity.Get<Data.Service.IUserService>().Update(RefereeUser);
                        MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                    }
                }
            }
        }
        #endregion



        #region 转出加速设置
        /// <summary>
        /// 转出加速设置   wp18070301
        /// </summary>
        /// 流通奖励加速：转出余额，加快积分释放（积分减少，增加到余额里）
        /// <param name="onUserId">投资会员</param>
        /// <param name="money">投资金额</param>
        /// <param name="cur">拿奖币种</param>
        public static void TransferOutUp(decimal money, Data.User onUser, Data.Currency jfCur, Data.Currency yeCur)
        {
            if (!string.IsNullOrEmpty(onUser.RefereePath))
            {
                var param2401 = cacheSysParam.SingleAndInit(x => x.ID == 2401);
                var param2402 = cacheSysParam.SingleAndInit(x => x.ID == 2402);

                string[] ids = onUser.RefereePath.Split(',');
                int maxDepth = param2402.Value2.ToInt();
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString()) && x.RefereeDepth >= (onUser.RefereeDepth - maxDepth)).ToList();

                if (userlist.Count() > 0)
                {
                    int depth = 0;//代数
                    decimal bonusMoney = 0;//奖金金额
                    int refereeCount = 0;//有效推荐人数   
                    string bonusDesc = "";//奖金描述

                    foreach (var tdjUser in userlist)
                    {
                        depth = onUser.RefereeDepth - tdjUser.RefereeDepth;
                        refereeCount = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.RefereeID == tdjUser.ID && x.IsActivation && !x.IsLock && x.UserLevel >= (int)JN.Data.Enum.UserLevel.Level2).Count();

                        //满足层数，推荐人数内，并且是会员级别
                        if (refereeCount >= param2402.Value.ToInt() && depth <= param2402.Value2.ToInt() && tdjUser.UserLevel >= (int)JN.Data.Enum.UserLevel.Level2)
                        {
                            bonusMoney = money * param2401.Value.ToDecimal();

                            //小于余额则等于余额账户的金额
                            bonusMoney = bonusMoney < (tdjUser.Cur3002 ?? 0) ? bonusMoney : (tdjUser.Cur3002 ?? 0);

                            if (bonusMoney > 0)
                            {
                                //兑换奖励加速
                                var release = new Data.ReleaseDetail();
                                release.Period = 0;
                                release.UID = tdjUser.ID;
                                release.UserName = tdjUser.UserName;
                                release.CurID = jfCur.ID;
                                release.CurName = jfCur.CurrencyName;
                                release.Money = bonusMoney;
                                release.IsSign = true;   //已领取                   
                                release.CreateTime = DateTime.Now;//创建时间     
                                release.EndTime = DateTime.Now;//失效时间
                                MvcCore.Unity.Get<Data.Service.IReleaseDetailService>().Add(release);

                                Wallets.changeWalletNoCommit(tdjUser.ID, -bonusMoney, (int)jfCur.WalletCurID, "会员【" + onUser.UserName + "】流通奖励加速", jfCur);//结算奖金并写入累计字段
                                Wallets.changeWallet(tdjUser.ID, bonusMoney, (int)yeCur.WalletCurID, "会员【" + onUser.UserName + "】流通奖励加速", yeCur);//结算奖金并写入累计字段
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 注册赠送 未用 
        /// <summary>
        /// 注册赠送
        /// </summary>
        /// <param name="UserID">用户ID号</param>
        public static void RegReward(int UserID)
        {
            var onUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.ID == UserID);
            //查找币种
            var curmodel = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => (x.IsRegReward ?? false)).Count() <= 0 ? null : MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => (x.IsRegReward ?? false)).FirstOrDefault();

            if (curmodel.RegRewardParam > 0)
            {
                string description = "注册赠送的" + curmodel.CurrencyName + "(" + curmodel.RegRewardParam.ToDecimal() + ")";
                Wallets.changeWallet(onUser.ID, curmodel.RegRewardParam.ToDecimal(), (int)curmodel.WalletCurID, description, curmodel);

                //写入奖金表
                var param = cacheSysParam.SingleAndInit(x => x.ID == 1104);
                MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Add(new Data.BonusDetail
                {
                    Period = 0,
                    BalanceTime = DateTime.Now,
                    BonusMoney = curmodel.RegRewardParam.ToDecimal(),
                    BonusID = param.ID,
                    BonusName = param.Name,
                    CreateTime = DateTime.Now,
                    Description = description,
                    BonusMoneyCF = 0,
                    BonusMoneyCFBA = 0,
                    IsBalance = true,
                    UID = onUser.ID,
                    UserName = onUser.UserName
                });
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 互助
        #region 确认收款时结算利息及奖金
        /// <summary>
        /// 提供订单完成付款并被确认时进行利息及奖金结算
        /// </summary>
        /// <param name="mModel">匹配实体</param>
        public static void Settlement(Data.Matching mModel)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 10000).ToList();

            var sModel = MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Single(x => x.SupplyNo == mModel.SupplyNo);
            var supplyMatchlist = MvcCore.Unity.Get<Data.Service.IMatchingService>().List(x => (x.SupplyNo == mModel.SupplyNo) && x.Status >= (int)Data.Enum.MatchingStatus.Verified).ToList();
            decimal supplyYCJ = supplyMatchlist.Count() > 0 ? supplyMatchlist.Sum(x => x.MatchAmount) : 0;
            //sModel.IsAccrualEffective = true;
            if (supplyYCJ + mModel.MatchAmount >= sModel.ExchangeAmount) //全部成交
            {
                sModel.Status = (int)Data.Enum.HelpStatus.AllDeal;
                sModel.FinishTime = DateTime.Now;
            }
            else
            {
                sModel.Status = (int)Data.Enum.HelpStatus.PartOfDeal;
            }
            MvcCore.Unity.Get<Data.Service.ISupplyHelpService>().Update(sModel);

            //更新接受单状态
            var aModel = MvcCore.Unity.Get<Data.Service.IAcceptHelpService>().Single(x => x.AcceptNo == mModel.AcceptNo);
            var acceptMatchlist = MvcCore.Unity.Get<Data.Service.IMatchingService>().List(x => x.AcceptNo == mModel.AcceptNo && x.Status >= (int)Data.Enum.MatchingStatus.Verified).ToList();
            decimal acceptYCJ = acceptMatchlist.Count() > 0 ? acceptMatchlist.Sum(x => x.MatchAmount) : 0;
            if (acceptYCJ + mModel.MatchAmount >= aModel.ExchangeAmount)//全部成交
            {
                aModel.AllDealTime = DateTime.Now;
                aModel.Status = (int)Data.Enum.HelpStatus.AllDeal;
            }
            else
                aModel.Status = (int)Data.Enum.HelpStatus.PartOfDeal;
            MvcCore.Unity.Get<Data.Service.IAcceptHelpService>().Update(aModel);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            if (sModel.Status == (int)Data.Enum.HelpStatus.AllDeal) //在整个提供订单都完成交易时才结算
            {
                //UpdateUserWallet(sModel.ExchangeAmount, sModel.SupplyNo, 1102, "本金", "来自排单号为“" + sModel.SupplyNo + "”的本金冻结", sModel.UID, sModel.UID, "Addup1107", false, false, DateTime.Now, sModel.SupplyNo);

                var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == aModel.CoinID);
                Wallets.changeWallet(sModel.UID, sModel.ExchangeAmount, c.WalletCurID ?? 0, "来自排单号为“" + sModel.SupplyNo + "”的本金结算", c);
                //更新用户累计提供
                var updateUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(mModel.SupplyUID);
                updateUser.AddupSupplyAmount = (updateUser.AddupSupplyAmount ?? 0) + (sModel.OrderMoney ?? 0);
                MvcCore.Unity.Get<Data.Service.IUserService>().Update(updateUser);

                //var jfCur = MvcCore.Unity.Get<Data.Service.ICurrencyService>().Single(2);//积分账户需要扣除
                //Bonus1103(sModel.SupplyAmount, sModel.UID, jfCur, c);//推荐奖
                //Bouns1104(sModel.SupplyAmount, sModel.UID, jfCur, c);//见点奖
                //#region 结算奖金
                ////结算提供订单产生的直推奖
                //var bonuslist = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => x.SupplyNo == mModel.SupplyNo && (x.BonusID == 1104) && x.IsBalance == false && (x.IsEffective ?? false)).ToList();
                //foreach (var item in bonuslist)
                //{
                //    Wallets.changeWallet(item.UID, item.BonusMoney, 2002, "来自排单号为“" + item.SupplyNo + "”的" + item.BonusName + "结算");
                //}
                ////结算完成，更新奖金表（奖金部分）
                //MvcCore.Unity.Get<Data.Service.IBonusDetailService>().Update(new Data.BonusDetail(), new Dictionary<string, string>() { { "IsBalance", "1" }, { "BalanceTime", DateTime.Now.ToString() } },"SupplyNo='" + mModel.SupplyNo + "' and BonusID in (1104) and IsBalance=0");
                //MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                //#endregion
            }
            ////更新匹配单最后成交时间
            //var updateMatch = MvcCore.Unity.Get<Data.Service.IMatchingService>().Single(mModel.ID);
            //updateMatch.AllDealTime = DateTime.Now;
            //MvcCore.Unity.Get<Data.Service.IMatchingService>().Update(updateMatch);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }
        #endregion

        #endregion

        #region 注册赠送矿机
        public static void RegRewardMachineExperience(int UserID, int UserLevel, string reg = "")
        {
            logs.WriteGiveLog("赠送会员id：" + UserID + ",赠送等级" + UserLevel + ",体验矿机");
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();
            var onUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.ID == UserID);
            //查找矿机
            var model = MvcCore.Unity.Get<JN.Data.Service.IShopProductService>().Single(1);
            if (model != null)
            {
                //var param = cacheSysParam.SingleAndInit(x => x.ID == UserLevel);
                for (int i = 0; i < 1; i++)
                {
                    var model2 = new Data.MachineOrder
                    {
                        InvestmentNo = GetOrderNumber(),//创建单号
                        InvestmentType = 1,//model.ClassId,//矿机类型
                        UID = onUser.ID,
                        UserName = onUser.UserName,
                        ApplyInvestment = model.RealPrice,
                        SettlementMoney = 0,
                        IsBalance = true,
                        BuyNum = 1,
                        Status = (int)Data.Enum.RechargeSatus.Sucess,//待确认
                        CreateTime = DateTime.Now,
                        AddupInterest = 0,//累计收益
                        ProductID = model.ID,//矿机ID
                        ProductName = model.ProductName,//产皮名称
                        ShopID = model.ShopID,
                        Duration = model.Duration ?? 1,
                        TopBonus = model.TopBonus,
                        WaitExtractIncome = 0,
                        ImageUrl = model.ImageUrl,
                        TimesType = model.TimesType ?? 0,
                        PayWay = "注册赠送",
                        ReserveBoolean1 = true,
                        ReserveInt2 = (int)(model.TopBonus / model.TimesType ?? 0)
                    };
                    MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().Add(model2);
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
                    logs.WriteGiveLog("赠送会员id：" + UserID + ",赠送等级" + UserLevel + ",赠送成功：" + model2.ID);
                    //Bonus.Bouns1103(model2.ID);
                    //AddupUp(model2.ID);
                }
            }
        }

        //生成真实订单号
        public static string GetOrderNumber()
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
        private static bool IsHave(string number)
        {
            return MvcCore.Unity.Get<Data.Service.IMachineOrderService>().List(x => x.InvestmentNo == number).Count() > 0;
        }
        #endregion


        #region 业绩累加  会员升级 KJ190416
        /// <summary>
        /// 业绩累加  会员升级 KJ190416
        /// </summary>
        /// <param name="money"></param>
        /// <param name="id"></param>
        /// <param name="onUser"></param>
        /// <param name="cacheSysParam"></param>
        public static void AchievementUp(decimal money, Data.User onUser, List<Data.SysParam> cacheSysParam, ref List<string> sqlString)
        {
            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List();
            //Dictionary<int, Data.TeamAchievement> dicTeamAchievement = teamAchievementList.ToDictionary(x => x.ID, x => x);//筛选出对应的信息 放入字典中

            var alluserlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => !x.IsLock && x.IsActivation).OrderByDescending(x => x.RefereeDepth).ToList();

            string[] ids = onUser.RefereePath.Split(',');
            var userlist = alluserlist.Where(x => (ids.Contains(x.ID.ToString()) && x.Investment > 0) || x.ID == onUser.ID).OrderByDescending(x => x.RefereeDepth).ToList();//当前会员算入，一起判断是否达到升级

            //Dictionary<int, Data.User> dicUser = userlist.ToDictionary(x => x.ID, x => x);//筛选出对应的信息 放入字典中
            var param1011 = cacheSysParam.SingleAndInit(x => x.ID == 1011).Value.ToInt();
            DateTime dt = DateTime.Now.Date;

            #region 获取参数
            var param1001 = cacheSysParam.SingleAndInit(x => x.ID == 1001);
            var param1002 = cacheSysParam.SingleAndInit(x => x.ID == 1002);
            #endregion

            foreach (var item in userlist)
            {
                decimal TeamAchievement = 0;
                var currentMonth = teamAchievementList.Where(x => System.Data.Entity.SqlServer.SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0 && x.UID == item.ID).ToList();//当月
                if (currentMonth.Count() > 0)
                {
                    TeamAchievement = currentMonth.FirstOrDefault().TeamInvestment += money;
                    string teamAchievement_str = "";
                    if (item.ID == onUser.ID) teamAchievement_str = " ,Investment+=" + money; //如果是自己  就给自己加个人业绩
                    sqlString.Add(string.Format("update [dbo].[TeamAchievement] set TeamInvestment={0}" + teamAchievement_str + " where id={1}", TeamAchievement, currentMonth.FirstOrDefault().ID));
                }
                else
                {
                    decimal cInvestment = 0;
                    if (item.ID == onUser.ID) { cInvestment = money; }//如果是自己  就给自己加个人业绩
                    //增加
                    sqlString.Add(string.Format("INSERT INTO [dbo].[TeamAchievement]([UID],[UserName] ,[Investment],[TeamInvestment],[Type],[CreateTime],[CurrentTime]) VALUES({0} ,'{1}',{2} ,{3},'{4}' ,'{5}','{6}')", item.ID, item.UserName, cInvestment, money, "month", DateTime.Now, DateTime.Now));
                    TeamAchievement = money;
                }

                string user_str = "";
                if (!(item.ReachingTime.Month == DateTime.Now.Month) && TeamAchievement >= param1001.Value.ToDecimal())
                {
                    item.ReachingNum += 1;//一年 团队业绩 达标次数
                    user_str += ",ReachingNum+=1,ReachingTime='" + DateTime.Now + "'";
                }
                bool isUserLevel = false;
                #region 升级判断
                if (item.UserLevel <= 1 && TeamAchievement >= param1002.Value.ToDecimal() && item.ReachingNum >= param1002.Value2.ToInt())//当月 团队业绩 达标
                {
                    item.UserLevel = 2;//高级矿工
                    isUserLevel = true;
                }
                else if (item.UserLevel < 1 && TeamAchievement >= param1001.Value.ToDecimal() && item.ReachingNum >= param1001.Value2.ToInt())
                {
                    item.UserLevel = 1;//矿工
                    isUserLevel = true;
                }

                //3级或者3级以上
                var redUserList = alluserlist.Where(x => x.RefereeID == item.ID).ToList();//我直推的会员
                int fhNumber = 0;//符合条件人数
                foreach (var reduser in redUserList)//查找每个市场 同一条线上有多个高级矿工只能算一个
                {
                    var childUserList = alluserlist.Where(x => ((x.RefereePath + ",").Contains("," + reduser.ID + ",") || x.ID == reduser.ID) && x.UserLevel >= 2).ToList();//伞下会员 包括自己
                    if (childUserList.Count() > 0)
                    {
                        fhNumber += 1;
                    }
                }
                for (var i = 1008; i >= 1003; i--)
                {
                    if (item.UserLevel == i - 1000) break;
                    int paramvalue = cacheSysParam.SingleAndInit(x => x.ID == i).Value.ToInt();
                    if (fhNumber >= paramvalue)
                    {
                        item.UserLevel = i - 1000;//
                        isUserLevel = true;
                        break;
                    }
                }
                #endregion

                if (isUserLevel) user_str += ",UserLevel=" + item.UserLevel;
                if (item.ID == onUser.ID) user_str += " ,Investment+=" + money; //如果是自己  就给自己加个人业绩
                sqlString.Add(string.Format("update [dbo].[User] set TeamInvestment+={0}" + user_str + " where id={1}", money, item.ID));
            }

        }
        #endregion
    }
}