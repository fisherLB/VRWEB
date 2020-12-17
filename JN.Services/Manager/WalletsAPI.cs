using JN.Data.Service;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.SqlServer;
using System.Linq;
using JN.Data;
using System.Text;
using static System.HttpHelper;
using JN.Data.Extensions;
using MvcCore.Extensions;
using JN.Services.Tool;
using System.Web;
using System.Data.Entity.Validation;
using System.IO;
using System.Threading;

namespace JN.Services.Manager
{
    /// <summary>
    ///钱包管理
    /// </summary>
    public partial class WalletsAPI
    {
        #region 钱包API操作
        private static string userNameSalt = "";//用户名加盐区分系统（以太坊）

        #region 获取离线地址
        /// <summary>
        /// 获取离线地址//没有则创建
        /// </summary>
        /// <param name="Umodel">用户实例</param>
        /// <param name="Currency">币种实例</param>
        /// <returns></returns>
        public static Result GetWalletAddress(JN.Data.User Umodel, JN.Data.Currency Currency)
        {
            Result result = new Result();
            try
            {
                string Racoin = JN.Services.Manager.WalletsAPI.GetWalletAddress((Currency.WalletCurID??0), Umodel);
                if (string.IsNullOrEmpty(Racoin))
                {
                    string CacheKey = Umodel.UserName + "CreateWalletAddress" + Currency.ID;
                    if (CacheExtensions.CheckCache(CacheKey))//查找缓存是否存在
                    {
                        throw new CustomException.CustomException("获取地址过于频繁");
                    }

                    if (string.IsNullOrEmpty(Currency.WalletKey) || Currency.WalletKey == "#") throw new JN.Services.CustomException.CustomException("未开放充值");


                    CacheExtensions.SetCache(CacheKey, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//每1分钟   

                    ReturnResult<string> result2 = new ReturnResult<string>();
                    if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Bitcoin)
                    {
                        #region 比特币
                        result2 = BitcionHelper.CreateNewAddressGetSqlServer(Currency.WalletKey, userNameSalt + Umodel.UserName, new Uri(HttpContext.Current.Request.Url, "/api/Bitcoin/AddressOfAmountChange").ToString());
                        //生成成功，保存地址并生成订单（要严格控制一个订单一个地址）
                        if (result2.Status == ReturnResultStatus.Succeed.GetShortValue())
                        {
                            //充值地址：result.Data
                            WalletsAPI.SetWalletAddressETH(Currency, Umodel, result2.Data);
                            //Wallets.SetWalletAddress(Currency, Umodel, result2.Data);
                            result.Message = result2.Data;
                            result.Status = 200;
                        }
                        else
                        {
                            result.Status = result2.Status;
                            result.Message = result2.Message;
                        }
                        #endregion
                    }
                    else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ethereum || Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EthereumContract)
                    {
                        #region 以太坊或代币
                        result2 = EthHelper.CreateNewAddress(Currency.WalletKey, userNameSalt + Umodel.UserName);//地址动态化 by Ann 2017/12/25
                        if (result2.Status == ReturnResultStatus.Succeed.GetShortValue())
                        {
                            WalletsAPI.SetWalletAddressETH(Currency, Umodel, result2.Data);
                            result.Message = result2.Data;
                            result.Status = 200;
                        }
                        else
                        {
                            result.Status = result2.Status;
                            result.Message = result2.Message;
                        }
                        #endregion
                    }
                    else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.NEO)
                    {
                        #region NEO
                        result2 = NEOHelper.CreateNewAddress(Currency.WalletKey, userNameSalt + Umodel.UserName);//地址动态化 by Ann 2017/12/25
                                                                                                                 //生成成功，保存地址并生成订单（要严格控制一个订单一个地址）
                        if (result2.Status == ReturnResultStatus.Succeed.GetShortValue())
                        {
                            //充值地址：result.Data
                            WalletsAPI.SetWalletAddress(Currency, Umodel, result2.Data);
                            result.Message = result2.Data;
                            result.Status = 200;
                        }
                        else
                        {
                            result.Status = result2.Status;
                            result.Message = result2.Message;
                        }
                        #endregion
                    }
                    else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ripple)
                    {
                        #region XRP
                        result.Message = XrpHelper.CreateNewAddress(Umodel.ID);
                        result.Status = 200;
                        WalletsAPI.SetWalletAddress(Currency, Umodel, result.Message);
                        #endregion
                    }
                    else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EOS || Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EOSContract)
                    {
                        #region EOS
                        result.Message = EosHelper.CreateNewAddress(Umodel.ID);
                        result.Status = 200;
                        WalletsAPI.SetWalletAddress(Currency, Umodel, result.Message);
                        #endregion
                    }
                    else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.USDT)
                    {
                        #region USDT
                        result2 = BitcionHelper.CreateNewAddressGetSqlServer(Currency.WalletKey, userNameSalt + Umodel.UserName, new Uri(HttpContext.Current.Request.Url, "/api/Bitcoin/AddressOfAmountChange").ToString());
                        //生成成功，保存地址并生成订单（要严格控制一个订单一个地址）
                        if (result2.Status == ReturnResultStatus.Succeed.GetShortValue())
                        {
                            //充值地址：result.Data
                            WalletsAPI.SetWalletAddressETH(Currency, Umodel, result2.Data);
                            //Wallets.SetWalletAddress(Currency, Umodel, result2.Data);
                            result.Message = result2.Data;
                            result.Status = 200;
                        }
                        else
                        {
                            result.Status = result2.Status;
                            result.Message = result2.Message;
                        }
                        #endregion
                    }
                }
                else
                {
                    result.Message = Racoin;
                    result.Status = 200;
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
        #endregion

        #region 写入转入表并获取转入充值记录
        /// <summary>
        /// 写入转入表并获取转入充值记录
        /// </summary>
        /// <param name="Umodel">用户实例</param>
        /// <param name="Currency">币种实例</param>
        /// <returns></returns>
        public static ReturnResult<List<ResponseListTransactions>> GetListTransactions(JN.Data.User Umodel, JN.Data.Currency Currency, IRaCoinService RaCoinService, ISysDBTool SysDBTool)
        {
            ReturnResult<List<ResponseListTransactions>> result = new ReturnResult<List<ResponseListTransactions>>();
            try
            {
                result.Status = 500;
                result.Message = "无法获取到新的记录";
                if (Currency == null) throw new JN.Services.CustomException.CustomException("获取失败，查无此币种");
                if (string.IsNullOrEmpty(Currency.WalletKey) || Currency.WalletKey == "#") throw new JN.Services.CustomException.CustomException("获取失败，未设置钱包API");
                //string CacheKey = Umodel.UserName + "GetListTransactions" + Currency.ID;
                //if (CacheExtensions.CheckCache(CacheKey))//查找缓存是否存在
                //{
                //    throw new CustomException.CustomException("同步数据过于频繁");
                //}
                //CacheExtensions.SetCache(CacheKey, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//每1分钟   

                if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Bitcoin)
                {
                    #region 比特币
                    result = BitcionHelper.GetListTransactionsGetSqlServer(Currency.WalletKey, userNameSalt + Umodel.UserName);
                    foreach (var item in result.Data.Where(x => x.category == "receive"))
                    {
                        if (RaCoinService.List(x => x.Txid == item.txid && x.UserName == Umodel.UserName && x.CurID == Currency.ID && x.Direction == "in").Count() <= 0)
                        {
                            //写入提现表
                            RaCoinService.Add(new Data.RaCoin
                            {
                                ActualDrawMoney = item.amount.ToDecimal(),
                                WalletAddress = item.address,
                                CreateTime = DateTime.Now,
                                DrawMoney = item.amount.ToDecimal(),
                                Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                Poundage = item.fee.ToDecimal(),
                                Direction = "in",
                                UID = Umodel.ID,
                                UserName = Umodel.UserName,
                                CurID = Currency.ID,
                                ReserveStr1 = item.timereceived.ToString(),
                                Txid = item.txid,
                                ReserveInt = item.confirmations,
                            });
                            Wallets.changeWallet(Umodel.ID, item.amount.ToDecimal(), (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                            result.Status = 200;//有充值收入
                        }
                    }
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ethereum)
                {
                    #region 以太坊
                    string Racoin = JN.Services.Manager.WalletsAPI.GetWalletAddress((int)Currency.WalletCurID, Umodel);
                    if (string.IsNullOrEmpty(Racoin))
                    {
                        throw new JN.Services.CustomException.CustomException("您还未生成离线地址，请先生成离线地址");
                    }
                    else
                    {
                        var trans = EthHelper.GetEthTransactions(Racoin);
                        if (trans.Data.result.Count > 0)
                        {
                            foreach (var item in trans.Data.result.Where(x => x.to.ToLower() == Racoin.ToLower() && x.value.ToDouble() > 0))
                            {
                                if (RaCoinService.List(x => x.Txid == item.hash && x.CurID == Currency.ID && x.Direction == "in").Count() <= 0)
                                {
                                    decimal drawMoney = (item.value.ToDecimal() / 1000000000000000000);
                                    if (drawMoney > 0)
                                    {
                                        //写入表
                                        RaCoinService.Add(new Data.RaCoin
                                        {
                                            ActualDrawMoney = drawMoney,
                                            WalletAddress = item.to,
                                            CreateTime = DateTime.Now,
                                            DrawMoney = drawMoney,
                                            Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                            Poundage = 0,
                                            Direction = "in",
                                            UID = Umodel.ID,
                                            UserName = Umodel.UserName,
                                            ReserveStr1 = item.timeStamp,
                                            Txid = item.hash,
                                            CurID = Currency.ID
                                        });
                                        Wallets.changeWallet(Umodel.ID, drawMoney, (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                                        result.Status = 200;//有充值收入
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EthereumContract)
                {
                    #region 以太坊合约

                    if (string.IsNullOrEmpty(Currency.AccountAddress)) throw new JN.Services.CustomException.CustomException("系统未设置主帐号地址，无法获取充值结果");
                    string Racoin = JN.Services.Manager.WalletsAPI.GetWalletAddress((int)Currency.WalletCurID, Umodel);
                    if (string.IsNullOrEmpty(Racoin))
                    {
                        throw new JN.Services.CustomException.CustomException("您还未生成离线地址，请先生成离线地址");
                    }
                    else
                    {
                        var trans = EthHelper.GetContractTransactionsNew(Currency.AccountAddress, Racoin);
                        if (trans.Data.result.Count > 0)
                        {
                            foreach (var item in trans.Data.result.Where(x => x.to.ToLower() == Racoin.ToLower() && x.value.ToDouble() > 0))
                            {
                                int havecount = RaCoinService.List(x => x.Txid == item.hash && x.CurID == Currency.ID && x.Direction == "in").Count();
                                if (havecount <= 0)
                                {
                                    decimal drawMoney = (decimal)(item.value.ToDouble() / 1000000000000000000);
                                    if (drawMoney > 0)
                                    {
                                        //写入表
                                        RaCoinService.Add(new Data.RaCoin
                                        {
                                            ActualDrawMoney = drawMoney,
                                            WalletAddress = item.to,
                                            CreateTime = DateTime.Now,
                                            DrawMoney = drawMoney,
                                            Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                            Poundage = 0,
                                            Direction = "in",
                                            UID = Umodel.ID,
                                            UserName = Umodel.UserName,
                                            ReserveStr1 = item.timeStamp,
                                            Txid = item.hash,
                                            CurID = Currency.ID
                                        });
                                        Wallets.changeWallet(Umodel.ID, drawMoney, (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                                        result.Status = 200;//有充值收入
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.NEO)
                {
                    #region NEO
                    string Racoin = JN.Services.Manager.WalletsAPI.GetWalletAddress((int)Currency.WalletCurID, Umodel);
                    var trans = NEOHelper.GetListTransactions(Currency.WalletKey, Racoin);
                    if (trans.Status == 200)
                    {
                        foreach (var item in trans.Data.Where(x => x.Address.ToLower() == Racoin.ToLower() && x.Value.ToDouble() > 0))
                        {
                            if (RaCoinService.List(x => x.Txid == item.Txid && x.CurID == Currency.ID && x.Direction == "in").Count() <= 0)
                            {
                                decimal drawMoney = (decimal)item.Value.ToDouble();
                                if (drawMoney > 0)
                                {
                                    //写入表
                                    RaCoinService.Add(new Data.RaCoin
                                    {
                                        ActualDrawMoney = drawMoney,
                                        WalletAddress = item.Address,
                                        CreateTime = DateTime.Now,
                                        DrawMoney = drawMoney,
                                        Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                        Poundage = 0,
                                        Direction = "in",
                                        UID = Umodel.ID,
                                        UserName = Umodel.UserName,
                                        ReserveStr1 = item.Time.ToString(),
                                        Txid = item.Txid,
                                        CurID = Currency.ID
                                    });
                                    Wallets.changeWallet(Umodel.ID, drawMoney, (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                                    result.Status = 200;//有充值收入
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ripple)
                {
                    #region XRP
                    string Racoin = JN.Services.Manager.WalletsAPI.GetWalletAddress((int)Currency.WalletCurID, Umodel);
                    var trans = XrpHelper.GetListTransactions(Currency.AccountAddress, Racoin);
                    if (trans.Status == 200)
                    {
                        foreach (var item in trans.Data.payments.Where(x => x.amount.ToDouble() > 0))
                        {
                            if (RaCoinService.List(x => x.Txid == item.tx_hash && x.CurID == Currency.ID && x.Direction == "in").Count() <= 0)
                            {
                                decimal drawMoney = (decimal)item.amount.ToDouble();
                                if (drawMoney > 0)
                                {
                                    //写入表
                                    RaCoinService.Add(new Data.RaCoin
                                    {
                                        ActualDrawMoney = drawMoney,
                                        WalletAddress = Currency.AccountAddress,
                                        Tag = Racoin,
                                        CreateTime = DateTime.Now,
                                        DrawMoney = drawMoney,
                                        Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                        Poundage = 0,
                                        Direction = "in",
                                        UID = Umodel.ID,
                                        UserName = Umodel.UserName,
                                        ReserveStr1 = item.executed_time,
                                        Txid = item.tx_hash,
                                        CurID = Currency.ID
                                    });
                                    Wallets.changeWallet(Umodel.ID, drawMoney, (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                                    result.Status = 200;//有充值收入
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.USDT)
                {
                    #region USDT
                    string useraddress = Users.GetWalletAddress(Currency.WalletCurID ?? 3005, Umodel);
                    var trans = OmnicoreHelper.GetListTransactions(Currency.WalletKey, useraddress);

                    foreach (var item in trans.Data.Where(x => x.valid && x.referenceaddress == useraddress))
                    {
                        if (RaCoinService.List(x => x.Remark == item.txid).Count() <= 0)
                        {

                            //写入表
                            RaCoinService.Add(new Data.RaCoin
                            {
                                ActualDrawMoney = item.amount.ToDecimal(),
                                WalletAddress = item.referenceaddress,
                                CreateTime = DateTime.Now,
                                DrawMoney = item.amount.ToDecimal(),
                                Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                Poundage = item.fee.ToDecimal(),
                                Direction = "in",
                                UID = Umodel.ID,
                                UserName = Umodel.UserName,
                                ReserveStr1 = item.blocktime.ToString(),
                                Remark = item.txid,
                                ReserveInt = item.confirmations,
                                CurID = Currency.ID
                            });
                            Wallets.changeWallet(Umodel.ID, item.amount.ToDecimal(), (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                            result.Status = 200;//有充值收入

                        }
                    }

                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EOS)
                {
                    #region EOS
                    string memo = WalletsAPI.GetWalletAddress((int)Currency.WalletCurID, Umodel);
                    var trans = EosHelper.GetListTransactions(Currency.AccountAddress, 1);
                    if (trans.Status == 200)
                    {
                        foreach (var item in trans.Data.data.trace_list.Where(x => x.memo == memo))
                        {
                            if (RaCoinService.List(x => x.Txid == item.trx_id && x.CurID == Currency.ID && x.Direction == "in").Count() <= 0)
                            {
                                decimal drawMoney = (decimal)item.quantity.ToDouble();
                                if (drawMoney > 0)
                                {
                                    //写入表
                                    RaCoinService.Add(new Data.RaCoin
                                    {
                                        ActualDrawMoney = drawMoney,
                                        WalletAddress = Currency.AccountAddress,
                                        Tag = memo,
                                        CreateTime = DateTime.Now,
                                        DrawMoney = drawMoney,
                                        Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                        Poundage = 0,
                                        Direction = "in",
                                        UID = Umodel.ID,
                                        UserName = Umodel.UserName,
                                        ReserveStr1 = item.timestamp,
                                        ReserveStr2 = item.sender,
                                        Txid = item.trx_id,
                                        CurID = Currency.ID
                                    });
                                    Wallets.changeWallet(Umodel.ID, drawMoney, (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                                    result.Status = 200;//有充值收入
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (JN.Services.CustomException.CustomException ex)
            {
                result.Message = ex.Message.ToString();
            }
            catch (DbEntityValidationException ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString() + Currency.EnSigns, ex);
            }
            catch (Exception ex)
            {
                result.Message = "网络系统繁忙，请稍候再试!";
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString() + Currency.EnSigns, ex);
            }
            return result;
        }
        #endregion

        #region 验证地址有效性
        /// <summary>
        /// 验证地址有效性
        /// </summary>
        /// <param name="walletaddress">效验地址</param>
        /// <param name="Currency">币种实例</param>
        /// <returns></returns>
        public static bool ValidateAddress(string WalletAddress, JN.Data.Currency Currency)
        {
            if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Bitcoin)
            {
                #region 比特币
                ReturnResult<bool> result2 = new ReturnResult<bool>();
                result2 = BitcionHelper.ValidateAddressGetSqlServer(Currency.WalletKey, WalletAddress);
                if (result2.Status != ReturnResultStatus.Succeed.GetShortValue() || !result2.Data)
                {
                    return false;
                }
                else
                {
                    return true;
                }
                #endregion
            }
            else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ethereum || Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EthereumContract)
            {
                #region 以太坊或合约
                if (WalletAddress.Trim().Length != 42)
                {
                    return false;
                }
                else
                {
                    return true;
                }
                #endregion
            }
            else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.NEO)
            {
                #region NEO
                ReturnResult<bool> result2 = new ReturnResult<bool>();
                result2 = NEOHelper.ValidateAddress(Currency.WalletKey, WalletAddress);
                if (result2.Status != ReturnResultStatus.Succeed.GetShortValue() || !result2.Data)
                {
                    return false;
                }
                else
                {
                    return true;
                }
                #endregion
            }
            else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ripple)
            {
                #region XRP
                return true;
                #endregion
            }
            else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EOS || Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EOSContract)//EOS
            {
                #region EOS
                if (WalletAddress.Trim().Length != 12)
                {
                    return false;
                }
                else
                {
                    return true;
                }
                #endregion
            }
            return true;
        }
        #endregion

        #region 转账操作
        /// <summary>
        /// 转账操作
        /// </summary>
        /// <param name="Currency">币种实例</param>
        /// <param name="WalletAddress">转账地址</param>
        /// <param name="ActualDrawMoney">转账金额</param>
        /// <param name="Umodel">会员实例</param>
        /// <param name="ordernumber">订单号</param>
        /// <param name="tousername">转账会员名</param>
        /// <returns></returns>
        public static ReturnResult<string> WalletTransfer(JN.Data.Currency Currency, string WalletAddress, decimal ActualDrawMoney, string ordernumber = "", string tousername = "", string tag = "", JN.Data.User Umodel = null)
        {
            ReturnResult<string> result = new ReturnResult<string>();
            string CacheKey = (Umodel != null ? Umodel.UserName : "") + "WalletTransfer" + Currency.ID;
            try
            {
                if (string.IsNullOrWhiteSpace(WalletAddress) || !ValidateAddress(WalletAddress, Currency)) throw new JN.Services.CustomException.CustomException("无效的地址");
                if (ActualDrawMoney <= 0) throw new JN.Services.CustomException.CustomException("无效的金额");
                if (CacheExtensions.CheckCache(CacheKey))//查找缓存是否存在
                {
                    throw new CustomException.CustomException("操作过于频繁");
                }
                CacheExtensions.SetCache(CacheKey, "", MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//每1分钟   

                if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Bitcoin)
                {
                    #region 比特币
                    result = BitcionHelper.TransferGetSqlServer(Currency.WalletKey, WalletAddress, ActualDrawMoney.ToDouble());
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ethereum)
                {
                    #region 以太坊
                    result = EthHelper.Transfer(Currency.WalletKey, ordernumber, WalletAddress, userNameSalt + tousername, ActualDrawMoney.ToDouble());
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EthereumContract)
                {
                    #region 代币
                    result = EthHelper.TransferContract(Currency.WalletKey, ordernumber, WalletAddress, userNameSalt + tousername, ActualDrawMoney.ToDouble());
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.NEO)
                {
                    #region NEO
                    result = NEOHelper.Transfer(Currency.WalletKey, WalletAddress, ActualDrawMoney.ToDouble());
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ripple)
                {
                    #region XRP
                    //判断是否为站内转账
                    if (Currency.AccountAddress.Trim().ToLower() != WalletAddress.Trim().ToLower())
                    {
                        result = XrpHelper.Transfer(Currency.WalletKey, Currency.AccountAddress, WalletAddress, ActualDrawMoney, tag);
                    }
                    else
                    {
                        //查找会员
                        var onUser = GetUserForWalletAddress((int)Currency.WalletCurID, tag);
                        if (onUser != null)
                        {
                            //写入表
                            MvcCore.Unity.Get<JN.Data.Service.IRaCoinService>().Add(new Data.RaCoin
                            {
                                ActualDrawMoney = ActualDrawMoney,
                                WalletAddress = Currency.AccountAddress,
                                Tag = tag,
                                CreateTime = DateTime.Now,
                                DrawMoney = ActualDrawMoney,
                                Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                Poundage = 0,
                                Direction = "in",
                                UID = onUser.ID,
                                UserName = onUser.UserName,
                                Txid = "站内转账",
                                CurID = Currency.ID
                            });
                            Wallets.changeWallet(onUser.ID, ActualDrawMoney, (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入", Currency);
                            result.Data = "站内转账";
                            result.Status = ReturnResultStatus.Succeed.GetShortValue();
                        }
                        else
                        {
                            result.Data = "站内转账出错，Tag不存在";
                        }
                    }
                    #endregion
                }
                else if (Currency.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.USDT)
                {
                    #region USDT
                    result = OmnicoreHelper.Transfer(Currency.WalletKey, WalletAddress, ActualDrawMoney.ToDouble());
                    #endregion
                }
                CacheExtensions.ClearCache(CacheKey);//移除缓存
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

        #region 同步转账结果

        #region 以太坊代币
        public static void EthContract_GetSyncTakeCash()
        {
            SysDbFactory sysDbFactory = new SysDbFactory();
            RaCoinService raCoinService = new RaCoinService(sysDbFactory);
            SysDBTool sysDBTool = new SysDBTool(sysDbFactory);

            StringBuilder sql = new StringBuilder();
            try
            {
                var curmodel = MvcCore.Unity.Get<ICurrencyService>().List(x => x.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.EthereumContract).FirstOrDefault();
                var alist = raCoinService.List(x => x.CurID == curmodel.ID && x.Direction == "out" && x.Status == (int)JN.Data.Enum.TakeCaseStatus.Deal).OrderByDescending(x => x.ID);

                int pageSize = 20;
                int maxPageIndex = (int)Math.Ceiling((double)alist.Count() / pageSize);
                DataCache.SetCache("TotalRow", maxPageIndex);
                for (var i = 1; i <= maxPageIndex; i++)
                {
                    string ordernumbers = String.Join(",", alist.ToPagedList(i, pageSize).Select(x => x.ID));

                    DataCache.SetCache("CurrentRow", i);
                    DataCache.SetCache("TransInfo", "正在同步第“" + i + "”批记录数据");
                    var result = EthHelper.QueryMultiContractTransfer(curmodel.WalletKey, ordernumbers);
                    if (result.Data != null && result.Data.Count > 0)
                    {
                        foreach (var item in result.Data)
                        {
                            string statustip = "";
                            switch (item.Status)
                            {
                                case 0:
                                    statustip = "打包中";
                                    break;
                                case 1:
                                    statustip = "发送成功";
                                    break;
                                case 2:
                                    statustip = "转帐成功";
                                    break;
                                case -1:
                                    statustip = "发送异常";
                                    break;
                                case -2:
                                    statustip = "终止";
                                    break;
                            }
                            sql.AppendFormat(" update [RaCoin] set ReserveStr1='{0}',Txid='{1}', Remark='{2}' where ID={3}", statustip, item.TransactionHash, item.Remark, item.OrderNumber);
                        }
                    }
                    Thread.Sleep(100);
                }
                if (sql.Length > 0)
                {
                    System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                    int count = sysDBTool.ExecuteSQL(sql.ToString(), s);
                }
                DataCache.SetCache("TotalRow", maxPageIndex);
                DataCache.SetCache("TransInfo", "已同步所有地址充值信息，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒，结束时间：" + DateTime.Now);
            }
            catch (Exception ex)
            {
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                StreamWriter FileWriter = new StreamWriter(path, true, System.Text.Encoding.UTF8); //创建日志文件
                FileWriter.Write(ex.ToString());
                FileWriter.Close(); //关闭StreamWriter对象
            }
        }
        #endregion

        #region XRP
        public static void Xrp_GetSyncTakeCash()
        {
            SysDbFactory sysDbFactory = new SysDbFactory();
            RaCoinService raCoinService = new RaCoinService(sysDbFactory);
            SysDBTool sysDBTool = new SysDBTool(sysDbFactory);

            StringBuilder sql = new StringBuilder();
            try
            {
                var curmodel = MvcCore.Unity.Get<ICurrencyService>().List(x => x.NetworkType == (int)JN.Data.Enum.CurrencyNetworkType.Ripple).FirstOrDefault();
                var alist = raCoinService.List(x => x.CurID == curmodel.ID && x.Direction == "out" && x.Status == (int)JN.Data.Enum.TakeCaseStatus.Deal && (x.ReserveStr1 == null || x.ReserveStr1 == "") && (x.Txid != "站内转账" && x.Txid != null && x.Txid != "")).OrderByDescending(x => x.ID).ToList();

                DataCache.SetCache("TotalRow", alist.Count());
                for (var i = 0; i < alist.Count(); i++)
                {
                    DataCache.SetCache("CurrentRow", i + 1);
                    DataCache.SetCache("TransInfo", "正在同步第“" + i + 1 + "”条记录数据");
                    var result = XrpHelper.GetTransactionsForTX(alist[i].Txid);
                    if (result.Data != null && result.Data.transaction != null && result.Data.transaction.meta != null)
                    {
                        string statustip = "";
                        switch (result.Data.transaction.meta.TransactionResult)
                        {
                            case "tesSUCCESS":
                                statustip = "交易已应用";
                                break;
                            default:
                                statustip = "发送异常";
                                break;
                        }
                        sql.AppendFormat(" update [RaCoin] set ReserveStr1='{1}', Remark='{2}' where ID={0}", alist[i].ID, statustip, result.Data.transaction.meta.TransactionResult);
                    }
                    Thread.Sleep(100);
                }
                if (sql.Length > 0)
                {
                    System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                    int count = sysDBTool.ExecuteSQL(sql.ToString(), s);
                }
                DataCache.SetCache("TotalRow", alist.Count());
                DataCache.SetCache("TransInfo", "已同步所有地址充值信息，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒，结束时间：" + DateTime.Now);
            }
            catch (Exception ex)
            {
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                StreamWriter FileWriter = new StreamWriter(path, true, System.Text.Encoding.UTF8); //创建日志文件
                FileWriter.Write(ex.ToString());
                FileWriter.Close(); //关闭StreamWriter对象
            }
        }
        #endregion

        #endregion


        #region 轮询EOS充值，实现充值自动化


        #region EOS
        public static void AutoEOSRecharge(int page)
        {
            var Currency = MvcCore.Unity.Get<ICurrencyService>().Single(x => x.EnSigns == "EOS");
            if (!string.IsNullOrEmpty(Currency.AccountAddress) && Currency.AccountAddress != "#")
            {
                var trans = EosHelper.GetListTransactions(Currency.AccountAddress, page);
                if (trans.Status == 200)
                {
                    foreach (var item in trans.Data.data.trace_list)
                    {
                        if (MvcCore.Unity.Get<IRaCoinService>().List(x => x.Txid == item.trx_id && x.CurID == Currency.ID && x.Direction == "in").Count() <= 0)
                        {
                            decimal drawMoney = (decimal)item.quantity.ToDouble();
                            if (drawMoney > 0)
                            {
                                string memo = item.memo;
                                var onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3003 == memo);
                                if (onUser != null)
                                {
                                    //写入表
                                    MvcCore.Unity.Get<IRaCoinService>().Add(new Data.RaCoin
                                    {
                                        ActualDrawMoney = drawMoney,
                                        WalletAddress = Currency.AccountAddress,
                                        Tag = memo,
                                        CreateTime = DateTime.Now,
                                        DrawMoney = drawMoney,
                                        Status = (int)JN.Data.Enum.TakeCaseStatus.Deal,
                                        Poundage = 0,
                                        Direction = "in",
                                        UID = onUser.ID,
                                        UserName = onUser.UserName,
                                        ReserveStr1 = item.timestamp,
                                        ReserveStr2 = item.sender,
                                        Txid = item.trx_id,
                                        CurID = Currency.ID
                                    });
                                    Wallets.changeWallet(onUser.ID, drawMoney, (int)Currency.WalletCurID, Currency.CurrencyName + "充值收入(自动入帐)", Currency);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #endregion

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
        public static void SetWalletAddress(JN.Data.Currency Currency, JN.Data.User Umodel, string Address)
        {
            //十个币种（预留）
            switch (Currency.WalletCurID)
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

        /// <summary>
        /// 一个钱包两个币种共用（为以太、以太合约做处理）
        /// </summary>
        /// <param name="curmodel">币种对象</param>
        /// <param name="onUser">用户实例</param>
        /// <param name="Address">离线地址</param>
        /// <returns></returns>
        public static void SetWalletAddressETH(JN.Data.Currency curmodel, JN.Data.User onUser, string Address)
        {
            foreach (var item in MvcCore.Unity.Get<ICurrencyService>().List(x => x.WalletKey == curmodel.WalletKey).ToList())
            {
                SetWalletAddress(item, onUser, Address);
            }
        }
        #endregion

        #region 根据币种地址获取用户
        /// <summary>
        /// 根据币种地址获取用户
        /// </summary>
        /// <param name="CurID"></param>
        /// <returns></returns>
        public static Data.User GetUserForWalletAddress(int CurID, string address)
        {
            Data.User onUser = null;
            //十个币种（预留）
            switch (CurID)
            {
                case 3001:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3001 == address.Trim());
                    break;
                case 3002:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3002 == address.Trim());
                    break;
                case 3003:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3003 == address.Trim());
                    break;
                case 3004:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3004 == address.Trim());
                    break;
                case 3005:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3005 == address.Trim());
                    break;
                case 3006:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3006 == address.Trim());
                    break;
                case 3007:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3007 == address.Trim());
                    break;
                case 3008:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3008 == address.Trim());
                    break;
                case 3009:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3009 == address.Trim());
                    break;
                case 3010:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3010 == address.Trim());
                    break;
                case 3011:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3011 == address.Trim());
                    break;
                case 3012:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3012 == address.Trim());
                    break;
                case 3013:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3013 == address.Trim());
                    break;
                case 3014:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3014 == address.Trim());
                    break;
                case 3015:
                    onUser = MvcCore.Unity.Get<IUserService>().Single(x => x.R3015 == address.Trim());
                    break;
            }
            return onUser;
        }
        #endregion
    }
}