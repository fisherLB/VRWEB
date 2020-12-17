 


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
        public DbSet<Shop_Order_Details> Shop_Order_Details { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Order_Details
    {

		
        
        /// <summary>
        /// 订单详情表ID
        /// </summary>  
				[DisplayName("订单详情表ID")]
				[Key]
		public long  DetailsId { get; set; }
		      
       
        
        /// <summary>
        /// 订单号
        /// </summary>  
				[DisplayName("订单号")]
		        [MaxLength(255,ErrorMessage="订单号最大长度为255")]
		public string  OrderNumber { get; set; }
		      
       
        
        /// <summary>
        /// 商品ID
        /// </summary>  
				[DisplayName("商品ID")]
				public long  GoodsId { get; set; }
		      
       
        
        /// <summary>
        /// 商品名称
        /// </summary>  
				[DisplayName("商品名称")]
		        [MaxLength(64,ErrorMessage="商品名称最大长度为64")]
		public string  GoodsName { get; set; }
		      
       
        
        /// <summary>
        /// 购买数量
        /// </summary>  
				[DisplayName("购买数量")]
				public int  ByCount { get; set; }
		      
       
        
        /// <summary>
        /// 单价
        /// </summary>  
				[DisplayName("单价")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  OneFee { get; set; }
		      
       
        
        /// <summary>
        /// 总价
        /// </summary>  
				[DisplayName("总价")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TotalFee { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Remark")]
				public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 图片路径
        /// </summary>  
				[DisplayName("图片路径")]
				public string  Img { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SkuId")]
				public int?  SkuId { get; set; }
		      
       
        
        /// <summary>
        /// 购买用户ID
        /// </summary>  
				[DisplayName("购买用户ID")]
				public int  UserId { get; set; }
		      
       
        
        /// <summary>
        /// 店铺ID
        /// </summary>  
				[DisplayName("店铺ID")]
				public long?  ShopId { get; set; }
		      
       
        
        /// <summary>
        /// 店铺名称
        /// </summary>  
				[DisplayName("店铺名称")]
		        [MaxLength(32,ErrorMessage="店铺名称最大长度为32")]
		public string  ShopName { get; set; }
		      
       
        
        /// <summary>
        /// 1：已付款；2以发货；3已收货；
        /// </summary>  
				[DisplayName("1：已付款；2以发货；3已收货；")]
				public int?  Status { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime?  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 发货时间
        /// </summary>  
				[DisplayName("发货时间")]
				public DateTime?  DeliverTime { get; set; }
		      
       
        
        /// <summary>
        /// 收货时间
        /// </summary>  
				[DisplayName("收货时间")]
				public DateTime?  DeliveryTime { get; set; }
		      
       
        
        /// <summary>
        /// 是否已经评论
        /// </summary>  
				[DisplayName("是否已经评论")]
				public bool  IsComment { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Order_Details()
        {
        //    DetailsId = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IShop_Order_DetailsService :IServiceBase<Shop_Order_Details> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Order_Details entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_Order_DetailsService :  ServiceBase<Shop_Order_Details>,IShop_Order_DetailsService
    {


        public Shop_Order_DetailsService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Order_Details entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
