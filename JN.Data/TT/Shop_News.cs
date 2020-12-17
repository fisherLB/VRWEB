 


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
        public DbSet<Shop_News> Shop_News { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_News
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
				[DisplayName("Title")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  Title { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TitleImageUrl")]
		        [MaxLength(2000,ErrorMessage="最大长度为2000")]
		public string  TitleImageUrl { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Hits")]
				public int  Hits { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Source")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  Source { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CateId")]
				public int  CateId { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("NewsContent")]
		        [MaxLength(16,ErrorMessage="最大长度为16")]
		public string  NewsContent { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsShow")]
				public bool  IsShow { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Status")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Desc")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  Desc { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_News()
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
    public interface IShop_NewsService :IServiceBase<Shop_News> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_News entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_NewsService :  ServiceBase<Shop_News>,IShop_NewsService
    {


        public Shop_NewsService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_News entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
