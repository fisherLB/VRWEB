 







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
        public DbSet<AdvertiseOrder> AdvertiseOrder { get; set; }
    }

	/// <summary>
    /// 广告匹配订单表
    /// </summary>
	[DisplayName("广告匹配订单表")]
    public partial class AdvertiseOrder
    {

		
        

        /// <summary>
        /// 订单ID
        /// </summary>  
		
		[DisplayName("订单ID")]
		


		[Key]
		public int  ID { get; set; }
		      
       
        

        /// <summary>
        /// 买入会员
        /// </summary>  
		
		[DisplayName("买入会员")]
		


		public int  BuyUID { get; set; }
		      
       
        

        /// <summary>
        /// 买入会员编号
        /// </summary>  
		
		[DisplayName("买入会员编号")]
		

        [MaxLength(50,ErrorMessage="买入会员编号最大长度为50")]


		public string  BuyUserName { get; set; }
		      
       
        

        /// <summary>
        /// 卖入会员
        /// </summary>  
		
		[DisplayName("卖入会员")]
		


		public int  SellUID { get; set; }
		      
       
        

        /// <summary>
        /// 卖入会员编号
        /// </summary>  
		
		[DisplayName("卖入会员编号")]
		

        [MaxLength(50,ErrorMessage="卖入会员编号最大长度为50")]


		public string  SellUserName { get; set; }
		      
       
        

        /// <summary>
        /// 订单ID
        /// </summary>  
		
		[DisplayName("订单ID")]
		


		public int  AdID { get; set; }
		      
       
        

        /// <summary>
        /// 订单号
        /// </summary>  
		
		[DisplayName("订单号")]
		

        [MaxLength(50,ErrorMessage="订单号最大长度为50")]


		public string  AdOderNO { get; set; }
		      
       
        

        /// <summary>
        /// 币种ID
        /// </summary>  
		
		[DisplayName("币种ID")]
		


		public int  CurID { get; set; }
		      
       
        

        /// <summary>
        /// 币种名称
        /// </summary>  
		
		[DisplayName("币种名称")]
		

        [MaxLength(50,ErrorMessage="币种名称最大长度为50")]


		public string  CurName { get; set; }
		      
       
        

        /// <summary>
        /// 币种英文缩写
        /// </summary>  
		
		[DisplayName("币种英文缩写")]
		

        [MaxLength(50,ErrorMessage="币种英文缩写最大长度为50")]


		public string  CurEnSigns { get; set; }
		      
       
        

        /// <summary>
        /// 订单号
        /// </summary>  
		
		[DisplayName("订单号")]
		

        [MaxLength(50,ErrorMessage="订单号最大长度为50")]


		public string  OrderID { get; set; }
		      
       
        

        /// <summary>
        /// 类型（0出售，1购买）
        /// </summary>  
		
		[DisplayName("类型（0出售，1购买）")]
		


		public int  Direction { get; set; }
		      
       
        

        /// <summary>
        /// 购买数量
        /// </summary>  
		
		[DisplayName("购买数量")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  Quantity { get; set; }
		      
       
        

        /// <summary>
        /// 购买金额
        /// </summary>  
		
		[DisplayName("购买金额")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  Amount { get; set; }
		      
       
        

        /// <summary>
        /// 付款币种ID（人民币）
        /// </summary>  
		
		[DisplayName("付款币种ID（人民币）")]
		


		public int  CoinID { get; set; }
		      
       
        

        /// <summary>
        /// 付款币种
        /// </summary>  
		
		[DisplayName("付款币种")]
		

        [MaxLength(50,ErrorMessage="付款币种最大长度为50")]


		public string  CoinName { get; set; }
		      
       
        

        /// <summary>
        /// 价格
        /// </summary>  
		
		[DisplayName("价格")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  Price { get; set; }
		      
       
        

        /// <summary>
        /// 买入手续费
        /// </summary>  
		
		[DisplayName("买入手续费")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  BuyPoundage { get; set; }
		      
       
        

        /// <summary>
        /// 卖出手续费
        /// </summary>  
		
		[DisplayName("卖出手续费")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  SellPoundage { get; set; }
		      
       
        

        /// <summary>
        /// 状态
        /// </summary>  
		
		[DisplayName("状态")]
		


		public int  Status { get; set; }
		      
       
        

        /// <summary>
        /// 买家是否参与评价
        /// </summary>  
		
		[DisplayName("买家是否参与评价")]
		


		public bool?  IsBuyAppraise { get; set; }
		      
       
        

        /// <summary>
        /// 卖家是否参与评价
        /// </summary>  
		
		[DisplayName("卖家是否参与评价")]
		


		public bool?  IsSellAppraise { get; set; }
		      
       
        

        /// <summary>
        /// 备注（取消原因）
        /// </summary>  
		
		[DisplayName("备注（取消原因）")]
		

        [MaxLength(100,ErrorMessage="备注（取消原因）最大长度为100")]


		public string  Remark { get; set; }
		      
       
        

        /// <summary>
        /// 订单到期时间
        /// </summary>  
		
		[DisplayName("订单到期时间")]
		


		public DateTime  PaymentTime { get; set; }
		      
       
        

        /// <summary>
        /// 确认收货时间
        /// </summary>  
		
		[DisplayName("确认收货时间")]
		


		public DateTime?  DeliveryTime { get; set; }
		      
       
        

        /// <summary>
        /// 创建时间
        /// </summary>  
		
		[DisplayName("创建时间")]
		


		public DateTime  CreateTime { get; set; }
		      
       
        

        /// <summary>
        /// 付款凭证
        /// </summary>  
		
		[DisplayName("付款凭证")]
		

        [MaxLength(250,ErrorMessage="付款凭证最大长度为250")]


		public string  Attachment { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ConfirmPayTime")]
		


		public DateTime?  ConfirmPayTime { get; set; }
		      
       
        

        /// <summary>
        /// 支付ID
        /// </summary>  
		
		[DisplayName("支付ID")]
		


		public int?  PayID { get; set; }
		      
       
        

        /// <summary>
        /// 支付方式
        /// </summary>  
		
		[DisplayName("支付方式")]
		

        [MaxLength(100,ErrorMessage="支付方式最大长度为100")]


		public string  PayMethod { get; set; }
		      
       
        

        /// <summary>
        /// 支付账户
        /// </summary>  
		
		[DisplayName("支付账户")]
		

        [MaxLength(100,ErrorMessage="支付账户最大长度为100")]


		public string  PayAccount { get; set; }
		      
       
        

        /// <summary>
        /// 支付账户图片
        /// </summary>  
		
		[DisplayName("支付账户图片")]
		

        [MaxLength(250,ErrorMessage="支付账户图片最大长度为250")]


		public string  PayAccountImgUrl { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public AdvertiseOrder()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 广告匹配订单表业务接口
    /// </summary>
    public interface IAdvertiseOrderService :IServiceBase<AdvertiseOrder> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(AdvertiseOrder entity);
	}
    /// <summary>
    /// 广告匹配订单表业务类
    /// </summary>
    public class AdvertiseOrderService :  ServiceBase<AdvertiseOrder>,IAdvertiseOrderService
    {


        public AdvertiseOrderService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(AdvertiseOrder entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
