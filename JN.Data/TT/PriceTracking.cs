 


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
        public DbSet<PriceTracking> PriceTracking { get; set; }
    }

	/// <summary>
    /// 价格跟踪表
    /// </summary>
	[DisplayName("价格跟踪表")]
    public partial class PriceTracking
    {

		
        
        /// <summary>
        /// 主键
        /// </summary>  
				[DisplayName("主键")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime?  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 创建人
        /// </summary>  
				[DisplayName("创建人")]
				public int?  CreateUserID { get; set; }
		      
       
        
        /// <summary>
        /// 已删除
        /// </summary>  
				[DisplayName("已删除")]
				public bool?  IsDelete { get; set; }
		      
       
        
        /// <summary>
        /// 删除时间
        /// </summary>  
				[DisplayName("删除时间")]
				public DateTime?  DeleteTime { get; set; }
		      
       
        
        /// <summary>
        /// 删除者
        /// </summary>  
				[DisplayName("删除者")]
				public int?  DeleteUserID { get; set; }
		      
       
        
        /// <summary>
        /// 当前价
        /// </summary>  
				[DisplayName("当前价")]
				public double?  CurrPrice { get; set; }
		      
       
        
        /// <summary>
        /// 最高价
        /// </summary>  
				[DisplayName("最高价")]
				public double?  MaxPrice { get; set; }
		      
       
        
        /// <summary>
        /// 最低价
        /// </summary>  
				[DisplayName("最低价")]
				public double?  MinPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OpenPrice")]
				public double?  OpenPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ClosePrice")]
				public double?  ClosePrice { get; set; }
		      
       
        
        /// <summary>
        /// 时间
        /// </summary>  
				[DisplayName("时间")]
				public DateTime?  Time { get; set; }
		      
       
        
        /// <summary>
        /// 源字符串（从新浪获取到的字符串）
        /// </summary>  
				[DisplayName("源字符串（从新浪获取到的字符串）")]
		        [MaxLength(4000,ErrorMessage="源字符串（从新浪获取到的字符串）最大长度为4000")]
		public string  SourceStr { get; set; }
		      
       
        
        /// <summary>
        /// 产品ID
        /// </summary>  
				[DisplayName("产品ID")]
				public int?  ProductID { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public PriceTracking()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 价格跟踪表业务接口
    /// </summary>
    public interface IPriceTrackingService :IServiceBase<PriceTracking> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(PriceTracking entity);
	}
    /// <summary>
    /// 价格跟踪表业务类
    /// </summary>
    public class PriceTrackingService :  ServiceBase<PriceTracking>,IPriceTrackingService
    {


        public PriceTrackingService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(PriceTracking entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 

