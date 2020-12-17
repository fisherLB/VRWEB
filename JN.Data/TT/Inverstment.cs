 


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
        public DbSet<Inverstment> Inverstment { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Inverstment
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID号
        /// </summary>  
				[DisplayName("用户ID号")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名称
        /// </summary>  
				[DisplayName("用户名称")]
		        [MaxLength(50,ErrorMessage="用户名称最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 投资金额
        /// </summary>  
				[DisplayName("投资金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  InvestmentMoney { get; set; }
		      
       
        
        /// <summary>
        /// 投资模式（1为周结，2为月结）
        /// </summary>  
				[DisplayName("投资模式（1为周结，2为月结）")]
				public int  InvestmentMode { get; set; }
		      
       
        
        /// <summary>
        /// 投资时的积分价格
        /// </summary>  
				[DisplayName("投资时的积分价格")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Price { get; set; }
		      
       
        
        /// <summary>
        /// 状态(1为正在分,2为停止)
        /// </summary>  
				[DisplayName("状态(1为正在分,2为停止)")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 分红时间
        /// </summary>  
				[DisplayName("分红时间")]
				public DateTime?  SettlementTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Inverstment()
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
    public interface IInverstmentService :IServiceBase<Inverstment> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Inverstment entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class InverstmentService :  ServiceBase<Inverstment>,IInverstmentService
    {


        public InverstmentService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Inverstment entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
