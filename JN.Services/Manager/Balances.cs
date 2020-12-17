using JN.Data.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Text;
using JN.Services.Tool;
using System.Data;
using System.Data.Entity.SqlServer;

namespace JN.Services.Manager
{
    /// <summary>
    ///分红结算
    /// </summary>
    public partial class Balances
    {
        #region 每日释放 在wp18080102 中
        /// <summary>
        /// 每日释放
        /// 积分释放余额
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void CalculateRelease(
            IReleaseService ReleaseService,
            IReleaseDetailService ReleaseDetailService,
            IUserService UserService,
            IWalletLogService WalletLogService,
            ICurrencyService CurrencyService,
            ISysDBTool SysDBTool,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
            int period = ReleaseService.List().Count() > 0 ? ReleaseService.List().Max(x => x.Period) + 1 : 1;  //每日释放总期数
            #region 初始变量定义
            //var param = cacheSysParam.SingleAndInit(x => x.ID == 1101);//释放        

            decimal PARAM_SFBL = 0;//静态释放比例 cacheSysParam.SingleAndInit(x => x.ID == 1203).Value.ToDecimal();
            decimal dynamic_SFBL = 0;//动态释放比例

            var jfWallet = CurrencyService.Single(x => x.ID == 2);//积分账户
            var yeWallet = CurrencyService.Single(x => x.ID == 3);//余额账户

            #region 奖金参数
            var param1102 = cacheSysParam.SingleAndInit(x => x.ID == 1102);//静态释放  
            var param1201 = cacheSysParam.SingleAndInit(x => x.ID == 1201);
            var param1202 = cacheSysParam.SingleAndInit(x => x.ID == 1202);
            var param1203 = cacheSysParam.SingleAndInit(x => x.ID == 1203);
            var param1204 = cacheSysParam.SingleAndInit(x => x.ID == 1204);
            var param1205 = cacheSysParam.SingleAndInit(x => x.ID == 1205);
            var param1103 = cacheSysParam.SingleAndInit(x => x.ID == 1103);//动态释放
            var param1301 = cacheSysParam.SingleAndInit(x => x.ID == 1301);
            var param1302 = cacheSysParam.SingleAndInit(x => x.ID == 1302);
            var param1303 = cacheSysParam.SingleAndInit(x => x.ID == 1303);
            var param1304 = cacheSysParam.SingleAndInit(x => x.ID == 1304);
            var param1305 = cacheSysParam.SingleAndInit(x => x.ID == 1305);
            var param1306 = cacheSysParam.SingleAndInit(x => x.ID == 1306);
            var param1307 = cacheSysParam.SingleAndInit(x => x.ID == 1307);
            var param1308 = cacheSysParam.SingleAndInit(x => x.ID == 1308);
            var param1309 = cacheSysParam.SingleAndInit(x => x.ID == 1309);
            var param1310 = cacheSysParam.SingleAndInit(x => x.ID == 1310);
            var param1311 = cacheSysParam.SingleAndInit(x => x.ID == 1311);
            var param1312 = cacheSysParam.SingleAndInit(x => x.ID == 1312);
            var param1313 = cacheSysParam.SingleAndInit(x => x.ID == 1313);
            var param1314 = cacheSysParam.SingleAndInit(x => x.ID == 1314);
            var param1315 = cacheSysParam.SingleAndInit(x => x.ID == 1315);
            var param1341 = cacheSysParam.SingleAndInit(x => x.ID == 1341);//直推标准
            var param1351 = cacheSysParam.SingleAndInit(x => x.ID == 1351);//拿奖代数参数
            var param1352 = cacheSysParam.SingleAndInit(x => x.ID == 1352);
            var param1353 = cacheSysParam.SingleAndInit(x => x.ID == 1353);
            var param1354 = cacheSysParam.SingleAndInit(x => x.ID == 1354);
            var param1355 = cacheSysParam.SingleAndInit(x => x.ID == 1355);
            #endregion

            StringBuilder userSql = new StringBuilder();//会员表更新语句  
            List<Data.WalletLog> addWalletLogList = new List<Data.WalletLog>();
            List<Data.ReleaseDetail> addReleaseDetailList = new List<Data.ReleaseDetail>();

            decimal totalLX = 0;
            int j = 1;
            string bonusDesc;
            decimal bonusMoney = 0;//最终获得金额
            decimal DTbonusMoney = 0;////动态金额

            DateTime time = DateTime.Now;//当前时间
            DateTime endTime = DateTime.Now.AddMinutes(param1102.Value2.ToInt());//失效时间
            var userListAll = UserService.List(x => x.IsLock == false && x.IsActivation).OrderBy(x => x.ID).ToList();//激活的未被冻结的所有会员
            var userList = userListAll.Where(x => (x.Cur3002 ?? 0) > 0).ToList();//积分账户有钱才可以

            Dictionary<int, Data.User> dicUser = userList.ToDictionary(d => d.ID, d => d); //把所有会员放到内存
            var onUser = new JN.Data.User();
            var pathUser = new JN.Data.User();
            //Dictionary<int, decimal> DicBouns = new Dictionary<int, decimal>();//存储会员ID以及奖金的金额更改
            #endregion                      

            if (jfWallet != null && yeWallet != null)
            {
                DataCache.SetCache("TotalRow", userList.Count);
                DataCache.SetCache("StartTime", DateTime.Now);

                #region 静态加速释放
                foreach (var item in userList)
                {
                    onUser = dicUser[item.ID];
                    if (onUser == null) continue;
                    DataCache.SetCache("CurrentRow", j);
                    DataCache.SetCache("TransInfo", "正在结算“" + onUser.UserName + "”");

                    var mix = (onUser.LeftDpMargin ?? 0) < (onUser.RightDpMargin ?? 0) ? (onUser.LeftDpMargin ?? 0) : (onUser.RightDpMargin ?? 0);
                    if (mix >= param1205.Value.ToDecimal())
                    {
                        PARAM_SFBL = param1205.Value2.ToDecimal();
                    }
                    else if (mix >= param1204.Value.ToDecimal())
                    {
                        PARAM_SFBL = param1204.Value2.ToDecimal();
                    }
                    else if (mix >= param1203.Value.ToDecimal())
                    {
                        PARAM_SFBL = param1203.Value2.ToDecimal();
                    }
                    else if (mix >= param1202.Value.ToDecimal())
                    {
                        PARAM_SFBL = param1202.Value2.ToDecimal();
                    }
                    else
                    {
                        PARAM_SFBL = param1201.Value2.ToDecimal();
                    }
                    bonusMoney = (dicUser[item.ID].Cur3002 ?? 0) * PARAM_SFBL;
                    bonusDesc = param1102.Name + "，时间：" + time;
                    if ((dicUser[item.ID].Cur3002 ?? 0) - bonusMoney < 0)
                    {
                        bonusMoney = (dicUser[item.ID].Cur3002 ?? 0);
                    }
                    if (bonusMoney > 0)
                    {
                        addReleaseDetailList.Add(new Data.ReleaseDetail
                        {
                            Period = period,
                            UID = onUser.ID,
                            UserName = onUser.UserName,
                            CurID = yeWallet.WalletCurID ?? 0,
                            CurName = yeWallet.CurrencyName,
                            Money = bonusMoney,
                            IsSign = false,   //未领取                   
                            CreateTime = time,//创建时间     
                            EndTime = endTime,//失效时间
                            Description = bonusDesc
                        });

                        addWalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = -bonusMoney,
                            Balance = (onUser.Cur3002 ?? 0) - bonusMoney,
                            CoinID = jfWallet.WalletCurID ?? 3002,
                            CoinName = jfWallet.CurrencyName,
                            CreateTime = DateTime.Now,
                            Description = bonusDesc,
                            UID = onUser.ID,
                            UserName = onUser.UserName
                        });
                        dicUser[item.ID].Cur3002 = (dicUser[item.ID].Cur3002 ?? 0) - bonusMoney;
                        //更新用户钱包
                        //item.Cur3002 = (item.Cur3002 ?? 0) - bonusMoney;
                        //更新用户钱包并清空当日的左右区业绩
                        userSql.AppendFormat("update [User] set Cur3002=ISNULL(Cur3002,0)-{0},LeftDpMargin = 0,RightDpMargin=0 where ID={1}", bonusMoney, onUser.ID);
                        //userSql.AppendFormat("update [User] set LeftDpMargin = 0,RightDpMargin=0 where ID={0}", item.ID);

                        #region 动态加速释放
                        decimal standardZT = param1341.Value.ToDecimal();
                        var resUser = userList.Where(x => onUser.RefereePath.Split(',').Contains(x.ID.ToString())).ToList();
                        foreach (var item2 in resUser)
                        {
                            pathUser = dicUser[item2.ID];
                            if (pathUser == null) continue;
                            int ZTRS = userListAll.Where(x => x.RefereeID == item2.ID && x.Investment >= standardZT).Count();//直推达标人数
                            int MaxDepth = 0;//最大拿奖层数
                            if (ZTRS >= param1355.Value.ToInt())
                            {
                                MaxDepth = param1355.Value2.ToInt();
                            }
                            else if (ZTRS >= param1354.Value.ToInt())
                            {
                                MaxDepth = param1354.Value2.ToInt();
                            }
                            else if (ZTRS >= param1353.Value.ToInt())
                            {
                                MaxDepth = param1353.Value2.ToInt();
                            }
                            else if (ZTRS >= param1352.Value.ToInt())
                            {
                                MaxDepth = param1352.Value2.ToInt();
                            }
                            else if (ZTRS >= param1351.Value.ToInt())
                            {
                                MaxDepth = param1351.Value2.ToInt();
                            }
                            else
                            {
                                MaxDepth = 0;
                            }

                            int depth = onUser.RefereeDepth - pathUser.RefereeDepth;
                            if (MaxDepth < depth)//层数小于最大拿奖层数则不得奖
                            {
                                continue;
                            }
                            switch (depth)
                            {
                                case 1:
                                    dynamic_SFBL = param1301.Value.ToDecimal();
                                    break;
                                case 2:
                                    dynamic_SFBL = param1302.Value.ToDecimal();
                                    break;
                                case 3:
                                    dynamic_SFBL = param1303.Value.ToDecimal();
                                    break;
                                case 4:
                                    dynamic_SFBL = param1304.Value.ToDecimal();
                                    break;
                                case 5:
                                    dynamic_SFBL = param1305.Value.ToDecimal();
                                    break;
                                case 6:
                                    dynamic_SFBL = param1306.Value.ToDecimal();
                                    break;
                                case 7:
                                    dynamic_SFBL = param1307.Value.ToDecimal();
                                    break;
                                case 8:
                                    dynamic_SFBL = param1308.Value.ToDecimal();
                                    break;
                                case 9:
                                    dynamic_SFBL = param1309.Value.ToDecimal();
                                    break;
                                case 10:
                                    dynamic_SFBL = param1310.Value.ToDecimal();
                                    break;
                                case 11:
                                    dynamic_SFBL = param1311.Value.ToDecimal();
                                    break;
                                case 12:
                                    dynamic_SFBL = param1312.Value.ToDecimal();
                                    break;
                                case 13:
                                    dynamic_SFBL = param1313.Value.ToDecimal();
                                    break;
                                case 14:
                                    dynamic_SFBL = param1314.Value.ToDecimal();
                                    break;
                                case 15:
                                    dynamic_SFBL = param1315.Value.ToDecimal();
                                    break;
                                default:
                                    dynamic_SFBL = 0;
                                    break;
                            }
                            DTbonusMoney = bonusMoney * dynamic_SFBL;
                            if ((dicUser[pathUser.ID].Cur3002 ?? 0) - DTbonusMoney < 0)
                            {
                                DTbonusMoney = (dicUser[pathUser.ID].Cur3002 ?? 0);
                            }
                            if (DTbonusMoney > 0)
                            {
                                bonusDesc = "来自第" + depth + "代会员" + onUser.UserName + "的" + param1103.Name;
                                addReleaseDetailList.Add(new Data.ReleaseDetail
                                {
                                    Period = period,
                                    UID = pathUser.ID,
                                    UserName = pathUser.UserName,
                                    CurID = yeWallet.ID,
                                    CurName = yeWallet.CurrencyName,
                                    Money = DTbonusMoney,
                                    IsSign = false,   //未领取                   
                                    CreateTime = time,//创建时间     
                                    EndTime = endTime,//失效时间
                                    Description = bonusDesc
                                });
                                addWalletLogList.Add(new Data.WalletLog
                                {
                                    ChangeMoney = -DTbonusMoney,
                                    Balance = (pathUser.Cur3002 ?? 0) - DTbonusMoney,
                                    CoinID = jfWallet.WalletCurID ?? 3002,
                                    CoinName = jfWallet.CurrencyName,
                                    CreateTime = DateTime.Now,
                                    Description = bonusDesc,
                                    UID = pathUser.ID,
                                    UserName = pathUser.UserName
                                });
                                dicUser[item2.ID].Cur3002 = (dicUser[item2.ID].Cur3002 ?? 0) - DTbonusMoney;
                                userSql.AppendFormat("update [User] set Cur3002=ISNULL(Cur3002,0)-{0} where ID={1}", DTbonusMoney, pathUser.ID);
                            }
                        }
                        #endregion
                    }
                    //给当前用户的
                    totalLX += bonusMoney;
                    j++;
                }
                #endregion

                if (addWalletLogList.Count() > 0)
                {
                    //加入事务
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //批量处理会员的信息
                        DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                        //写入钱包表
                        WalletLogService.BulkInsert(addWalletLogList);
                        //批量写入释放详情记录
                        ReleaseDetailService.BulkInsert(addReleaseDetailList);
                        ReleaseService.Add(new Data.Release { BalanceMode = balancemode, CreateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00")), Period = period, TotalBonus = totalLX, TotalUser = userList.Count });
                        SysDBTool.Commit();

                        DataCache.SetCache("TransInfo", "成功对" + userList.Count + "会员进行结算，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");
                        ts.Complete();
                    }
                }
            }
        }
        #endregion

        #region 补发每日释放的动态释放 在wp18080102中 弃用
        /// <summary>
        /// 补发动态奖金程序 2018-08-19  by:pengyh
        /// </summary>
        /// <param name="tiembf">补发日期</param>
        /// <returns></returns>
        public static void ReissueDynamicRelease_old(DateTime tiembf)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(timeb)) throw new Exception("请输入补发日期");
                //DateTime tiembf = DateTime.Parse(timeb);

                //工厂类业务类定义
                SysDbFactory SysDbFactory = new SysDbFactory();
                ExchangeDetailService ExchangeDetailService = new ExchangeDetailService(SysDbFactory);
                UserService UserService = new UserService(SysDbFactory);
                SysParamService SysParamService = new SysParamService(SysDbFactory);
                ReleaseService ReleaseService = new ReleaseService(SysDbFactory);
                CurrencyService CurrencyService = new CurrencyService(SysDbFactory);
                WalletLogService WalletLogService = new WalletLogService(SysDbFactory);
                ReleaseDetailService ReleaseDetailService = new ReleaseDetailService(SysDbFactory);
                SysDBTool SysDBTool = new SysDBTool(SysDbFactory);

                //查找兑换记录
                var exchangelist = ExchangeDetailService.List(x => System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", x.CreateTime, tiembf) == 0).ToList();
                //初始化会员列表
                var _userList = UserService.List().OrderBy(x => x.ID).ToList().ToDictionary(d => d.ID, d => d);
                #region 模拟当天业绩增加
                //当日业绩初始化
                _userList.ForEach(x => x.Value.LeftDpMargin = 0);
                _userList.ForEach(x => x.Value.RightDpMargin = 0);
                //模拟业绩增加
                decimal exchangeMoney = 0;
                foreach (var item in exchangelist.OrderBy(x => x.UID).GroupBy(x => x.UID))//循环分组
                {
                    exchangeMoney = item.Sum(x => x.ApplyMoney);
                    //模拟业绩提升
                    //_userList[item.Key].Investment = _userList[item.Key].Investment + exchangeMoney;
                    if (!string.IsNullOrEmpty(_userList[item.Key].RefereePath))
                    {
                        string[] ids = _userList[item.Key].ParentPath.Split(',');
                        var userlistRef = _userList.Where(x => ids.Contains(x.Value.ID.ToString())).OrderByDescending(x => x.Value.ID).ToList();
                        if (userlistRef.Count() > 0)
                        {
                            int childPlace = _userList[item.Key].ChildPlace;
                            foreach (var tdjUser in userlistRef)
                            {
                                if (childPlace == 1)
                                {
                                    _userList[tdjUser.Key].LeftDpMargin = (_userList[tdjUser.Key].LeftDpMargin ?? 0) + exchangeMoney;
                                }
                                else
                                {
                                    _userList[tdjUser.Key].RightDpMargin = (_userList[tdjUser.Key].RightDpMargin ?? 0) + exchangeMoney;
                                }
                                childPlace = tdjUser.Value.ChildPlace;
                            }
                        }
                    }
                }
                #endregion

                #region 补发释放
                //初始定义参数列表
                var cacheSysParam = SysParamService.List(x => x.ID < 6000).ToList();
                int balancemode = 1;//模式为手动

                int period = ReleaseService.List().Count() > 0 ? ReleaseService.List().Max(x => x.Period) + 1 : 1;  //每日释放总期数
                #region 初始变量定义
                //var param = cacheSysParam.SingleAndInit(x => x.ID == 1101);//释放        

                decimal PARAM_SFBL = 0;//静态释放比例 cacheSysParam.SingleAndInit(x => x.ID == 1203).Value.ToDecimal();
                decimal dynamic_SFBL = 0;//动态释放比例

                var jfWallet = CurrencyService.Single(x => x.ID == 2);//积分账户
                var yeWallet = CurrencyService.Single(x => x.ID == 3);//余额账户

                #region 奖金参数
                var param1102 = cacheSysParam.SingleAndInit(x => x.ID == 1102);//静态释放  
                var param1201 = cacheSysParam.SingleAndInit(x => x.ID == 1201);
                var param1202 = cacheSysParam.SingleAndInit(x => x.ID == 1202);
                var param1203 = cacheSysParam.SingleAndInit(x => x.ID == 1203);
                var param1204 = cacheSysParam.SingleAndInit(x => x.ID == 1204);
                var param1205 = cacheSysParam.SingleAndInit(x => x.ID == 1205);
                var param1103 = cacheSysParam.SingleAndInit(x => x.ID == 1103);//动态释放
                var param1301 = cacheSysParam.SingleAndInit(x => x.ID == 1301);
                var param1302 = cacheSysParam.SingleAndInit(x => x.ID == 1302);
                var param1303 = cacheSysParam.SingleAndInit(x => x.ID == 1303);
                var param1304 = cacheSysParam.SingleAndInit(x => x.ID == 1304);
                var param1305 = cacheSysParam.SingleAndInit(x => x.ID == 1305);
                var param1306 = cacheSysParam.SingleAndInit(x => x.ID == 1306);
                var param1307 = cacheSysParam.SingleAndInit(x => x.ID == 1307);
                var param1308 = cacheSysParam.SingleAndInit(x => x.ID == 1308);
                var param1309 = cacheSysParam.SingleAndInit(x => x.ID == 1309);
                #endregion

                System.Text.StringBuilder userSql = new System.Text.StringBuilder();//会员表更新语句  
                List<Data.WalletLog> addWalletLogList = new List<Data.WalletLog>();
                List<Data.ReleaseDetail> addReleaseDetailList = new List<Data.ReleaseDetail>();

                decimal totalLX = 0;
                int j = 1;
                string bonusDesc;
                decimal bonusMoney = 0;//最终获得金额
                decimal DTbonusMoney = 0;////动态金额

                DateTime time = DateTime.Now;//当前时间
                DateTime endTime = DateTime.Now.AddMinutes(param1102.Value2.ToInt());//失效时间

                var userList = _userList.Values.Where(x => (x.Cur3002 ?? 0) > 0 && x.IsLock == false && x.IsActivation).OrderBy(x => x.ID).ToList();//积分账户有钱才可以

                Dictionary<int, Data.User> dicUser = userList.ToDictionary(d => d.ID, d => d); //把所有会员放到内存
                var onUser = new JN.Data.User();
                var pathUser = new JN.Data.User();
                //Dictionary<int, decimal> DicBouns = new Dictionary<int, decimal>();//存储会员ID以及奖金的金额更改
                #endregion

                if (jfWallet != null && yeWallet != null)
                {
                    Services.Tool.DataCache.SetCache("TotalRow", userList.Count);
                    Services.Tool.DataCache.SetCache("StartTime", DateTime.Now);

                    #region 静态加速释放
                    foreach (var item in userList)
                    {
                        onUser = dicUser[item.ID];
                        if (onUser == null) continue;
                        Services.Tool.DataCache.SetCache("CurrentRow", j);
                        Services.Tool.DataCache.SetCache("TransInfo", "正在结算“" + onUser.UserName + "”");

                        var mix = (onUser.LeftDpMargin ?? 0) < (onUser.RightDpMargin ?? 0) ? (onUser.LeftDpMargin ?? 0) : (onUser.RightDpMargin ?? 0);
                        if (mix >= param1205.Value.ToDecimal())
                        {
                            PARAM_SFBL = param1205.Value2.ToDecimal();
                        }
                        else if (mix >= param1204.Value.ToDecimal())
                        {
                            PARAM_SFBL = param1204.Value2.ToDecimal();
                        }
                        else if (mix >= param1203.Value.ToDecimal())
                        {
                            PARAM_SFBL = param1203.Value2.ToDecimal();
                        }
                        else if (mix >= param1202.Value.ToDecimal())
                        {
                            PARAM_SFBL = param1202.Value2.ToDecimal();
                        }
                        else
                        {
                            PARAM_SFBL = param1201.Value2.ToDecimal();
                        }
                        bonusMoney = (dicUser[item.ID].Cur3002 ?? 0) * PARAM_SFBL;
                        bonusDesc = param1102.Name + "，时间：" + time;
                        if ((dicUser[item.ID].Cur3002 ?? 0) - bonusMoney < 0)
                        {
                            bonusMoney = (dicUser[item.ID].Cur3002 ?? 0);
                        }
                        if (bonusMoney > 0)
                        {
                            //addReleaseDetailList.Add(new Data.ReleaseDetail
                            //{
                            //    Period = period,
                            //    UID = onUser.ID,
                            //    UserName = onUser.UserName,
                            //    CurID = yeWallet.WalletCurID ?? 0,
                            //    CurName = yeWallet.CurrencyName,
                            //    Money = bonusMoney,
                            //    IsSign = false,   //未领取                   
                            //    CreateTime = time,//创建时间     
                            //    EndTime = endTime,//失效时间
                            //    Description = bonusDesc
                            //});

                            //addWalletLogList.Add(new Data.WalletLog
                            //{
                            //    ChangeMoney = -bonusMoney,
                            //    Balance = (onUser.Cur3002 ?? 0) - bonusMoney,
                            //    CoinID = jfWallet.WalletCurID ?? 3002,
                            //    CoinName = jfWallet.CurrencyName,
                            //    CreateTime = DateTime.Now,
                            //    Description = bonusDesc,
                            //    UID = onUser.ID,
                            //    UserName = onUser.UserName
                            //});
                            //dicUser[item.ID].Cur3002 = (dicUser[item.ID].Cur3002 ?? 0) - bonusMoney;
                            //更新用户钱包
                            //item.Cur3002 = (item.Cur3002 ?? 0) - bonusMoney;
                            //更新用户钱包并清空当日的左右区业绩
                            //userSql.AppendFormat("update [User] set Cur3002=ISNULL(Cur3002,0)-{0},LeftDpMargin = 0,RightDpMargin=0 where ID={1}", bonusMoney, onUser.ID);
                            //userSql.AppendFormat("update [User] set LeftDpMargin = 0,RightDpMargin=0 where ID={0}", item.ID);

                            #region 动态加速释放
                            var resUser = userList.Where(x => onUser.RefereePath.Split(',').Contains(x.ID.ToString())).ToList();
                            foreach (var item2 in resUser)
                            {
                                pathUser = dicUser[item2.ID];
                                if (pathUser == null) continue;
                                int depth = onUser.RefereeDepth - pathUser.RefereeDepth;
                                switch (depth)
                                {
                                    case 1:
                                        dynamic_SFBL = param1301.Value.ToDecimal();
                                        break;
                                    case 2:
                                        dynamic_SFBL = param1302.Value.ToDecimal();
                                        break;
                                    case 3:
                                        dynamic_SFBL = param1303.Value.ToDecimal();
                                        break;
                                    case 4:
                                        dynamic_SFBL = param1304.Value.ToDecimal();
                                        break;
                                    case 5:
                                        dynamic_SFBL = param1305.Value.ToDecimal();
                                        break;
                                    case 6:
                                        dynamic_SFBL = param1306.Value.ToDecimal();
                                        break;
                                    case 7:
                                        dynamic_SFBL = param1307.Value.ToDecimal();
                                        break;
                                    case 8:
                                        dynamic_SFBL = param1308.Value.ToDecimal();
                                        break;
                                    case 9:
                                        dynamic_SFBL = param1309.Value.ToDecimal();
                                        break;
                                    default:
                                        dynamic_SFBL = 0;
                                        break;
                                }
                                DTbonusMoney = bonusMoney * dynamic_SFBL;
                                if ((dicUser[pathUser.ID].Cur3002 ?? 0) - DTbonusMoney < 0)
                                {
                                    DTbonusMoney = (dicUser[pathUser.ID].Cur3002 ?? 0);
                                }
                                if (DTbonusMoney > 0)
                                {
                                    bonusDesc = "来自第" + depth + "代会员" + onUser.UserName + "的" + param1103.Name + "补发";
                                    addReleaseDetailList.Add(new Data.ReleaseDetail
                                    {
                                        Period = period,
                                        UID = pathUser.ID,
                                        UserName = pathUser.UserName,
                                        CurID = yeWallet.ID,
                                        CurName = yeWallet.CurrencyName,
                                        Money = DTbonusMoney,
                                        IsSign = false,   //未领取                   
                                        CreateTime = time,//创建时间     
                                        EndTime = endTime,//失效时间
                                        Description = bonusDesc
                                    });
                                    addWalletLogList.Add(new Data.WalletLog
                                    {
                                        ChangeMoney = -DTbonusMoney,
                                        Balance = (pathUser.Cur3002 ?? 0) - DTbonusMoney,
                                        CoinID = jfWallet.WalletCurID ?? 3002,
                                        CoinName = jfWallet.CurrencyName,
                                        CreateTime = DateTime.Now,
                                        Description = bonusDesc,
                                        UID = pathUser.ID,
                                        UserName = pathUser.UserName
                                    });
                                    dicUser[item2.ID].Cur3002 = (dicUser[item2.ID].Cur3002 ?? 0) - DTbonusMoney;
                                    userSql.AppendFormat("update [User] set Cur3002=ISNULL(Cur3002,0)-{0} where ID={1}", DTbonusMoney, pathUser.ID);
                                }
                            }
                            #endregion
                        }
                        //给当前用户的
                        totalLX += bonusMoney;
                        j++;
                    }
                    #endregion

                    if (addWalletLogList.Count() > 0)
                    {
                        //加入事务
                        using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                        {
                            //批量处理会员的信息
                            System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                            SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                            //写入钱包表
                            WalletLogService.BulkInsert(addWalletLogList);
                            //批量写入释放详情记录
                            ReleaseDetailService.BulkInsert(addReleaseDetailList);
                            ReleaseService.Add(new Data.Release { BalanceMode = balancemode, CreateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00")), Period = period, TotalBonus = totalLX, TotalUser = userList.Count });
                            SysDBTool.Commit();

                            Services.Tool.DataCache.SetCache("TransInfo", "成功对" + userList.Count + "会员进行结算，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");
                            ts.Complete();
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(ex);
            }
        }
        #endregion

        #region 补发每日释放的动态释放 在wp18080102中
        /// <summary>
        /// 补发动态奖金程序 2018-08-19  by:pengyh
        /// </summary>
        /// <param name="tiembf">补发日期</param>
        /// <returns></returns>
        public static void ReissueDynamicRelease(DateTime tiembf)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(timeb)) throw new Exception("请输入补发日期");
                //DateTime tiembf = DateTime.Parse(timeb);

                //工厂类业务类定义
                SysDbFactory SysDbFactory = new SysDbFactory();
                ExchangeDetailService ExchangeDetailService = new ExchangeDetailService(SysDbFactory);
                UserService UserService = new UserService(SysDbFactory);
                SysParamService SysParamService = new SysParamService(SysDbFactory);
                ReleaseService ReleaseService = new ReleaseService(SysDbFactory);
                CurrencyService CurrencyService = new CurrencyService(SysDbFactory);
                WalletLogService WalletLogService = new WalletLogService(SysDbFactory);
                ReleaseDetailService ReleaseDetailService = new ReleaseDetailService(SysDbFactory);
                SysDBTool SysDBTool = new SysDBTool(SysDbFactory);

                //查找动态释放记录
                var releaseDynamicList = ReleaseDetailService.List(x => x.Description.Contains("动态") && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", x.CreateTime, tiembf) == 0).ToList();
                //查找静态释放记录
                var releaseStaticList = ReleaseDetailService.List(x => x.Description.Contains("静态") && System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", x.CreateTime, tiembf) == 0).ToList();

                #region 补发释放
                //初始定义参数列表
                var cacheSysParam = SysParamService.List(x => x.ID < 6000).ToList();
                int balancemode = 1;//模式为手动

                int period = ReleaseService.List(x => System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", x.CreateTime, tiembf) == 0).Count() > 0 ? ReleaseService.List(x => System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", x.CreateTime, tiembf) == 0).Max(x => x.Period) : 0;  //当时释放期数
                #region 初始变量定义

                decimal dynamic_SFBL = 0;//动态释放比例

                var jfWallet = CurrencyService.Single(x => x.ID == 2);//积分账户
                var yeWallet = CurrencyService.Single(x => x.ID == 3);//余额账户

                #region 奖金参数
                var param1102 = cacheSysParam.SingleAndInit(x => x.ID == 1102);//静态释放  
                var param1103 = cacheSysParam.SingleAndInit(x => x.ID == 1103);//动态释放
                var param1301 = cacheSysParam.SingleAndInit(x => x.ID == 1301);
                var param1302 = cacheSysParam.SingleAndInit(x => x.ID == 1302);
                var param1303 = cacheSysParam.SingleAndInit(x => x.ID == 1303);
                var param1304 = cacheSysParam.SingleAndInit(x => x.ID == 1304);
                var param1305 = cacheSysParam.SingleAndInit(x => x.ID == 1305);
                var param1306 = cacheSysParam.SingleAndInit(x => x.ID == 1306);
                var param1307 = cacheSysParam.SingleAndInit(x => x.ID == 1307);
                var param1308 = cacheSysParam.SingleAndInit(x => x.ID == 1308);
                var param1309 = cacheSysParam.SingleAndInit(x => x.ID == 1309);
                #endregion

                System.Text.StringBuilder userSql = new System.Text.StringBuilder();//会员表更新语句  
                List<Data.WalletLog> addWalletLogList = new List<Data.WalletLog>();
                List<Data.ReleaseDetail> addReleaseDetailList = new List<Data.ReleaseDetail>();

                decimal totalLX = 0;
                int j = 1;
                string bonusDesc;
                decimal bonusMoney = 0;//最终获得金额
                decimal DTbonusMoney = 0;////动态金额

                DateTime time = DateTime.Now;//当前时间
                DateTime endTime = DateTime.Now.AddMinutes(param1102.Value2.ToInt());//失效时间

                var userList = UserService.List(x => (x.Cur3002 ?? 0) > 0 && x.IsLock == false && x.IsActivation).OrderBy(x => x.ID).ToList();//积分账户有钱才可以

                Dictionary<int, Data.User> dicUser = userList.ToDictionary(d => d.ID, d => d); //把所有会员放到内存
                var onUser = new JN.Data.User();
                var pathUser = new JN.Data.User();
                #endregion

                if (jfWallet != null && yeWallet != null)
                {
                    Services.Tool.DataCache.SetCache("TotalRow", userList.Count);
                    Services.Tool.DataCache.SetCache("StartTime", DateTime.Now);

                    #region 静态加速释放
                    foreach (var item in releaseStaticList.OrderBy(x => x.UID).GroupBy(x => x.UID))//遍历的是静态释放记录// 分组遍历
                    {
                        onUser = dicUser[item.Key];
                        if (onUser == null) continue;
                        Services.Tool.DataCache.SetCache("CurrentRow", j);
                        Services.Tool.DataCache.SetCache("TransInfo", "正在结算“" + onUser.UserName + "”");

                        bonusMoney = item.Sum(x => x.Money);
                        if (bonusMoney > 0)
                        {
                            #region 动态加速释放
                            var resUser = userList.Where(x => onUser.RefereePath.Split(',').Contains(x.ID.ToString())).ToList();
                            foreach (var item2 in resUser)
                            {
                                pathUser = dicUser[item2.ID];
                                if (pathUser == null) continue;
                                int depth = onUser.RefereeDepth - pathUser.RefereeDepth;
                                switch (depth)
                                {
                                    case 1:
                                        dynamic_SFBL = param1301.Value.ToDecimal();
                                        break;
                                    case 2:
                                        dynamic_SFBL = param1302.Value.ToDecimal();
                                        break;
                                    case 3:
                                        dynamic_SFBL = param1303.Value.ToDecimal();
                                        break;
                                    case 4:
                                        dynamic_SFBL = param1304.Value.ToDecimal();
                                        break;
                                    case 5:
                                        dynamic_SFBL = param1305.Value.ToDecimal();
                                        break;
                                    case 6:
                                        dynamic_SFBL = param1306.Value.ToDecimal();
                                        break;
                                    case 7:
                                        dynamic_SFBL = param1307.Value.ToDecimal();
                                        break;
                                    case 8:
                                        dynamic_SFBL = param1308.Value.ToDecimal();
                                        break;
                                    case 9:
                                        dynamic_SFBL = param1309.Value.ToDecimal();
                                        break;
                                    default:
                                        dynamic_SFBL = 0;
                                        break;
                                }
                                DTbonusMoney = bonusMoney * dynamic_SFBL;
                                if ((dicUser[pathUser.ID].Cur3002 ?? 0) - DTbonusMoney < 0)
                                {
                                    DTbonusMoney = (dicUser[pathUser.ID].Cur3002 ?? 0);
                                }
                                if (DTbonusMoney > 0)
                                {
                                    bonusDesc = "来自第" + depth + "代会员" + onUser.UserName + "的" + param1103.Name + "补发";
                                    addReleaseDetailList.Add(new Data.ReleaseDetail
                                    {
                                        Period = period,
                                        UID = pathUser.ID,
                                        UserName = pathUser.UserName,
                                        CurID = yeWallet.ID,
                                        CurName = yeWallet.CurrencyName,
                                        Money = DTbonusMoney,
                                        IsSign = false,   //未领取                   
                                        CreateTime = time,//创建时间     
                                        EndTime = endTime,//失效时间
                                        Description = bonusDesc
                                    });
                                    addWalletLogList.Add(new Data.WalletLog
                                    {
                                        ChangeMoney = -DTbonusMoney,
                                        Balance = (pathUser.Cur3002 ?? 0) - DTbonusMoney,
                                        CoinID = jfWallet.WalletCurID ?? 3002,
                                        CoinName = jfWallet.CurrencyName,
                                        CreateTime = DateTime.Now,
                                        Description = bonusDesc,
                                        UID = pathUser.ID,
                                        UserName = pathUser.UserName
                                    });
                                    dicUser[item2.ID].Cur3002 = (dicUser[item2.ID].Cur3002 ?? 0) - DTbonusMoney;
                                    userSql.AppendFormat("update [User] set Cur3002=ISNULL(Cur3002,0)-{0} where ID={1}", DTbonusMoney, pathUser.ID);
                                }
                            }
                            #endregion
                        }
                        //给当前用户的
                        totalLX += bonusMoney;
                        j++;
                    }
                    #endregion

                    if (addWalletLogList.Count() > 0)
                    {
                        //加入事务
                        using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                        {
                            //批量处理会员的信息
                            System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                            SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                            //写入钱包表
                            WalletLogService.BulkInsert(addWalletLogList);
                            //批量写入释放详情记录
                            ReleaseDetailService.BulkInsert(addReleaseDetailList);
                            ReleaseService.Add(new Data.Release { BalanceMode = balancemode, CreateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00")), Period = period, TotalBonus = totalLX, TotalUser = userList.Count });
                            SysDBTool.Commit();

                            Services.Tool.DataCache.SetCache("TransInfo", "成功对" + userList.Count + "会员进行结算，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");
                            ts.Complete();
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(ex);
            }
        }
        #endregion

        #region 同步兑换金额 在wp18080102中
        /// <summary>
        /// 同步兑换金额 在wp18080102中
        /// </summary>
        /// <returns></returns>
        public static void Synchronization()
        {
            try
            {
                //工厂类业务类定义
                SysDbFactory SysDbFactory = new SysDbFactory();
                ExchangeDetailService ExchangeDetailService = new ExchangeDetailService(SysDbFactory);
                UserService UserService = new UserService(SysDbFactory);
                SysDBTool SysDBTool = new SysDBTool(SysDbFactory);

                var ExchangeDetailList = ExchangeDetailService.List().ToList();//所有兑换记录
                Dictionary<int, Data.User> dicUser = UserService.List().ToDictionary(d => d.ID, d => d); //把所有会员放到内存
                StringBuilder userSql = new StringBuilder();//会员表更新语句  
                var onUser = new JN.Data.User();
                decimal bonusMoney = 0;//兑换金额
                foreach (var item in ExchangeDetailList.OrderBy(x => x.UID).GroupBy(x => x.UID))//遍历的是兑换记录// 分组遍历
                {
                    onUser = dicUser[item.Key];
                    if (onUser.UserName == "12683")
                    {
                        var aa = item.Sum(x => x.ApplyMoney);
                    }
                    if (onUser == null) continue;
                    bonusMoney = item.Sum(x => x.ApplyMoney);
                    userSql.AppendFormat(" update [User] set Investment={0} where ID={1}", bonusMoney, onUser.ID);
                }
                if (userSql.ToString().Length > 0)
                {
                    //加入事务
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //批量处理会员的信息
                        DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);
                        ts.Complete();
                    }
                }

            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(ex);
            }
        }
        #endregion

        #region VIP流通奖励
        /// <summary>
        /// VIP流通奖励
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void VIPDayBonus(
            IBonusDetailService BonusDetailService,
            ITransferService TransferService,
            IUserService UserService,
            IWalletLogService WalletLogService,
            ICurrencyService CurrencyService,
            ISysDBTool SysDBTool,
            ISysSettingService SysSettingService,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
            //int period = ReleaseService.List().Count() > 0 ? ReleaseService.List().Max(x => x.Period) + 1 : 1;  //每日释放总期数
            #region 初始变量定义
            var param = cacheSysParam.SingleAndInit(x => x.ID == 1102);//VIP流通奖励      

            decimal PARAM_AllBL = param.Value.ToDecimal();//全区比例：整个区块总流通
            decimal PARAM_TDBL = param.Value2.ToDecimal();//团队比例：团队下VIP会员总流通

            var jfWallet = CurrencyService.Single(x => x.ID == 2);//积分账户
            //var yeWallet = CurrencyService.Single(x => x.ID == 3);//余额账户

            StringBuilder userSql = new StringBuilder();//会员表更新语句  
            List<Data.WalletLog> addWalletLogList = new List<Data.WalletLog>();
            List<Data.BonusDetail> addBonusDetailList = new List<Data.BonusDetail>();

            decimal totalLX = 0;
            int j = 1;
            string bonusDesc;
            decimal all_bonusMoney = 0;//全区奖励金额
            decimal td_bonusMoney = 0;//团队奖励金额         

            DateTime time = DateTime.Now;//当前时间
            DateTime reserveTime = DateTime.Now.AddDays(-1);//为空时

            var sysSet = SysSettingService.List().FirstOrDefault();
            DateTime lastTime = sysSet.ReserveDate2 ?? time.AddYears(-1); //BonusDetailService.List(x => x.BonusID == param.ID).Count() > 0 ? BonusDetailService.List(x => x.BonusID == param.ID).OrderByDescending(x => x.CreateTime).First().CreateTime : time.AddYears(-1);  //上次释放时间

            //所有VIP并且今天没有结算的
            var userlist = UserService.List(x => x.UserLevel >= (int)JN.Data.Enum.UserLevel.Level3 && x.IsLock == false && x.IsActivation).OrderBy(x => x.ID).ToList();// && SqlFunctions.DateDiff("DAY", (x.ReserveDate1 ?? reserveTime), DateTime.Now) != 0

            decimal dayTransfer = 0;//transferList.Sum(x => x.TransferMoney); //全区今日转账金额总和
            decimal tdTransfer = 0; //团队今日转账金额总和

            //今日转账列表
            var transferList = TransferService.ListInclude(x => x.OutTable_User).Where(x => x.CreateTime >= lastTime).ToList();//SqlFunctions.DateDiff("DAY", x.CreateTime, DateTime.Now) == 0

            #endregion                      

            if (jfWallet != null && transferList.Count() > 0)
            {
                DataCache.SetCache("TotalRow2", userlist.Count);
                DataCache.SetCache("StartTime2", DateTime.Now);

                foreach (var item in userlist)
                {
                    DataCache.SetCache("CurrentRow2", j);
                    DataCache.SetCache("TransInfo2", "正在结算“" + item.UserName + "”");

                    //string[] ids = item.ParentPath.Split(',');
                    //var list2 = MvcCore.Unity.Get<IUserService>().List(x => x.RefereePath.Contains("," + item.ID + ",") || x.RefereeID == item.ID).Select(x => x.ID).ToList();
                    //var userIds = list2.ToArray();

                    //var afsf = transferList.Where(x => !userIds.Contains(x.UID.ToString())).Count();
                    //伞下，不包括自己
                    dayTransfer = transferList.Where(x => x.UID != item.ID && (x.OutTable_User.RefereePath.Contains("," + item.ID + ",") || x.OutTable_User.RefereeID == item.ID)).Count() > 0 ? transferList.Where(x => x.UID != item.ID && (x.OutTable_User.RefereePath.Contains("," + item.ID + ",") || x.OutTable_User.RefereeID == item.ID)).Sum(x => x.TransferMoney) : 0;

                    //伞下，不包括自己
                    tdTransfer = transferList.Where(x => x.UID != item.ID && x.OutTable_User.UserLevel >= (int)JN.Data.Enum.UserLevel.Level3 && (x.OutTable_User.RefereePath.Contains("," + item.ID + ",") || x.OutTable_User.RefereeID == item.ID)).Count() > 0 ? transferList.Where(x => x.UID != item.ID && x.OutTable_User.UserLevel >= (int)JN.Data.Enum.UserLevel.Level3 && (x.OutTable_User.RefereePath.Contains("," + item.ID + ",") || x.OutTable_User.RefereeID == item.ID)).Sum(x => x.TransferMoney) : 0;

                    all_bonusMoney = dayTransfer * PARAM_AllBL;
                    td_bonusMoney = tdTransfer * PARAM_TDBL;

                    if (all_bonusMoney > 0)
                    {
                        bonusDesc = "【团队全体会员】" + param.Name + "：" + dayTransfer.ToDouble() + "*" + PARAM_AllBL;
                        addWalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = all_bonusMoney,
                            Balance = (item.Cur3002 ?? 0) + all_bonusMoney,
                            CoinID = jfWallet.ID,
                            CoinName = jfWallet.CurrencyName,
                            CreateTime = time,
                            Description = bonusDesc,
                            UID = item.ID,
                            UserName = item.UserName
                        });

                        //写入奖金表
                        addBonusDetailList.Add(new Data.BonusDetail
                        {
                            Period = 0,
                            BalanceTime = time,
                            BonusMoney = all_bonusMoney,
                            BonusID = param.ID,
                            BonusName = param.Name,
                            CreateTime = time,
                            Description = bonusDesc,
                            IsBalance = true,
                            UID = item.ID,
                            UserName = item.UserName
                        });

                        //更新用户钱包
                        item.Cur3002 = (item.Cur3002 ?? 0) + all_bonusMoney;
                        item.ReserveDecamal2 = (item.ReserveDecamal2 ?? 0) + all_bonusMoney;//进入累计
                    }

                    if (td_bonusMoney > 0)
                    {
                        bonusDesc = "【团队VIP会员】" + param.Name + "：" + tdTransfer.ToDouble() + "*" + PARAM_TDBL;
                        addWalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = td_bonusMoney,
                            Balance = (item.Cur3002 ?? 0) + td_bonusMoney,
                            CoinID = jfWallet.ID,
                            CoinName = jfWallet.CurrencyName,
                            CreateTime = time,
                            Description = bonusDesc,
                            UID = item.ID,
                            UserName = item.UserName
                        });

                        //写入奖金表
                        addBonusDetailList.Add(new Data.BonusDetail
                        {
                            Period = 0,
                            BalanceTime = time,
                            BonusMoney = td_bonusMoney,
                            BonusID = param.ID,
                            BonusName = param.Name,
                            CreateTime = time,
                            Description = bonusDesc,
                            IsBalance = true,
                            UID = item.ID,
                            UserName = item.UserName
                        });

                        //更新用户钱包
                        item.Cur3002 = (item.Cur3002 ?? 0) + td_bonusMoney;
                        item.ReserveDecamal2 = (item.ReserveDecamal2 ?? 0) + td_bonusMoney;//进入累计
                    }

                    if (all_bonusMoney + td_bonusMoney > 0)
                    {
                        userSql.AppendFormat(" update [User] set Cur3002=ISNULL(Cur3002,0)+{0},ReserveDate1='{1}',ReserveDecamal2=ISNULL(ReserveDecamal2,0)+{2} where ID={3}", (all_bonusMoney + td_bonusMoney), time, item.ReserveDecamal2, item.ID);
                    }

                    //给当前用户的
                    totalLX += (all_bonusMoney + td_bonusMoney);
                    j++;
                }

                if (addWalletLogList.Count() > 0)
                {
                    //加入事务
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //批量处理会员的信息
                        DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                        //写入钱包表
                        WalletLogService.BulkInsert(addWalletLogList);
                        //批量写入奖金详情记录
                        BonusDetailService.BulkInsert(addBonusDetailList);

                        sysSet.ReserveDate2 = time;//记录更新时间
                        SysSettingService.Update(sysSet);

                        SysDBTool.Commit();

                        DataCache.SetCache("TransInfo2", "成功对" + userlist.Count + "会员进行结算，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime2")) + "秒");
                        ts.Complete();
                    }
                }
            }
        }
        #endregion

        #region 扣除矿机费用
        /// <summary>
        /// 扣除矿机费用
        /// </summary>
        public static void SubMineCost(IUserService UserService)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            var param = cacheSysParam.Single(x => x.ID == 3109);

            var thisDay = DateTime.Now.Day.ToInt();
            if (thisDay == param.Value.ToInt())
            {
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                //获取投资订单
                var MOlist = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().List(x => x.IsBalance).OrderBy(x => x.ID).ToList();
                var MOlist2 = MOlist.GroupBy(x => x.UID).ToList();//分组

                StringBuilder sql = new StringBuilder();//SQLBuilder实例
                var cModel = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3001);
                Dictionary<int, Data.User> dicUser = UserService.List().ToList().ToDictionary(d => d.ID, d => d);

                foreach (var item in MOlist2)
                {
                    decimal bonusMoney = 0;  //金额
                    //当前会员
                    var onUser = userAllList.Single(x => x.ID == item.Key);

                    var num = item.Count();

                    bonusMoney = num * param.Value2.ToDecimal();

                    string description = "扣除矿机月费:(" + param.Value2 + "×" + num + ")";
                    if ((dicUser[onUser.ID].Cur3001 ?? 0) < bonusMoney)
                    {
                        var bookkeeping = bonusMoney - (dicUser[onUser.ID].Cur3001 ?? 0);//记账数额
                        dicUser[onUser.ID].Cur3001 = (dicUser[onUser.ID].Cur3001 ?? 0) - (dicUser[onUser.ID].Cur3001 ?? 0);
                        dicUser[onUser.ID].Cur3005 = (dicUser[onUser.ID].Cur3005 ?? 0) + bookkeeping;

                        var balance = dicUser[onUser.ID].Cur3001 ?? 0;
                        var balance3005 = dicUser[onUser.ID].Cur3005 ?? 0;

                        description += "余额不足，记账:" + bookkeeping;
                        dicUser[onUser.ID].ReserveInt1 = 1;//更改标记

                        WalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = -(dicUser[onUser.ID].Cur3001 ?? 0),
                            Balance = balance,
                            CoinID = 3001,
                            CoinName = cModel.CurrencyName,
                            CreateTime = DateTime.Now,
                            Description = description,
                            UID = onUser.ID,
                            UserName = onUser.UserName,
                        });
                        WalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = bookkeeping,
                            Balance = balance3005,
                            CoinID = 3005,
                            CoinName = "记账钱包",
                            CreateTime = DateTime.Now,
                            Description = description,
                            UID = onUser.ID,
                            UserName = onUser.UserName,
                        });
                    }
                    else
                    {
                        dicUser[onUser.ID].Cur3001 = (dicUser[onUser.ID].Cur3001 ?? 0) - bonusMoney;
                        var balance = dicUser[onUser.ID].Cur3001 ?? 0;
                        dicUser[onUser.ID].ReserveInt1 = 1;//更改标记
                        //string description = "扣除矿机月费:(" + param.Value2 + "×" + num + ")";
                        WalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = -bonusMoney,
                            Balance = balance,
                            CoinID = 3001,
                            CoinName = cModel.CurrencyName,
                            CreateTime = DateTime.Now,
                            Description = description,
                            UID = onUser.ID,
                            UserName = onUser.UserName,
                        });
                    }
                    #region 未记账版
                    //dicUser[onUser.ID].Cur3001 = (dicUser[onUser.ID].Cur3001 ?? 0) - bonusMoney;
                    //var balance = dicUser[onUser.ID].Cur3001 ?? 0;
                    //dicUser[onUser.ID].ReserveInt1 = 1;//更改标记
                    //string description = "扣除矿机月费:(" + param.Value2 + "×" + num + ")";
                    //WalletLogList.Add(new Data.WalletLog
                    //{
                    //    ChangeMoney = -bonusMoney,
                    //    Balance = balance,
                    //    CoinID = 3001,
                    //    CoinName = cModel.CurrencyName,
                    //    CreateTime = DateTime.Now,
                    //    Description = description,
                    //    UID = onUser.ID,
                    //    UserName = onUser.UserName,
                    //});
                    #endregion

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

        #region 结算矿机收益
        /// <summary>
        /// 预结算矿机收益
        /// </summary>
        public static void AdvanceMills()
        {
            var MOlist = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().List(x => x.IsBalance).ToList();
            StringBuilder MachineOrderSql = new StringBuilder();//矿机表更新语句
            DateTime time = DateTime.Now;
            decimal bonusMoney = 0;//最终获得金额
            foreach (var item in MOlist)
            {
                if (item.ShopID == 1)
                {
                    bonusMoney = item.TimesType / item.Duration * item.TodayMiningTime;
                    if ((item.AddupInterest ?? 0) + bonusMoney >= item.TopBonus)//达到收益封顶
                    {
                        item.ReserveStr1 += "累计收益：" + (item.AddupInterest ?? 0) + "，本次收益：" + bonusMoney + "，封顶收益：" + item.TopBonus;
                        bonusMoney = item.TopBonus - (item.AddupInterest ?? 0);
                        item.IsBalance = false;
                    }
                    if ((item.ReserveInt1 ?? 0) + 1 == (item.ReserveInt2 ?? 0))
                    {
                        item.ReserveStr1 += "累计收益：" + (item.AddupInterest ?? 0) + "，本次收益：" + bonusMoney + "，封顶收益：" + item.TopBonus + "，使用时间已到：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        item.IsBalance = false;
                    }
                }
                else
                {
                    bonusMoney = item.TimesType;
                    if ((item.AddupInterest ?? 0) + bonusMoney >= item.TopBonus)//达到收益封顶
                    {
                        item.ReserveStr1 += "累计收益：" + (item.AddupInterest ?? 0) + "，本次收益：" + bonusMoney + "，封顶收益：" + item.TopBonus;
                        bonusMoney = item.TopBonus - (item.AddupInterest ?? 0);
                        item.IsBalance = false;
                    }
                }


                item.WaitExtractIncome = (item.WaitExtractIncome ?? 0) + bonusMoney;
                item.ReserveInt1 = (item.ReserveInt1 ?? 0) + 1;
                item.AddupInterest = (item.AddupInterest ?? 0) + bonusMoney;
                item.LastProfitTime = DateTime.Now;
                MachineOrderSql.AppendFormat(" update [MachineOrder] set IsBalance={0}, WaitExtractIncome={1}, AddupInterest={2}, LastProfitTime='{3}', ReserveStr1='{4}', ReserveInt1='{5}',TodaySettlement={6} where ID={7}", (item.IsBalance == true ? 1 : 0), item.WaitExtractIncome, item.AddupInterest, item.LastProfitTime, item.ReserveStr1, item.ReserveInt1, bonusMoney, item.ID);
            }
            if (MachineOrderSql.Length != 0)
            {
                //批量处理矿机表的信息
                System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().ExecuteSQL(MachineOrderSql.ToString(), dbparam);
                MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();
            }
        }
        #endregion

        #region 发放矿机奖励-静态收益、直推奖、管理奖  KJ190416
        /// <summary>
        ///  发放矿机奖励-静态收益、直推奖、管理奖  KJ190416
        /// </summary>
        /// <param name="cacheSysParam"></param>
        /// <param name="cacheCurrency"></param>
        public static void ExtractProfit(List<Data.SysParam> cacheSysParam, List<Data.Currency> cacheCurrency,int balancemode)
        {
            //var blParam = cacheSysParam.Single(x => x.ID == 1110);
            //decimal FJB_BL = blParam.Value.ToDecimal();
            //decimal FJJ_BL = blParam.Value2.ToDecimal();
            var releaseList = MvcCore.Unity.Get<IReleaseService>().List(x => x.Type == 1);
            int period = releaseList.Count() > 0 ? releaseList.Max(x => x.Period) + 1 : 1;  //每日释放总期数

            var MOlist = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().List(x => x.IsBalance).ToList();

            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            if (userAllList.Count() <= 0) return;
            //把相关会员放到内存
            Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);

            List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
            List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
            //StringBuilder sql = new StringBuilder();//矿机表更新语句
            var sqlString = new List<string>();

            DateTime time = DateTime.Now;
            decimal bonusMoney = 0;//最终获得金额
            decimal totalLX = 0;

            var cModel3003 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3003);
            var param1101 = cacheSysParam.SingleAndInit(x=>x.ID==1101);

            foreach (var item in MOlist)
            {
                var onUser = dicUser[item.UID];
                bonusMoney = item.HashnestNum * item.TimesType;
                if (bonusMoney > 0)
                {
                    string description = "矿机" + item.InvestmentNo + "的收益：" + bonusMoney;//奖金描述
                    dicUser[item.UID].Cur3003 = (dicUser[item.UID].Cur3003 ?? 0) + bonusMoney;

                    //写入财务明细表、奖金明细表
                    Bonus.UpdateUserWallet(bonusMoney, param1101.ID, param1101.Name, (dicUser[item.UID].Cur3003 ?? 0), description, onUser, onUser, cModel3003, ref BonusDetailList, ref WalletLogList);
                    
                    dicUser[item.UID].ReserveInt1 = 123456;//更改标记
                    item.AddupInterest = (item.AddupInterest ?? 0) + bonusMoney;
                    item.LastProfitTime = DateTime.Now;
                    item.ReserveInt1 = (item.ReserveInt1 ?? 0) + 1;

                    string str = "";
                    if((item.ReserveInt1 ?? 0)>= (item.Duration*item.ActivationNums)) str = " ,IsBalance=0 ";//判断是否出局
                    sqlString.Add(string.Format(" update [MachineOrder] set AddupInterest={0}, LastProfitTime='{1}', ReserveStr1='{2}', ReserveInt1='{3}',TodaySettlement={4}"+ str + " where ID={5}", item.AddupInterest, item.LastProfitTime, item.ReserveStr1, item.ReserveInt1, bonusMoney, item.ID));

                    #region 直推奖 、管理奖
                    Bonus1102_1103(onUser, bonusMoney, cacheSysParam, cacheCurrency, userAllList, "矿机" + item.InvestmentNo + "收益", ref BonusDetailList, ref WalletLogList, ref dicUser);
                    #endregion
                    totalLX += bonusMoney;
                }
            }

            #region 更新提交
            foreach (var item in dicUser)
            {
                if ((item.Value.ReserveInt1 ?? 0) == 123456)
                {
                    sqlString.Add(string.Format(" update [User] set Cur3003={0},Addup1102={1},Addup1103={2} where ID={3} ", (item.Value.Cur3003 ?? 0), (item.Value.Addup1102 ?? 0), (item.Value.Addup1103 ?? 0), item.Value.ID));
                }
            }

            if (sqlString.Count > 0)
            {
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //导入
                   if(WalletLogList.Count()>0) MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);
                    if (BonusDetailList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);

                    MvcCore.Unity.Get<JN.Data.Service.IReleaseService>().Add(new Data.Release { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = MOlist.Count, Type=1 });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                    var skip = 0;
                    while (skip < sqlString.Count)
                    {
                        var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                        skip += 2000;
                        MvcCore.Unity.Get<ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                    }
                    sqlString.Clear();


                    DataCache.SetCache("TransInfo", "结算成功，用时：" + DateTimeDiff.DateDiff_Sec(DateTime.Now, (DateTime)DataCache.GetCache("StartTime")) + "秒");
                    ts.Complete();
                }
            }
            #endregion
        }
        #endregion

        #region 直推奖、管理奖  KJ190416
        /// <summary>
        ///  直推奖、管理奖  KJ190416
        /// </summary>
        /// <param name="onUser"></param>
        /// <param name="money"></param>
        /// <param name="cacheSysParam"></param>
        /// <param name="cacheCurrency"></param>
        /// <param name="userAllList"></param>
        /// <param name="BonusDetailList"></param>
        /// <param name="WalletLogList"></param>
        /// <param name="dicUser"></param>
        public static void Bonus1102_1103(JN.Data.User onUser,decimal money, List<Data.SysParam> cacheSysParam,List<Data.Currency> cacheCurrency,List<Data.User> userAllList,string descri, ref List<Data.BonusDetail> BonusDetailList, ref List<Data.WalletLog> WalletLogList, ref Dictionary<int, Data.User> dicUser)
        {
            var cModel3003 = cacheCurrency.SingleAndInit(x=>(x.WalletCurID??0)==3003);

            #region 直推奖  --普通会员每天拿直推静态收益的10%。如果不购买矿机，就不能获得直推奖；只有普通会员 获得直推奖  如果升级了 就拿不到直推奖了
            var redModel = userAllList.SingleAndInit(x => x.UserLevel == 0 && x.ID == onUser.RefereeID && x.IsActivation && !x.IsLock && x.Investment>0);
            if (redModel.ID>0)
            {
                var param1102 = cacheSysParam.SingleAndInit(x=>x.ID==1102);
                decimal redMoney = money * param1102.Value.ToDecimal();
                if (redMoney > 0)
                {
                   string bonusDesc = "来自会员【" + onUser.UserName + "】"+ descri + "的" + param1102.Name + "(" + money.ToDouble() + "*" + param1102.Value + ")";
                    dicUser[redModel.ID].Cur3003 = (dicUser[redModel.ID].Cur3003 ?? 0) + redMoney;
                    dicUser[redModel.ID].Addup1102 = (dicUser[redModel.ID].Addup1102 ?? 0) + redMoney;
                    dicUser[redModel.ID].ReserveInt1 = 123456;//更改标记
                    //写入财务明细表、奖金明细表
                    Bonus.UpdateUserWallet(redMoney, param1102.ID, param1102.Name, (dicUser[onUser.ID].Cur3003 ?? 0), bonusDesc, redModel, onUser, cModel3003, ref BonusDetailList, ref WalletLogList);
                }
            }
            #endregion

            #region 管理奖 --成为矿工后可拿伞下无限代静态收益的10%（不可重复）
            string[] ids = onUser.RefereePath.Split(',');
            var refUserlist = userAllList.Where(x => ids.Contains(x.ID.ToString()) && x.IsActivation && !x.IsLock).OrderByDescending(x => x.RefereeDepth).ToList();
            if (refUserlist.Count() > 0)
            {
                var param1103 = cacheSysParam.SingleAndInit(x => x.ID == 1103);
                decimal DoValue = 0;  //最高可拿
                decimal jicha = 0;
                foreach (var gljUser in refUserlist)
                {
                    decimal PARAM_GLJFD = 0;  //极差奖可拿比例
                    if(gljUser.UserLevel>=1 && gljUser.UserLevel <= 8)
                    {
                        int level = 1300 + gljUser.UserLevel;
                        PARAM_GLJFD = cacheSysParam.SingleAndInit(x => x.ID == level).Value.ToDecimal();
                    }
    
                    if (jicha >= PARAM_GLJFD)
                    {
                        continue;
                    }
                    else
                    {
                        DoValue = PARAM_GLJFD - jicha;  //可得奖金比例
                        jicha = PARAM_GLJFD;
                    }

                    if (DoValue > 0)  //管理奖还可拿并且级别符合条件
                    {
                        decimal bonusMoney = money * DoValue;
                        if (bonusMoney > 0)            //推荐几单拿几代
                        {
                            string bonusDesc = "来自第"+ (onUser.RefereeDepth-gljUser.RefereeDepth) + "代会员【" + onUser.UserName + "】" + descri + "的" + param1103.Name + "(" + money.ToDouble() + "*" + DoValue + ")";
                            dicUser[gljUser.ID].Cur3003 = (dicUser[gljUser.ID].Cur3003 ?? 0) + bonusMoney;
                            dicUser[gljUser.ID].Addup1103 = (dicUser[gljUser.ID].Addup1103 ?? 0) + bonusMoney;
                            dicUser[gljUser.ID].ReserveInt1 = 123456;//更改标记
                            //写入财务明细表、奖金明细表
                            Bonus.UpdateUserWallet(bonusMoney, param1103.ID, param1103.Name, (dicUser[onUser.ID].Cur3003 ?? 0), bonusDesc, gljUser, onUser, cModel3003, ref BonusDetailList, ref WalletLogList);
                        }
                    }
                }
            }
            #endregion
        }
        #endregion
        
        #region 个人销售佣金   KJ190416
        /// <summary>
        ///  个人销售佣金   KJ190416
        /// </summary>
        /// 假定A会员某月个人业绩达6000元，那么A的个人佣金为6000*9%=540BC
        /// <param name="money"></param>
        /// <param name="user"></param>
        /// <param name="UserService"></param>
        public static void Bonus1104(List<Data.SysParam> cacheSysParam, List<Data.Currency> cacheCurrency, int balancemode)
        {
            var releaseList = MvcCore.Unity.Get<IReleaseService>().List(x => x.Type == 2);
            int period = releaseList.Count() > 0 ? releaseList.Max(x => x.Period) + 1 : 1;  //每日释放总期数

            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            //获取当月业绩
            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List(x => SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0).ToList();

            if (teamAchievementList.Count > 0)
            {
                var param = cacheSysParam.Single(x => x.ID == 1104);//奖金对象
                var cModel3004 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3004);

                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                // StringBuilder sql = new StringBuilder();//SQLBuilder实例
                List<string> sqlString = new List<string>();
                decimal totalLX = 0;

                foreach (var item in teamAchievementList)
                {
                    //logs.WriteLog(model.Key.ToString());
                    //当前会员
                    var onUser = dicUser[item.UID];

                    #region 获取比例
                    decimal param_BL = 0;//比例
                    decimal param_YJ = 0;//业绩值
                    for (int i = 1207; i >= 1201; i--)
                    {
                        var param1200 = cacheSysParam.SingleAndInit(x => x.ID == i);
                        if (item.Investment >= param1200.Value2.ToDecimal())
                        {
                            param_BL = param1200.Value.ToDecimal();
                            param_YJ = param1200.Value2.ToDecimal();
                            break;
                        }
                    }
                    #endregion

                    decimal bonusMoney = item.Investment * param_BL;  //金额
                    string description = "个人业绩达" + param_YJ + ",获得" + param.Name + "(" + item.Investment + "×" + param_BL + ")";//奖金描述
                    if (bonusMoney > 0)
                    {
                        dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney;

                        dicUser[onUser.ID].Addup1104 = (dicUser[onUser.ID].Addup1104 ?? 0) + bonusMoney;
                        dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记

                        //写入财务明细表、奖金明细表
                        Bonus.UpdateUserWallet(bonusMoney, param.ID, param.Name, (dicUser[onUser.ID].Cur3004 ?? 0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                        totalLX += bonusMoney;
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 123456789)
                    {
                        sqlString.Add(string.Format(" update [User] set Cur3004={0},Addup1104={1} where ID={2} ", (item.Value.Cur3004 ?? 0), (item.Value.Addup1104 ?? 0), item.Value.ID));
                    }
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //导入
                    if (BonusDetailList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);
                    if (WalletLogList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);

                    MvcCore.Unity.Get<JN.Data.Service.IReleaseService>().Add(new Data.Release { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = teamAchievementList.Count, Type = 2 });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    if (sqlString.Count > 0)
                    {
                        System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        var skip = 0;
                        while (skip < sqlString.Count)
                        {
                            var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                            skip += 2000;
                            MvcCore.Unity.Get<ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                        }
                        sqlString.Clear();
                    }
                    
                    ts.Complete();
                }
            }

        }
        #endregion

        #region 市场推广佣金   KJ190416
        /// <summary>
        ///  市场推广佣金   KJ190416
        /// </summary>
        /// <param name="money"></param>
        /// <param name="user"></param>
        /// <param name="UserService"></param>
        public static void Bonus1105(List<Data.SysParam> cacheSysParam, List<Data.Currency> cacheCurrency, int balancemode)
        {
            var releaseList = MvcCore.Unity.Get<IReleaseService>().List(x => x.Type == 3);
            int period = releaseList.Count() > 0 ? releaseList.Max(x => x.Period) + 1 : 1;  //每日释放总期数
            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            //获取当月业绩
            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List(x => SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0);

            if (teamAchievementList.Count() > 0)
            {
                var param1105 = cacheSysParam.Single(x => x.ID == 1105);//奖金对象
                var param1106 = cacheSysParam.Single(x => x.ID == 1106);//奖金对象
                var param1107 = cacheSysParam.Single(x => x.ID == 1107);//奖金对象
                var cModel3004 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3004);

                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                List<string> sqlString = new List<string>();
                decimal totalLX = 0;

                foreach (var item in teamAchievementList)
                {
                    //logs.WriteLog(model.Key.ToString());
                    //当前会员
                    var onUser = dicUser[item.UID];
                    
                    #region 获取比例
                    decimal param_BL = 0;//比例
                    decimal param_YJ = 0;//业绩值
                    for (int i = 1207; i >= 1201; i--)
                    {
                        var param1200 = cacheSysParam.SingleAndInit(x => x.ID == i);
                        if (item.TeamInvestment >= param1200.Value2.ToDecimal()) //团队业绩
                        {
                            param_BL = param1200.Value.ToDecimal();
                            param_YJ = param1200.Value2.ToDecimal();
                            break;
                        }
                    }
                    #endregion

                    #region  获取市场下的业绩
                    decimal childUserInvestment = 0;//市场下的业绩
                    
                    decimal allchildUserInvestment = 0;//伞下市场总业绩
                    decimal childUserInvestment1106 = 0;//条件 1)完成伞下一个达标市场，即总业绩达到125000；，即总业绩达到125000
                    decimal childUserInvestment1107 = 0;//条件 1)完成伞下2个达标市场，即总业绩达到125000；，即总业绩达到125000

                    var redUserList = userAllList.Where(x => x.RefereeID == item.UID).ToList();//我直推的会员
                    foreach (var reduser in redUserList)//查找每个市场 
                    {
                        if (teamAchievementList.Where(x => x.UID == reduser.ID).Count()>0)
                        {
                            var childUser = teamAchievementList.Single(x => x.UID == reduser.ID);//

                            #region 市场推广佣金  获取数据
                            for (int i = 1207; i >= 1201; i--)
                            {
                                var param1200 = cacheSysParam.SingleAndInit(x => x.ID == i);
                                if (childUser.TeamInvestment >= param1200.Value2.ToDecimal()) //团队业绩
                                {
                                    childUserInvestment += childUser.TeamInvestment * param1200.Value.ToDecimal();
                                    param_YJ = param1200.Value2.ToDecimal();
                                    break;
                                }
                            }
                            #endregion

                            #region 培育奖 获取数据
                            allchildUserInvestment += childUser.TeamInvestment;
                            if (childUser.TeamInvestment >= param1106.Value2.ToDecimal()) //团队业绩
                            {
                                childUserInvestment1106 += childUser.TeamInvestment;
                            }
                            #endregion
                            
                            #region 拓展奖 获取数据
                            if (childUser.TeamInvestment >= param1107.Value2.ToDecimal()) //团队业绩
                            {
                                childUserInvestment1107 += childUser.TeamInvestment;
                            }
                            #endregion
                        }
                    }
                    #endregion

                    #region 市场推广佣金
                    decimal bonusMoney1105 = item.TeamInvestment * param_BL- childUserInvestment;  //金额
                    string description = "获得" + param1105.Name;//奖金描述
                    if (bonusMoney1105 > 0)
                    {
                        dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney1105;

                        dicUser[onUser.ID].Addup1105 = (dicUser[onUser.ID].Addup1105 ?? 0) + bonusMoney1105;
                        dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记

                        //写入财务明细表、奖金明细表
                        Bonus.UpdateUserWallet(bonusMoney1105, param1105.ID, param1105.Name, (dicUser[onUser.ID].Cur3004??0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                        totalLX += bonusMoney1105;
                    }
                    #endregion

                    #region  培育奖
                    //条件 1)完成伞下1个达标市场，即1个达标市场总业绩达到125000
                    // 条件 2)其它非达标市场总业绩达50000；
                    // 公式：达标市场总业绩的4%
                    decimal nonattainment = allchildUserInvestment - childUserInvestment1106;//非达标市场总业绩 = 伞下总业绩 - 达标市场业绩
                    if (nonattainment >= param1106.Value3.ToDecimal())
                    {
                        decimal bonusMoney1106 = childUserInvestment1106 * param1106.Value.ToDecimal();  //金额
                        description = "获得" + param1106.Name;//奖金描述
                        if (bonusMoney1106 > 0)
                        {
                            dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney1106;
                            dicUser[onUser.ID].Addup1106 = (dicUser[onUser.ID].Addup1106 ?? 0) + bonusMoney1106;
                            dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记

                            //写入财务明细表、奖金明细表
                            Bonus.UpdateUserWallet(bonusMoney1106, param1106.ID, param1106.Name, (dicUser[onUser.ID].Cur3004 ?? 0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                            totalLX += bonusMoney1106;
                        }
                    }
                    #endregion

                    #region  拓展奖
                    //条件 1)完成伞下两个达标市场，即两个达标市场总业绩达到125000
                    // 条件 2)其它非达标市场总业绩达50000；
                    //公式：个人+未达标市场总业绩的2%
                    decimal nonattainment1107 = allchildUserInvestment - childUserInvestment1107;
                    if (nonattainment1107 >= param1107.Value3.ToDecimal() && childUserInvestment1107>= param1107.Value2.ToDecimal()*2)
                    {
                        decimal bonusMoney1107 =(item.Investment+ nonattainment1107) * param1107.Value.ToDecimal();  //金额
                        description = "获得" + param1107.Name;//奖金描述
                        if (bonusMoney1107 > 0)
                        {
                            dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney1107;
                            dicUser[onUser.ID].Addup1107 = (dicUser[onUser.ID].Addup1107 ?? 0) + bonusMoney1107;
                            dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记
                            //写入财务明细表、奖金明细表
                            Bonus.UpdateUserWallet(bonusMoney1107, param1107.ID, param1107.Name, (dicUser[onUser.ID].Cur3004 ?? 0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                            totalLX += bonusMoney1107;
                        }
                    }
                    #endregion
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 123456789)
                    {
                        sqlString.Add(string.Format(" update [User] set Cur3004={0},Addup1105={1},Addup1106={2},Addup1107={3} where ID={4} ", (item.Value.Cur3004 ?? 0), (item.Value.Addup1105 ?? 0), (item.Value.Addup1106 ?? 0), (item.Value.Addup1107 ?? 0), item.Value.ID));
                    }
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //导入
                    if (BonusDetailList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);
                    if (WalletLogList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);
                    MvcCore.Unity.Get<JN.Data.Service.IReleaseService>().Add(new Data.Release { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = teamAchievementList.Count(), Type = 3 });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    if (sqlString.Count > 0)
                    {
                        System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        var skip = 0;
                        while (skip < sqlString.Count)
                        {
                            var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                            skip += 2000;
                            MvcCore.Unity.Get<ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                        }
                        sqlString.Clear();
                    }

                    ts.Complete();
                }
            }

        }
        #endregion

        #region 培育奖   KJ190416
        /// <summary>
        ///  培育奖   KJ190416
        /// </summary>
        /// <param name="money"></param>
        /// <param name="user"></param>
        /// <param name="UserService"></param>
        public static void Bonus1106(List<Data.SysParam> cacheSysParam, List<Data.Currency> cacheCurrency)
        {
            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            //获取当月业绩
            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List(x => SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0).ToList();

            if (teamAchievementList.Count > 0)
            {
                var param = cacheSysParam.Single(x => x.ID == 1106);//奖金对象
                var cModel3004 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3004);

                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                // StringBuilder sql = new StringBuilder();//SQLBuilder实例
                List<string> sqlString = new List<string>();

                foreach (var item in teamAchievementList)
                {
                    //logs.WriteLog(model.Key.ToString());
                    //当前会员
                    var onUser = dicUser[item.UID];

                    #region  获取市场下的业绩
                    // 条件 1)完成伞下一个达标市场，即总业绩达到125000；
                    decimal childUserInvestment = 0;//标市场，即总业绩达到125000
                    decimal allchildUserInvestment = 0;//伞下市场总业绩
                    var redUserList = userAllList.Where(x => x.RefereeID == item.ID).ToList();//我直推的会员
                    foreach (var reduser in redUserList)//查找每个市场 
                    {
                        var childUser = teamAchievementList.Single(x => x.UID == reduser.ID);//
                        if (childUser != null)
                        {
                            allchildUserInvestment += (childUser.Investment + childUser.TeamInvestment);
                            if ((childUser.Investment + childUser.TeamInvestment) >= param.Value2.ToDecimal()) //自身业绩+团队业绩
                            {
                                childUserInvestment += (childUser.Investment + childUser.TeamInvestment);
                            }
                        }
                    }
                    // 条件 2)其它非达标市场总业绩达50000；
                    decimal nonattainment = allchildUserInvestment - childUserInvestment;
                    #endregion
                    if (nonattainment >= param.Value3.ToDecimal())
                    {

                        decimal bonusMoney = childUserInvestment * param.Value.ToDecimal();  //金额
                        string description = "获得" + param.Name;//奖金描述
                        if (bonusMoney > 0)
                        {
                            dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney;

                            dicUser[onUser.ID].Addup1106 = (dicUser[onUser.ID].Addup1106 ?? 0) + bonusMoney;
                            dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记

                            //写入财务明细表、奖金明细表
                            Bonus.UpdateUserWallet(bonusMoney, param.ID, param.Name, (dicUser[onUser.ID].Cur3004 ?? 0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                        }

                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 123456789)
                    {
                        sqlString.Add(string.Format(" update [User] set Cur3004={0},Addup1106={1} where ID={2} ", (item.Value.Cur3004 ?? 0), (item.Value.Addup1106 ?? 0), item.Value.ID));
                    }
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //导入
                    if (BonusDetailList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);
                    if (WalletLogList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);

                    if (sqlString.Count > 0)
                    {
                        System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        var skip = 0;
                        while (skip < sqlString.Count)
                        {
                            var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                            skip += 2000;
                            MvcCore.Unity.Get<ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                        }
                        sqlString.Clear();
                    }
                    ts.Complete();
                }
            }
        }
        #endregion

        #region 拓展奖   KJ190416
        /// <summary>
        ///  拓展奖   KJ190416
        /// </summary>
        /// <param name="money"></param>
        /// <param name="user"></param>
        /// <param name="UserService"></param>
        public static void Bonus1107(List<Data.SysParam> cacheSysParam, List<Data.Currency> cacheCurrency)
        {
            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            //获取当月业绩
            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List(x => SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0).ToList();

            if (teamAchievementList.Count > 0)
            {
                var param = cacheSysParam.Single(x => x.ID == 1107);//奖金对象
                var cModel3004 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3004);

                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                // StringBuilder sql = new StringBuilder();//SQLBuilder实例
                List<string> sqlString = new List<string>();

                foreach (var item in teamAchievementList)
                {
                    //logs.WriteLog(model.Key.ToString());
                    //当前会员
                    var onUser = dicUser[item.UID];

                    #region  获取市场下的业绩
                    // 条件 1)完成伞下一个达标市场，即总业绩达到125000；
                    decimal childUserInvestment = 0;//标市场，即总业绩达到125000
                    decimal allchildUserInvestment = 0;//伞下市场总业绩
                    var redUserList = userAllList.Where(x => x.RefereeID == item.ID).ToList();//我直推的会员
                    foreach (var reduser in redUserList)//查找每个市场 
                    {
                        var childUser = teamAchievementList.Single(x => x.UID == reduser.ID);//
                        if (childUser != null)
                        {
                            allchildUserInvestment += (childUser.Investment + childUser.TeamInvestment);
                            if ((childUser.Investment + childUser.TeamInvestment) >= param.Value2.ToDecimal()) //自身业绩+团队业绩
                            {
                                childUserInvestment += (childUser.Investment + childUser.TeamInvestment);
                            }
                        }
                    }
                    // 条件 2)其它非达标市场总业绩达50000；
                    decimal nonattainment = allchildUserInvestment - childUserInvestment;
                    #endregion
                    if (nonattainment >= param.Value3.ToDecimal() && childUserInvestment >= param.Value2.ToDecimal()*2)
                    {
                        decimal bonusMoney = nonattainment * param.Value.ToDecimal();  //金额
                        string description = "获得" + param.Name;//奖金描述
                        if (bonusMoney > 0)
                        {
                            dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney;

                            dicUser[onUser.ID].Addup1107 = (dicUser[onUser.ID].Addup1107 ?? 0) + bonusMoney;
                            dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记

                            //写入财务明细表、奖金明细表
                            Bonus.UpdateUserWallet(bonusMoney, param.ID, param.Name, (dicUser[onUser.ID].Cur3004 ?? 0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                        }
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 123456789)
                    {
                        sqlString.Add(string.Format(" update [User] set Cur3004={0},Addup1107={1} where ID={2} ", (item.Value.Cur3004 ?? 0), (item.Value.Addup1107 ?? 0), item.Value.ID));
                    }
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //导入
                    if (BonusDetailList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);
                    if (WalletLogList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);

                    if (sqlString.Count > 0)
                    {
                        System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        var skip = 0;
                        while (skip < sqlString.Count)
                        {
                            var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                            skip += 2000;
                            MvcCore.Unity.Get<ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                        }
                        sqlString.Clear();
                    }
                    ts.Complete();
                }
            }
        }
        #endregion

        #region 深度培育奖   KJ190416
        /// <summary>
        ///  深度培育奖   KJ190416
        /// </summary>
        /// 条件：完成伞下A、B、C三个达标市场，即三个达标市场总业绩达到125000；
        ///公式：A、B、C三个市场以下达标市场（总业绩达125000）无限代的1%；  拿这三个达标市场   伞下达标会员的业绩来*比例

        /// <param name="money"></param>
        /// <param name="user"></param>
        /// <param name="UserService"></param>
        public static void Bonus1108(List<Data.SysParam> cacheSysParam, List<Data.Currency> cacheCurrency,int balancemode)
        {
            var releaseList = MvcCore.Unity.Get<IReleaseService>().List(x => x.Type == 6);
            int period = releaseList.Count() > 0 ? releaseList.Max(x => x.Period) + 1 : 1;  //每日释放总期数
            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            //获取当月业绩
            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List(x => SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0).ToList();

            if (teamAchievementList.Count > 0)
            {
                var param = cacheSysParam.Single(x => x.ID == 1108);//奖金对象
                var cModel3004 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3004);

                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                List<string> sqlString = new List<string>();
                decimal totalLX = 0;

                foreach (var item in teamAchievementList)
                {
                    //当前会员
                    var onUser = dicUser[item.UID];
                    #region  获取市场下的业绩
                    // 条件 1)完成伞下一个达标市场，即总业绩达到125000；
                    decimal childUserInvestment = 0;//标市场，即总业绩达到125000
                    var redUserList = userAllList.Where(x => x.RefereeID == onUser.ID).ToList();//我直推的会员
                    int[] red_ids = redUserList.Select(x => x.ID).ToArray();
                    var redTeam = teamAchievementList.Where(x => red_ids.Contains(x.UID) && x.TeamInvestment >= param.Value2.ToDecimal());
                    if (redTeam.Count() >= 3)
                    {
                        foreach (var reduser in redUserList)//查找每个市场 
                        {
                            var childUserList = userAllList.Where(x => (x.RefereePath + ",").Contains("," + reduser.ID + ",")).ToList();//伞下会员
                            if (childUserList.Count() > 0)
                            {
                                int[] child_ids = childUserList.Select(x => x.ID).ToArray();
                                var childUser = teamAchievementList.Where(x => child_ids.Contains(x.UID) && x.TeamInvestment >= param.Value2.ToDecimal());//
                                if (childUser.Count() > 0)
                                {
                                    childUserInvestment += childUser.Sum(x => x.TeamInvestment); //自身业绩+团队业绩
                                }
                            }
                        }

                        #endregion

                        decimal bonusMoney = childUserInvestment * param.Value.ToDecimal();  //金额
                        string description = "获得" + param.Name;//奖金描述
                        if (bonusMoney > 0)
                        {
                            dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney;
                            dicUser[onUser.ID].Addup1108 = dicUser[onUser.ID].Addup1108 + bonusMoney;
                            dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记
                                                                       //写入财务明细表、奖金明细表
                            Bonus.UpdateUserWallet(bonusMoney, param.ID, param.Name, (dicUser[onUser.ID].Cur3004 ?? 0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                            totalLX += bonusMoney;
                        }
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 123456789)
                    {
                        sqlString.Add(string.Format(" update [User] set Cur3004={0},Addup1108={1} where ID={2} ", (item.Value.Cur3004 ?? 0), item.Value.Addup1108, item.Value.ID));
                    }
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //导入
                    if (BonusDetailList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);
                    if (WalletLogList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);
                    MvcCore.Unity.Get<JN.Data.Service.IReleaseService>().Add(new Data.Release { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = teamAchievementList.Count, Type = 6 });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    if (sqlString.Count > 0)
                    {
                        System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        var skip = 0;
                        while (skip < sqlString.Count)
                        {
                            var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                            skip += 2000;
                            MvcCore.Unity.Get<ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                        }
                        sqlString.Clear();
                    }
                    ts.Complete();
                }
            }
        }
        #endregion

        #region 高级权益   KJ190416
        /// <summary>
        ///  高级权益   KJ190416
        /// </summary>
        /// 权益：年终股东分红  0.25%（加权分红）
        /// 公式：年营业额*0.25%，除以全网符合特资深矿池数量
        
        /// <param name="money"></param>
        /// <param name="user"></param>
        /// <param name="UserService"></param>
        public static void Bonus1109(List<Data.SysParam> cacheSysParam, List<Data.Currency> cacheCurrency,int balancemode)
        {
            var releaseList = MvcCore.Unity.Get<IReleaseService>().List(x => x.Type == 7);
            int period = releaseList.Count() > 0 ? releaseList.Max(x => x.Period) + 1 : 1;  //每日释放总期数
            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List(x=>x.IsActivation && !(x.IsLock) && x.UserLevel>=3).ToList();
            //获取当月业绩
            var teamAchievementList = MvcCore.Unity.Get<Data.Service.ITeamAchievementService>().List(x => SqlFunctions.DateDiff("month", x.CreateTime, DateTime.Now) == 0).ToList();

            if (userAllList.Count > 0)
            {
                var param = cacheSysParam.Single(x => x.ID == 1109);//奖金对象
                var cModel3004 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3004);

                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                List<string> sqlString = new List<string>();
                decimal totalLX = 0;

                decimal cMoney = param.Value.ToDecimal() * param.Value2.ToDecimal();//年营业额*0.25%
                foreach (var item in userAllList)
                {
                    //当前会员
                    var onUser = dicUser[item.ID];
                    var rUserList = userAllList.Where(x => x.UserLevel == item.UserLevel).ToList();//统计同级别有多少人
                    decimal bonusMoney = cMoney / rUserList.Count();  //金额
                    string description = "获得" + param.Name+"加权分红("+ param.Value.ToDecimal() + "×" + param.Value2.ToDecimal()+"÷"+rUserList.Count()+")";//奖金描述
                    if (bonusMoney > 0)
                    {
                        dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + bonusMoney;
                        dicUser[onUser.ID].Addup1109 = dicUser[onUser.ID].Addup1109 + bonusMoney;
                        dicUser[onUser.ID].ReserveInt1 = 123456789;//更改标记
                        //写入财务明细表、奖金明细表
                        Bonus.UpdateUserWallet(bonusMoney, param.ID, param.Name, (dicUser[onUser.ID].Cur3004 ?? 0), description, onUser, onUser, cModel3004, ref BonusDetailList, ref WalletLogList);
                        totalLX += bonusMoney;
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 123456789)
                    {
                        sqlString.Add(string.Format(" update [User] set Cur3004={0},Addup1109={1} where ID={2} ", (item.Value.Cur3004 ?? 0), item.Value.Addup1109, item.Value.ID));
                    }
                }

                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //导入
                    if (BonusDetailList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);
                    if (WalletLogList.Count() > 0) MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);
                    MvcCore.Unity.Get<JN.Data.Service.IReleaseService>().Add(new Data.Release { BalanceMode = balancemode, CreateTime = DateTime.Now, Period = period, TotalBonus = totalLX, TotalUser = teamAchievementList.Count, Type = 7 });
                    MvcCore.Unity.Get<JN.Data.Service.ISysDBTool>().Commit();

                    if (sqlString.Count > 0)
                    {
                        System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        var skip = 0;
                        while (skip < sqlString.Count)
                        {
                            var cmd = string.Join(Environment.NewLine, sqlString.Skip(skip).Take(2000));
                            skip += 2000;
                            MvcCore.Unity.Get<ISysDBTool>().ExecuteSQL(cmd, dbparam);//提交数据 
                        }
                        sqlString.Clear();
                    }
                    ts.Complete();
                }
            }
        }
        #endregion


        #region 团队分红
        /// <summary>
        /// 团队分红
        /// </summary>
        /// <param name="money"></param>
        /// <param name="user"></param>
        /// <param name="UserService"></param>
        public static void Bonus1103(IUserService UserService)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();
            var param = cacheSysParam.Single(x => x.ID == 1103);//奖金对象

            var blParam = cacheSysParam.Single(x => x.ID == 1110);
            decimal FJB_BL = blParam.Value.ToDecimal();
            decimal FJJ_BL = blParam.Value2.ToDecimal();

            var userAllList = MvcCore.Unity.Get<Data.Service.IUserService>().List().ToList();
            //获取当天奖金记录和封顶参数
            var bonusDayList = MvcCore.Unity.Get<Data.Service.IBonusDetailService>().List(x => SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0).ToList();
            //获取投资订单
            var MOlist = MvcCore.Unity.Get<JN.Data.Service.IMachineOrderService>().List(x => x.TodaySettlement > 0).OrderBy(x => x.ID).ToList();

            var MOlist2 = MOlist.Where(x => x.TodaySettlement > 0).GroupBy(x => x.UID).ToList();//分组

            var cModel = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3001);
            var cModel2 = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.WalletCurID == 3004);

            if (MOlist2.Count > 0)
            {
                Dictionary<int, Data.User> dicUser = UserService.List().ToList().ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                StringBuilder sql = new StringBuilder();//SQLBuilder实例

                foreach (var model in MOlist2)
                {

                    //当前会员
                    var onUser = userAllList.Single(x => x.ID == model.Key);
                    //会员等级
                    var userLevel = MOlist.Where(x => x.UID == onUser.ID).Max(x => x.ShopID);
                    if (userLevel > 0)
                    {
                        decimal PARAM_BL = 0; //动态奖比例
                        decimal bonusMoney = 0;  //金额
                        decimal dMoney = 0;  //分红金额
                        string description = "";//奖金描述

                        var userparam = cacheSysParam.Single(x => x.ID == 1000 + userLevel);
                        var paramTD = cacheSysParam.Single(x => x.ID == 1300 + userLevel);

                        PARAM_BL = paramTD.Value.ToDecimal();

                        var refererList = Users.GetAllRefereeChild(onUser, 0);  //获得用户所有子用户用户集合
                        string ids = ",";
                        foreach (var team in refererList)
                        {
                            ids += team.ID + ",";
                        }
                        //旗下会员释放记录
                        var bonusList = MOlist.Where(x => ids.Contains("," + x.UID.ToString() + ",")).ToList();
                        bonusMoney = bonusList.Sum(x => x.TodaySettlement);

                        dMoney = bonusMoney * PARAM_BL;

                        description = "来自" + param.Name + "的奖金结算:(" + bonusMoney + "×" + PARAM_BL + ")";
                        dMoney = BonuTop(onUser.ID, dMoney, ref description, (userparam.Value2.ToDecimal()), bonusDayList);

                        if (dMoney > 0)
                        {


                            dicUser[onUser.ID].Cur3001 = (dicUser[onUser.ID].Cur3001 ?? 0) + (dMoney * FJB_BL);
                            dicUser[onUser.ID].Cur3004 = (dicUser[onUser.ID].Cur3004 ?? 0) + (dMoney * FJJ_BL);

                            dicUser[onUser.ID].Addup1102 = (dicUser[onUser.ID].Addup1102 ?? 0) + dMoney;
                            var balance = dicUser[onUser.ID].Cur3001 ?? 0;
                            dicUser[onUser.ID].ReserveInt1 = 1;//更改标记

                            WalletLogList.Add(new Data.WalletLog
                            {
                                ChangeMoney = dMoney * FJB_BL,
                                Balance = balance,
                                CoinID = 3001,
                                CoinName = cModel.CurrencyName,
                                CreateTime = DateTime.Now,
                                Description = description,
                                UID = onUser.ID,
                                UserName = onUser.UserName,
                            });
                            WalletLogList.Add(new Data.WalletLog
                            {
                                ChangeMoney = dMoney * FJJ_BL,
                                Balance = (dicUser[onUser.ID].Cur3004 ?? 0),
                                CoinID = 3004,
                                CoinName = cModel2.CurrencyName,
                                CreateTime = DateTime.Now,
                                Description = description,
                                UID = onUser.ID,
                                UserName = onUser.UserName,
                            });
                            //写入奖金表
                            BonusDetailList.Add(new Data.BonusDetail
                            {
                                Period = 0,
                                CoinID = 3001,
                                BalanceTime = DateTime.Now,
                                BonusMoney = dMoney * FJB_BL,
                                BonusID = param.ID,
                                BonusName = param.Name,//投资分红
                                CreateTime = DateTime.Now,
                                Description = description + "×" + FJB_BL,
                                IsBalance = true,
                                UID = onUser.ID,
                                UserName = onUser.UserName,
                            });
                            //写入奖金表
                            BonusDetailList.Add(new Data.BonusDetail
                            {
                                Period = 0,
                                CoinID = 3004,
                                BalanceTime = DateTime.Now,
                                BonusMoney = dMoney * FJJ_BL,
                                BonusID = param.ID,
                                BonusName = param.Name,//投资分红
                                CreateTime = DateTime.Now,
                                Description = description + "×" + FJJ_BL,
                                IsBalance = true,
                                UID = onUser.ID,
                                UserName = onUser.UserName,
                            });

                        }
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 1)
                    {
                        sql.AppendFormat(" update [User] set Cur3001={0},Cur3004={1} where ID={2} ", (item.Value.Cur3001 ?? 0), (item.Value.Cur3004 ?? 0), item.Value.ID);
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
                        MvcCore.Unity.Get<JN.Data.Service.IBonusDetailService>().BulkInsert(BonusDetailList);
                        MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().BulkInsert(WalletLogList);

                        ts.Complete();
                    }
                }

            }

        }
        #endregion

        #region 更新矿机
        /// <summary>
        /// 更新矿机
        /// </summary>
        /// <param name="MachineOrderService"></param>
        /// <param name="SysDBTool"></param>
        public static void UpMachine(IMachineOrderService MachineOrderService, ISysDBTool SysDBTool)
        {
            var bonuslist = MachineOrderService.List(x => x.TodaySettlement > 0).OrderByDescending(x => x.ID).ToList();
            StringBuilder MachineOrderSql = new StringBuilder();//矿机表更新语句
            if (bonuslist.Count() > 0)
            {
                MachineOrderSql.AppendFormat(" update [MachineOrder] set TodaySettlement =0,WaitExtractIncome=0,TodayMiningTime=0 where TodaySettlement >0");
            }
            if (MachineOrderSql.Length != 0)
            {
                //批量处理矿机表的信息
                System.Data.Common.DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                SysDBTool.ExecuteSQL(MachineOrderSql.ToString(), dbparam);
                SysDBTool.Commit();
            }
        }
        #endregion

        #region 奖金封顶
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onUserId">实例ID</param>
        /// <param name="bonusMoney">奖金金额</param>
        /// <param name="Description">描述</param>
        /// <param name="DiurnalTop">奖金封顶金额</param>
        /// <param name="bonusDayList">历史奖金列表</param>
        /// <param name="dicUser">用户数据字典</param>
        /// <returns></returns>
        public static decimal BonuTop(int onUserId, decimal bonusMoney, ref string Description, decimal DiurnalTop, List<Data.BonusDetail> bonusDayList)
        {
            //获取当日已获得的业绩奖金
            decimal bonusMoneyDay = bonusDayList.Where(x => x.UID == onUserId).Count() > 0 ?
                bonusDayList.Where(x => x.UID == onUserId).Sum(x => x.BonusMoney) : 0;
            if (bonusMoney + bonusMoneyDay >= DiurnalTop)
            {
                Description += "，达到日封顶，取差值";
                bonusMoney = DiurnalTop - bonusMoneyDay;
            }
            bonusMoney = bonusMoney > 0 ? bonusMoney : 0;
            return bonusMoney;
        }
        #endregion
        
        #region 节点奖励
        /// <summary>
        /// 节点奖励(fj币)
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void FH_NodeBonus1104(
            IBonusDetailService BonusDetailService,
            IUserService UserService,
            IWalletLogService WalletLogService,
            ICurrencyService CurrencyService,
            ISysDBTool SysDBTool,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
            var param = cacheSysParam.Single(x => x.ID == 1104);//奖金对象

            decimal monNum = param.Value.ToDecimal();
            var userAllList = UserService.List(x => (x.Cur3001 ?? 0) >= monNum).ToList();//所有奖励会员

            if (userAllList.Count > 0 && userAllList != null)
            {
                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                StringBuilder sql = new StringBuilder();//SQLBuilder实例

                var curBonusID = 3000 + param.Value3.ToInt();
                var cModel = CurrencyService.Single(x => x.WalletCurID == curBonusID);

                decimal bonusMoney = 0;  //金额
                string description = "";//奖金描述
                decimal PARAM_BL = param.Value2.ToDecimal(); //动态奖比例
                decimal balance = 0;//余额
                foreach (var item in userAllList)
                {
                    bonusMoney = (item.Cur3001 ?? 0) * PARAM_BL;

                    if (bonusMoney > 0)
                    {
                        description = "获得：" + param.Name + "(" + (item.Cur3001 ?? 0).ToString("F2") + "×" + PARAM_BL + ")";
                        switch (curBonusID)
                        {
                            case 3001:
                                dicUser[item.ID].Cur3001 = (dicUser[item.ID].Cur3001 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3001 ?? 0);
                                break;
                            case 3002:
                                dicUser[item.ID].Cur3002 = (dicUser[item.ID].Cur3002 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3002 ?? 0);
                                break;
                            case 3003:
                                dicUser[item.ID].Cur3003 = (dicUser[item.ID].Cur3003 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3003 ?? 0);
                                break;
                            case 3004:
                                dicUser[item.ID].Cur3004 = (dicUser[item.ID].Cur3004 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3004 ?? 0);
                                break;
                        }
                        dicUser[item.ID].ReserveInt1 = 1;//更改标记

                        WalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = bonusMoney,
                            Balance = balance,
                            CoinID = curBonusID,
                            CoinName = cModel.CurrencyName,
                            CreateTime = DateTime.Now,
                            Description = description,
                            UID = item.ID,
                            UserName = item.UserName,
                        });
                        //写入奖金表
                        BonusDetailList.Add(new Data.BonusDetail
                        {
                            Period = 0,
                            CoinID = curBonusID,
                            BalanceTime = DateTime.Now,
                            BonusMoney = bonusMoney,
                            BonusID = param.ID,
                            BonusName = param.Name,//投资分红
                            CreateTime = DateTime.Now,
                            Description = description,
                            IsBalance = true,
                            UID = item.ID,
                            UserName = item.UserName,
                        });
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 1)
                    {
                        switch (curBonusID)
                        {
                            case 3001:
                                sql.AppendFormat(" update [User] set Cur3001={0} where ID={1} ", (item.Value.Cur3001 ?? 0), item.Value.ID);
                                break;
                            case 3002:
                                sql.AppendFormat(" update [User] set Cur3002={0} where ID={1} ", (item.Value.Cur3002 ?? 0), item.Value.ID);
                                break;
                            case 3003:
                                sql.AppendFormat(" update [User] set Cur3003={0} where ID={1} ", (item.Value.Cur3003 ?? 0), item.Value.ID);
                                break;
                            case 3004:
                                sql.AppendFormat(" update [User] set Cur3004={0} where ID={1} ", (item.Value.Cur3004 ?? 0), item.Value.ID);
                                break;
                        }
                    }
                }
                if (sql.Length > 0)
                {
                    System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                    int count = SysDBTool.ExecuteSQL(sql.ToString(), s);
                    sql.Clear();
                    //导入
                    BonusDetailService.BulkInsert(BonusDetailList);
                    WalletLogService.BulkInsert(WalletLogList);
                }

            }
        }
        #endregion

        #region 节点奖励fj卷
        /// <summary>
        /// 节点奖励(fj卷)
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void FH_NodeBonus1105(
            IBonusDetailService BonusDetailService,
            IUserService UserService,
            IWalletLogService WalletLogService,
            ICurrencyService CurrencyService,
            ISysDBTool SysDBTool,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
            var param = cacheSysParam.Single(x => x.ID == 1105);//奖金对象
            decimal monNum = param.Value.ToDecimal();
            var userAllList = UserService.List(x => (x.Cur3004 ?? 0) >= monNum).ToList();//所有奖励会员

            if (userAllList.Count > 0 && userAllList != null)
            {
                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                StringBuilder sql = new StringBuilder();//SQLBuilder实例

                var curBonusID = 3000 + param.Value3.ToInt();
                var cModel = CurrencyService.Single(x => x.WalletCurID == curBonusID);

                decimal bonusMoney = 0;  //金额
                string description = "";//奖金描述
                decimal PARAM_BL = param.Value2.ToDecimal(); //动态奖比例
                decimal balance = 0;//余额
                foreach (var item in userAllList)
                {
                    bonusMoney = (item.Cur3001 ?? 0) * PARAM_BL;

                    if (bonusMoney > 0)
                    {
                        description = "获得：" + param.Name + "(" + (item.Cur3001 ?? 0).ToString("F2") + "×" + PARAM_BL + ")";
                        switch (curBonusID)
                        {
                            case 3001:
                                dicUser[item.ID].Cur3001 = (dicUser[item.ID].Cur3001 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3001 ?? 0);
                                break;
                            case 3002:
                                dicUser[item.ID].Cur3002 = (dicUser[item.ID].Cur3002 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3002 ?? 0);
                                break;
                            case 3003:
                                dicUser[item.ID].Cur3003 = (dicUser[item.ID].Cur3003 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3003 ?? 0);
                                break;
                            case 3004:
                                dicUser[item.ID].Cur3004 = (dicUser[item.ID].Cur3004 ?? 0) + bonusMoney;
                                balance = (dicUser[item.ID].Cur3004 ?? 0);
                                break;
                        }
                        dicUser[item.ID].ReserveInt1 = 1;//更改标记

                        WalletLogList.Add(new Data.WalletLog
                        {
                            ChangeMoney = bonusMoney,
                            Balance = balance,
                            CoinID = curBonusID,
                            CoinName = cModel.CurrencyName,
                            CreateTime = DateTime.Now,
                            Description = description,
                            UID = item.ID,
                            UserName = item.UserName,
                        });
                        //写入奖金表
                        BonusDetailList.Add(new Data.BonusDetail
                        {
                            Period = 0,
                            CoinID = curBonusID,
                            BalanceTime = DateTime.Now,
                            BonusMoney = bonusMoney,
                            BonusID = param.ID,
                            BonusName = param.Name,//投资分红
                            CreateTime = DateTime.Now,
                            Description = description,
                            IsBalance = true,
                            UID = item.ID,
                            UserName = item.UserName,
                        });
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 1)
                    {
                        switch (curBonusID)
                        {
                            case 3001:
                                sql.AppendFormat(" update [User] set Cur3001={0} where ID={1} ", (item.Value.Cur3001 ?? 0), item.Value.ID);
                                break;
                            case 3002:
                                sql.AppendFormat(" update [User] set Cur3002={0} where ID={1} ", (item.Value.Cur3002 ?? 0), item.Value.ID);
                                break;
                            case 3003:
                                sql.AppendFormat(" update [User] set Cur3003={0} where ID={1} ", (item.Value.Cur3003 ?? 0), item.Value.ID);
                                break;
                            case 3004:
                                sql.AppendFormat(" update [User] set Cur3004={0} where ID={1} ", (item.Value.Cur3004 ?? 0), item.Value.ID);
                                break;
                        }
                    }
                }
                if (sql.Length > 0)
                {
                    System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                    int count = SysDBTool.ExecuteSQL(sql.ToString(), s);
                    sql.Clear();
                    //导入
                    BonusDetailService.BulkInsert(BonusDetailList);
                    WalletLogService.BulkInsert(WalletLogList);

                }

            }
        }
        #endregion
        
        #region 更新交易量
        public static void Update_TradingVolume(
          IUserService UserService,
          ISysDBTool SysDBTool,
          ISettlementService SettlementService,
          List<Data.SysParam> cacheSysParam,
          int balancemode)
        {
            var userCount = UserService.List(x => x.BuyTradeNum > 0 || x.SellTradeNum > 0).Count();
            //发放期数
            var param = cacheSysParam.Single(x => x.ID == 1111);//投资分红参数

            int period = SettlementService.List(x => x.BalanceType == param.ID).Count() > 0 ? SettlementService.List(x => x.BalanceType == param.ID).Max(x => x.Period) + 1 : 1;  //每日分红总期数

            var lastTime = SettlementService.List(x => x.BalanceType == param.ID && x.Period == period - 1).FirstOrDefault();

            if (lastTime == null || lastTime.CreateTime.AddMinutes(param.Value.ToInt()) <= DateTime.Now)
            {
                StringBuilder sql = new StringBuilder();//SQLBuilder实例
                sql.AppendFormat(" update [User] set BuyTradeNum=0,SellTradeNum=0,ShowBuyTradeNum=0,ShowSellTradeNum where BuyTradeNum >0 or SellTradeNum >0");

                System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                int count = SysDBTool.ExecuteSQL(sql.ToString(), s);
                sql.Clear();

                SettlementService.Add(new Data.Settlement
                {
                    BalanceMode = balancemode,
                    CreateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00")),
                    Period = period,
                    TotalBonus = userCount,
                    TotalUser = userCount,
                    BalanceType = param.ID
                });
                SysDBTool.Commit();

            }


        }
        #endregion

        #region 股东分红
        public static void FH_ShareholderBonus(
          IBonusDetailService BonusDetailService,
          IUserService UserService,
          IAdvertiseOrderService AdvertiseOrderService,
          IWalletLogService WalletLogService,
          ICurrencyService CurrencyService,
          ISysDBTool SysDBTool,
          List<Data.SysParam> cacheSysParam,
          int balancemode)
        {
            var param = cacheSysParam.Single(x => x.ID == 1106);//奖金对象

            decimal monNum = cacheSysParam.Single(x => x.ID == 1104).Value.ToDecimal();//节点等级门槛

            var advertiseOrderList = AdvertiseOrderService.List(x => SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) <= 30 && x.Status == (int)Data.Enum.AdvertiseOrderStatus.Completed).ToList();
            var userListAll = UserService.List(x => x.IsActivation && !(x.IsLock)).ToList();//所有会员

            if (userListAll.Count > 0 && userListAll != null)
            {
                //相关会员id集合
                Dictionary<int, Data.User> dicUser = userListAll.ToDictionary(d => d.ID, d => d);

                StringBuilder sql = new StringBuilder();//SQLBuilder实例
                //初始化参数
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表

                var curBonusID = 3000 + param.Value3.ToInt();
                var cModel = CurrencyService.Single(x => x.WalletCurID == curBonusID);//分红币种

                decimal bonusMoney = param.Value2.ToDecimal();  //金额
                string description = "";//奖金描述
                decimal balance = 0;//余额

                foreach (var item in userListAll)
                {
                    var refererList = userListAll.Where(x => (x.RefereePath.Contains("," + item.ID + ",") || x.RefereeID == item.ID) && x.IsActivation);  //获得用户所有子用户用户集合
                    string ids = ",";
                    foreach (var team in refererList)
                    {
                        ids += team.ID + ",";
                    }

                    //旗下会员释放记录
                    var reAdvertiseOrde = advertiseOrderList.Where(x => ids.Contains("," + x.BuyUID.ToString() + ",")).ToList();
                    var reNum = reAdvertiseOrde.Sum(x => x.Quantity);
                    if (reNum >= param.Value.ToDecimal() && ((item.Cur3001 ?? 0) >= monNum))
                    {
                        if (bonusMoney > 0)
                        {
                            description = "获得：" + param.Name + ",金额:(" + (bonusMoney).ToString("F2") + ")";
                            switch (curBonusID)
                            {
                                case 3001:
                                    dicUser[item.ID].Cur3001 = (dicUser[item.ID].Cur3001 ?? 0) + bonusMoney;
                                    balance = (dicUser[item.ID].Cur3001 ?? 0);
                                    break;
                                case 3002:
                                    dicUser[item.ID].Cur3002 = (dicUser[item.ID].Cur3002 ?? 0) + bonusMoney;
                                    balance = (dicUser[item.ID].Cur3002 ?? 0);
                                    break;
                                case 3003:
                                    dicUser[item.ID].Cur3003 = (dicUser[item.ID].Cur3003 ?? 0) + bonusMoney;
                                    balance = (dicUser[item.ID].Cur3003 ?? 0);
                                    break;
                                case 3004:
                                    dicUser[item.ID].Cur3004 = (dicUser[item.ID].Cur3004 ?? 0) + bonusMoney;
                                    balance = (dicUser[item.ID].Cur3004 ?? 0);
                                    break;
                            }
                            dicUser[item.ID].ReserveInt1 = 1;//更改标记

                            WalletLogList.Add(new Data.WalletLog
                            {
                                ChangeMoney = bonusMoney,
                                Balance = balance,
                                CoinID = curBonusID,
                                CoinName = cModel.CurrencyName,
                                CreateTime = DateTime.Now,
                                Description = description,
                                UID = item.ID,
                                UserName = item.UserName,
                            });
                            //写入奖金表
                            BonusDetailList.Add(new Data.BonusDetail
                            {
                                Period = 0,
                                CoinID = curBonusID,
                                BalanceTime = DateTime.Now,
                                BonusMoney = bonusMoney,
                                BonusID = param.ID,
                                BonusName = param.Name,//投资分红
                                CreateTime = DateTime.Now,
                                Description = description,
                                IsBalance = true,
                                UID = item.ID,
                                UserName = item.UserName,
                            });
                        }
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 1)
                    {
                        switch (curBonusID)
                        {
                            case 3001:
                                sql.AppendFormat(" update [User] set Cur3001={0} where ID={1} ", (item.Value.Cur3001 ?? 0), item.Value.ID);
                                break;
                            case 3002:
                                sql.AppendFormat(" update [User] set Cur3002={0} where ID={1} ", (item.Value.Cur3002 ?? 0), item.Value.ID);
                                break;
                            case 3003:
                                sql.AppendFormat(" update [User] set Cur3003={0} where ID={1} ", (item.Value.Cur3003 ?? 0), item.Value.ID);
                                break;
                            case 3004:
                                sql.AppendFormat(" update [User] set Cur3004={0} where ID={1} ", (item.Value.Cur3004 ?? 0), item.Value.ID);
                                break;
                        }
                    }
                }
                if (sql.Length > 0)
                {
                    System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                    int count = SysDBTool.ExecuteSQL(sql.ToString(), s);
                    sql.Clear();
                    //导入
                    BonusDetailService.BulkInsert(BonusDetailList);
                    WalletLogService.BulkInsert(WalletLogList);

                }
            }


        }


        #endregion
        
        #region 节点奖

        /// <summary>
        /// 节点奖
        /// </summary>
        /// <param name="balancemode">发放模式，手动/自动</param>
        public static void FH_NodeBonus1107(
            IBonusDetailService BonusDetailService,
            IUserService UserService,
            IWalletLogService WalletLogService,
            ICurrencyService CurrencyService,
            ISysDBTool SysDBTool,
            List<Data.SysParam> cacheSysParam,
            int balancemode)
        {
            var param = cacheSysParam.Single(x => x.ID == 1107);//奖金对象

            decimal BL1 = param.Value.ToDecimal();
            decimal BL2 = param.Value2.ToDecimal();
            decimal BL3 = param.Value3.ToDecimal();

            var userAllList = UserService.List().ToList();
            //获取当天奖金记录和封顶参数
            var bonusDayList = BonusDetailService.List(x => SqlFunctions.DateDiff("day", x.CreateTime, DateTime.Now) == 0 && x.BonusID == 1104).OrderBy(x => x.UID).ToList();

            if (bonusDayList.Count > 0 && bonusDayList != null)
            {
                Dictionary<int, Data.User> dicUser = userAllList.ToDictionary(d => d.ID, d => d);
                List<Data.BonusDetail> BonusDetailList = new List<Data.BonusDetail>();//奖金明细列表
                List<Data.WalletLog> WalletLogList = new List<Data.WalletLog>();//账户明细列表
                StringBuilder sql = new StringBuilder();//SQLBuilder实例

                var curBonusID = 3000 + param.Value4.ToInt();
                var cModel = CurrencyService.Single(x => x.WalletCurID == curBonusID);//分红币种
                string description = "";//奖金描述

                decimal PARAM_BL = 0; //动态奖比例
                decimal bonusMoney = 0;
                decimal balance = 0;//余额

                foreach (var item in bonusDayList)
                {
                    var onUser = userAllList.Single(x => x.ID == item.UID);
                    if (!string.IsNullOrEmpty(onUser.RefereePath))
                    {
                        string[] ids = onUser.RefereePath.Split(',');

                        var reuserlist = userAllList.Where(x => ids.Contains(x.ID.ToString()) && x.Depth >= (onUser.RefereeDepth - 3));

                        if (reuserlist.Count() > 0 && reuserlist != null)
                        {
                            foreach (var reUser in reuserlist)
                            {
                                bonusMoney = 0;
                                PARAM_BL = 0;
                                int Depth = onUser.RefereeDepth - reUser.RefereeDepth;  //差几代
                                if (Depth == 1)
                                {
                                    PARAM_BL = BL1;
                                }
                                else if (Depth == 2)
                                {
                                    PARAM_BL = BL2;
                                }
                                else if (Depth == 3)
                                {
                                    PARAM_BL = BL3;
                                }

                                bonusMoney = item.BonusMoney * PARAM_BL;  //奖金
                                if (bonusMoney > 0)
                                {
                                    description = "获得：" + param.Name + ",金额:(" + (bonusMoney).ToString("F2") + ")";
                                    switch (curBonusID)
                                    {
                                        case 3001:
                                            dicUser[reUser.ID].Cur3001 = (dicUser[reUser.ID].Cur3001 ?? 0) + bonusMoney;
                                            balance = (dicUser[reUser.ID].Cur3001 ?? 0);
                                            break;
                                        case 3002:
                                            dicUser[reUser.ID].Cur3002 = (dicUser[reUser.ID].Cur3002 ?? 0) + bonusMoney;
                                            balance = (dicUser[reUser.ID].Cur3002 ?? 0);
                                            break;
                                        case 3003:
                                            dicUser[reUser.ID].Cur3003 = (dicUser[reUser.ID].Cur3003 ?? 0) + bonusMoney;
                                            balance = (dicUser[reUser.ID].Cur3003 ?? 0);
                                            break;
                                        case 3004:
                                            dicUser[reUser.ID].Cur3004 = (dicUser[reUser.ID].Cur3004 ?? 0) + bonusMoney;
                                            balance = (dicUser[reUser.ID].Cur3004 ?? 0);
                                            break;
                                    }
                                    dicUser[reUser.ID].ReserveInt1 = 1;//更改标记

                                    WalletLogList.Add(new Data.WalletLog
                                    {
                                        ChangeMoney = bonusMoney,
                                        Balance = balance,
                                        CoinID = curBonusID,
                                        CoinName = cModel.CurrencyName,
                                        CreateTime = DateTime.Now,
                                        Description = description,
                                        UID = reUser.ID,
                                        UserName = reUser.UserName,
                                    });
                                    //写入奖金表
                                    BonusDetailList.Add(new Data.BonusDetail
                                    {
                                        Period = 0,
                                        CoinID = curBonusID,
                                        BalanceTime = DateTime.Now,
                                        BonusMoney = bonusMoney,
                                        BonusID = param.ID,
                                        BonusName = param.Name,//投资分红
                                        CreateTime = DateTime.Now,
                                        Description = description,
                                        IsBalance = true,
                                        UID = reUser.ID,
                                        UserName = reUser.UserName,
                                    });
                                }
                            }
                        }
                    }
                }

                foreach (var item in dicUser)
                {
                    if ((item.Value.ReserveInt1 ?? 0) == 1)
                    {
                        switch (curBonusID)
                        {
                            case 3001:
                                sql.AppendFormat(" update [User] set Cur3001={0} where ID={1} ", (item.Value.Cur3001 ?? 0), item.Value.ID);
                                break;
                            case 3002:
                                sql.AppendFormat(" update [User] set Cur3002={0} where ID={1} ", (item.Value.Cur3002 ?? 0), item.Value.ID);
                                break;
                            case 3003:
                                sql.AppendFormat(" update [User] set Cur3003={0} where ID={1} ", (item.Value.Cur3003 ?? 0), item.Value.ID);
                                break;
                            case 3004:
                                sql.AppendFormat(" update [User] set Cur3004={0} where ID={1} ", (item.Value.Cur3004 ?? 0), item.Value.ID);
                                break;
                        }
                    }
                }
                if (sql.Length > 0)
                {
                    System.Data.Common.DbParameter[] s = new System.Data.Common.DbParameter[] { };
                    int count = SysDBTool.ExecuteSQL(sql.ToString(), s);
                    sql.Clear();
                    //导入
                    BonusDetailService.BulkInsert(BonusDetailList);
                    WalletLogService.BulkInsert(WalletLogList);

                }

            }



        }

        #endregion

    }
}