using JN.Data;
using JN.Data.Enum;
using JN.Data.Extensions;
using JN.Data.Service;
using JN.Services.Tool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace JN.Services.Manager
{
    /// <summary>
    /// C2C交易逻辑
    /// </summary>
    public partial class Advertises
    {
        private static List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();

        public static void HeavyLoad()
        {
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
        }

        #region 发布广告       
        /// <summary>
        /// 发布广告
        /// </summary>
        /// <param name="form">form表单</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="AdvertiseService">广告单实体</param>
        /// <param name="AdvertiseOrderService">广告订单实体</param>
        /// <param name="CurrencyService">币种</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoNewAdvertise(FormCollection fc, JN.Data.Advertise entity, JN.Data.User Umodel, IAdvertiseService AdvertiseService, ICurrencyService CurrencyService, ISysDBTool SysDBTool)
        {

            Result result = new Result();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoNewAdvertise" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("DoNewAdvertise" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
            //判断数据
            //if (entity.Premium.ToDouble() > 99.99 || entity.Premium.ToDouble() < -99.99) throw new CustomException.CustomException("请输入-99.99至99.99的数值");

            //if (entity.MaximumLimit <= 0 || entity.MinimumLimit == null || entity.MinimumLimit <= 0) throw new CustomException.CustomException("请您填写最大、最小交易额");
            //if (entity.MaximumLimit <= entity.MinimumLimit) throw new CustomException.CustomException("最小交易额不可大于最大交易额");
            //if (entity.EndLimitedTime != null || entity.StartLimitedTime != null)//要么两个填，要么全部不填
            //{
            //    if (entity.EndLimitedTime == null || entity.StartLimitedTime == null)
            //    {
            //        throw new CustomException.CustomException("开始和结束时间必须同时填，两个不填，则不限制");
            //    }
            //    if (!StringHelp.IsDateString(entity.StartLimitedTime)) throw new CustomException.CustomException("开启时间格式不正确！");
            //    if (!StringHelp.IsDateString(entity.EndLimitedTime)) throw new CustomException.CustomException("关闭时间格式不正确！");
            //}


            //if (string.IsNullOrEmpty(entity.AdMessage))
            //{
            //    throw new CustomException.CustomException("请填写留言");
            //}
            //if (entity.AdMessage.Length > 350) throw new CustomException.CustomException("留言信息不能超过350个字符");

            //entity.AdMessage = Services.Tool.StringHelp.FilterSqlHtml(entity.AdMessage);//过滤敏感字符

            var cur = CurrencyService.Single(x => x.ID == entity.CurID);//交易币种
                var payCur = CurrencyService.Single(x => x.ID == entity.CoinID);//付款币种
                                                                                       //var Payment = cacheSysParam.Single(x => x.ID == entity.PaymentID);//付款方式

                var thisMon = JN.Services.Manager.PriceHelps.getcurrentprice(cur);

                var minBL = cacheSysParam.Single(x => x.ID == 2105).Value.ToDecimal();
                var maxBL = cacheSysParam.Single(x => x.ID == 2105).Value2.ToDecimal();

                //if (Payment == null) throw new CustomException.CustomException("没有这个付款方式");
                if (cur == null || payCur == null) throw new CustomException.CustomException("请选择币种");
                if (entity.CoinID != 0 && payCur == null) throw new CustomException.CustomException("没有这个币种");

                if (cur.TranSwitch == false) throw new CustomException.CustomException("当前不可新增挂单");

                if (entity.Quantity <= 0) throw new CustomException.CustomException("您还没填写交易数量");

                if (entity.Quantity < cur.MinLimit) throw new CustomException.CustomException("交易数量最低为" + cur.MinLimit);

                decimal price = (fc["price"] ?? "0").ToDecimal();

                if (price < thisMon * (1 - minBL) || price > thisMon * (1 + maxBL))
                    throw new CustomException.CustomException("价格请输入" + (thisMon * (1 - minBL)).ToString("F2") + "至" + (thisMon * (1 + maxBL)).ToString("F2") + "的数值");

                decimal totalMoney = 0;
                decimal fee = 0;
                if (entity.Direction == 0)
                {
                    //要判断是不是添加有银行卡，没有银行卡不给发布
                    var bankDefault = MvcCore.Unity.Get<JN.Data.Service.IUserBankCardService>().Single(x => x.UID == Umodel.ID && x.IsDefault);
                    if (bankDefault == null) throw new CustomException.CustomException("请先设置默认银行卡");

                    //if (entity.Quantity <= 0) throw new CustomException.CustomException("您还没填写交易数量");

                    //if (entity.Quantity < cur.MinLimit) throw new CustomException.CustomException("交易数量最低为" + cur.MinLimit);

                    if (entity.Quantity.ToDecimal() % cacheSysParam.Single(x => x.ID == 3005).Value.ToDecimal() != 0) throw new CustomException.CustomException("交易数量必须为" + cacheSysParam.Single(x => x.ID == 3005).Value + "的倍数");

                    //拿到会员当前数量剩余
                    decimal balance = Users.WalletCur((int)cur.WalletCurID, Umodel);

                    fee = entity.Quantity * cur.SellPoundage;//手续费
                    if (balance < (entity.Quantity+ fee)) throw new CustomException.CustomException("您的余额不足");
                }
                else
                {
                    //if (entity.Direction == 1 && payCur.ID == 3)
                    //{
                    //    //拿到会员当前数量剩余
                    //    decimal balance = Users.WalletCur((int)payCur.WalletCurID, Umodel);
                    //    totalMoney = entity.Quantity * price;
                    //    if (balance < entity.Quantity) throw new CustomException.CustomException("您的余额不足");
                    //}

                    fee = 0;//entity.Quantity * cur.BuyPoundage;//购买没有手续费
                    entity.Quantity = entity.Quantity - fee;
                    ////找到最低限制币种
                    //var minMoney = MD_CacheEntrustsData.GetCurrencyList().Where(x => (x.IsLimit ?? false) && x.MinLimit > 0).Count() > 0 ? CurrencyService.ListCache("coinList").Where(x => (x.IsLimit ?? false) && x.MinLimit > 0).Min(x => x.MinLimit) : 0;
                    ////拿到会员当前数量剩余
                    //decimal balance = Users.WalletCur((int)curmodel.WalletCurID, Umodel.ID);
                    //if (balance < minMoney) throw new CustomException.CustomException("任意账户上必须大于" + minMoney.ToDouble() + "个");
                }
                entity.Poundage = fee;
                entity.CoinID = payCur.ID;
                entity.CoinName = payCur.CurrencyName ?? "";
                //entity.PaymentName = Payment.Name;
                entity.CurName = cur.CurrencyName;
                //获取该币种的API的数据：实时价格(默认是人民币)
                // string url = "https://api.coinmarketcap.com/v1/ticker/" + curmodel.English + "/?convert=CNY";
                // string ssprice = JN.Services.Tool.StringHelp.GetPrice(url, "CNY");
                //bool IsPremium = HttpContext.Current.Request["IsPremium"].ToBool();

                //if (IsPremium)
                //{
                //    string ssprice = HttpContext.Current.Request["ssprice"];
                //    if (!JN.Services.Tool.StringHelp.IsNumber(ssprice) || ssprice.ToDecimal() <= 0) throw new CustomException.CustomException("获取实时价格错误，请联系管理员");
                //    price = ssprice.ToDecimal() + ssprice.ToDecimal() * (entity.Premium / 100);//得到最终价格
                //}
                //else
                //{
                //    string reprice = HttpContext.Current.Request["Price"];
                //    if (!JN.Services.Tool.StringHelp.IsNumber(reprice)) throw new CustomException.CustomException("请您填写正确的价格");
                //    entity.Premium = 0;
                //    price = reprice.ToDecimal();
                //}

                //string tradePassword = fc["tradeinPassword"];
                //if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException.CustomException("交易密码不正确");

                entity.HaveQuantity = 0;//(entity.Direction == 1 ? entity.MaximumLimit : entity.Quantity);
                entity.LocationID = 0;
                entity.LocationName = "";
                entity.Premium = 0;
                entity.Price = price;
                entity.OrderID = entity.Direction == 1 ? "B" + JN.Services.Tool.StringHelp.GetRandomNumber(6) : "S" + JN.Services.Tool.StringHelp.GetRandomNumber(6);
                entity.UserName = Umodel.UserName;
                entity.UID = Umodel.ID;
                entity.Status = (int)JN.Data.Enum.AdvertiseStatus.Underway;
                entity.IsEnableSecurity = entity.IsEnableSecurity.ToBool();
                entity.IsTrustOnly = entity.IsTrustOnly.ToBool();
                entity.CreateTime = DateTime.Now;

                entity.CurEnSigns = cur.EnSigns;
                AdvertiseService.Add(entity);

                if (entity.Direction == 0)//出售卖出的时候
                {
                    Wallets.changeWallet(Umodel.ID, -(entity.Quantity.ToDecimal() + fee), (int)cur.WalletCurID, "出售订单号为[" + entity.OrderID + "]扣除", cur);//扣除钱包              
                }
                //else if (entity.Direction == 1 && payCur.ID == 3)
                //{
                //    Wallets.changeWallet(Umodel.ID, -totalMoney, (int)payCur.WalletCurID, "购买订单号为[" + entity.OrderID + "]扣除", payCur);//扣除钱包  
                //}
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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
            finally
            {
                MvcCore.Extensions.CacheExtensions.ClearCache("DoNewAdvertise" + Umodel.ID);//清除缓存
            }
            return result;

        }
        #endregion

        #region 购买广告       
        /// <summary>
        /// 购买广告
        /// </summary>
        /// <param name="form">form表单</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="AdvertiseService">广告单实体</param>
        /// <param name="AdvertiseOrderService">广告订单实体</param>
        /// <param name="CurrencyService">币种</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoBuy(FormCollection form, JN.Data.User Umodel, IAdvertiseService AdvertiseService, IAdvertiseOrderService AdvertiseOrderService, ICurrencyService CurrencyService, IUserService UserService, ISysDBTool SysDBTool)
        {

            Result result = new Result();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoBuy" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("DoBuy" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                int adid = form["adid"].ToInt();
                //decimal Quantity = 0;
                //decimal Amount = HttpContext.Current.Request["Amount"].ToDecimal();
                decimal Amount = 0;
                decimal Quantity = form["quantity"].ToDecimal();

                //if (Umodel == null)
                //{
                //    throw new CustomException.CustomException("对不起，您还没有登录，请您登录后操作");
                //}
                string tradePassword = form["tradeoutPassword"];
                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException.CustomException("交易密码不正确");

                var Advertise = AdvertiseService.Single(x => x.Direction == 0 && x.ID == adid);//购买出售的单子
                if (Advertise == null) throw new CustomException.CustomException("没有这个订单");
                if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Cancel) throw new CustomException.CustomException("该订单已下架");
                //if (Amount <= 0) throw new CustomException.CustomException("请输入金额");
                if (Quantity <= 0) throw new CustomException.CustomException("请输入数量");

                var User = UserService.Single(x => x.ID == Advertise.UID);///查找当前会员
                if (Umodel.ID == User.ID) throw new CustomException.CustomException("对不起，不能购买自己的订单");

                var bankDefault = MvcCore.Unity.Get<JN.Data.Service.IUserBankCardService>().Single(x => x.UID == Advertise.UID && x.IsDefault);
                if (bankDefault == null) throw new CustomException.CustomException("卖家未完善收款信息或者未设置默认");

                if (AdvertiseOrderService.List(x => x.BuyUID == Umodel.ID && x.Direction == 1 && x.Status > 0 && x.Status < (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived).Count() > 0)
                {
                    throw new CustomException.CustomException("您还有一个订单未完成");
                }
                var cur = CurrencyService.Single(x => x.ID == Advertise.CurID);///查找当前币种                

                #region 未用
                decimal residualQuantity = Math.Round((Advertise.HaveQuantity - Advertise.HaveQuantity * cur.SellPoundage), 6, MidpointRounding.AwayFromZero);
                //decimal residualQuantity = (Advertise.HaveQuantity ?? 0) - (Advertise.HaveQuantity ?? 0) * c.SellPoundage;

                //if (Amount < Advertise.MinimumLimit) throw new CustomException.CustomException("输入金额不能小于" + Advertise.MinimumLimit.ToDouble());
                //if (Amount > Advertise.MaximumLimit) throw new CustomException.CustomException("输入金额不能大于" + Advertise.MaximumLimit.ToDouble());
                //if (Quantity < Advertise.MinimumLimit)
                //{
                //    if (Quantity < residualQuantity)
                //    {
                //        throw new CustomException.CustomException("输入数量不能小于" + residualQuantity.ToDouble());
                //    }
                //    else if (Advertise.MinimumLimit < residualQuantity)
                //    {
                //        throw new CustomException.CustomException("输入数量不能小于" + Advertise.MinimumLimit.ToDouble());
                //    }
                //}
                //if (Quantity > Advertise.MaximumLimit)
                //{
                //    if (Quantity > residualQuantity)
                //    {
                //        throw new CustomException.CustomException("输入数量不能大于" + residualQuantity.ToDouble());
                //    }
                //    else
                //    {
                //        throw new CustomException.CustomException("输入数量不能大于" + Advertise.MaximumLimit.ToDouble());
                //    }
                //}
                //if (Advertise.IsEnableSecurity && (!(Umodel.IsAuthentication ?? false) || !(Umodel.IsMobile ?? false))) throw new CustomException.CustomException("此单需要实名验证和手机验证才能交易");

                //var user = UserService.Single(Advertise.UID);
                //if (Advertise.IsTrustOnly && !user.TrustPath.Contains(Umodel.ID + ",")) throw new CustomException.CustomException("此单只能我信任的人才能交易");

                ////判断开放时间
                //if (Advertise.StartLimitedTime != null)
                //{
                //    string starttime = Advertise.StartLimitedTime.Replace(":", "");
                //    string endtime = Advertise.EndLimitedTime.Replace(":", "");

                //    //if (DateTime.Now < DateTime.Parse(starttime))
                //    if (DateTime.Now.ToShortTimeString().Replace(":", "").ToInt() < starttime.ToInt() || DateTime.Now.ToShortTimeString().Replace(":", "").ToInt() > endtime.ToInt())
                //        throw new CustomException.CustomException("此单的开放时间是：" + Advertise.StartLimitedTime + "-" + Advertise.EndLimitedTime);
                //}
                //if (Advertise.StartLimitedTime != null)
                //{
                //    string starttime = DateTime.Now.ToShortDateString() + " " + Advertise.StartLimitedTime;
                //    string endtime = DateTime.Now.ToShortDateString() + " " + Advertise.EndLimitedTime;
                //    if (DateTime.Now < DateTime.Parse(starttime)) throw new CustomException.CustomException("此单的开放时间是：" + Advertise.StartLimitedTime + "-" + Advertise.EndLimitedTime);
                //    if (DateTime.Now > DateTime.Parse(endtime)) throw new CustomException.CustomException("此单的开放时间是：" + Advertise.StartLimitedTime + "-" + Advertise.EndLimitedTime);
                //}
                #endregion


                if (Quantity > (Advertise.Quantity - Advertise.HaveQuantity)) throw new CustomException.CustomException("输入数量不能大于" + (Advertise.Quantity - Advertise.HaveQuantity).ToDouble());


                //Quantity = Amount / Advertise.Price;   //得到数量
                Amount = Quantity * Advertise.Price;//得到价格
                decimal buyPoundage = Quantity * cur.BuyPoundage;//得到买入的手续费
                decimal sellPoundage = Quantity * cur.SellPoundage;// Amount * cur.SellPoundage;//(Quantity / (1 - c.SellPoundage)) - Quantity;//得到卖出的手续费

                //decimal tolQuantity = Math.Round((Quantity + sellPoundage), 6, MidpointRounding.AwayFromZero);//得到最后的数量（+手续费的）
                //decimal tolQuantity = Quantity + sellPoundage;//得到最后的数量（+手续费的）

                //拿到对方会员当前数量剩余
                //decimal balance= Users.WalletCur((int)c.WalletCurID,User);
                //if (balance < tolQuantity)
                //{
                //    throw new CustomException.CustomException("余额不足");
                //}

                decimal actualAmount = Amount + buyPoundage;//实付金额
                //var payCur = CurrencyService.SingleAndInit(x => x.ID == Advertise.CoinID);//付款币种
                //if (Advertise.CoinID == 3)//余额交易区
                //{
                //    //拿到会员当前数量剩余
                //    decimal balance = Users.WalletCur((int)payCur.WalletCurID, Umodel);
                //    if (balance < actualAmount) throw new CustomException.CustomException("您的余额不足");
                //    Wallets.changeWalletNoCommit(Umodel.ID, -actualAmount, (int)payCur.WalletCurID, "购买订单号为[" + Advertise.OrderID + "]扣除，手续费：" + buyPoundage.ToDouble(), payCur);//扣除钱包  
                //}

                string orderNO = "BA" + JN.Services.Tool.StringHelp.GetRandomNumber(6);

                decimal wcxs = 0.000001M;//为误差做的小数 2018/4/11
                //出售单子时，查看剩余数量
                //if (Advertise.HaveQuantity < Quantity) throw new CustomException.CustomException("对不起，对方当前余额不足！");
                //Advertise.HaveQuantity = Math.Round(Advertise.HaveQuantity, 6, MidpointRounding.AwayFromZero) - Math.Round(tolQuantity, 6, MidpointRounding.AwayFromZero);//剩余额度
                //Advertise.HaveQuantity = (Advertise.HaveQuantity ?? 0).ToString("F6").ToDecimal() - tolQuantity.ToString("F6").ToDecimal();//剩余额度
                if (Advertise.HaveQuantity < 0 && Advertise.HaveQuantity >= -wcxs)//同样也是为误差做处理，防止剩余数量为负数
                {
                    Advertise.HaveQuantity = 0;
                }
                Advertise.HaveQuantity += Quantity;
                if (Advertise.HaveQuantity == Advertise.Quantity) Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Completed;
                AdvertiseService.Update(Advertise);

                ////余额交易则已完成
                //int status = (Advertise.CoinID == 2 ? (int)JN.Data.Enum.AdvertiseOrderStatus.Completed : (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder);
                JN.Data.AdvertiseOrder Order = new AdvertiseOrder();
                Order.AdID = Advertise.ID;
                Order.AdOderNO = Advertise.OrderID;
                Order.Amount = Amount;
                Order.BuyPoundage = buyPoundage;
                Order.BuyUID = Umodel.ID;
                Order.BuyUserName = Umodel.UserName;
                Order.CoinID = Advertise.CoinID;
                Order.CoinName = Advertise.CoinName;
                Order.CreateTime = DateTime.Now;
                Order.CurID = cur.ID;
                Order.CurName = cur.CurrencyName;
                Order.CurEnSigns = cur.EnSigns;
                Order.Direction = 1;
                Order.OrderID = orderNO;
                Order.PaymentTime = DateTime.Now.AddMinutes(cacheSysParam.Single(x => x.ID == 3001).Value.ToInt());
                Order.Price = Advertise.Price;
                Order.Quantity = Quantity;
                Order.Remark = "";
                Order.SellPoundage = sellPoundage;
                Order.SellUID = User.ID;
                Order.SellUserName = User.UserName;
                Order.Status = (int)Data.Enum.AdvertiseOrderStatus.PlaceOrder; //(Advertise.CoinID == 3 ? (int)JN.Data.Enum.AdvertiseOrderStatus.Completed : (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder);//余额则已完成
                if (Order.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.Completed)
                {
                    Order.ConfirmPayTime = DateTime.Now;//确认收货时间
                }

                Order.PayID = bankDefault.ID;//银行卡ID
                Order.PayMethod = bankDefault.BankName;//银行名
                Order.PayAccount = bankDefault.BankCard;//银行账户
                Order.PayAccountImgUrl = bankDefault.BankImgUrl;//银行账户图片
                AdvertiseOrderService.Add(Order);

                SysDBTool.Commit();

                //直接成交
                //if (Order.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.Completed)
                //{
                //    decimal buyMoney = Order.Quantity - Order.BuyPoundage;
                //    decimal sellMoney = Order.Amount - Order.SellPoundage;

                //    if (Order.Direction == 1)////A挂虚拟币 B是人民币 A为挂单着出售单
                //    {
                //        Wallets.changeWalletNoCommit(Order.BuyUID, buyMoney, (int)cur.WalletCurID, "出售订单号为[" + Order.AdOderNO + "]的购买订单[" + Order.OrderID + "]结算,手续费:" + Order.BuyPoundage.ToDouble(), cur);//结算给出售单者
                //    }
                //    else
                //    {
                //        //A挂人民币 B是虚拟币  A为挂单着购买单 
                //        Wallets.changeWalletNoCommit(Order.BuyUID, buyMoney, (int)cur.WalletCurID, "购买订单号为[" + Order.AdOderNO + "]的出售订单[" + Order.OrderID + "]结算,手续费:" + Order.BuyPoundage.ToDouble(), cur);//结算给购买单者
                //    }

                //    ////A挂人民币 B是虚拟币  A为挂单着购买单 
                //    //Wallets.changeWallet(Order.SellUID, sellMoney, (int)payCur.WalletCurID, "购买订单号为[" + Order.AdOderNO + "]的出售订单[" + Order.OrderID + "]结算,手续费:" + Order.BuyPoundage.ToDouble(), payCur);//出售者获得余额

                //    string userSql = "UPDATE [User] set TransactionNum=isnull(TransactionNum,0)+1  where id=" + Order.SellUID;
                //    userSql += " UPDATE [User] set TransactionNum=isnull(TransactionNum,0)+1  where id=" + Umodel.ID;

                //    //批量处理会员的信息
                //    DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                //    SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                //    //写入图表
                //    decimal _volume = Order.Quantity;
                //    decimal _tranprice = Order.Price;
                //    decimal _totalamount = _volume * _tranprice;
                //    WriteChart(cur, _volume, _tranprice, _totalamount);
                //}

                //发送短信
                JN.Services.Tool.SMSHelper.WebChineseMSM(User.Mobile, "您有新的订单,单号为[" + orderNO + "],详情请登录网站查看！");
                //发送短信
                JN.Services.Tool.SMSHelper.WebChineseMSM(Umodel.Mobile, "您下单成功，订单号为[" + orderNO + "],详情请登录网站查看！");
                result.Status = 200;
                result.Message = Order.ID.ToString();
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("DoBuy" + Umodel.ID);//清除缓存
            }
            return result;

        }
        #endregion

        #region 出售广告       
        /// <summary>
        /// 出售广告
        /// </summary>
        /// <param name="form">form表单</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="AdvertiseService">广告单实体</param>
        /// <param name="AdvertiseOrderService">广告订单实体</param>
        /// <param name="CurrencyService">币种</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoSell(FormCollection form, JN.Data.User Umodel, IAdvertiseService AdvertiseService, IAdvertiseOrderService AdvertiseOrderService, ICurrencyService CurrencyService, IUserService UserService, ISysDBTool SysDBTool)
        {

            Result result = new Result();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoSell" + Umodel.ID))//检测缓存是否存在，区分大小写
                {
                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");
                }
                MvcCore.Extensions.CacheExtensions.SetCache("DoSell" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                int adid = form["adid"].ToInt();
                //decimal Quantity = 0;
                //decimal Amount = Request["Amount"].ToDecimal();
                int bankID = form["bankID"].ToInt();
                decimal Amount = 0;
                decimal Quantity = form["quantity"].ToDecimal();
                //var Umodel = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(JN.Services.UserLoginHelper.CurrentUser().ID);
                if (Umodel == null)
                {
                    throw new CustomException.CustomException("对不起，您还没有登录，请您登录后操作");
                }
                string tradePassword = form["tradeoutPassword"];

                cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

                if (Umodel.Password2 != tradePassword.ToMD5().ToMD5()) throw new CustomException.CustomException("交易密码不正确");

                var Advertise = AdvertiseService.Single(x => x.Direction == 1 && x.ID == adid);//出售购买 的单子
                if (Advertise == null) throw new CustomException.CustomException("没有这个订单");
                if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Cancel) throw new CustomException.CustomException("该订单已下架");

                UserBankCard bankModel = null;
                //要判断是不是添加有银行卡，没有银行卡不给发布
                if (bankID != -3003)
                {
                    var bankDefault = MvcCore.Unity.Get<JN.Data.Service.IUserBankCardService>().Single(x => x.UID == Umodel.ID && x.IsDefault);
                    if (bankDefault == null) throw new CustomException.CustomException("请先设置默认银行卡");

                    bankModel = MvcCore.Unity.Get<JN.Data.Service.IUserBankCardService>().Single(x => x.ID == bankID);
                    if (bankModel == null) throw new CustomException.CustomException("该银行卡不存在");
                }

                //if (Amount <= 0) throw new CustomException.CustomException("请输入金额");
                if (Quantity <= 0) throw new CustomException.CustomException("请输入数量");

                //限制一个会员只能存在一次只能买一个订单

                if (AdvertiseOrderService.List(x => x.BuyUID == Umodel.ID && x.Direction == 0 && x.Status > 0 && x.Status < (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived).Count() > 0)
                {
                    throw new CustomException.CustomException("您还有一个订单未完成");
                }

                var param2106 = cacheSysParam.Single(x => x.ID == 2106);

                if (Quantity % param2106.Value2.ToDecimal() != 0) throw new CustomException.CustomException("数量需要为" + param2106.Value2 + "的倍数");
                if (Quantity < param2106.Value.ToDecimal()) throw new CustomException.CustomException("输入数量不能小于" + param2106.Value);


                //if (Advertise.MinimumLimit >= Advertise.HaveQuantity) Advertise.MinimumLimit = Advertise.HaveQuantity.ToDecimal();

                //if (Quantity < Advertise.MinimumLimit) throw new CustomException.CustomException("输入数量不能小于" + Advertise.MinimumLimit.ToDouble());
                //if (Quantity > Advertise.MaximumLimit) throw new CustomException.CustomException("输入数量不能大于" + Advertise.MaximumLimit.ToDouble());

                if (Quantity > (Advertise.Quantity - Advertise.HaveQuantity)) throw new CustomException.CustomException("输入数量不能大于" + (Advertise.Quantity - Advertise.HaveQuantity).ToDouble());

                Advertise.HaveQuantity += Quantity;//剩余额度
                if (Advertise.HaveQuantity.ToDecimal() >= Advertise.Quantity)
                {
                    Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Completed;

                }
                AdvertiseService.Update(Advertise);
                var User = UserService.Single(Advertise.UID);///查找当前会员
                
                if (Umodel.ID == User.ID) throw new CustomException.CustomException("对不起，不能出售自己的订单");

                var cur = CurrencyService.Single(x => x.ID == Advertise.CurID);///查找当前币种
                //Quantity = Amount / Advertise.Price;   //得到数量
                Amount = Quantity * Advertise.Price;//得到价格                                                    
                decimal buyPoundage = Quantity * cur.BuyPoundage;//得到买入的手续费
                decimal sellPoundage = Quantity * cur.SellPoundage;//得到卖出的手续费
                decimal tolQuantity = Quantity + sellPoundage;//得到最后的数量（+手续费的）


                //查看自己当前数量剩余 {出售币种，查看当前账务余额是否足够
                decimal balanceUmodel = Users.WalletCur((int)cur.WalletCurID, Umodel);
                if (balanceUmodel < tolQuantity) throw new CustomException.CustomException("对不起，您当前" + cur.CurrencyName + "数量不足！");
                //查看自己当前数量剩余 {出售币种，查看当前账务余额是否足够

                //if (bankID == -3003)
                //{
                //    var paycur = CurrencyService.Single(x => x.WalletCurID == 3003);///查找当前币种
                //    decimal balance = Users.WalletCur((int)paycur.WalletCurID, User);
                //    if (balance < Amount) throw new CustomException.CustomException("对不起，当前买家" + paycur.CurrencyName + "数量不足！");
                //}

                string orderNO = "SA" + JN.Services.Tool.StringHelp.GetRandomNumber(6);

                JN.Data.AdvertiseOrder Order = new AdvertiseOrder();
                Order.AdID = Advertise.ID;
                Order.AdOderNO = Advertise.OrderID;
                Order.Amount = Amount;
                Order.BuyPoundage = buyPoundage;
                Order.BuyUID = User.ID;
                Order.BuyUserName = User.UserName;
                Order.CoinID = Advertise.CoinID;
                Order.CoinName = Advertise.CoinName;
                Order.CreateTime = DateTime.Now;
                //if (bankID != -3003)
                //{
                    Order.PayID = bankModel.ID;//银行卡ID
                    Order.PayMethod = bankModel.BankName;//银行名
                    Order.PayAccount = bankModel.BankCard;//银行账户
                    Order.PayAccountImgUrl = bankModel.BankImgUrl;//银行账户图片
                //}
                //else
                //{
                //    Order.PayID = -3003;//银行卡ID   银行表会出现3003号ID，以负数区别
                //    Order.PayMethod = "商务钱包";//银行名
                //    Order.PayAccount = "商务钱包";//银行账户
                //}

                Order.CurID = cur.ID;
                Order.CurName = cur.CurrencyName;
                Order.CurEnSigns = cur.EnSigns;
                Order.Direction = 0;
                Order.OrderID = orderNO;
                Order.PaymentTime = DateTime.Now.AddMinutes(cacheSysParam.Single(x => x.ID == 3001).Value.ToInt());
                Order.Price = Advertise.Price;
                Order.Quantity = Quantity;
                Order.Remark = "";
                Order.SellPoundage = sellPoundage;
                Order.SellUID = Umodel.ID;
                Order.SellUserName = Umodel.UserName;
                Order.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder;// (bankID == -3003 ? (int)JN.Data.Enum.AdvertiseOrderStatus.Completed : (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder);//余额交易则已完成
                if (Order.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.Completed)
                {
                    Order.ConfirmPayTime = DateTime.Now;//确认收货时间
                }
                AdvertiseOrderService.Add(Order);

                //扣除余额，并放入冻结钱包,购买时，扣除对方数量
                Wallets.changeWallet(Umodel.ID, -tolQuantity, (int)cur.WalletCurID, "会员[" + User.UserName + "]的购买订单号为[" + Advertise.OrderID + "]的出售订单[" + orderNO + "]，您出售扣除,手续费:" + sellPoundage.ToDouble(), cur);//扣除钱包    

                ////直接成交
                #region 直接成交 未用
                //if (Order.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.Completed)
                //{
                //    decimal buyMoney = Order.Quantity;
                //    decimal sellMoney = Order.Amount;

                //    var payCur = CurrencyService.SingleAndInit(x => x.WalletCurID == 3003);//付款币种
                //    if (Order.Direction == 1)////A挂虚拟币 B是人民币 A为挂单着出售单
                //    {
                //        Wallets.changeWalletNoCommit(Order.BuyUID, buyMoney, (int)cur.WalletCurID, "出售订单号为[" + Order.AdOderNO + "]的购买订单[" + Order.OrderID + "]结算,手续费:" + Order.BuyPoundage.ToDouble(), cur);//结算给出售单者
                //    }
                //    else
                //    {
                //        //A挂人民币 B是虚拟币  A为挂单着购买单 
                //        Wallets.changeWalletNoCommit(Order.BuyUID, buyMoney, (int)cur.WalletCurID, "购买订单号为[" + Order.AdOderNO + "]的出售订单[" + Order.OrderID + "]结算,手续费:" + Order.BuyPoundage.ToDouble(), cur);//结算给购买单者

                //        Wallets.changeWallet(Order.BuyUID, -sellMoney, (int)payCur.WalletCurID, "购买订单号为[" + Order.AdOderNO + "]的出售订单[" + Order.OrderID + "]结算,手续费:" + Order.BuyPoundage.ToDouble(), payCur);
                //    }


                //    //A挂人民币 B是虚拟币  A为挂单着购买单 
                //    Wallets.changeWallet(Order.SellUID, sellMoney, (int)payCur.WalletCurID, "购买订单号为[" + Order.AdOderNO + "]的出售订单[" + Order.OrderID + "]结算,手续费:" + Order.BuyPoundage.ToDouble(), payCur);//出售者获得余额


                //    System.Text.StringBuilder sql = new System.Text.StringBuilder();//SQLBuilder实例
                //    sql.AppendFormat(" update [User] set TransactionNum=isnull(TransactionNum,0)+1,SellTradeNum=isnull(SellTradeNum,0)+{0},ShowSellTradeNum=isnull(ShowSellTradeNum,0)+{1},AddSellTradeNum=isnull(AddSellTradeNum,0)+{2} where ID={3}", Order.Quantity, Order.Quantity, Order.Quantity, Order.SellUID);

                //    sql.AppendFormat(" update [User] set TransactionNum=isnull(TransactionNum,0)+1,BuyTradeNum=isnull(BuyTradeNum,0)+{0},ShowBuyTradeNum=isnull(ShowBuyTradeNum,0)+{1},AddBuyTradeNum=isnull(AddBuyTradeNum,0)+{2} where ID={3}", Order.Quantity, Order.Quantity, Order.Quantity, Order.BuyUID);

                //    //批量处理会员的信息
                //    DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                //    //SysDBTool.ExecuteSQL(selluserSql.ToString(), dbparam);
                //    //SysDBTool.ExecuteSQL(buyuserSql.ToString(), dbparam);

                //    SysDBTool.ExecuteSQL(sql.ToString(), dbparam);
                //    //string userSql = "UPDATE [User] set TransactionNum=isnull(TransactionNum,0)+1  where id=" + Order.SellUID;
                //    //userSql += " UPDATE [User] set TransactionNum=isnull(TransactionNum,0)+1  where id=" + Umodel.ID;

                //    ////批量处理会员的信息
                //    //DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                //    //SysDBTool.ExecuteSQL(userSql.ToString(), dbparam);

                //    //写入图表
                //    decimal _volume = Order.Quantity;
                //    decimal _tranprice = Order.Price;
                //    decimal _totalamount = _volume * _tranprice;
                //    WriteChart(cur, _volume, _tranprice, _totalamount);
                //}
                #endregion

                //发送短信
                var ParamCYmsm = cacheSysParam.Single(x => x.ID == 3512);
                if (ParamCYmsm.Value == "1")
                {
                    string tempid = User.CountryCode == "+86" ? ParamCYmsm.Value3 : ParamCYmsm.Value4;
                    SMSHelper.CYmsm(User.CountryCode + User.Mobile, orderNO, tempid);
                    //JN.Services.Tool.SMSHelper.WebChineseMSM(User.Mobile, "您有新的订单,单号为[" + orderNO + "],详情请登录网站查看！");//买家
                }

                var ParamCYmsm2 = cacheSysParam.Single(x => x.ID == 3513);
                if (ParamCYmsm2.Value == "1")
                {
                    string tempid = Umodel.CountryCode == "+86" ? ParamCYmsm2.Value3 : ParamCYmsm2.Value4;
                    //发送短信
                    SMSHelper.CYmsm(Umodel.CountryCode + Umodel.Mobile, orderNO, tempid);
                    //JN.Services.Tool.SMSHelper.WebChineseMSM(Umodel.Mobile, "您下单成功，订单号为[" + orderNO + "],详情请登录网站查看！");//卖家
                }


                result.Status = 200;
                result.Message = Order.ID.ToString();
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("DoSell" + Umodel.ID);//清除缓存
            }
            return result;

        }
        #endregion

        #region 确认付款       
        /// <summary>
        /// 确认付款
        /// </summary>
        /// <param name="id">订单id</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="file">上传文件</param>
        /// <param name="AdvertiseOrderService">广告订单实体</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <param name="isAdmin">是否管理员操作</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoConfirmpayment(int id, string payimg, User Umodel, IAdvertiseOrderService AdvertiseOrderService, IUserService UserService, ISysDBTool SysDBTool, bool isAdmin = false)
        {

            Result result = new Result();
            try
            {
                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoConfirmpayment" + Umodel.ID))//检测缓存是否存在，区分大小写
                {                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");                }                MvcCore.Extensions.CacheExtensions.SetCache("DoConfirmpayment" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
                if (AdvertiseOrder == null) throw new CustomException.CustomException("没有记录");
                if (isAdmin == false && string.IsNullOrWhiteSpace(payimg)) throw new CustomException.CustomException("请您上传付款凭证");
                //string imgurl = HttpContext.Current.Request["Request"];

                //购买人是自己，且订单为已下单
                if (AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder && AdvertiseOrder.BuyUID == Umodel.ID)
                {
                    payimg = payimg.Replace("|", "");
                    AdvertiseOrder.Attachment = payimg;
                    AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.PendingPayment;
                    AdvertiseOrder.DeliveryTime = DateTime.Now.AddMinutes(cacheSysParam.Single(x => x.ID == 3002).Value.ToInt());//确认收货时间
                    AdvertiseOrderService.Update(AdvertiseOrder);
                    SysDBTool.Commit();
                    //发送短信
                    //JN.Services.Tool.SMSHelper.WebChineseMSM(UserService.Single(x => x.ID == AdvertiseOrder.SellUID).Mobile, "您的订单[" + AdvertiseOrder.OrderID + "]对方已付款成功,请您尽快核实！");
                    //发送短信
                    var ParamCYmsm = cacheSysParam.Single(x => x.ID == 3514);
                    if (ParamCYmsm.Value == "1")
                    {
                        var User = UserService.Single(x => x.ID == AdvertiseOrder.SellUID);
                        string tempid = User.CountryCode == "+86" ? ParamCYmsm.Value3 : ParamCYmsm.Value4;
                        SMSHelper.CYmsm(User.CountryCode + User.Mobile, "", tempid);
                        //JN.Services.Tool.SMSHelper.WebChineseMSM(User.Mobile, "您有新的订单,单号为[" + orderNO + "],详情请登录网站查看！");//买家
                    }
                    result.Status = 200;
                }
                else
                {
                    throw new CustomException.CustomException("当前状态不能操作");
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("DoConfirmpayment" + Umodel.ID);//清除缓存
            }
            return result;

        }
        #endregion

        #region 确认收货       
        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="id">订单id</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="AdvertiseOrderService">广告订单实体</param>
        /// <param name="CurrencyService">币种实体</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <param name="isAdmin">是否管理员操作</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoConfirmReceipt(int id, JN.Data.User Umodel, IAdvertiseOrderService AdvertiseOrderService, ICurrencyService CurrencyService, IUserService UserService, ISysDBTool SysDBTool, bool isAdmin = false)
        {
            Result result = new Result();
            try
            {                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoConfirmReceipt" + Umodel.ID))//检测缓存是否存在，区分大小写
                {                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");                }                MvcCore.Extensions.CacheExtensions.SetCache("DoConfirmReceipt" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
                if (AdvertiseOrder == null) throw new CustomException.CustomException("没有记录");

                //必须是已付款，且出售人（卖出）是自己
                if (AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PendingPayment && (isAdmin == true || (isAdmin == false && AdvertiseOrder.SellUID == Umodel.ID)))
                {
                    AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.Completed;//GoodsReceived
                    AdvertiseOrder.ConfirmPayTime = DateTime.Now;//确认收货时间
                    AdvertiseOrderService.Update(AdvertiseOrder);

                    var cur = CurrencyService.Single(x => x.ID == AdvertiseOrder.CurID);
                    var cur3004 = CurrencyService.Single(x => (x.WalletCurID??0)== 3004);

                    List<string> sqlString = new List<string>();
                    #region 手续费分成
                    var param1112 = cacheSysParam.Single(x => x.ID == 1112);
                    if (param1112.Value.ToDecimal() > 0)
                    {
                        var sellUser = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(AdvertiseOrder.SellUID);
                        if (sellUser != null)
                        {
                            string[] red_ids = sellUser.RefereePath.Split(',');
                            var redUserList = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => red_ids.Contains(x.ID.ToString()) && !x.IsLock && x.IsActivation && x.UserLevel >= 4).ToList();
                            if (redUserList.Count() > 0)
                            {
                                decimal money = param1112.Value.ToDecimal() * AdvertiseOrder.SellPoundage / redUserList.Count();//计算分成
                                foreach(var redItem in redUserList)
                                {
                                    sqlString.Add(string.Format("insert into BonusDetail(UID, UserName,BonusID,BonusName,BonusMoney,Description, CreateTime, IsBalance, BalanceTime, IsEffective,EffectiveTime,FromUID,FromUserName, SupplyNo, ReserveInt1,Period,BonusMoneyCF,BonusMoneyCFBA) " +
                                                           "values({0}, '{1}', {2},'{3}', {4}, '{5}','{6}',{7},'{8}',{9},'{10}',{11},'{12}','{13}',{14},{15},{16},{17})",
                                                           redItem.ID, redItem.UserName, param1112.ID, param1112.Name, money, "来自订单" + AdvertiseOrder.AdOderNO + "完成交易获得" + param1112.Name, DateTime.Now, 1, DateTime.Now, 1, DateTime.Now, AdvertiseOrder.SellUID, AdvertiseOrder.SellUserName, AdvertiseOrder.AdOderNO, 0, 0,0, 0));
                                    sqlString.Add(string.Format("insert into WalletLog(UID, UserName, CoinID, CoinName, ChangeMoney, Balance, Description, CreateTime) values({0}, '{1}', {2},'{3}', {4}, {5},'{6}','{7}')",
                                         redItem.ID, redItem.UserName, (cur3004.WalletCurID ?? 0), cur3004.CurrencyName, money, (redItem.Cur3004??0) + money, "来自订单" + AdvertiseOrder.AdOderNO + "完成交易获得" + param1112.Name, DateTime.Now));
                                    sqlString.Add(string.Format(" update [User] set Cur3004=isnull(Cur3004,0)+{0} where ID ={1} ", money, redItem.ID));
                                }
                            }
                        }
                    }
                    
                    
                    #endregion
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                    {
                        decimal changMoney = AdvertiseOrder.Quantity - AdvertiseOrder.BuyPoundage;
                        //交易已经完成，扣除手续费，加入币种给买入者
                        //交易已经完成，扣除手续费，加入币种给买入者
                        if (AdvertiseOrder.Direction == 1)////A挂虚拟币 B是人民币 A为挂单着出售单
                        {
                            Wallets.changeWallet(AdvertiseOrder.BuyUID, changMoney, (int)cur.WalletCurID, "出售订单号为[" + AdvertiseOrder.AdOderNO + "]的购买订单[" + AdvertiseOrder.OrderID + "]结算,手续费:" + AdvertiseOrder.BuyPoundage.ToDouble(), cur);//结算给出售单者
                        }
                        else
                        {
                            //A挂人民币 B是虚拟币  A为挂单着购买单 
                            Wallets.changeWallet(AdvertiseOrder.BuyUID, changMoney, (int)cur.WalletCurID, "购买订单号为[" + AdvertiseOrder.AdOderNO + "]的出售订单[" + AdvertiseOrder.OrderID + "]结算,手续费:" + AdvertiseOrder.BuyPoundage.ToDouble(), cur);//结算给购买单者
                        }

                        //计算奖项给会员（推荐人）
                        //if ((c.IsInvitaReward ?? false))
                        //{
                        //    var buyuser = UserService.Single(x => x.ID == AdvertiseOrder.BuyUID);
                        //    //查找推荐人(购买者)
                        //    var rebuyuser = UserService.Single(x => x.ID == buyuser.RefereeID);

                        //    decimal bounsMoney = AdvertiseOrder.Quantity * c.InvitaRewardParam.ToDecimal();
                        //    if (rebuyuser != null)
                        //    {
                        //        MD_TradeBLL.updateUserWallet(rebuyuser.ID, c.ID, bounsMoney, "来自会员[" + AdvertiseOrder.BuyUserName + "]单号[" + AdvertiseOrder.OrderID + "]完成交易，得到邀请奖励");
                        //        //Wallets.changeWallet(rebuyuser.ID, bounsMoney, (int)c.WalletCurID, "来自会员[" + AdvertiseOrder.BuyUserName + "]单号[" + AdvertiseOrder.OrderID + "]完成交易，得到邀请奖励", c);
                        //    }
                        //    var selluser = UserService.Single(x => x.ID == AdvertiseOrder.SellUID);
                        //    //查找推荐人(出售者)
                        //    var reselluser = UserService.Single(x => x.ID == selluser.RefereeID);
                        //    if (rebuyuser != null)
                        //    {
                        //        MD_TradeBLL.updateUserWallet(reselluser.ID,  c.ID, bounsMoney, "来自会员[" + AdvertiseOrder.SellUserName + "]单号[" + AdvertiseOrder.OrderID + "]完成交易，得到邀请奖励");
                        //        //Wallets.changeWallet(reselluser.ID, bounsMoney, (int)c.WalletCurID, "来自会员[" + AdvertiseOrder.SellUserName + "]单号[" + AdvertiseOrder.OrderID + "]完成交易，得到邀请奖励", c);
                        //    }
                        //}

                        ////给购买人和出售人添加交易量
                        //var selluser = UserService.Single(x => x.ID == AdvertiseOrder.SellUID);
                        //var buyuser = UserService.Single(x => x.ID == AdvertiseOrder.BuyUID);
                        //selluser.TransactionNum = (selluser.TransactionNum ?? 0) + 1;
                        //buyuser.TransactionNum = (buyuser.TransactionNum ?? 0) + 1;
                        //UserService.Update(selluser);
                        //UserService.Update(buyuser);


                        SysDBTool.Commit();



                        var ParamCYmsm = cacheSysParam.Single(x => x.ID == 3515);
                        if (ParamCYmsm.Value == "1")
                        {
                            if (AdvertiseOrder.Direction == 1)////A挂虚拟币 B是人民币 A为挂单着出售单
                            {

                                var User = UserService.Single(x => x.ID == AdvertiseOrder.BuyUID);
                                string tempid = User.CountryCode == "+86" ? ParamCYmsm.Value3 : ParamCYmsm.Value4;
                                SMSHelper.CYmsm(User.CountryCode + User.Mobile, "", tempid);
                                //发送短信
                                //JN.Services.Tool.SMSHelper.WebChineseMSM(UserService.Single(x => x.ID == AdvertiseOrder.BuyUID).Mobile, "您的订单[" + AdvertiseOrder.OrderID + "]对方已确认,请您尽快核实！");
                            }
                            else
                            {
                                var User = UserService.Single(x => x.ID == AdvertiseOrder.BuyUID);
                                string tempid = User.CountryCode == "+86" ? ParamCYmsm.Value3 : ParamCYmsm.Value4;
                                SMSHelper.CYmsm(User.CountryCode + User.Mobile, "", tempid);
                                //发送短信
                                //JN.Services.Tool.SMSHelper.WebChineseMSM(UserService.Single(x => x.ID == AdvertiseOrder.BuyUID).Mobile, "您的订单[" + AdvertiseOrder.OrderID + "]对方已确认,请您尽快核实！");
                            }
                        }
                        //string selluserSql = "UPDATE [User] set TransactionNum=isnull(TransactionNum,0)+1  where id=" + AdvertiseOrder.SellUID;
                        //string buyuserSql = "UPDATE [User] set TransactionNum=isnull(TransactionNum,0)+1  where id=" + AdvertiseOrder.BuyUID;

                       // System.Text.StringBuilder sql = new System.Text.StringBuilder();//SQLBuilder实例
                        sqlString.Add(string.Format(" update [User] set TransactionNum=isnull(TransactionNum,0)+1,BuyTradeNum=isnull(BuyTradeNum,0)+{0},ShowBuyTradeNum=isnull(ShowBuyTradeNum,0)+{1},AddBuyTradeNum=isnull(AddBuyTradeNum,0)+{2} where ID={3}", AdvertiseOrder.Quantity, AdvertiseOrder.Quantity, AdvertiseOrder.Quantity, AdvertiseOrder.BuyUID));

                        sqlString.Add(string.Format(" update [User] set TransactionNum=isnull(TransactionNum,0)+1,SellTradeNum=isnull(SellTradeNum,0)+{0},ShowSellTradeNum=isnull(ShowSellTradeNum,0)+{1},AddSellTradeNum=isnull(AddSellTradeNum,0)+{2} where ID={3}", AdvertiseOrder.Quantity, AdvertiseOrder.Quantity, AdvertiseOrder.Quantity, AdvertiseOrder.SellUID));


                        //DbParameter[] dbparam = new System.Data.Common.DbParameter[] { };
                        ////SysDBTool.ExecuteSQL(selluserSql.ToString(), dbparam);
                        ////SysDBTool.ExecuteSQL(buyuserSql.ToString(), dbparam);

                        //SysDBTool.ExecuteSQL(sql.ToString(), dbparam);

                        //写入图表
                        decimal _volume = AdvertiseOrder.Quantity;
                        decimal _tranprice = AdvertiseOrder.Price;
                        decimal _totalamount = _volume * _tranprice;

                        if (cur.TranSwitch == true)
                            UpdataPrice(cur, _volume);
                        WriteChart(cur, _volume, _tranprice, _totalamount);

                        //批量处理会员的信息
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
                        result.Status = 200;
                    }
                }
                else
                {
                    throw new CustomException.CustomException("当前状态不能操作");
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("DoConfirmReceipt" + Umodel.ID);//清除缓存
            }
            return result;

        }
        #endregion

        #region 取消订单 
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="id">订单id</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="AdvertiseOrderService">广告订单实体</param>
        /// <param name="CurrencyService">币种实体</param>
        /// <param name="AdvertiseService">广告单实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <param name="isAdmin">是否管理员操作</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoConfirmCancel(int id, JN.Data.User Umodel, IAdvertiseOrderService AdvertiseOrderService, ICurrencyService CurrencyService, IAdvertiseService AdvertiseService, ISysDBTool SysDBTool, bool isAdmin = false)
        {
            Result result = new Result();
            try
            {                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoConfirmCancel" + Umodel.ID))//检测缓存是否存在，区分大小写
                {                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");                }                MvcCore.Extensions.CacheExtensions.SetCache("DoConfirmCancel" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存
                //查找订单
                var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
                if (AdvertiseOrder == null) throw new CustomException.CustomException("没有记录");
                //必须是未完成的，且是订单本人
                if (
                    ((AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder && (AdvertiseOrder.SellUID == Umodel.ID || AdvertiseOrder.BuyUID == Umodel.ID)) && isAdmin == false)
                    || ((AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder || AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.PendingPayment) && isAdmin == true)
                    )
                {
                    AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.Cancel;

                    //把币返还到钱包或者订单中
                    var Advertise = AdvertiseService.Single(x => x.ID == AdvertiseOrder.AdID);
                    var c = CurrencyService.Single(x => x.ID == Advertise.CurID);

                    if (Advertise.Direction == 0)
                    {
                        Advertise.HaveQuantity -= AdvertiseOrder.Quantity;//(AdvertiseOrder.Quantity + AdvertiseOrder.SellPoundage);//返还余额
                        if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed) Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Underway;//如果订单是已完成状态，改成正在进行
                        AdvertiseService.Update(Advertise);
                    }
                    else
                    {
                        Advertise.HaveQuantity -= AdvertiseOrder.Quantity;//返还余额
                        if (Advertise.Status == (int)JN.Data.Enum.AdvertiseStatus.Completed) Advertise.Status = (int)JN.Data.Enum.AdvertiseStatus.Underway;//如果订单是已完成状态，改成正在进行
                        AdvertiseService.Update(Advertise);
                        //if (Advertise.CoinID == 3) //&& Advertise.Direction == 1
                        //{
                        //    var payCur = CurrencyService.SingleAndInit(x => x.ID == Advertise.CoinID);//付款币种
                        //    Wallets.changeWallet(AdvertiseOrder.BuyUID, (AdvertiseOrder.Amount + AdvertiseOrder.BuyPoundage), (int)payCur.WalletCurID, "取消订单[" + AdvertiseOrder.OrderID + "]返还", payCur);
                        //}

                        Wallets.changeWallet(AdvertiseOrder.SellUID, (AdvertiseOrder.Quantity + AdvertiseOrder.SellPoundage), (int)c.WalletCurID, "取消订单[" + AdvertiseOrder.OrderID + "]返还", c);
                    }

                    AdvertiseOrderService.Update(AdvertiseOrder);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
                else { throw new CustomException.CustomException("当前状态不能操作"); }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("DoConfirmCancel" + Umodel.ID);//清除缓存
            }
            return result;

        }
        #endregion

        #region 投诉提交 
        /// <summary>
        /// 投诉提交
        /// </summary>
        /// <param name="form">form表单</param>
        /// <param name="file">上传文件</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="MessageService">信息实体</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoComplaintWrite(FormCollection form, HttpPostedFileBase file, JN.Data.User Umodel, IMessageService MessageService, IUserService UserService, ISysDBTool SysDBTool)
        {

            Result result = new Result();
            try
            {
                string recipient = "Admin";
                string subject = form["subject"];
                string content = form["comment"];
                string messagetype = form["messagetype"];

                if (subject.Trim().Length == 0)
                {
                    throw new CustomException.CustomException("标题不能为空");
                }

                int toUID = 0;
                if (recipient == "Admin") recipient = "管理员";
                if (recipient.Trim() != "管理员")
                {
                    if (UserService.List(x => x.UserName.Equals(recipient.Trim())).Count() <= 0)
                        throw new CustomException.CustomException("收件人不存在");
                    else
                        toUID = UserService.Single(x => x.UserName.Equals(recipient.Trim())).ID;

                }

                subject = Services.Tool.StringHelp.FilterSqlHtml(subject);//过滤敏感字符
                content = Services.Tool.StringHelp.FilterSqlHtml(content);//过滤敏感字符

                //HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (file != null)
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {
                        throw new CustomException.CustomException("非法上传，您只可以上传图片格式的文件");
                        //ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                    }

                    //20160711安全更新 ---------------- start
                    var newfilename = "MAIL_" + Umodel.UserName + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(HttpContext.Current.Request.MapPath("~/Upload/Complaint")))
                        Directory.CreateDirectory(HttpContext.Current.Request.MapPath("~/Upload/Complaint"));

                    if (Path.GetExtension(file.FileName).ToLower().Contains("aspx") || Path.GetExtension(file.FileName).ToLower().Contains("php"))
                    {
                        var wlog = new Data.WarningLog();
                        wlog.CreateTime = DateTime.Now;
                        wlog.IP = HttpContext.Current.Request.UserHostAddress;
                        if (HttpContext.Current.Request.UrlReferrer != null)
                            wlog.Location = HttpContext.Current.Request.UrlReferrer.ToString();
                        wlog.Platform = "会员";
                        wlog.WarningMsg = "试图上传木马文件";
                        wlog.WarningLevel = "严重";
                        wlog.ResultMsg = "拒绝";
                        wlog.UserName = Umodel.UserName;
                        MvcCore.Unity.Get<IWarningLogService>().Add(wlog);

                        Umodel.IsLock = true;
                        Umodel.LockTime = DateTime.Now;
                        Umodel.LockReason = "试图上传木马文件";
                        UserService.Update(Umodel);
                        SysDBTool.Commit();
                        throw new Exception("试图上传木马文件，您的帐号已被冻结");
                    }

                    var fileName = Path.Combine(HttpContext.Current.Request.MapPath("~/Upload/Complaint"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, HttpContext.Current.Request.MapPath("~/Upload/"), 1024, 768, "EQU");
                        imgurl = "/Upload/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("上传失败：" + ex.Message);
                    }
                    ////finally
                    ////{
                    ////    System.IO.File.Delete(fileName); //删除原文件
                    ////}
                    //20160711安全更新  --------------- end
                }

                var model = new Data.Message();
                model.Attachment = imgurl;
                model.MessageType = messagetype;
                model.Content = content;
                model.CreateTime = DateTime.Now;
                model.FormUID = Umodel.ID;
                model.FormUserName = Umodel.UserName;
                model.ForwardID = 0;
                model.IsFlag = false;
                model.IsForward = false;
                model.IsRead = false;
                model.IsReply = false;
                model.IsSendSuccessful = true;
                model.IsStar = false;
                model.ReplyID = 0;
                model.Title = subject;
                model.ToUID = toUID;
                model.ToUserName = recipient.Trim();
                model.UID = Umodel.ID;

                var model2 = new Data.Message();
                model2.Attachment = model.Attachment;
                model2.MessageType = model.MessageType;
                model2.Content = model.Content;
                model2.CreateTime = model.CreateTime;
                model2.FormUID = model.FormUID;
                model2.FormUserName = model.FormUserName;
                model2.ForwardID = model.ForwardID;
                model2.IsFlag = model.IsFlag;
                model2.IsForward = model.IsForward;
                model2.IsRead = model.IsRead;
                model2.IsReply = model.IsReply;
                model2.IsSendSuccessful = model.IsSendSuccessful;
                model2.IsStar = model.IsStar;
                model2.ReplyID = model.ReplyID;
                model2.Title = model.Title;
                model2.ToUID = model.ToUID;
                model2.ToUserName = model.ToUserName;
                model2.UID = model.ToUID;

                MessageService.Add(model);
                MessageService.Add(model2);
                SysDBTool.Commit();

                if (model.ID > 0 && model2.ID > 0)
                {
                    model.RelationID = model2.ID;
                    MessageService.Update(model);
                    SysDBTool.Commit();
                    model2.RelationID = model.ID;
                    MessageService.Update(model2);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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

        #region 下架广告 
        /// <summary>
        /// 下架广告
        /// </summary>
        /// <param name="form">form表单</param>
        /// <param name="file">上传文件</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="MessageService">信息实体</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <param name="isAdmin">是否管理员操作</param>
        /// <returns>返回状态码和信息</returns>
        public static Result DoCancelAdvertise(JN.Data.User Umodel, IAdvertiseService AdvertiseService, IAdvertiseOrderService AdvertiseOrderService, ICurrencyService CurrencyService, ISysDBTool SysDBTool, bool isAdmin = false)
        {

            Result result = new Result();
            try
            {                if (MvcCore.Extensions.CacheExtensions.CheckCache("DoCancelAdvertise" + Umodel.ID))//检测缓存是否存在，区分大小写
                {                    throw new JN.Services.CustomException.CustomException("正在处理相关数据,请您稍后再试");                }                MvcCore.Extensions.CacheExtensions.SetCache("DoCancelAdvertise" + Umodel.ID, Umodel.ID, MvcCore.Extensions.CacheTimeType.ByMinutes, 1);//使用缓存

                int adid = HttpContext.Current.Request["adid"].ToInt();
                var ad = AdvertiseService.Single(x => x.ID == adid);
                if (ad == null) throw new CustomException.CustomException("广告单不存在！");

                if (ad.Status == (int)JN.Data.Enum.AdvertiseStatus.Underway && (isAdmin == true || (isAdmin == false && ad.UID == Umodel.ID)))
                {
                    //查看此订单有没有子单正在进行的
                    var adorder = AdvertiseOrderService.Single(x => x.AdID == ad.ID && x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.PlaceOrder && x.Status < (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived);
                    if (adorder == null)
                    {
                        decimal quantity = 0;//退还数量
                        decimal havefee = 0;//已成交手续费
                        if (ad.Direction == 0)//如果是出售单，返还币种给会员
                        {
                            var cur = CurrencyService.Single(x => x.ID == ad.CurID);
                            havefee = AdvertiseOrderService.List(x => x.AdID == ad.ID && x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived).Count() > 0 ? AdvertiseOrderService.List(x => x.AdID == ad.ID && x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived).Sum(x => x.BuyPoundage) : 0;

                            quantity = ad.Quantity - ad.HaveQuantity + ad.Poundage - havefee;//挂卖数量-已成交数量
                            Wallets.changeWallet(ad.UID, quantity, (int)cur.WalletCurID, "来自出售广告单[" + ad.OrderID + "]的下架退还", cur);
                        }
                        //else
                        //{
                        //    //购买没有手续费，先不做处理
                        //    if (ad.CoinID == 3) //&& Advertise.Direction == 1
                        //    {
                        //        var payCur = CurrencyService.SingleAndInit(x => x.ID == ad.CoinID);//付款币种
                        //        quantity = (ad.Quantity - ad.HaveQuantity) * ad.Price;//剩余数量*价格
                        //        Wallets.changeWallet(ad.UID, quantity, (int)payCur.WalletCurID, "来自购买广告单[" + ad.OrderID + "]的下架退还", payCur);
                        //    }
                        //}

                        ad.Status = (int)AdvertiseStatus.Cancel;
                        AdvertiseService.Update(ad);
                        SysDBTool.Commit();
                        result.Status = 200;
                    }
                    else throw new CustomException.CustomException("当前广告单有未完成的交易，不能下架");
                }
                else throw new CustomException.CustomException("当前状态不能下架");
            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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
            }            finally            {                MvcCore.Extensions.CacheExtensions.ClearCache("DoCancelAdvertise" + Umodel.ID);//清除缓存
            }
            return result;

        }
        #endregion

        #region 评价卖家买家
        /// <summary>
        /// 评价
        /// </summary>
        /// <returns></returns>
        public static Result DoEvaluate(JN.Data.User Umodel, IAdvertiseOrderService AdvertiseOrderService, IUserService UserService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                int evaluate = HttpContext.Current.Request["evaluate"].ToInt();
                int id = HttpContext.Current.Request["adid"].ToInt();
                var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == id);
                if (AdvertiseOrder == null) throw new CustomException.CustomException("没有记录");
                if (AdvertiseOrder.Status == (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived && (Umodel.ID == AdvertiseOrder.BuyUID || Umodel.ID == AdvertiseOrder.SellUID))
                {
                    var buyuser = UserService.Single(x => x.ID == AdvertiseOrder.BuyUID);
                    var selluser = UserService.Single(x => x.ID == AdvertiseOrder.SellUID);

                    if (Umodel.ID == AdvertiseOrder.SellUID && !(AdvertiseOrder.IsSellAppraise ?? false))//如果是出售者
                    {
                        if (evaluate == 1) buyuser.Positive = (buyuser.Positive ?? 0) + 1;
                        if (evaluate == 2) buyuser.Neutral = (buyuser.Neutral ?? 0) + 1;
                        if (evaluate == 3) buyuser.Negative = (buyuser.Negative ?? 0) + 1;
                        UserService.Update(buyuser);
                        AdvertiseOrder.IsSellAppraise = true;//卖家评价
                        AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.PendingEvalu;
                    }
                    else if (Umodel.ID == AdvertiseOrder.BuyUID && !(AdvertiseOrder.IsBuyAppraise ?? false))
                    {
                        if (evaluate == 1) selluser.Positive = (buyuser.Positive ?? 0) + 1;
                        if (evaluate == 2) selluser.Neutral = (buyuser.Neutral ?? 0) + 1;
                        if (evaluate == 3) selluser.Negative = (buyuser.Negative ?? 0) + 1;
                        UserService.Update(selluser);
                        AdvertiseOrder.IsBuyAppraise = true;//买家评价
                        AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.PendingEvalu;
                    }
                    else { throw new CustomException.CustomException("您已评价，不能继续评价"); }

                    //双方都评价后直接已完成状态
                    if ((AdvertiseOrder.IsBuyAppraise ?? false) && (AdvertiseOrder.IsSellAppraise ?? false))
                    {
                        AdvertiseOrder.Status = (int)JN.Data.Enum.AdvertiseOrderStatus.Completed;
                    }
                    AdvertiseOrderService.Update(AdvertiseOrder);
                    SysDBTool.Commit();
                    result.Status = 200;
                }
                else { throw new CustomException.CustomException("当前状态不能操作"); }

            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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

        #region 聊天内容提交

        /// <summary>
        /// 聊天内容提交
        /// </summary>
        /// <returns></returns>
        public static Result DoMsgSubmit(HttpPostedFileBase file, User Umodel, IAdvertiseOrderService AdvertiseOrderService, IChatingService ChatingService, ISysDBTool SysDBTool)
        {
            Result result = new Result();
            try
            {
                string msgcontent = HttpContext.Current.Request["msgconetent"];
                int adOrderid = HttpContext.Current.Request["adorderid"].ToInt();
                var AdvertiseOrder = AdvertiseOrderService.Single(x => x.ID == adOrderid);
                if (AdvertiseOrder == null) throw new CustomException.CustomException("没有这个订单");

                if (file == null && string.IsNullOrEmpty(msgcontent))
                {
                    throw new CustomException.CustomException("请填写内容");
                }

                msgcontent = Services.Tool.StringHelp.FilterSqlHtml(msgcontent);//过滤敏感字符

                string imgurl = "";
                if (file != null && file.FileName != "")
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {

                        throw new CustomException.CustomException("非法上传，您只可以上传图片格式的文件");
                    }

                    //20160711安全更新 ---------------- start
                    var newfilename = "Chating_" + Umodel.UserName + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(HttpContext.Current.Request.MapPath("~/Upload/Chating")))
                        Directory.CreateDirectory(HttpContext.Current.Request.MapPath("~/Upload/Chating"));

                    if (Path.GetExtension(file.FileName).ToLower().Contains("aspx") || Path.GetExtension(file.FileName).ToLower().Contains("php"))
                    {
                        var wlog = new Data.WarningLog();
                        wlog.CreateTime = DateTime.Now;
                        wlog.IP = HttpContext.Current.Request.UserHostAddress;
                        if (HttpContext.Current.Request.UrlReferrer != null)
                            wlog.Location = HttpContext.Current.Request.UrlReferrer.ToString();
                        wlog.Platform = "会员";
                        wlog.WarningMsg = "试图上传木马文件";
                        wlog.WarningLevel = "严重";
                        wlog.ResultMsg = "拒绝";
                        wlog.UserName = Umodel.UserName;
                        MvcCore.Unity.Get<IWarningLogService>().Add(wlog);

                        Umodel.IsLock = true;
                        Umodel.LockTime = DateTime.Now;
                        Umodel.LockReason = "试图上传木马文件";
                        MvcCore.Unity.Get<IUserService>().Update(Umodel);
                        SysDBTool.Commit();
                        throw new Exception("试图上传木马文件，您的帐号已被冻结");
                    }

                    var fileName = Path.Combine(HttpContext.Current.Request.MapPath("~/Upload/Chating"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, HttpContext.Current.Request.MapPath("~/Upload/Chating/"), 1024, 768, "EQU");
                        imgurl = "/Upload/Chating/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new CustomException.CustomException("上传文件失败");
                    }
                    //20160711安全更新  --------------- end
                }

                ChatingService.Add(new JN.Data.Chating()
                {
                    AdOrderID = AdvertiseOrder.ID,
                    Attachment = imgurl,
                    CreateTime = DateTime.Now,
                    MsgType = "订单聊天",
                    SendFace = Umodel.HeadFace,
                    OrderNo = AdvertiseOrder.OrderID,
                    SendUID = Umodel.ID,
                    SendUserName = Umodel.UserName,
                    RecUID = 0,//暂时用不到，预留
                    MsgContent = msgcontent,
                    RecUserName = "",
                    RecFace = ""
                });
                SysDBTool.Commit();
                result.Status = 200;


            }
            catch (CustomException.CustomException ex)
            {
                result.Message = ex.Message;
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

        #region 记录图表，写入数据      
        /// <summary>
        /// 记录图表，写入数据 
        /// </summary>
        /// <param name="id">订单id</param>
        /// <param name="Umodel">当前会员</param>
        /// <param name="AdvertiseOrderService">广告订单实体</param>
        /// <param name="CurrencyService">币种实体</param>
        /// <param name="UserService">会员实体</param>
        /// <param name="SysDBTool">工具实体</param>
        /// <returns>返回状态码和信息</returns>
        public static void WriteChart(JN.Data.Currency cur, decimal _volume, decimal _tranprice, decimal _totalamount)
        {
            //有成交的话记数据
            if (_volume > 0)
            {
                //把当天成交的数据加入到缓存 ------ lin 2017年7月27日14:08:09 -------
                CacheTransactionDataHelper.SetTradeListDay();

                var chartService = MvcCore.Unity.Get<IAdvertiseChartDayService>();
                var chartDayService = MvcCore.Unity.Get<IPriceTracking1DayService>();
                //记K线图
                if (chartService.List(x => SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) == 0 && x.CurID == cur.ID).Count() <= 0)
                {
                    decimal ycloseprice = PriceHelps.getyescloseprice(cur);//昨日关盘价
                    var yesterday = chartService.List(x => x.CurID == cur.ID).OrderByDescending(x => x.ID).FirstOrDefault();
                    if (yesterday != null) ycloseprice = yesterday.ClosePrice;

                    var ma5list = chartService.List(x => x.CurID == cur.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) <= 5);
                    decimal ma5 = ma5list.Count() > 0 ? ma5list.Average(x => x.ClosePrice) : 0;

                    var ma10list = chartService.List(x => x.CurID == cur.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) <= 10);
                    decimal ma10 = ma10list.Count() > 0 ? ma10list.Average(x => x.ClosePrice) : 0;

                    var ma30list = chartService.List(x => x.CurID == cur.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) <= 30);
                    decimal ma30 = ma30list.Count() > 0 ? ma30list.Average(x => x.ClosePrice) : 0;

                    //这是获取最新价格，涨幅，成交量信息
                    JN.Data.AdvertiseChartDay scd = new Data.AdvertiseChartDay
                    {
                        YesterdayClosePrice = ycloseprice,
                        Turnover = _totalamount,
                        UpsAndDownsPrice = 0,
                        MA5 = ma5,
                        MA10 = ma10,
                        MA30 = ma30,
                        UpOrDown = "",
                        ClosePrice = _tranprice,
                        CreateTime = DateTime.Now,
                        HightPrice = _tranprice,
                        LowPrice = _tranprice,
                        OpenPrice = _tranprice,
                        StockDate = DateTime.Now,
                        Volume = _volume,
                        CurImages = cur.CurrencyLogo,
                        TotalValue = _tranprice * cur.TotalIssued,
                        CurEnglish = cur.EnSigns,
                        CurName = cur.CurrencyName,
                        UpsAndDownsScale = 0,
                        TotalStock = 0,
                        RegionID = 0,
                        CurID = cur.ID,
                        AreaID = 0,
                    };
                    chartService.Add(scd);

                    //日K
                    chartDayService.Add(new Data.PriceTracking1Day()
                    {
                        ClosePrice = _tranprice.ToDouble(),
                        CreateTime = DateTime.Now,
                        CurrPrice = _tranprice.ToDouble(),
                        MaxPrice = _tranprice.ToDouble(),
                        MinPrice = _tranprice.ToDouble(),
                        Volume = _volume,
                        OpenPrice = _tranprice.ToDouble(),
                        ProductID = cur.ID,
                        Time = DateTime.Now,
                        SourceStr = ""
                    });

                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                }
                else
                {
                    //这是获取最新价格，涨幅，成交量信息
                    var chart = chartService.Single(x => SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) == 0 && x.CurID == cur.ID);
                    if (chart != null)
                    {
                        if (_tranprice > chart.HightPrice) chart.HightPrice = _tranprice;
                        if (_tranprice < chart.LowPrice) chart.LowPrice = _tranprice;
                        chart.Volume = chart.Volume + _volume;
                        chart.Turnover = chart.Turnover + _totalamount;
                        chart.UpOrDown = (_tranprice > chart.ClosePrice ? "↑" : _tranprice < chart.ClosePrice ? "↓" : "");//价格涨跌
                        chart.ClosePrice = _tranprice;
                        chart.TotalValue = _tranprice * cur.TotalIssued;
                        chart.RegionID = 0;
                        chart.TotalValue = 0;
                        chart.CurEnglish = cur.EnSigns;
                        chart.CurName = cur.CurrencyName;
                        chart.CurImages = cur.CurrencyLogo;
                        chart.UpsAndDownsScale = (double)((chart.ClosePrice - chart.OpenPrice) / chart.OpenPrice);
                        chart.UpsAndDownsPrice = chart.ClosePrice - chart.OpenPrice;
                        var ma5list = chartService.List(x => x.CurID == cur.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) <= 5);
                        decimal ma5 = ma5list.Count() > 0 ? ma5list.Average(x => x.ClosePrice) : 0;
                        var ma10list = chartService.List(x => x.CurID == cur.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) <= 10);
                        decimal ma10 = ma10list.Count() > 0 ? ma10list.Average(x => x.ClosePrice) : 0;
                        var ma30list = chartService.List(x => x.CurID == cur.ID && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) <= 30);
                        decimal ma30 = ma30list.Count() > 0 ? ma30list.Average(x => x.ClosePrice) : 0;
                        chart.MA5 = ma5;
                        chart.MA10 = ma10;
                        chart.MA30 = ma30;
                        chartService.Update(chart);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }

                    //日K
                    var chartDayline = chartDayService.Single(x => SqlFunctions.DateDiff("DAY", x.Time, DateTime.Now) == 0 && x.ProductID == cur.ID);
                    if (chartDayline != null)
                    {
                        if (_tranprice > chart.HightPrice) chartDayline.MaxPrice = _tranprice.ToDouble();
                        if (_tranprice < chart.LowPrice) chartDayline.MinPrice = _tranprice.ToDouble();
                        chartDayline.Volume = chart.Volume + _volume;
                        chartDayline.CurrPrice = _tranprice.ToDouble();
                        chartDayline.ClosePrice = _tranprice.ToDouble();
                        chartDayService.Update(chartDayline);
                        MvcCore.Unity.Get<ISysDBTool>().Commit();
                    }
                }
            }
        }
        #endregion

        #region 更新价格
        public static void UpdataPrice(JN.Data.Currency cur, decimal _volume)
        {
            var cModel = MvcCore.Unity.Get<CurrencyService>().Single(x => x.ID == cur.ID);
            cModel.TranTotal += _volume;

            var a = (cModel.TranTotal ?? 1) / (cModel.IncreaseConditions ?? 100);

            var b = Math.Floor(a);
            if (b != cModel.IncreaseTimes)
            {
                cModel.TranPrice = cModel.OriginalPrice + (b * (cModel.IncreasePrice ?? 0));
            }
            MvcCore.Unity.Get<CurrencyService>().Update(cModel);
            MvcCore.Unity.Get<ISysDBTool>().Commit();
        }
        #endregion
    }
}