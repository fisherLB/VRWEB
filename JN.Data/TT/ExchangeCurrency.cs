 


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
        public DbSet<ExchangeCurrency> ExchangeCurrency { get; set; }
    }

	/// <summary>
    /// 币种兑换规则表
    /// </summary>
	[DisplayName("币种兑换规则表")]
    public partial class ExchangeCurrency
    {

		
        
        /// <summary>
        /// 表ID
        /// </summary>  
				[DisplayName("表ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 规则名称
        /// </summary>  
				[DisplayName("规则名称")]
		        [MaxLength(10,ErrorMessage="规则名称最大长度为10")]
		public string  ExchangeName { get; set; }
		      
       
        
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
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Rate { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MinNumber")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  MinNumber { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MaxNumber")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  MaxNumber { get; set; }
		      
       
        
        /// <summary>
        /// 是否使用汇率自动计数比例
        /// </summary>  
				[DisplayName("是否使用汇率自动计数比例")]
				public bool?  ISEchangeRate { get; set; }
		      
       
        
        /// <summary>
        /// 是否使用
        /// </summary>  
				[DisplayName("是否使用")]
				public bool?  IsUse { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 预留字符串字段1
        /// </summary>  
				[DisplayName("预留字符串字段1")]
		        [MaxLength(50,ErrorMessage="预留字符串字段1最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留数值型字段1
        /// </summary>  
				[DisplayName("预留数值型字段1")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留数时间字段
        /// </summary>  
				[DisplayName("预留数时间字段")]
				public DateTime?  ReserveDate { get; set; }
		      
       
        
        /// <summary>
        /// 预留计数字段
        /// </summary>  
				[DisplayName("预留计数字段")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  ReserveDecamal { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ExchangeCurrency()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 币种兑换规则表业务接口
    /// </summary>
    public interface IExchangeCurrencyService :IServiceBase<ExchangeCurrency> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ExchangeCurrency entity);
	}
    /// <summary>
    /// 币种兑换规则表业务类
    /// </summary>
    public class ExchangeCurrencyService :  ServiceBase<ExchangeCurrency>,IExchangeCurrencyService
    {


        public ExchangeCurrencyService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ExchangeCurrency entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
