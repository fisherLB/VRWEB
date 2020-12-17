 


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
        public DbSet<Shop_Tmp_Pro_Img> Shop_Tmp_Pro_Img { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Tmp_Pro_Img
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
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ImgName")]
				public string  ImgName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ImgUrl")]
				public string  ImgUrl { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ImgType")]
				public int  ImgType { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ImgAlt")]
				public string  ImgAlt { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ProId")]
				public long  ProId { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Tmp_Pro_Img()
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
    public interface IShop_Tmp_Pro_ImgService :IServiceBase<Shop_Tmp_Pro_Img> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Tmp_Pro_Img entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_Tmp_Pro_ImgService :  ServiceBase<Shop_Tmp_Pro_Img>,IShop_Tmp_Pro_ImgService
    {


        public Shop_Tmp_Pro_ImgService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Tmp_Pro_Img entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
