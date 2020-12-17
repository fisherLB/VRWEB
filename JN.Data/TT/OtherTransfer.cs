 


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
        public DbSet<OtherTransfer> OtherTransfer { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class OtherTransfer
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 单号
        /// </summary>  
				[DisplayName("单号")]
		        [MaxLength(50,ErrorMessage="单号最大长度为50")]
		public string  No { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(20,ErrorMessage="用户名最大长度为20")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 变更金额
        /// </summary>  
				[DisplayName("变更金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  ChangeMoney { get; set; }
		      
       
        
        /// <summary>
        /// 价格
        /// </summary>  
				[DisplayName("价格")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Price { get; set; }
		      
       
        
        /// <summary>
        /// 币种ID
        /// </summary>  
				[DisplayName("币种ID")]
				public int  CoinID { get; set; }
		      
       
        
        /// <summary>
        /// 类型 1转入 2转出
        /// </summary>  
				[DisplayName("类型 1转入 2转出")]
				public int  doType { get; set; }
		      
       
        
        /// <summary>
        /// 状态 0等待确认, 10确认
        /// </summary>  
				[DisplayName("状态 0等待确认, 10确认")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 转账用户id
        /// </summary>  
				[DisplayName("转账用户id")]
				public int  BindUserId { get; set; }
		      
       
        
        /// <summary>
        /// 绑定用户名
        /// </summary>  
				[DisplayName("绑定用户名")]
		        [MaxLength(20,ErrorMessage="绑定用户名最大长度为20")]
		public string  BindUserName { get; set; }
		      
       
        
        /// <summary>
        /// 变更时间
        /// </summary>  
				[DisplayName("变更时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Poundage { get; set; }
		      
       
        
        /// <summary>
        /// 实际金额
        /// </summary>  
				[DisplayName("实际金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  ActualMoney { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public OtherTransfer()
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
    public interface IOtherTransferService :IServiceBase<OtherTransfer> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(OtherTransfer entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class OtherTransferService :  ServiceBase<OtherTransfer>,IOtherTransferService
    {


        public OtherTransferService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(OtherTransfer entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
