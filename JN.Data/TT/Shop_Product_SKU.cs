 


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
        public DbSet<Shop_Product_SKU> Shop_Product_SKU { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Product_SKU
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
				[DisplayName("SKU_Name")]
		        [MaxLength(100,ErrorMessage="最大长度为100")]
		public string  SKU_Name { get; set; }
		      
       
        
        /// <summary>
        /// 图片路径
        /// </summary>  
				[DisplayName("图片路径")]
		        [MaxLength(250,ErrorMessage="图片路径最大长度为250")]
		public string  ImgUrl { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Stock")]
				public int  Stock { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SKU_Code")]
				public string  SKU_Code { get; set; }
		      
       
        
        /// <summary>
        /// 商品ID
        /// </summary>  
				[DisplayName("商品ID")]
				public int  Pid { get; set; }
		      
       
        
        /// <summary>
        /// 店铺ID
        /// </summary>  
				[DisplayName("店铺ID")]
				public int?  SId { get; set; }
		      
       
        
        /// <summary>
        /// 是否锁定
        /// </summary>  
				[DisplayName("是否锁定")]
				public bool  IsLock { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Product_SKU()
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
    public interface IShop_Product_SKUService :IServiceBase<Shop_Product_SKU> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Product_SKU entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_Product_SKUService :  ServiceBase<Shop_Product_SKU>,IShop_Product_SKUService
    {


        public Shop_Product_SKUService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Product_SKU entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
