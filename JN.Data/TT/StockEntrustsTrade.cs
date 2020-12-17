 


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
        public DbSet<StockEntrustsTrade> StockEntrustsTrade { get; set; }
    }

	/// <summary>
    /// 委托交易
    /// </summary>
	[DisplayName("委托交易")]
    public partial class StockEntrustsTrade
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("EntrustsNo")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  EntrustsNo { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名称
        /// </summary>  
				[DisplayName("用户名称")]
		        [MaxLength(50,ErrorMessage="用户名称最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 币种ＩＤ号
        /// </summary>  
				[DisplayName("币种ＩＤ号")]
				public int  CurID { get; set; }
		      
       
        
        /// <summary>
        /// 交易方向(0买入,1卖出)
        /// </summary>  
				[DisplayName("交易方向(0买入,1卖出)")]
				public int  Direction { get; set; }
		      
       
        
        /// <summary>
        /// 委托数量
        /// </summary>  
				[DisplayName("委托数量")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Quantity { get; set; }
		      
       
        
        /// <summary>
        /// 挂单单价
        /// </summary>  
				[DisplayName("挂单单价")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Price { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TotalAmount")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  TotalAmount { get; set; }
		      
       
        
        /// <summary>
        /// 已成交数量
        /// </summary>  
				[DisplayName("已成交数量")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  HaveTurnover { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Poundage { get; set; }
		      
       
        
        /// <summary>
        /// 状态
        /// </summary>  
				[DisplayName("状态")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 撤消时间
        /// </summary>  
				[DisplayName("撤消时间")]
				public DateTime?  CancelTime { get; set; }
		      
       
        
        /// <summary>
        /// 成交时间
        /// </summary>  
				[DisplayName("成交时间")]
				public DateTime?  TurnoverTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public StockEntrustsTrade()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 委托交易业务接口
    /// </summary>
    public interface IStockEntrustsTradeService :IServiceBase<StockEntrustsTrade> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(StockEntrustsTrade entity);
	}
    /// <summary>
    /// 委托交易业务类
    /// </summary>
    public class StockEntrustsTradeService :  ServiceBase<StockEntrustsTrade>,IStockEntrustsTradeService
    {


        public StockEntrustsTradeService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(StockEntrustsTrade entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
