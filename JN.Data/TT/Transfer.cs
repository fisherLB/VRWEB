 


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
        public DbSet<Transfer> Transfer { get; set; }
    }

	/// <summary>
    /// 会员转帐表
    /// </summary>
	[DisplayName("会员转帐表")]
    public partial class Transfer
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
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 转给用户ID
        /// </summary>  
				[DisplayName("转给用户ID")]
				public int  ToUID { get; set; }
		      
       
        
        /// <summary>
        /// 转给用户名
        /// </summary>  
				[DisplayName("转给用户名")]
		        [MaxLength(50,ErrorMessage="转给用户名最大长度为50")]
		public string  ToUserName { get; set; }
		      
       
        
        /// <summary>
        /// 转账币种id
        /// </summary>  
				[DisplayName("转账币种id")]
				public int  CurID { get; set; }
		      
       
        
        /// <summary>
        /// 转账币种名称
        /// </summary>  
				[DisplayName("转账币种名称")]
		        [MaxLength(50,ErrorMessage="转账币种名称最大长度为50")]
		public string  CurName { get; set; }
		      
       
        
        /// <summary>
        /// 转帐金额
        /// </summary>  
				[DisplayName("转帐金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  TransferMoney { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Poundage { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ActualTransferMoney")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  ActualTransferMoney { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Remark")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Transfer()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 会员转帐表业务接口
    /// </summary>
    public interface ITransferService :IServiceBase<Transfer> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Transfer entity);
	}
    /// <summary>
    /// 会员转帐表业务类
    /// </summary>
    public class TransferService :  ServiceBase<Transfer>,ITransferService
    {


        public TransferService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Transfer entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
