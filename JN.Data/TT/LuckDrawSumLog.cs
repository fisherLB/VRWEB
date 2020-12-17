 


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
        public DbSet<LuckDrawSumLog> LuckDrawSumLog { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class LuckDrawSumLog
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CoinID")]
				public int?  CoinID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CoinName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  CoinName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ChangeMoney")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  ChangeMoney { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Balance")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Balance { get; set; }
		      
       
        
        /// <summary>
        /// 描述
        /// </summary>  
				[DisplayName("描述")]
		        [MaxLength(250,ErrorMessage="描述最大长度为250")]
		public string  Description { get; set; }
		      
       
        
        /// <summary>
        /// 变更时间
        /// </summary>  
				[DisplayName("变更时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public LuckDrawSumLog()
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
    public interface ILuckDrawSumLogService :IServiceBase<LuckDrawSumLog> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(LuckDrawSumLog entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class LuckDrawSumLogService :  ServiceBase<LuckDrawSumLog>,ILuckDrawSumLogService
    {


        public LuckDrawSumLogService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(LuckDrawSumLog entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
