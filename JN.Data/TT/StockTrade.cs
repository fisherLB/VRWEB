 


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
        public DbSet<StockTrade> StockTrade { get; set; }
    }

	/// <summary>
    /// 交易记录表
    /// </summary>
	[DisplayName("交易记录表")]
    public partial class StockTrade
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 交易号
        /// </summary>  
				[DisplayName("交易号")]
		        [MaxLength(50,ErrorMessage="交易号最大长度为50")]
		public string  TradeNo { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Direction")]
				public int  Direction { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  BuyUID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurID")]
				public int  CurID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名称
        /// </summary>  
				[DisplayName("用户名称")]
		        [MaxLength(50,ErrorMessage="用户名称最大长度为50")]
		public string  BuyUserName { get; set; }
		      
       
        
        /// <summary>
        /// 委托买单编号
        /// </summary>  
				[DisplayName("委托买单编号")]
		        [MaxLength(50,ErrorMessage="委托买单编号最大长度为50")]
		public string  BuyEntrusNo { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,5)]
		public decimal  BuyPoundage { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  SellUID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名称
        /// </summary>  
				[DisplayName("用户名称")]
		        [MaxLength(50,ErrorMessage="用户名称最大长度为50")]
		public string  SellUserName { get; set; }
		      
       
        
        /// <summary>
        /// 委托卖单编号
        /// </summary>  
				[DisplayName("委托卖单编号")]
		        [MaxLength(50,ErrorMessage="委托卖单编号最大长度为50")]
		public string  SellEntrusNo { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,5)]
		public decimal  SellPoundage { get; set; }
		      
       
        
        /// <summary>
        /// 成交数量
        /// </summary>  
				[DisplayName("成交数量")]
		        [Filters.DecimalPrecision(18,5)]
		public decimal  Quantiry { get; set; }
		      
       
        
        /// <summary>
        /// 总金额
        /// </summary>  
				[DisplayName("总金额")]
		        [Filters.DecimalPrecision(18,5)]
		public decimal  TotaAmount { get; set; }
		      
       
        
        /// <summary>
        /// 单价
        /// </summary>  
				[DisplayName("单价")]
		        [Filters.DecimalPrecision(18,5)]
		public decimal  Price { get; set; }
		      
       
        
        /// <summary>
        /// 交易时间
        /// </summary>  
				[DisplayName("交易时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public StockTrade()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 交易记录表业务接口
    /// </summary>
    public interface IStockTradeService :IServiceBase<StockTrade> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(StockTrade entity);
	}
    /// <summary>
    /// 交易记录表业务类
    /// </summary>
    public class StockTradeService :  ServiceBase<StockTrade>,IStockTradeService
    {


        public StockTradeService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(StockTrade entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
