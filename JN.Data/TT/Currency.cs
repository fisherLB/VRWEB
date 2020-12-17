 







using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MvcCore;
using System.Data.Entity;
using System.ComponentModel;
using MvcCore.Infrastructure;
using System.Data.Entity.Validation;
namespace JN.Data
{
    public partial class SysDbContext : FrameworkContext
    {
        /// <summary>
        /// 把实体添加到EF上下文
        /// </summary>
        public DbSet<Currency> Currency { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Currency
    {

		
        

        /// <summary>
        /// 表id
        /// </summary>  
		
		[DisplayName("表id")]
		


		[Key]
		public int  ID { get; set; }
		      
       
        

        /// <summary>
        /// 币种名称
        /// </summary>  
		
		[DisplayName("币种名称")]
		

        [MaxLength(50,ErrorMessage="币种名称最大长度为50")]


		public string  CurrencyName { get; set; }
		      
       
        

        /// <summary>
        /// 币种logo
        /// </summary>  
		
		[DisplayName("币种logo")]
		

        [MaxLength(200,ErrorMessage="币种logo最大长度为200")]


		public string  CurrencyLogo { get; set; }
		      
       
        

        /// <summary>
        /// 币种英文标识
        /// </summary>  
		
		[DisplayName("币种英文标识")]
		

        [MaxLength(50,ErrorMessage="币种英文标识最大长度为50")]


		public string  EnSigns { get; set; }
		      
       
        

        /// <summary>
        /// 全拼英文
        /// </summary>  
		
		[DisplayName("全拼英文")]
		

        [MaxLength(50,ErrorMessage="全拼英文最大长度为50")]


		public string  English { get; set; }
		      
       
        

        /// <summary>
        /// 币种排序
        /// </summary>  
		
		[DisplayName("币种排序")]
		


		public int?  Sort { get; set; }
		      
       
        

        /// <summary>
        /// 发行量
        /// </summary>  
		
		[DisplayName("发行量")]
		


		public int  TotalIssued { get; set; }
		      
       
        

        /// <summary>
        /// 原始价格
        /// </summary>  
		
		[DisplayName("原始价格")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  OriginalPrice { get; set; }
		      
       
        

        /// <summary>
        /// 交易价格
        /// </summary>  
		
		[DisplayName("交易价格")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  TranPrice { get; set; }
		      
       
        

        /// <summary>
        /// 提币手续费
        /// </summary>  
		
		[DisplayName("提币手续费")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  TbPoundage { get; set; }
		      
       
        

        /// <summary>
        /// 提币最大额度（单笔）
        /// </summary>  
		
		[DisplayName("提币最大额度（单笔）")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  TbMaxLimit { get; set; }
		      
       
        

        /// <summary>
        /// 提币最小额度（单笔）
        /// </summary>  
		
		[DisplayName("提币最小额度（单笔）")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  TbMinLimit { get; set; }
		      
       
        

        /// <summary>
        /// 卖出手续费
        /// </summary>  
		
		[DisplayName("卖出手续费")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  SellPoundage { get; set; }
		      
       
        

        /// <summary>
        /// 买入手续费
        /// </summary>  
		
		[DisplayName("买入手续费")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  BuyPoundage { get; set; }
		      
       
        

        /// <summary>
        /// 链接地址
        /// </summary>  
		
		[DisplayName("链接地址")]
		

        [MaxLength(100,ErrorMessage="链接地址最大长度为100")]


		public string  LinkAddress { get; set; }
		      
       
        

        /// <summary>
        /// 用户名
        /// </summary>  
		
		[DisplayName("用户名")]
		

        [MaxLength(50,ErrorMessage="用户名最大长度为50")]


		public string  UserName { get; set; }
		      
       
        

        /// <summary>
        /// 密码
        /// </summary>  
		
		[DisplayName("密码")]
		

        [MaxLength(50,ErrorMessage="密码最大长度为50")]


		public string  PassWord { get; set; }
		      
       
        

        /// <summary>
        /// 地址
        /// </summary>  
		
		[DisplayName("地址")]
		

        [MaxLength(100,ErrorMessage="地址最大长度为100")]


		public string  Address { get; set; }
		      
       
        

        /// <summary>
        /// 端口号
        /// </summary>  
		
		[DisplayName("端口号")]
		


		public int?  PortNumber { get; set; }
		      
       
        

        /// <summary>
        /// 钱包地址
        /// </summary>  
		
		[DisplayName("钱包地址")]
		

        [MaxLength(200,ErrorMessage="钱包地址最大长度为200")]


		public string  WalletKey { get; set; }
		      
       
        

        /// <summary>
        /// 是否ICO平台(默认是交易所平台)
        /// </summary>  
		
		[DisplayName("是否ICO平台(默认是交易所平台)")]
		


		public bool  IsICO { get; set; }
		      
       
        

        /// <summary>
        /// 上线开关
        /// </summary>  
		
		[DisplayName("上线开关")]
		


		public bool  LineSwitch { get; set; }
		      
       
        

        /// <summary>
        /// 交易开关
        /// </summary>  
		
		[DisplayName("交易开关")]
		


		public bool  TranSwitch { get; set; }
		      
       
        

        /// <summary>
        /// 买入币种
        /// </summary>  
		
		[DisplayName("买入币种")]
		


		public int  BuyCurrency { get; set; }
		      
       
        

        /// <summary>
        /// 币种介绍链接
        /// </summary>  
		
		[DisplayName("币种介绍链接")]
		

        [MaxLength(100,ErrorMessage="币种介绍链接最大长度为100")]


		public string  CurrencyDetailsLink { get; set; }
		      
       
        

        /// <summary>
        /// 限制位数
        /// </summary>  
		
		[DisplayName("限制位数")]
		


		public int?  LimitDigit { get; set; }
		      
       
        

        /// <summary>
        /// 币种钱包ID号
        /// </summary>  
		
		[DisplayName("币种钱包ID号")]
		


		public int?  WalletCurID { get; set; }
		      
       
        

        /// <summary>
        /// 每天最高涨幅限制
        /// </summary>  
		
		[DisplayName("每天最高涨幅限制")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  Increase { get; set; }
		      
       
        

        /// <summary>
        /// 是否注册奖励
        /// </summary>  
		
		[DisplayName("是否注册奖励")]
		


		public bool?  IsRegReward { get; set; }
		      
       
        

        /// <summary>
        /// 奖励数量
        /// </summary>  
		
		[DisplayName("奖励数量")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  RegRewardParam { get; set; }
		      
       
        

        /// <summary>
        /// 是否加入邀请奖励
        /// </summary>  
		
		[DisplayName("是否加入邀请奖励")]
		


		public bool?  IsInvitaReward { get; set; }
		      
       
        

        /// <summary>
        /// 邀请奖励比例
        /// </summary>  
		
		[DisplayName("邀请奖励比例")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  InvitaRewardParam { get; set; }
		      
       
        

        /// <summary>
        /// 是否进入冻结
        /// </summary>  
		
		[DisplayName("是否进入冻结")]
		


		public bool?  IsFreeze { get; set; }
		      
       
        

        /// <summary>
        /// 进入冻结比例
        /// </summary>  
		
		[DisplayName("进入冻结比例")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  FreezeParam { get; set; }
		      
       
        

        /// <summary>
        /// 每次释放额度
        /// </summary>  
		
		[DisplayName("每次释放额度")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  ReleaseAmount { get; set; }
		      
       
        

        /// <summary>
        /// 每天开始时间
        /// </summary>  
		
		[DisplayName("每天开始时间")]
		

        [MaxLength(100,ErrorMessage="每天开始时间最大长度为100")]


		public string  StartTime { get; set; }
		      
       
        

        /// <summary>
        /// 每天停止时间
        /// </summary>  
		
		[DisplayName("每天停止时间")]
		

        [MaxLength(100,ErrorMessage="每天停止时间最大长度为100")]


		public string  StopTime { get; set; }
		      
       
        

        /// <summary>
        /// 创建时间
        /// </summary>  
		
		[DisplayName("创建时间")]
		


		public DateTime  CreateTime { get; set; }
		      
       
        

        /// <summary>
        /// 是否加入最低购买限制
        /// </summary>  
		
		[DisplayName("是否加入最低购买限制")]
		


		public bool?  IsLimit { get; set; }
		      
       
        

        /// <summary>
        /// 最低购买限制
        /// </summary>  
		
		[DisplayName("最低购买限制")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  MinLimit { get; set; }
		      
       
        

        /// <summary>
        /// 是否以太坊网络
        /// </summary>  
		
		[DisplayName("是否以太坊网络")]
		


		public bool?  IsEthereum { get; set; }
		      
       
        

        /// <summary>
        /// 是否比特币
        /// </summary>  
		
		[DisplayName("是否比特币")]
		


		public bool?  IsBitcoin { get; set; }
		      
       
        

        /// <summary>
        /// 以太坊及以太坊衍生出的钱包地址（用于充值收款）
        /// </summary>  
		
		[DisplayName("以太坊及以太坊衍生出的钱包地址（用于充值收款）")]
		

        [MaxLength(250,ErrorMessage="以太坊及以太坊衍生出的钱包地址（用于充值收款）最大长度为250")]


		public string  EthereumAddress { get; set; }
		      
       
        

        /// <summary>
        /// 是否自动充值/提现
        /// </summary>  
		
		[DisplayName("是否自动充值/提现")]
		


		public bool?  IsAutomatic { get; set; }
		      
       
        

        /// <summary>
        /// 卖出总量
        /// </summary>  
		
		[DisplayName("卖出总量")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  TotalSelling { get; set; }
		      
       
        

        /// <summary>
        /// 平台手续费
        /// </summary>  
		
		[DisplayName("平台手续费")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  TotalPoundage { get; set; }
		      
       
        

        /// <summary>
        /// 获取价格类型（默认为0获取第三方，1为自己定义的）
        /// </summary>  
		
		[DisplayName("获取价格类型（默认为0获取第三方，1为自己定义的）")]
		


		public int?  GetPriceType { get; set; }
		      
       
        

        /// <summary>
        /// 是否开启充值
        /// </summary>  
		
		[DisplayName("是否开启充值")]
		


		public bool  IsRecharge { get; set; }
		      
       
        

        /// <summary>
        /// 是否开启提现
        /// </summary>  
		
		[DisplayName("是否开启提现")]
		


		public bool  IsCashTransfer { get; set; }
		      
       
        

        /// <summary>
        /// 是否现金币
        /// </summary>  
		
		[DisplayName("是否现金币")]
		


		public bool?  IsCashCurrency { get; set; }
		      
       
        

        /// <summary>
        /// 交易总量
        /// </summary>  
		
		[DisplayName("交易总量")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  TranTotal { get; set; }
		      
       
        

        /// <summary>
        /// 涨幅价格
        /// </summary>  
		
		[DisplayName("涨幅价格")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  IncreasePrice { get; set; }
		      
       
        

        /// <summary>
        /// 涨幅条件
        /// </summary>  
		
		[DisplayName("涨幅条件")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  IncreaseConditions { get; set; }
		      
       
        

        /// <summary>
        /// 涨幅次数
        /// </summary>  
		
		[DisplayName("涨幅次数")]
		


		public int?  IncreaseTimes { get; set; }



        /// <summary>
        /// API网络类型
        /// </summary>  
        [DisplayName("API网络类型")]
        public int NetworkType { get; set; }



        /// <summary>
        /// 账号（可选）部分API网络需要配置
        /// </summary>  
        [DisplayName("账号（可选）部分API网络需要配置")]
        [MaxLength(250, ErrorMessage = "账号（可选）部分API网络需要配置最大长度为250")]
        public string AccountAddress { get; set; }



        [DisplayName("私钥")]
        [MaxLength(250, ErrorMessage = "最大长度为250")]
        public string Privatekey { get; set; }




        /// <summary>
        /// 构造函数
        /// </summary>

        public Currency()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface ICurrencyService :IServiceBase<Currency> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Currency entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class CurrencyService :  ServiceBase<Currency>,ICurrencyService
    {


        public CurrencyService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Currency entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
