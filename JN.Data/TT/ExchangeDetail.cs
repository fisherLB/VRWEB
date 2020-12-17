 


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
        public DbSet<ExchangeDetail> ExchangeDetail { get; set; }
    }

	/// <summary>
    /// 币种兑换明细表
    /// </summary>
	[DisplayName("币种兑换明细表")]
    public partial class ExchangeDetail
    {

		
        
        /// <summary>
        /// 表ID
        /// </summary>  
				[DisplayName("表ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 兑换币种ID
        /// </summary>  
				[DisplayName("兑换币种ID")]
				public int  FromCoinID { get; set; }
		      
       
        
        /// <summary>
        /// 兑换币种名称
        /// </summary>  
				[DisplayName("兑换币种名称")]
		        [MaxLength(50,ErrorMessage="兑换币种名称最大长度为50")]
		public string  FromCoinName { get; set; }
		      
       
        
        /// <summary>
        /// 目标币种ID
        /// </summary>  
				[DisplayName("目标币种ID")]
				public int  ToCoinID { get; set; }
		      
       
        
        /// <summary>
        /// 目标币种名称
        /// </summary>  
				[DisplayName("目标币种名称")]
		        [MaxLength(50,ErrorMessage="目标币种名称最大长度为50")]
		public string  ToCoinName { get; set; }
		      
       
        
        /// <summary>
        /// 兑换比例
        /// </summary>  
				[DisplayName("兑换比例")]
		        [Filters.DecimalPrecision(18,6)]
		public decimal  Rate { get; set; }
		      
       
        
        /// <summary>
        /// 申请金额
        /// </summary>  
				[DisplayName("申请金额")]
		        [Filters.DecimalPrecision(18,6)]
		public decimal  ApplyMoney { get; set; }
		      
       
        
        /// <summary>
        /// 实际兑换金额
        /// </summary>  
				[DisplayName("实际兑换金额")]
		        [Filters.DecimalPrecision(18,6)]
		public decimal  ExchangeMoney { get; set; }
		      
       
        
        /// <summary>
        /// 申请时间
        /// </summary>  
				[DisplayName("申请时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
		        [MaxLength(50,ErrorMessage="预留字段最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public DateTime?  ReserveDate { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ExchangeDetail()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 币种兑换明细表业务接口
    /// </summary>
    public interface IExchangeDetailService :IServiceBase<ExchangeDetail> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ExchangeDetail entity);
	}
    /// <summary>
    /// 币种兑换明细表业务类
    /// </summary>
    public class ExchangeDetailService :  ServiceBase<ExchangeDetail>,IExchangeDetailService
    {


        public ExchangeDetailService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ExchangeDetail entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
