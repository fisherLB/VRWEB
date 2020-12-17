 


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
        public DbSet<Shop_Comments> Shop_Comments { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Comments
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Uid")]
				public int  Uid { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("pid")]
				public int  pid { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("sid")]
				public int  sid { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Content")]
				public string  Content { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AnswerId")]
				public bool?  AnswerId { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("proComment")]
		        [MaxLength(200,ErrorMessage="最大长度为200")]
		public string  proComment { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("shopComment")]
		        [MaxLength(200,ErrorMessage="最大长度为200")]
		public string  shopComment { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("proStar")]
				public double?  proStar { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("shopStart")]
				public double?  shopStart { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("wlStart")]
				public double?  wlStart { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsLock")]
				public bool  IsLock { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OrderNumber")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  OrderNumber { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Comments()
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
    public interface IShop_CommentsService :IServiceBase<Shop_Comments> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Comments entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_CommentsService :  ServiceBase<Shop_Comments>,IShop_CommentsService
    {


        public Shop_CommentsService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Comments entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
