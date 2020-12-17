 


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
        public DbSet<Shop_Cart> Shop_Cart { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Cart
    {

		
        
        /// <summary>
        /// 购物车ID
        /// </summary>  
				[DisplayName("购物车ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  Uid { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 产品ID
        /// </summary>  
				[DisplayName("产品ID")]
				public int  Pid { get; set; }
		      
       
        
        /// <summary>
        /// 产品名称
        /// </summary>  
				[DisplayName("产品名称")]
		        [MaxLength(250,ErrorMessage="产品名称最大长度为250")]
		public string  ProductName { get; set; }
		      
       
        
        /// <summary>
        /// 店铺ID
        /// </summary>  
				[DisplayName("店铺ID")]
				public int  Sid { get; set; }
		      
       
        
        /// <summary>
        /// 店铺名称
        /// </summary>  
				[DisplayName("店铺名称")]
		        [MaxLength(50,ErrorMessage="店铺名称最大长度为50")]
		public string  ShopName { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Cart()
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
    public interface IShop_CartService :IServiceBase<Shop_Cart> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Cart entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_CartService :  ServiceBase<Shop_Cart>,IShop_CartService
    {


        public Shop_CartService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Cart entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
