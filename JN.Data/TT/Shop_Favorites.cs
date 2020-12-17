 


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
        public DbSet<Shop_Favorites> Shop_Favorites { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Favorites
    {

		
        
        /// <summary>
        /// 收藏表ID
        /// </summary>  
				[DisplayName("收藏表ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 商品ID
        /// </summary>  
				[DisplayName("商品ID")]
				public int  Pid { get; set; }
		      
       
        
        /// <summary>
        /// 商品名称
        /// </summary>  
				[DisplayName("商品名称")]
		        [MaxLength(250,ErrorMessage="商品名称最大长度为250")]
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
        /// 收藏者ID
        /// </summary>  
				[DisplayName("收藏者ID")]
				public int  Uid { get; set; }
		      
       
        
        /// <summary>
        /// 收藏者用户名
        /// </summary>  
				[DisplayName("收藏者用户名")]
		        [MaxLength(50,ErrorMessage="收藏者用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  createtime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Favorites()
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
    public interface IShop_FavoritesService :IServiceBase<Shop_Favorites> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Favorites entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_FavoritesService :  ServiceBase<Shop_Favorites>,IShop_FavoritesService
    {


        public Shop_FavoritesService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Favorites entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
