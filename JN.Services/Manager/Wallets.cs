using JN.Data.Service;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.SqlServer;
using System.Linq;
using JN.Data;
using System.Text;

namespace JN.Services.Manager
{
    /// <summary>
    ///钱包管理
    /// </summary>
    public partial class Wallets
    {
        private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();

        /// <summary>
        /// 清除缓存后重新加载
        /// </summary>
        public static void HeavyLoad()
        {
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 6000).ToList();
        }

        #region 用户提币（现金币）
        /// <summary>
        /// 用户提币操作
        /// </summary>
        /// <param name="onUser">用户实体</param>
        /// <param name="drawmoney">提币金额</param>
        /// <param name="remark">提币备注</param>
        /// <param name="BankID">选择银行卡</param>
        public static void doTakeCash(Data.User onUser, decimal drawmoney, string remark, int BankID, Data.Currency cur, string currencyAddress)
        {
            //var bank = MvcCore.Unity.Get<JN.Data.Service.IUserBankCardService>().Single(x => x.UID == onUser.ID && x.ID == BankID);
            //系统参数
            decimal PARAM_POUNDAGEBL = cur.TbPoundage ?? 0; //提币手续费
            decimal actualChangeMoney = drawmoney * (1 - PARAM_POUNDAGEBL);
            decimal poundage = drawmoney * PARAM_POUNDAGEBL;
            string description = "提取现金（手续费：" + poundage + "）";
            //int bankid = bank.BankNameID;
            //写入提币表
            MvcCore.Unity.Get<JN.Data.Service.ITakeCashService>().Add(new Data.TakeCash
            {
                FromAgent = onUser.AgentUser,
                ActualDrawMoney = actualChangeMoney,
                Balance = 0,
                BankCard = currencyAddress,
                BankName = "外币",
                BankOfDeposit = "外币",
                BankUser = onUser.UserName, //bank.BankUser,//onUser.BankUser,
                CreateTime = DateTime.Now,
                DrawMoney = drawmoney,
                Status = (int)JN.Data.Enum.TakeCaseStatus.Wait,
                Poundage = poundage,
                Remark = remark,
                UID = onUser.ID,
                UserName = onUser.UserName,
                CurID = cur.ID,
                CurName = cur.CurrencyName,
                //TakeCashWay = takecashway,
            });
            //写入资金明细表
            changeWallet(onUser.ID, 0 - drawmoney, (int)cur.WalletCurID, description, cur);
        }

        #endregion

        #region 用户提币离线
        /// <summary>
        /// 用户提币离线 （转出xx币种 Name：Lin  Time：2017年7月18日14:32:41）
        /// </summary>
        /// <param name="onUser">操作的用户</param>
        public static void doTakeCashRa(Data.User onUser, decimal drawmoney, string address, JN.Data.Currency c, string tag = null)
        {
            //系统参数
            decimal PARAM_POUNDAGEBL = c.TbPoundage.ToDecimal();// cacheSysParam.SingleAndInit(x => x.ID == 1908).Value2.ToDecimal(); //提币手续费
            decimal actualChangeMoney = drawmoney * (1 - PARAM_POUNDAGEBL);
            decimal poundage = drawmoney * PARAM_POUNDAGEBL;
            string description = c.CurrencyName + "提币（手续费：" + poundage + "）";

            //写入提币表
            MvcCore.Unity.Get<JN.Data.Service.IRaCoinService>().Add(new Data.RaCoin
            {
                ActualDrawMoney = actualChangeMoney,
                WalletAddress = address,
                Tag = tag,
                CreateTime = DateTime.Now,
                DrawMoney = drawmoney,
                Status = (int)JN.Data.Enum.TakeCaseStatus.Wait,
                Poundage = Convert.ToDecimal(poundage),
                Direction = "out",
                CurID = c.ID,
                UID = onUser.ID,
                UserName = onUser.UserName
            });
            //全否为NEO、、提现扣除手续费
            decimal takeCashRaMoney = c.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.NEO ? 0 - (drawmoney + poundage) : 0 - drawmoney;
            //写入资金明细表
            changeWallet(onUser.ID, takeCashRaMoney, (int)c.WalletCurID, description, c);
        }
        #endregion

        #region 用户转帐（虚拟币之间）
        /// <summary>
        /// 用户转帐（虚拟币之间）
        /// </summary>
        public static void doTransfer(Data.User onUser, Data.User recUser, decimal transfermoney, int transCoin, string remark, JN.Data.Currency cmodel)
        {

            //系统参数
            decimal PARAM_POUNDAGEBL = 0;
            if (transCoin == 3001) PARAM_POUNDAGEBL = cacheSysParam.Single(x => x.ID == 3102).Value3.ToDecimal(); //转帐手续费

            decimal realtransfermoney = (transfermoney.ToDecimal() * (1 + PARAM_POUNDAGEBL));
            decimal poundage = transfermoney * PARAM_POUNDAGEBL;
            string description = "用户转帐（转给【" + recUser.UserName + "】";

            //写入转帐表
            MvcCore.Unity.Get<Data.Service.ITransferService>().Add(new Data.Transfer
            {
                ActualTransferMoney = realtransfermoney,
                CreateTime = DateTime.Now,
                Poundage = poundage,
                CurID = cmodel.ID,
                CurName = cmodel.CurrencyName,

                Remark = remark,
                ToUID = recUser.ID,
                ToUserName = recUser.UserName,
                TransferMoney = transfermoney,
                UID = onUser.ID,
                UserName = onUser.UserName
            });

            //写入资金明细表（转帐方）
            changeWalletNoCommit(onUser.ID, 0 - realtransfermoney, transCoin, description, cmodel);

            //写入资金明细表（接收方）
            description = "用户转帐（从【" + onUser.UserName + "】转入";
            changeWalletNoCommit(recUser.ID, transfermoney, transCoin, description, cmodel);

            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();

        }
        #endregion

        #region 币种操作
        /// <summary>
        /// 币种流动数据操作
        /// </summary>
        /// <param name="onUserID">操作用户id</param>
        /// <param name="changeMoney">变动金额</param>
        /// <param name="coinid">变动钱包ID</param>
        /// <param name="description">详细详情</param>
        /// <param name="c">币种实体</param>
        /// <param name="isFrozen">是否是冻结（默认不冻结）</param>
        public static void changeWallet(int onUserID, decimal changeMoney, int coinid, string description, JN.Data.Currency curmodel = null, bool isFrozen = false)
        {
            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(onUserID);
            decimal changeWallet = 0;
            if (!isFrozen)
            {
                switch (coinid)
                {
                    case 2001:
                        changeWallet = onUser.Wallet2001;
                        onUser.Wallet2001 = onUser.Wallet2001 + changeMoney;
                        break;
                    case 2002:
                        changeWallet = onUser.Wallet2002;
                        onUser.Wallet2002 = onUser.Wallet2002 + changeMoney;
                        break;
                    case 2003:
                        changeWallet = onUser.Wallet2003;
                        onUser.Wallet2003 = onUser.Wallet2003 + changeMoney;
                        break;
                    case 2004:
                        changeWallet = onUser.Wallet2004;
                        onUser.Wallet2004 = onUser.Wallet2004 + changeMoney;
                        break;

                    case 3001:
                        changeWallet = (onUser.Cur3001 ?? 0);
                        onUser.Cur3001 = (onUser.Cur3001 ?? 0) + changeMoney;
                        break;
                    case 3002:
                        changeWallet = (onUser.Cur3002 ?? 0);
                        onUser.Cur3002 = (onUser.Cur3002 ?? 0) + changeMoney;
                        if (changeMoney > 0)
                        {
                            onUser.ReserveDecamal2 = (onUser.ReserveDecamal2 ?? 0) + changeMoney;
                        }
                        break;
                    case 3003:
                        changeWallet = (onUser.Cur3003 ?? 0);
                        onUser.Cur3003 = (onUser.Cur3003 ?? 0) + changeMoney;

                        break;
                    case 3004:
                        changeWallet = (onUser.Cur3004 ?? 0);
                        onUser.Cur3004 = (onUser.Cur3004 ?? 0) + changeMoney;

                        break;

                    case 3005:
                        changeWallet = (onUser.Cur3005 ?? 0);
                        onUser.Cur3005 = (onUser.Cur3005 ?? 0) + changeMoney;

                        break;
                    case 3006:
                        changeWallet = (onUser.Cur3006 ?? 0);
                        onUser.Cur3006 = (onUser.Cur3006 ?? 0) + changeMoney;

                        break;
                    case 3007:
                        changeWallet = (onUser.Cur3007 ?? 0);
                        onUser.Cur3007 = (onUser.Cur3007 ?? 0) + changeMoney;
                        break;
                    case 3008:
                        changeWallet = (onUser.Cur3008 ?? 0);
                        onUser.Cur3008 = (onUser.Cur3008 ?? 0) + changeMoney;

                        break;
                    case 3009:
                        changeWallet = (onUser.Cur3009 ?? 0);
                        onUser.Cur3009 = (onUser.Cur3009 ?? 0) + changeMoney;

                        break;
                    case 3010:
                        changeWallet = (onUser.Cur3010 ?? 0);
                        onUser.Cur3010 = (onUser.Cur3010 ?? 0) + changeMoney;

                        break;

                    case 3011:
                        changeWallet = (onUser.Cur3011 ?? 0);
                        onUser.Cur3011 = (onUser.Cur3011 ?? 0) + changeMoney;

                        break;

                    case 3012:
                        changeWallet = (onUser.Cur3012 ?? 0);
                        onUser.Cur3012 = (onUser.Cur3012 ?? 0) + changeMoney;
                        break;

                    case 3013:
                        changeWallet = (onUser.Cur3013 ?? 0);
                        onUser.Cur3013 = (onUser.Cur3013 ?? 0) + changeMoney;

                        break;

                    case 3014:
                        changeWallet = (onUser.Cur3014 ?? 0);
                        onUser.Cur3014 = (onUser.Cur3014 ?? 0) + changeMoney;

                        break;
                    case 3015:
                        changeWallet = (onUser.Cur3015 ?? 0);
                        onUser.Cur3015 = (onUser.Cur3015 ?? 0) + changeMoney;
                        break;
                }
            }
            else
            {

                switch (coinid)
                {
                    case 2001:
                        changeWallet = onUser.Wallet2001;
                        onUser.Wallet2001 = onUser.Wallet2001 + changeMoney;
                        break;
                    case 2002:
                        changeWallet = onUser.Wallet2002;
                        onUser.Wallet2002 = onUser.Wallet2002 + changeMoney;
                        break;
                    case 2003:
                        changeWallet = onUser.Wallet2003;
                        onUser.Wallet2003 = onUser.Wallet2003 + changeMoney;
                        break;
                    case 2004:
                        changeWallet = onUser.Wallet2004;
                        onUser.Wallet2004 = onUser.Wallet2004 + changeMoney;
                        break;
                    case 3001:
                        changeWallet = (onUser.CurFro3001 ?? 0);//冻结
                        onUser.CurFro3001 = (onUser.CurFro3001 ?? 0) + changeMoney;
                        break;
                    case 3002:
                        changeWallet = (onUser.CurFro3002 ?? 0);//冻结
                        onUser.CurFro3002 = (onUser.CurFro3002 ?? 0) + changeMoney;
                        break;
                    case 3003:
                        changeWallet = (onUser.CurFro3003 ?? 0);//冻结
                        onUser.CurFro3003 = (onUser.CurFro3003 ?? 0) + changeMoney;
                        break;
                    case 3004:
                        changeWallet = (onUser.CurFro3004 ?? 0);//冻结
                        onUser.CurFro3004 = (onUser.CurFro3004 ?? 0) + changeMoney;
                        break;
                    case 3005:
                        changeWallet = (onUser.CurFro3005 ?? 0);//冻结
                        onUser.CurFro3005 = (onUser.CurFro3005 ?? 0) + changeMoney;
                        break;
                    case 3006:
                        changeWallet = (onUser.CurFro3006 ?? 0);//冻结
                        onUser.CurFro3006 = (onUser.CurFro3006 ?? 0) + changeMoney;
                        break;
                    case 3007:
                        changeWallet = (onUser.CurFro3007 ?? 0);//冻结
                        onUser.CurFro3007 = (onUser.CurFro3007 ?? 0) + changeMoney;
                        break;
                    case 3008:
                        changeWallet = (onUser.CurFro3008 ?? 0);//冻结
                        onUser.CurFro3008 = (onUser.CurFro3008 ?? 0) + changeMoney;
                        break;
                    case 3009:
                        changeWallet = (onUser.CurFro3009 ?? 0);//冻结
                        onUser.CurFro3009 = (onUser.CurFro3009 ?? 0) + changeMoney;
                        break;
                    case 3010:
                        changeWallet = (onUser.CurFro3010 ?? 0);//冻结
                        onUser.CurFro3010 = (onUser.CurFro3010 ?? 0) + changeMoney;
                        break;
                    case 3011:
                        changeWallet = (onUser.CurFro3011 ?? 0);//冻结
                        onUser.CurFro3011 = (onUser.CurFro3011 ?? 0) + changeMoney;
                        break;
                    case 3012:
                        changeWallet = (onUser.CurFro3012 ?? 0);//冻结
                        onUser.CurFro3012 = (onUser.CurFro3012 ?? 0) + changeMoney;
                        break;
                    case 3013:
                        changeWallet = (onUser.CurFro3013 ?? 0);//冻结
                        onUser.CurFro3013 = (onUser.CurFro3013 ?? 0) + changeMoney;
                        break;
                    case 3014:
                        changeWallet = (onUser.CurFro3014 ?? 0);//冻结
                        onUser.CurFro3014 = (onUser.CurFro3014 ?? 0) + changeMoney;
                        break;
                    case 3015:
                        changeWallet = (onUser.CurFro3015 ?? 0);//冻结
                        onUser.CurFro3015 = (onUser.CurFro3015 ?? 0) + changeMoney;
                        break;
                }

            }

            //写入明细
            MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().Add(new Data.WalletLog
            {
                ChangeMoney = changeMoney,
                Balance = changeWallet + changeMoney,
                CoinID = (curmodel == null ? coinid : (curmodel.WalletCurID ?? 0)),
                CoinName = (curmodel == null ? cacheSysParam.Single(x => x.ID == coinid).Name : curmodel.CurrencyName),
                CreateTime = DateTime.Now,
                Description = description,
                UID = onUser.ID,
                UserName = onUser.UserName
            });
            //更新用户钱包
            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }

        /// <summary>
        /// 币种流动数据操作
        /// </summary>
        /// <param name="onUserID">操作会员id</param>
        /// <param name="changeMoney">变动金额</param>
        /// <param name="coinid">变动钱包ID</param>
        /// <param name="description">详细详情</param>
        /// <param name="c">币种实体</param>
        /// <param name="bonusid">奖金id</param>
        public static void changeWalletNoCommitAddup(int onUserID, decimal changeMoney, int bonusid, string description, JN.Data.Currency curmodel)
        {
            var cacheSysParam = new SysParamService(new SysDbFactory()).ListCache("sysparams", x => x.ID < 8000).ToList();

            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(onUserID);
            decimal changeWallet = 0;
            switch (curmodel.WalletCurID)
            {
                case 3001:
                    changeWallet = (onUser.Cur3001 ?? 0);
                    onUser.Cur3001 = (onUser.Cur3001 ?? 0) + changeMoney;
                    break;
                case 3002:
                    changeWallet = (onUser.Cur3002 ?? 0);
                    onUser.Cur3002 = (onUser.Cur3002 ?? 0) + changeMoney;
                    if (changeMoney > 0)
                    {
                        onUser.ReserveDecamal2 = (onUser.ReserveDecamal2 ?? 0) + changeMoney;
                    }
                    break;
                case 3003:
                    changeWallet = (onUser.Cur3003 ?? 0);
                    onUser.Cur3003 = (onUser.Cur3003 ?? 0) + changeMoney;

                    break;
                case 3004:
                    changeWallet = (onUser.Cur3004 ?? 0);
                    onUser.Cur3004 = (onUser.Cur3004 ?? 0) + changeMoney;

                    break;

                case 3005:
                    changeWallet = (onUser.Cur3005 ?? 0);
                    onUser.Cur3005 = (onUser.Cur3005 ?? 0) + changeMoney;

                    break;
                case 3006:
                    changeWallet = (onUser.Cur3006 ?? 0);
                    onUser.Cur3006 = (onUser.Cur3006 ?? 0) + changeMoney;

                    break;
                case 3007:
                    changeWallet = (onUser.Cur3007 ?? 0);
                    onUser.Cur3007 = (onUser.Cur3007 ?? 0) + changeMoney;
                    break;
                case 3008:
                    changeWallet = (onUser.Cur3008 ?? 0);
                    onUser.Cur3008 = (onUser.Cur3008 ?? 0) + changeMoney;

                    break;
                case 3009:
                    changeWallet = (onUser.Cur3009 ?? 0);
                    onUser.Cur3009 = (onUser.Cur3009 ?? 0) + changeMoney;

                    break;
                case 3010:
                    changeWallet = (onUser.Cur3010 ?? 0);
                    onUser.Cur3010 = (onUser.Cur3010 ?? 0) + changeMoney;

                    break;

                case 3011:
                    changeWallet = (onUser.Cur3011 ?? 0);
                    onUser.Cur3011 = (onUser.Cur3011 ?? 0) + changeMoney;

                    break;

                case 3012:
                    changeWallet = (onUser.Cur3012 ?? 0);
                    onUser.Cur3012 = (onUser.Cur3012 ?? 0) + changeMoney;
                    break;

                case 3013:
                    changeWallet = (onUser.Cur3013 ?? 0);
                    onUser.Cur3013 = (onUser.Cur3013 ?? 0) + changeMoney;

                    break;

                case 3014:
                    changeWallet = (onUser.Cur3014 ?? 0);
                    onUser.Cur3014 = (onUser.Cur3014 ?? 0) + changeMoney;

                    break;
                case 3015:
                    changeWallet = (onUser.Cur3015 ?? 0);
                    onUser.Cur3015 = (onUser.Cur3015 ?? 0) + changeMoney;
                    break;
            }

            switch (bonusid)
            {
                case 1101:
                    onUser.Addup1101 = (onUser.Addup1101 ?? 0) + changeMoney;
                    break;
                case 1102:
                    onUser.Addup1102 = (onUser.Addup1102 ?? 0) + changeMoney;
                    break;
                case 1103:
                    onUser.Addup1103 = (onUser.Addup1103 ?? 0) + changeMoney;
                    break;
                case 1104:
                    onUser.Addup1104 = (onUser.Addup1104 ?? 0) + changeMoney;
                    break;
                case 1105:
                    onUser.Addup1105 = (onUser.Addup1105 ?? 0) + changeMoney;
                    break;
                case 1106:
                    onUser.Addup1106 = (onUser.Addup1106 ?? 0) + changeMoney;
                    break;
                case 1107:
                    onUser.Addup1107 = (onUser.Addup1107 ?? 0) + changeMoney;
                    break;
                case 1108:
                    onUser.Addup1802 = (onUser.Addup1802 ?? 0) + changeMoney;
                    break;
            }


            //写入明细
            MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().Add(new Data.WalletLog
            {
                ChangeMoney = changeMoney,
                Balance = changeWallet + changeMoney,
                CoinID = curmodel.WalletCurID ?? 0,
                CoinName = curmodel.CurrencyName,
                CreateTime = DateTime.Now,
                Description = description,
                UID = onUser.ID,
                UserName = onUser.UserName
            });

            //更新用户钱包
            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);
            //MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }
        #endregion

        #region 币种操作没有Commit
        /// <summary>
        /// 直接对币种进行操作
        /// </summary>
        /// <param name="onUser">操作对像</param>
        /// <param name="changeMoney">变更金额</param>
        /// <param name="changeCoin">变更币种</param>
        /// <param name="description">备注</param>
        public static void changeWalletNoCommit(int onUserID, decimal changeMoney, int coinid, string description, JN.Data.Currency c = null)
        {
            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(onUserID);
            decimal changeWallet = 0;
            switch (coinid)
            {
                case 2001:
                    changeWallet = onUser.Wallet2001;
                    onUser.Wallet2001 = onUser.Wallet2001 + changeMoney;
                    if (description.Contains("Fc生态系统活跃度生成能量")) onUser.Addup1106 = (onUser.Addup1106 ?? 0) + changeMoney;
                    break;
                case 2002:
                    changeWallet = onUser.Wallet2002;
                    onUser.Wallet2002 = onUser.Wallet2002 + changeMoney;
                    break;
                case 2003:
                    changeWallet = onUser.Wallet2003;
                    onUser.Wallet2003 = onUser.Wallet2003 + changeMoney;
                    break;
                case 2004:
                    changeWallet = onUser.Wallet2004;
                    onUser.Wallet2004 = onUser.Wallet2004 + changeMoney;
                    break;

                case 3001:
                    changeWallet = (onUser.Cur3001 ?? 0);
                    onUser.Cur3001 = (onUser.Cur3001 ?? 0) + changeMoney;
                    break;
                case 3002:
                    changeWallet = (onUser.Cur3002 ?? 0);
                    onUser.Cur3002 = (onUser.Cur3002 ?? 0) + changeMoney;
                    if (changeMoney > 0)
                    {
                        onUser.ReserveDecamal2 = (onUser.ReserveDecamal2 ?? 0) + changeMoney;
                    }
                    break;
                case 3003:
                    changeWallet = (onUser.Cur3003 ?? 0);
                    onUser.Cur3003 = (onUser.Cur3003 ?? 0) + changeMoney;
                    break;
                case 3004:
                    changeWallet = (onUser.Cur3004 ?? 0);
                    onUser.Cur3004 = (onUser.Cur3004 ?? 0) + changeMoney;
                    break;

                case 3005:
                    changeWallet = (onUser.Cur3005 ?? 0);
                    onUser.Cur3005 = (onUser.Cur3005 ?? 0) + changeMoney;
                    break;
                case 3006:
                    changeWallet = (onUser.Cur3006 ?? 0);
                    onUser.Cur3006 = (onUser.Cur3006 ?? 0) + changeMoney;
                    break;
                case 3007:
                    changeWallet = (onUser.Cur3007 ?? 0);
                    onUser.Cur3007 = (onUser.Cur3007 ?? 0) + changeMoney;
                    break;
                case 3008:
                    changeWallet = (onUser.Cur3008 ?? 0);
                    onUser.Cur3008 = (onUser.Cur3008 ?? 0) + changeMoney;
                    break;
                case 3009:
                    changeWallet = (onUser.Cur3009 ?? 0);
                    onUser.Cur3009 = onUser.Cur3009 ?? 0 + changeMoney;
                    break;
                case 3010:
                    changeWallet = (onUser.Cur3010 ?? 0);
                    onUser.Cur3010 = onUser.Cur3010 ?? 0 + changeMoney;
                    break;

                case 3011:
                    changeWallet = (onUser.Cur3011 ?? 0);
                    onUser.Cur3011 = (onUser.Cur3011 ?? 0) + changeMoney;
                    break;

                case 3012:
                    changeWallet = (onUser.Cur3012 ?? 0);
                    onUser.Cur3012 = (onUser.Cur3012 ?? 0) + changeMoney;
                    break;

                case 3013:
                    changeWallet = (onUser.Cur3013 ?? 0);
                    onUser.Cur3013 = (onUser.Cur3013 ?? 0) + changeMoney;
                    break;

                case 3014:
                    changeWallet = (onUser.Cur3014 ?? 0);
                    onUser.Cur3014 = (onUser.Cur3014 ?? 0) + changeMoney;
                    break;
                case 3015:
                    changeWallet = (onUser.Cur3015 ?? 0);
                    onUser.Cur3015 = (onUser.Cur3015 ?? 0) + changeMoney;
                    break;
            }

            //写入明细
            MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().Add(new Data.WalletLog
            {
                ChangeMoney = changeMoney,
                Balance = changeWallet + changeMoney,
                CoinID = c == null ? coinid : (c.WalletCurID ?? 0),
                CoinName = c == null ? cacheSysParam.Single(x => x.ID == coinid).Name : c.CurrencyName,
                CreateTime = DateTime.Now,
                Description = description,
                UID = onUser.ID,
                UserName = onUser.UserName
            });
            //更新用户钱包
            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);

        }
        #endregion

        #region 币种互兑
        /// <summary>
        /// 互兑
        /// </summary>
        public static void doExchange(int onUserID, decimal drawmoney, int fromCoin, int toCoin)
        {
            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(onUserID);
            ////币种
            decimal PARAM_POUNDAGEBL = cacheSysParam.Single(x => x.ID == 1904).Value2.ToDecimal(); //手续费

            decimal actualChangeMoney = drawmoney * (1 - PARAM_POUNDAGEBL);
            decimal poundage = drawmoney * PARAM_POUNDAGEBL;
            string description = "能量转换货币";
            Wallets.changeWallet(onUser.ID, 0 - actualChangeMoney, fromCoin, description);
            Wallets.changeWallet(onUser.ID, (actualChangeMoney / 100), toCoin, description);
        }
        #endregion

        #region 解冻钱包 （每周解冻5%的额度，每个币种）
        /// <summary>
        /// 解冻钱包 （每周解冻5%的额度，每个币种）
        /// </summary>
        public static void ThawWallet()
        {
            //查找全部用户
            var userlist = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => (x.IsAuthentication ?? false)).ToList();
            var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => (x.IsFreeze ?? false)).ToList();//获取划入冻结的币种
            foreach (var item in userlist)
            {
                //判断每个冻结钱包的余额（15个钱包）
                foreach (var citem in c)
                {
                    decimal money = Users.WalletCurFroCount(citem.ID, item.ID);
                    decimal chengMoeny = money * (citem.ReleaseAmount ?? 0);//得到比例
                    if (chengMoeny > 0)
                        Wallets.changeWallet(item.ID, chengMoeny, (int)citem.WalletCurID, "来自【" + DateTime.Now + "】释放", citem, true);//放入主钱包
                }
            }

        }
        #endregion

        #region 检查到期未付款
        /// <summary>
        /// 检查到期未付款
        /// </summary>
        public static void Nonpayment()
        {
            SysDbFactory SysDbFactory = new SysDbFactory();
            UserService UserService = new UserService(SysDbFactory);
            var list = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().ListWithTracking(x => x.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder && x.PaymentTime < DateTime.Now).ToList();
            try
            {
                foreach (var item in list)
                {
                    //直接取消订单，冻结购买用户
                    //var buyuser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.ID == item.BuyUID);
                    //buyuser.IsLock = true;
                    //string LockReason = "订单号为" + item.OrderID + "超时未付款冻结";
                    //item.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.Cancel;
                    //MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(buyuser);
                    //string buyuserSql = "UPDATE [User] set IsLock=1,LockReason=" + LockReason + "  where id=" + item.BuyUID;

                    ////批量处理用户的信息
                    //DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                    //MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().ExecuteSQL(buyuserSql.ToString(), dbparam);

                    var Advertise = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseService>().ListWithTracking
                        ().Single(x => x.ID == item.AdID);
                    var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == item.CurID);

                    //返回金额给卖家
                    if (Advertise.Direction == 0)
                    {
                        Advertise.HaveQuantity -= item.Quantity;//返还余额
                        //Advertise.HaveQuantity += (item.Quantity + item.BuyPoundage);//返还余额
                        if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed) Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Underway;//如果订单是已完成状态，改成正在进行
                    }
                    else
                    {
                        Advertise.HaveQuantity -= item.Quantity;//返还余额
                        if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed) Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Underway;//如果订单是已完成状态，改成正在进行
                        Wallets.changeWallet(item.SellUID, (item.Quantity + item.SellPoundage), (int)c.WalletCurID, "超时未付款：取消订单[" + item.OrderID + "]返还", c);
                    }
                    //买入用户未打款
                    //int credit = cacheSysParam.SingleAndInit(x => x.ID == 3004).Value.ToInt();//扣除信用值
                    var buyUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(x => x.ID == item.BuyUID);

                    //int userCredit = buyUser.CreditValue - credit;//用户信用
                    //buyUser.CreditValue = userCredit <= 0 ? 0 : userCredit;//扣除信用值(不会小于0)
                    if (buyUser.IsLock != true)
                    {
                        buyUser.IsLock = true;
                        buyUser.LockReason = "到期未付款冻结(" + buyUser.CreditValue + ")";//更改此处文字需在解冻处做相应更改
                        buyUser.LockTime = DateTime.Now;
                        MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(buyUser);
                    }

                    item.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.Cancel;
                    MvcCore.Unity.Get<JN.Data.Service.IAdvertiseService>().Update(Advertise);
                    MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().Update(item);
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        #endregion

        #region 检查到期未确认收货
        /// <summary>
        /// 检查到期未确认收货
        /// 超时不用自动成交  只是扣信用值  然后后台去点击成交或者是未确认的会员去点击确认
        /// </summary>
        public static void ConfirmReceiptTimeout()
        {
            SysDbFactory SysDbFactory = new SysDbFactory();
            UserService UserService = new UserService(SysDbFactory);
            var list = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().ListWithTracking(x => x.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PendingPayment && (x.DeliveryTime ?? DateTime.Now) < DateTime.Now).ToList();
            foreach (var item in list)
            {
                item.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.TimeoutUnConfirmed;//订单超时 //(int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived;//直接已收货
                MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().Update(item);

                //卖出用户未确认
                //int credit = cacheSysParam.SingleAndInit(x => x.ID == 3004).Value.ToInt();//扣除信用值
                var sellUser = UserService.Single(x => x.ID == item.SellUID);

                //int userCredit = sellUser.CreditValue - credit;//用户信用
                //sellUser.CreditValue = userCredit <= 0 ? 0 : userCredit;//扣除信用值(不会小于0)
                //if (sellUser.CreditValue == 0)
                if (sellUser.IsLock != true)
                {
                    sellUser.IsLock = true;
                    sellUser.LockReason = "到期未确认收货冻结";//更改此处文字需在解冻处做相应更改
                    sellUser.LockTime = DateTime.Now;
                    UserService.Update(sellUser);
                }


                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            }
        }

        private static decimal getcurrentprice(Currency c)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 检查是否有待确认的转入  检查wp17112701
        public static void checkOtherTransfer(Data.User Umodel)
        {
            var cmodelList = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List(x => x.TranSwitch && !x.IsICO).OrderByDescending(x => x.ID).ToList();
            if (cmodelList.Count() > 0)
            {
                var cmodel = cmodelList.FirstOrDefault();
                var list = MvcCore.Unity.Get<JN.Data.Service.IOtherTransferService>().ListWithTracking(x => x.UID == Umodel.ID && x.doType == (int)JN.Data.Enum.OtherTransfersType.IN && x.Status == (int)JN.Data.Enum.OtherTransfersStatus.Wait).ToList();
                if (list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        item.Status = (int)JN.Data.Enum.OtherTransfersStatus.Sucess;
                        MvcCore.Unity.Get<JN.Data.Service.IOtherTransferService>().Update(item);

                        Wallets.changeWallet(Umodel.ID, item.ChangeMoney, cmodel.WalletCurID ?? 0, "由记录" + item.No + "转入", cmodel);
                    }

                }
            }

        }
        #endregion

        #region 获取价格
        /// <summary>
        /// 获取价格
        /// </summary>
        public static void GetPriceList()
        {
            var curAllList = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().List().ToList();
            var curList = curAllList.Where(x => x.IsICO).ToList();
            var curPriceList = new List<Data.Currency>();

            var curPrice = new Data.Currency();
            var price = JN.Services.Tool.SMSHelper.GetPrice("https://api.coinmarketcap.com/v1/ticker/" + "bitcoin", "usd");
            curPrice.TranPrice = price.ToDecimal();
            curPrice.EnSigns = "BTC";
            curPrice.English = "bitcoin";
            curPrice.CurrencyLogo = "/Theme/APP/img/btc.png";
            curPriceList.Add(curPrice);

            var curPrice2 = new Data.Currency();
            var price2 = JN.Services.Tool.SMSHelper.GetPrice("https://api.coinmarketcap.com/v1/ticker/" + "ethereum", "usd");
            curPrice2.TranPrice = price2.ToDecimal();
            curPrice2.EnSigns = "ETH";
            curPrice2.English = "ethereum";
            curPrice2.CurrencyLogo = "/Theme/APP/img/eth.png";
            curPriceList.Add(curPrice2);

            var curPrice3 = new Data.Currency();
            var price3 = JN.Services.Tool.SMSHelper.GetPrice("https://api.coinmarketcap.com/v1/ticker/" + "tether", "usd");
            curPrice3.TranPrice = price3.ToDecimal();
            curPrice3.EnSigns = "USDT";
            curPrice3.English = "tether";
            curPrice3.CurrencyLogo = "/Theme/APP/img/usdt.png";
            curPriceList.Add(curPrice3);

            //写入缓存
            MvcCore.Extensions.CacheExtensions.SetCache("curPriceList", curPriceList, MvcCore.Extensions.CacheTimeType.ByMinutes, 5);
        }
        #endregion

        #region 检测是否记账
        public static void NoBookkeepingt()
        {

            var list = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => x.Cur3001 > 0 && x.Cur3005 > 0).ToList();

            if (list.Count > 0)
            {
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                StringBuilder sql = new StringBuilder();//SQLBuilder实例
                Dictionary<int, Data.User> dicUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List().ToList().ToDictionary(d => d.ID, d => d);
                var cModel = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3001);

                foreach (var item in list)
                {
                    decimal changMoney = 0;
                    string description = "矿机月费记账扣除:";
                    if ((item.Cur3001 ?? 0) >= (item.Cur3005 ?? 0))
                    {
                        changMoney = (dicUser[item.ID].Cur3005 ?? 0);
                    }
                    else
                    {
                        changMoney = (dicUser[item.ID].Cur3001 ?? 0);
                    }
                    dicUser[item.ID].Cur3001 = (dicUser[item.ID].Cur3001 ?? 0) - changMoney;
                    dicUser[item.ID].Cur3005 = (dicUser[item.ID].Cur3005 ?? 0) - changMoney;

                    var balance = dicUser[item.ID].Cur3001 ?? 0;
                    var balance3005 = dicUser[item.ID].Cur3005 ?? 0;
                    dicUser[item.ID].ReserveInt1 = 1;//更改标记

                    description += changMoney + "剩余记账:" + balance3005;
                    WalletLogList.Add(new Data.WalletLog
                    {
                        ChangeMoney = -changMoney,
                        Balance = balance,
                        CoinID = 3001,
                        CoinName = cModel.CurrencyName,
                        CreateTime = DateTime.Now,
                        Description = description,
                        UID = item.ID,
                        UserName = item.UserName,
                    });
                    WalletLogList.Add(new Data.WalletLog
                    {
                        ChangeMoney = -changMoney,
                        Balance = balance3005,
                        CoinID = 3005,
                        CoinName = "记账钱包",
                        CreateTime = DateTime.Now,
                        Description = description,
                        UID = item.ID,
                        UserName = item.UserName,
                    });
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 1)
                    {
                        sql.AppendFormat(" update [User] set Cur3001={0},Cur3005={1} where ID={2} ", (item.Value.Cur3001 ?? 0), (item.Value.Cur3005 ?? 0), item.Value.ID);
                    }
                }
                if (sql.Length > 0)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                        int count = MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().ExecuteSQL(sql.ToString(), s);
                        sql.Clear();

                        //导入
                        MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);

                        ts.Complete();
                    }
                }

            }
        }

        #endregion
        
    }
}