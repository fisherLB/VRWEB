 


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
        public DbSet<ShopOrder> ShopOrder { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class ShopOrder
    {

		
        
        /// <summary>
        /// 订单主表ID
        /// </summary>  
				[DisplayName("订单主表ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 买家ID
        /// </summary>  
				[DisplayName("买家ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 买家登录名
        /// </summary>  
				[DisplayName("买家登录名")]
		        [MaxLength(50,ErrorMessage="买家登录名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ShopID")]
				public int  ShopID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ShopName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ShopName { get; set; }
		      
       
        
        /// <summary>
        /// 订单编号
        /// </summary>  
				[DisplayName("订单编号")]
		        [MaxLength(50,ErrorMessage="订单编号最大长度为50")]
		public string  OrderNumber { get; set; }
		      
       
        
        /// <summary>
        /// 省
        /// </summary>  
				[DisplayName("省")]
		        [MaxLength(50,ErrorMessage="省最大长度为50")]
		public string  Province { get; set; }
		      
       
        
        /// <summary>
        /// 市
        /// </summary>  
				[DisplayName("市")]
		        [MaxLength(50,ErrorMessage="市最大长度为50")]
		public string  City { get; set; }
		      
       
        
        /// <summary>
        /// 县
        /// </summary>  
				[DisplayName("县")]
		        [MaxLength(50,ErrorMessage="县最大长度为50")]
		public string  Town { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecAddress")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecAddress { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecLinkMan")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecLinkMan { get; set; }
		      
       
        
        /// <summary>
        /// 收货电话
        /// </summary>  
				[DisplayName("收货电话")]
		        [MaxLength(50,ErrorMessage="收货电话最大长度为50")]
		public string  RecPhone { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecZip")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecZip { get; set; }
		      
       
        
        /// <summary>
        /// 支付时间
        /// </summary>  
				[DisplayName("支付时间")]
				public DateTime?  PayTime { get; set; }
		      
       
        
        /// <summary>
        /// 交易完成时间
        /// </summary>  
				[DisplayName("交易完成时间")]
				public DateTime?  FinishTime { get; set; }
		      
       
        
        /// <summary>
        /// 发货时间
        /// </summary>  
				[DisplayName("发货时间")]
				public DateTime?  DelivertTime { get; set; }
		      
       
        
        /// <summary>
        /// 订单总数
        /// </summary>  
				[DisplayName("订单总数")]
				public int  TotalCount { get; set; }
		      
       
        
        /// <summary>
        /// 正常金额
        /// </summary>  
				[DisplayName("正常金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TotalPrice { get; set; }
		      
       
        
        /// <summary>
        /// 买家留言
        /// </summary>  
				[DisplayName("买家留言")]
		        [MaxLength(250,ErrorMessage="买家留言最大长度为250")]
		public string  BuyMsg { get; set; }
		      
       
        
        /// <summary>
        /// 备注
        /// </summary>  
				[DisplayName("备注")]
				public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 订单状态，见枚举
        /// </summary>  
				[DisplayName("订单状态，见枚举")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 物流信息
        /// </summary>  
				[DisplayName("物流信息")]
		        [MaxLength(250,ErrorMessage="物流信息最大长度为250")]
		public string  Logistics { get; set; }
		      
       
        
        /// <summary>
        /// 运费
        /// </summary>  
				[DisplayName("运费")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ShipFreight { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr1")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr2")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr3")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveInt1")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveInt2")]
				public int?  ReserveInt2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDate")]
				public DateTime?  ReserveDate { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ShopOrder()
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
    public interface IShopOrderService :IServiceBase<ShopOrder> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ShopOrder entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class ShopOrderService :  ServiceBase<ShopOrder>,IShopOrderService
    {


        public ShopOrderService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ShopOrder entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
