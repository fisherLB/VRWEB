 


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
        public DbSet<Release> Release { get; set; }
    }

	/// <summary>
    /// 释放表
    /// </summary>
	[DisplayName("释放表")]
    public partial class Release
    {

		
        
        /// <summary>
        /// ID
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 期数
        /// </summary>  
				[DisplayName("期数")]
				public int  Period { get; set; }
		      
       
        
        /// <summary>
        /// 金额
        /// </summary>  
				[DisplayName("金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Bonus { get; set; }
		      
       
        
        /// <summary>
        /// 释放方式（0系统,1手工）
        /// </summary>  
				[DisplayName("释放方式（0系统,1手工）")]
				public int  BalanceMode { get; set; }
		      
       
        
        /// <summary>
        /// 释放会员数
        /// </summary>  
				[DisplayName("释放会员数")]
				public int  TotalUser { get; set; }
		      
       
        
        /// <summary>
        /// 释放总金额
        /// </summary>  
				[DisplayName("释放总金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  TotalBonus { get; set; }
		      
       
        
        /// <summary>
        /// 释放时间
        /// </summary>  
				[DisplayName("释放时间")]
				public DateTime  CreateTime { get; set; }



        /// <summary>
        /// 类型
        /// </summary>  
        [DisplayName("类型")]
        public int Type { get; set; }




        /// <summary>
        /// 构造函数
        /// </summary>

        public Release()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 释放表业务接口
    /// </summary>
    public interface IReleaseService :IServiceBase<Release> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Release entity);
	}
    /// <summary>
    /// 释放表业务类
    /// </summary>
    public class ReleaseService :  ServiceBase<Release>,IReleaseService
    {


        public ReleaseService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Release entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
