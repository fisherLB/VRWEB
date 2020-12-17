 


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
        public DbSet<RaCoin> RaCoin { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class RaCoin
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// in/out(充入/提出)
        /// </summary>  
				[DisplayName("in/out(充入/提出)")]
		        [MaxLength(50,ErrorMessage="in/out(充入/提出)最大长度为50")]
		public string  Direction { get; set; }
		      
       
        
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
        /// 
        /// </summary>  
				[DisplayName("CurID")]
				public int  CurID { get; set; }
		      
       
        
        /// <summary>
        /// 离线地址
        /// </summary>  
				[DisplayName("离线地址")]
		        [MaxLength(50,ErrorMessage="离线地址最大长度为50")]
		public string  WalletAddress { get; set; }
		      
       
        
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
        /// 审核时间
        /// </summary>  
				[DisplayName("审核时间")]
				public DateTime?  PassTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Status")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr1")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr2")]
		        [MaxLength(1000,ErrorMessage="最大长度为1000")]
		public string  ReserveStr2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveInt")]
				public int?  ReserveInt { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  ReserveDecamal { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDate")]
				public DateTime?  ReserveDate { get; set; }



        /// <summary>
        /// 事务ID
        /// </summary>  
        [DisplayName("事务ID")]
        [MaxLength(250, ErrorMessage = "事务ID最大长度为250")]
        public string Txid { get; set; }



        /// <summary>
        /// 标签
        /// </summary>  
        [DisplayName("标签")]
        [MaxLength(50, ErrorMessage = "标签最大长度为50")]
        public string Tag { get; set; }




        /// <summary>
        /// 构造函数
        /// </summary>

        public RaCoin()
        {
           // ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IRaCoinService :IServiceBase<RaCoin> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(RaCoin entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class RaCoinService :  ServiceBase<RaCoin>,IRaCoinService
    {


        public RaCoinService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(RaCoin entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
