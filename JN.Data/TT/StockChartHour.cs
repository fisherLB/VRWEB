 


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
        public DbSet<StockChartHour> StockChartHour { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class StockChartHour
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OpenPrice")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  OpenPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ClosePrice")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  ClosePrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MaxPrice")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  MaxPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MinPrice")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  MinPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TotalTrade")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  TotalTrade { get; set; }
		      
       
        
        /// <summary>
        /// 涨跌幅
        /// </summary>  
				[DisplayName("涨跌幅")]
				public double  UpsAndDowns { get; set; }
		      
       
        
        /// <summary>
        /// 振幅
        /// </summary>  
				[DisplayName("振幅")]
				public double  Amplitude { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("StockHour")]
				public DateTime  StockHour { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("StockDate")]
				public DateTime?  StockDate { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public StockChartHour()
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
    public interface IStockChartHourService :IServiceBase<StockChartHour> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(StockChartHour entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class StockChartHourService :  ServiceBase<StockChartHour>,IStockChartHourService
    {


        public StockChartHourService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(StockChartHour entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
