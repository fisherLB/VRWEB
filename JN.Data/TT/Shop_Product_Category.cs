 


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
        public DbSet<Shop_Product_Category> Shop_Product_Category { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Product_Category
    {

		
        
        /// <summary>
        /// 商品分类ID
        /// </summary>  
				[DisplayName("商品分类ID")]
				[Key]
		public int  Id { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 分类名称
        /// </summary>  
				[DisplayName("分类名称")]
		        [MaxLength(50,ErrorMessage="分类名称最大长度为50")]
		public string  Name { get; set; }
		      
       
        
        /// <summary>
        /// 图片路径
        /// </summary>  
				[DisplayName("图片路径")]
				public string  CateImg { get; set; }
		      
       
        
        /// <summary>
        /// 父级ID
        /// </summary>  
				[DisplayName("父级ID")]
				public int  ParentId { get; set; }
		      
       
        
        /// <summary>
        /// 父级名称
        /// </summary>  
				[DisplayName("父级名称")]
		        [MaxLength(50,ErrorMessage="父级名称最大长度为50")]
		public string  parentName { get; set; }
		      
       
        
        /// <summary>
        /// 从属关系路径
        /// </summary>  
				[DisplayName("从属关系路径")]
				public string  Ppacth { get; set; }
		      
       
        
        /// <summary>
        /// 分类样式
        /// </summary>  
				[DisplayName("分类样式")]
		        [MaxLength(10,ErrorMessage="分类样式最大长度为10")]
		public string  Color { get; set; }
		      
       
        
        /// <summary>
        /// 排序
        /// </summary>  
				[DisplayName("排序")]
				public int  Sort { get; set; }
		      
       
        
        /// <summary>
        /// 是否置顶
        /// </summary>  
				[DisplayName("是否置顶")]
				public bool?  IsNavTop { get; set; }
		      
       
        
        /// <summary>
        /// 是否显示
        /// </summary>  
				[DisplayName("是否显示")]
				public bool?  IsShow { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Product_Category()
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
    public interface IShop_Product_CategoryService :IServiceBase<Shop_Product_Category> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Product_Category entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_Product_CategoryService :  ServiceBase<Shop_Product_Category>,IShop_Product_CategoryService
    {


        public Shop_Product_CategoryService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Product_Category entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
