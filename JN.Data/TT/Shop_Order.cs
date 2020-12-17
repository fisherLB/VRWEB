 


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
        public DbSet<Shop_Order> Shop_Order { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Order
    {

		
        
        /// <summary>
        /// 订单表ID
        /// </summary>  
				[DisplayName("订单表ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 订单编号
        /// </summary>  
				[DisplayName("订单编号")]
		        [MaxLength(250,ErrorMessage="订单编号最大长度为250")]
		public string  OrderNumber { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("DealTime")]
				public DateTime?  DealTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("DealType")]
				public int?  DealType { get; set; }
		      
       
        
        /// <summary>
        /// 购买数量
        /// </summary>  
				[DisplayName("购买数量")]
				public int  TotalCount { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TempMoney")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TempMoney { get; set; }
		      
       
        
        /// <summary>
        /// 单价
        /// </summary>  
				[DisplayName("单价")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  RealMoney { get; set; }
		      
       
        
        /// <summary>
        /// 购买者ID
        /// </summary>  
				[DisplayName("购买者ID")]
				public long?  BuyerId { get; set; }
		      
       
        
        /// <summary>
        /// 购买者用户名
        /// </summary>  
				[DisplayName("购买者用户名")]
		        [MaxLength(250,ErrorMessage="购买者用户名最大长度为250")]
		public string  Buyer { get; set; }
		      
       
        
        /// <summary>
        /// 收件地址
        /// </summary>  
				[DisplayName("收件地址")]
		        [MaxLength(250,ErrorMessage="收件地址最大长度为250")]
		public string  Address { get; set; }
		      
       
        
        /// <summary>
        /// 收件电话
        /// </summary>  
				[DisplayName("收件电话")]
		        [MaxLength(50,ErrorMessage="收件电话最大长度为50")]
		public string  Phone { get; set; }
		      
       
        
        /// <summary>
        /// 购买钱包ID
        /// </summary>  
				[DisplayName("购买钱包ID")]
		        [MaxLength(250,ErrorMessage="购买钱包ID最大长度为250")]
		public string  BuyMsg { get; set; }
		      
       
        
        /// <summary>
        /// 记录取货中心地址
        /// </summary>  
				[DisplayName("记录取货中心地址")]
				public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 状态（已支付 =1 未付=0）
        /// </summary>  
				[DisplayName("状态（已支付 =1 未付=0）")]
				public int?  Status { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Coupons")]
				public string  Coupons { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SendTime")]
				public DateTime?  SendTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsSee")]
				public bool  IsSee { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldMoney")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldMoney { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldCash")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldCash { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldUse")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldUse { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldTemp")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldTemp { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldMax")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldMax { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldTax")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldTax { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldShare")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldShare { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldStock")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldStock { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldTradeShare")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldTradeShare { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldTradeinn")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldTradeinn { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("oldTradeinnLock")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  oldTradeinnLock { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldMoney")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldMoney { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldCash")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldCash { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldUse")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldUse { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldTemp")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldTemp { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldMax")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldMax { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldTax")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldTax { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldShare")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldShare { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldTradeShare")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldTradeShare { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldStock")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldStock { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldTradeinn")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldTradeinn { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("toldTradeinnLock")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  toldTradeinnLock { get; set; }
		      
       
        
        /// <summary>
        /// 商铺ID
        /// </summary>  
				[DisplayName("商铺ID")]
				public int  Sid { get; set; }
		      
       
        
        /// <summary>
        /// 商铺名称
        /// </summary>  
				[DisplayName("商铺名称")]
		        [MaxLength(250,ErrorMessage="商铺名称最大长度为250")]
		public string  ShopName { get; set; }
		      
       
        
        /// <summary>
        /// 收件人
        /// </summary>  
				[DisplayName("收件人")]
		        [MaxLength(50,ErrorMessage="收件人最大长度为50")]
		public string  Addressee { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Postcode")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Postcode { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ComplaintReason")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  ComplaintReason { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ComplaintTime")]
				public DateTime?  ComplaintTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CancelReason")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  CancelReason { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime?  CancelTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Order()
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
    public interface IShop_OrderService :IServiceBase<Shop_Order> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Order entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_OrderService :  ServiceBase<Shop_Order>,IShop_OrderService
    {


        public Shop_OrderService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Order entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
