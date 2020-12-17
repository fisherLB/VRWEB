 


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
        public DbSet<Shop_ReceiptAddress> Shop_ReceiptAddress { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_ReceiptAddress
    {

		
        
        /// <summary>
        /// 收件地址表ID
        /// </summary>  
				[DisplayName("收件地址表ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 会员ID
        /// </summary>  
				[DisplayName("会员ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 收件人名称
        /// </summary>  
				[DisplayName("收件人名称")]
		        [MaxLength(100,ErrorMessage="收件人名称最大长度为100")]
		public string  Addressee { get; set; }
		      
       
        
        /// <summary>
        /// 收件人电话号码
        /// </summary>  
				[DisplayName("收件人电话号码")]
		        [MaxLength(20,ErrorMessage="收件人电话号码最大长度为20")]
		public string  Phone { get; set; }
		      
       
        
        /// <summary>
        /// 是否为默认地址
        /// </summary>  
				[DisplayName("是否为默认地址")]
				public bool  IsDefault { get; set; }
		      
       
        
        /// <summary>
        /// 省份
        /// </summary>  
				[DisplayName("省份")]
		        [MaxLength(100,ErrorMessage="省份最大长度为100")]
		public string  Province { get; set; }
		      
       
        
        /// <summary>
        /// 城市
        /// </summary>  
				[DisplayName("城市")]
		        [MaxLength(100,ErrorMessage="城市最大长度为100")]
		public string  City { get; set; }
		      
       
        
        /// <summary>
        /// 地区/县
        /// </summary>  
				[DisplayName("地区/县")]
		        [MaxLength(100,ErrorMessage="地区/县最大长度为100")]
		public string  County { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 收件详细地址
        /// </summary>  
				[DisplayName("收件详细地址")]
		        [MaxLength(200,ErrorMessage="收件详细地址最大长度为200")]
		public string  Detail { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_ReceiptAddress()
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
    public interface IShop_ReceiptAddressService :IServiceBase<Shop_ReceiptAddress> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_ReceiptAddress entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_ReceiptAddressService :  ServiceBase<Shop_ReceiptAddress>,IShop_ReceiptAddressService
    {


        public Shop_ReceiptAddressService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_ReceiptAddress entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
