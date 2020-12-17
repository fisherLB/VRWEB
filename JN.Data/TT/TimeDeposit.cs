 


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
        public DbSet<TimeDeposit> TimeDeposit { get; set; }
    }

	/// <summary>
    /// 定期存款表
    /// </summary>
	[DisplayName("定期存款表")]
    public partial class TimeDeposit
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
				[DisplayName("OrderNumber")]
		        [MaxLength(20,ErrorMessage="最大长度为20")]
		public string  OrderNumber { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 订单金额
        /// </summary>  
				[DisplayName("订单金额")]
		        [Filters.DecimalPrecision(18,3)]
		public decimal  DepositAmount { get; set; }
		      
       
        
        /// <summary>
        /// 1可分红,2期满
        /// </summary>  
				[DisplayName("1可分红,2期满")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 利息
        /// </summary>  
				[DisplayName("利息")]
		        [Filters.DecimalPrecision(18,3)]
		public decimal  AccrualMoney { get; set; }
		      
       
        
        /// <summary>
        /// 今日累计利息
        /// </summary>  
				[DisplayName("今日累计利息")]
		        [Filters.DecimalPrecision(18,3)]
		public decimal?  TodayAccrual { get; set; }
		      
       
        
        /// <summary>
        /// 利息时
        /// </summary>  
				[DisplayName("利息时")]
				public int  AccrualHour { get; set; }
		      
       
        
        /// <summary>
        /// 剩余利息时
        /// </summary>  
				[DisplayName("剩余利息时")]
				public int  SurplusHour { get; set; }
		      
       
        
        /// <summary>
        /// 时利率
        /// </summary>  
				[DisplayName("时利率")]
		        [Filters.DecimalPrecision(18,3)]
		public decimal?  AccruaRate { get; set; }
		      
       
        
        /// <summary>
        /// 是否还计算利息
        /// </summary>  
				[DisplayName("是否还计算利息")]
				public bool  IsAccruaCount { get; set; }
		      
       
        
        /// <summary>
        /// 利息停止原因
        /// </summary>  
				[DisplayName("利息停止原因")]
		        [MaxLength(50,ErrorMessage="利息停止原因最大长度为50")]
		public string  AccrualStopReason { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
		        [MaxLength(50,ErrorMessage="预留最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public DateTime?  ReserveDate1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public bool?  ReserveBoolean1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
		        [Filters.DecimalPrecision(18,3)]
		public decimal?  ReserveDecamal1 { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public TimeDeposit()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 定期存款表业务接口
    /// </summary>
    public interface ITimeDepositService :IServiceBase<TimeDeposit> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(TimeDeposit entity);
	}
    /// <summary>
    /// 定期存款表业务类
    /// </summary>
    public class TimeDepositService :  ServiceBase<TimeDeposit>,ITimeDepositService
    {


        public TimeDepositService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(TimeDeposit entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
