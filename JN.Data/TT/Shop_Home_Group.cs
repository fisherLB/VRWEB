 


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
        public DbSet<Shop_Home_Group> Shop_Home_Group { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Home_Group
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
				[DisplayName("Name")]
		        [MaxLength(100,ErrorMessage="最大长度为100")]
		public string  Name { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Uid")]
				public int  Uid { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(100,ErrorMessage="最大长度为100")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Title")]
		        [MaxLength(200,ErrorMessage="最大长度为200")]
		public string  Title { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Home_Group()
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
    public interface IShop_Home_GroupService :IServiceBase<Shop_Home_Group> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Home_Group entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_Home_GroupService :  ServiceBase<Shop_Home_Group>,IShop_Home_GroupService
    {


        public Shop_Home_GroupService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Home_Group entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
