 


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
        public DbSet<PriceTracking15Min> PriceTracking15Min { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class PriceTracking15Min
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
				[DisplayName("CreateTime")]
				public DateTime?  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateUserID")]
				public int?  CreateUserID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsDelete")]
				public bool?  IsDelete { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("DeleteTime")]
				public DateTime?  DeleteTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("DeleteUserID")]
				public int?  DeleteUserID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurrPrice")]
				public double?  CurrPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MaxPrice")]
				public double?  MaxPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MinPrice")]
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
        /// 
        /// </summary>  
				[DisplayName("Volume")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Volume { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Time")]
				public DateTime?  Time { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SourceStr")]
		        [MaxLength(4000,ErrorMessage="最大长度为4000")]
		public string  SourceStr { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ProductID")]
				public int?  ProductID { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public PriceTracking15Min()
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
    public interface IPriceTracking15MinService :IServiceBase<PriceTracking15Min> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(PriceTracking15Min entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class PriceTracking15MinService :  ServiceBase<PriceTracking15Min>,IPriceTracking15MinService
    {


        public PriceTracking15MinService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(PriceTracking15Min entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 

