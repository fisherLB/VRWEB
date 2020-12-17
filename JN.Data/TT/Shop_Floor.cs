 


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
        public DbSet<Shop_Floor> Shop_Floor { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Floor
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Id")]
				[Key]
		public int  Id { get; set; }
		      
       
        
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
				public long  SId { get; set; }
		      
       
        
        /// <summary>
        /// 店铺名称
        /// </summary>  
				[DisplayName("店铺名称")]
		        [MaxLength(250,ErrorMessage="店铺名称最大长度为250")]
		public string  ShopName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Tag")]
				public int  Tag { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsUse")]
				public bool  IsUse { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("type")]
				public int  type { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Floor()
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
    public interface IShop_FloorService :IServiceBase<Shop_Floor> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Floor entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_FloorService :  ServiceBase<Shop_Floor>,IShop_FloorService
    {


        public Shop_FloorService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Floor entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
