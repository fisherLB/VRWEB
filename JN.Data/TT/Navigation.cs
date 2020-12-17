 


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
        public DbSet<Navigation> Navigation { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Navigation
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 上级ID
        /// </summary>  
				[DisplayName("上级ID")]
				public int  ParentID { get; set; }
		      
       
        
        /// <summary>
        /// 子栏目数
        /// </summary>  
				[DisplayName("子栏目数")]
				public int  Child { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Icon")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Icon { get; set; }
		      
       
        
        /// <summary>
        /// 标题
        /// </summary>  
				[DisplayName("标题")]
		        [MaxLength(50,ErrorMessage="标题最大长度为50")]
		public string  Title { get; set; }
		      
       
        
        /// <summary>
        /// 地址
        /// </summary>  
				[DisplayName("地址")]
		        [MaxLength(250,ErrorMessage="地址最大长度为250")]
		public string  Url { get; set; }
		      
       
        
        /// <summary>
        /// 排序
        /// </summary>  
				[DisplayName("排序")]
				public int  Sort { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsShow")]
				public bool  IsShow { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsSubAccout")]
				public bool?  IsSubAccout { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Navigation()
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
    public interface INavigationService :IServiceBase<Navigation> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Navigation entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class NavigationService :  ServiceBase<Navigation>,INavigationService
    {


        public NavigationService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Navigation entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
