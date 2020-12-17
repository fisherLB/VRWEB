 


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
        public DbSet<ShopSysSetting> ShopSysSetting { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class ShopSysSetting
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Id")]
				[Key]
		public int  Id { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserCanLogin")]
				public bool  UserCanLogin { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserCanReg")]
				public bool  UserCanReg { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IpsPayId")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  IpsPayId { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IpsPayKey")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  IpsPayKey { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IpsPayPost")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  IpsPayPost { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TestVipDay")]
				public int?  TestVipDay { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MsgId")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  MsgId { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MsgKey")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  MsgKey { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OpenBuyTime")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  OpenBuyTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Logo1")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  Logo1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("adiscount")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  adiscount { get; set; }
		      
       
        
        /// <summary>
        /// 初级代理积分额度
        /// </summary>  
				[DisplayName("初级代理积分额度")]
				public int?  Agentintegral { get; set; }
		      
       
        
        /// <summary>
        /// 消费积分比例
        /// </summary>  
				[DisplayName("消费积分比例")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Integralratio { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MostStore")]
				public bool?  MostStore { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl1")]
		        [MaxLength(2000,ErrorMessage="最大长度为2000")]
		public string  KefuUrl1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl2")]
		        [MaxLength(2000,ErrorMessage="最大长度为2000")]
		public string  KefuUrl2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl3")]
		        [MaxLength(2000,ErrorMessage="最大长度为2000")]
		public string  KefuUrl3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl4")]
		        [MaxLength(2000,ErrorMessage="最大长度为2000")]
		public string  KefuUrl4 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl5")]
		        [MaxLength(2000,ErrorMessage="最大长度为2000")]
		public string  KefuUrl5 { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ShopSysSetting()
        {
        //    Id = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IShopSysSettingService :IServiceBase<ShopSysSetting> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ShopSysSetting entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class ShopSysSettingService :  ServiceBase<ShopSysSetting>,IShopSysSettingService
    {


        public ShopSysSettingService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ShopSysSetting entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
