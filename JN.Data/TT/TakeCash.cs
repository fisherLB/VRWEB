 


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
        public DbSet<TakeCash> TakeCash { get; set; }
    }

	/// <summary>
    /// 提现表
    /// </summary>
	[DisplayName("提现表")]
    public partial class TakeCash
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("FromAgent")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  FromAgent { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AgentMobile")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  AgentMobile { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 银行名称
        /// </summary>  
				[DisplayName("银行名称")]
		        [MaxLength(50,ErrorMessage="银行名称最大长度为50")]
		public string  BankName { get; set; }
		      
       
        
        /// <summary>
        /// 银行卡号
        /// </summary>  
				[DisplayName("银行卡号")]
		        [MaxLength(50,ErrorMessage="银行卡号最大长度为50")]
		public string  BankCard { get; set; }
		      
       
        
        /// <summary>
        /// 银行户名
        /// </summary>  
				[DisplayName("银行户名")]
		        [MaxLength(50,ErrorMessage="银行户名最大长度为50")]
		public string  BankUser { get; set; }
		      
       
        
        /// <summary>
        /// 开户行
        /// </summary>  
				[DisplayName("开户行")]
		        [MaxLength(50,ErrorMessage="开户行最大长度为50")]
		public string  BankOfDeposit { get; set; }
		      
       
        
        /// <summary>
        /// 申请金额
        /// </summary>  
				[DisplayName("申请金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  DrawMoney { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Poundage { get; set; }
		      
       
        
        /// <summary>
        /// 应支付金额
        /// </summary>  
				[DisplayName("应支付金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  ActualDrawMoney { get; set; }
		      
       
        
        /// <summary>
        /// 余额
        /// </summary>  
				[DisplayName("余额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Balance { get; set; }
		      
       
        
        /// <summary>
        /// 备注
        /// </summary>  
				[DisplayName("备注")]
		        [MaxLength(250,ErrorMessage="备注最大长度为250")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 申请时间
        /// </summary>  
				[DisplayName("申请时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 支付时间
        /// </summary>  
				[DisplayName("支付时间")]
				public DateTime?  PayTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Status")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 币种id
        /// </summary>  
				[DisplayName("币种id")]
				public int?  CurID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  CurName { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public TakeCash()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 提现表业务接口
    /// </summary>
    public interface ITakeCashService :IServiceBase<TakeCash> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(TakeCash entity);
	}
    /// <summary>
    /// 提现表业务类
    /// </summary>
    public class TakeCashService :  ServiceBase<TakeCash>,ITakeCashService
    {


        public TakeCashService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(TakeCash entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
