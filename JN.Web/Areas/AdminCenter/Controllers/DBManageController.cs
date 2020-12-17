using JN.Data;
using JN.Data.Service;
using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Manager;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class DBManageController : BaseController
    {
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public DBManageController(ISysDBTool SysDBTool, IActLogService ActLogService)
        {
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
        }

        public ActionResult Index(int? page)
        {
            ViewBag.Title = "数据库备份与恢复";
            ActMessage = ViewBag.Title;

            int pageIndex = page ?? 1;
            IOHelper.CreateDirectory(Server.MapPath("/DBBackUp/"));

            var files = IOHelper.GetAllFilesInDirectory(Server.MapPath("/DBBackUp/"));
            var lst = new List<Data.Extensions.DbBackFile>();
            foreach (FileInfo info in files)
            {
                lst.Add(new Data.Extensions.DbBackFile { BackFileName = info.Name, BackFileFullName = info.FullName, BackFileSize = info.Length > 0 ? info.Length / 1024 : 0, CreateTime = info.LastWriteTime });
            }

            //Linq进行排序
            var query = from items in lst orderby items.CreateTime descending select items;
            return View(query.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Backup()
        {
            ActMessage = "备份数据库";
            IOHelper.CreateDirectory(Server.MapPath("/DBBackUp/"));
            SqlParameter[] para_sys = new SqlParameter[]
            {
            new SqlParameter ("@BKPATH", Server.MapPath("/DBBackUp/") + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak" ),
            };

            int count = MvcCore.Unity.Get<ISysDBTool>().Execute<object>("backup database " + ConfigHelper.GetConfigString("DBName") + " to disk=@BKPATH", para_sys).Count();

            if (count >= 0)
            {
               
                ViewBag.SuccessMsg = "数据库备份成功！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "数据库备份失败！请查看系统日志。";
            return View("Error");
        }

        public ActionResult Restore(string backfilename)
        {
            ActMessage = "恢复数据库";
            SqlParameter[] para = new SqlParameter[]
            {
                new SqlParameter ("@BKFILE", Server.MapPath("/DBBackUp/") + backfilename),
            };
            int count = MvcCore.Unity.Get<ISysDBTool>().Execute<object>("ALTER DATABASE [" + ConfigHelper.GetConfigString("DBName") + "] SET OFFLINE WITH ROLLBACK IMMEDIATE; restore database " + ConfigHelper.GetConfigString("DBName") + " from disk=@BKFILE WITH REPLACE;ALTER DATABASE [" + ConfigHelper.GetConfigString("DBName") + "] SET ONLINE", para).Count();

            if (count >= 0)
            {
                Users.ClearCacheAll();//清空缓存
                ViewBag.SuccessMsg = "数据库恢复成功！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "数据库恢复失败！请查看系统日志。";
            return View("Error");
        }

        public ActionResult DBClear()
        {
            ActMessage = "清空数据库";
            ////清空前先备份
            //IOHelper.CreateDirectory(Server.MapPath("/DBBackUp/"));
            SqlParameter[] para_sys = new SqlParameter[]
            {
            new SqlParameter ("@BKPATH", Server.MapPath("/DBBackUp/") + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak" ),
            };

            int count = MvcCore.Unity.Get<ISysDBTool>().Execute<object>("backup database " + ConfigHelper.GetConfigString("DBName") + " to disk=@BKPATH", para_sys).Count();

            DbParameter[] param = new DbParameter[] { };
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [AcceptHelp]", param);//接收帮助表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [AccountRelation]", param);//账户关系表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ActLog]", param);//管理员与会员行为日志表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [AdminAuthority]", param);//管理员权限表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [AdminRole]", param);//管理员角色日志表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [AdminUser]", param);//管理员账户表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Advertise]", param);//广告表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [AdvertiseChartDay]", param);//点对点交易日线
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [AdvertiseOrder]", param);//广告订单表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Article]", param);//文章表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ArticleClass]", param);//文章分类表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [BonusDetail]", param);//奖金明细表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Chating]", param);//发布广告聊天记录表
            // SysDBTool.ExecuteSQL("DELETE FROM  [Currency] ", param);//币种不清空
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [ExchangeCurrency]", param);//币种互兑规则表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ExchangeDetail]", param);//兑换详情表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Inverstment]", param);//投资记录表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [Language]", param);//多语言表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [LeaveWord]", param);//互助留言表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [LuckDrawSumLog]", param);//抽奖记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Matching]", param);//匹配表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Message]", param);//邮件记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Navigation]", param);//菜单导航表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Notice]", param);//公告表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [OtherTransfer]", param);//对外转账表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PINCode]", param);//激活码表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking]", param);//价格跟踪表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking15Min]", param);//15分钟K线记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking1Day]", param);//1日K线记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking1Min]", param);//1分钟记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking30Min]", param);//30分钟记录表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking300Min]", param);//300分钟记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking5Min]", param);//5分钟记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [PriceTracking60Min]", param);//60分钟记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [RaCoin]", param);//虚拟币转入转出记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Release]", param);//每日释放记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ReleaseDetail]", param);//释放详情表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Remittance]", param);//现金币充值表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Settlement]", param);//分红记录表  

            SysDBTool.ExecuteSQL("TRUNCATE TABLE [MachineOrder]", param);//分红记录表  

            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Cart]", param);//购物车表 
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Comments]", param);//商品评论表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Favorites]", param);//商品收藏表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Floor]", param);//首页商品表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Home_Group]", param);//首页显示分组表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Info]", param);//店铺表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_News]", param);//新闻表 
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Order]", param);//订单表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Order_Details]", param);//订单详情表 
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Product]", param);//商品表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Product_Category]", param);//商品分列表 
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Product_SKU]", param);//商品库存表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_ReceiptAddress]", param);//收货地址表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Shop_Tmp_Pro_Img]", param);//商品临时图片表

            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopInfo]", param);//店铺信息表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopOrder]", param);//店铺订单表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopOrderDetail]", param);//店铺订单详情
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [ShopProduct]", param);//商品表

            SysDBTool.ExecuteSQL("TRUNCATE TABLE [SMSLog]", param);//短信日志表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [StockChartDay]", param);//每日价格记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [StockChartHour]", param);//小时记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [StockEntrustsTrade]", param);//委托记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [StockTrade]", param);//成交记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [SupplyHelp]", param);//提供帮助表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [SysLog]", param);//系统日志表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [SysParam]", param);//参数表
            //SysDBTool.ExecuteSQL("TRUNCATE TABLE [SysSetting]", param);//系统设置表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [TakeCash]", param);//提现表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [TimeDeposit]", param);//定期存款表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [Transfer]", param);//转账表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [User]", param);//会员表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [UserBankCard]", param);//会员银行卡信息表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [UserVerify]", param);//用户页面密码级别表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [WalletLog]", param);//账户流水记录表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [WarningLog]", param);//预警日志表
            SysDBTool.ExecuteSQL("TRUNCATE TABLE [TeamAchievement]", param);//团队业绩表
            //清空币种表累计
            string currencySql = "update [Currency] set [TotalSelling]=0,[TotalPoundage]=0;";
            SysDBTool.ExecuteSQL(currencySql, param);

            Users.ClearCacheAll();//清空缓存

            ViewBag.SuccessMsg = "除管理员表及系统参数设置外全部数据已全部清空！清空数据后用户平台必须重新登录。";

            return View("Success");
        }
    }
}
