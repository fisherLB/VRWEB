 


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
        public DbSet<Article> Article { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Article
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 文章标题
        /// </summary>  
				[DisplayName("文章标题")]
		        [MaxLength(50,ErrorMessage="文章标题最大长度为50")]
		public string  Title { get; set; }
		      
       
        
        /// <summary>
        /// 二级分类ID
        /// </summary>  
				[DisplayName("二级分类ID")]
				public int  ClassID { get; set; }
		      
       
        
        /// <summary>
        /// 内容
        /// </summary>  
				[DisplayName("内容")]
		        [MaxLength(16,ErrorMessage="内容最大长度为16")]
		public string  Content { get; set; }
		      
       
        
        /// <summary>
        /// 作者
        /// </summary>  
				[DisplayName("作者")]
		        [MaxLength(50,ErrorMessage="作者最大长度为50")]
		public string  Author { get; set; }
		      
       
        
        /// <summary>
        /// 来源于
        /// </summary>  
				[DisplayName("来源于")]
		        [MaxLength(50,ErrorMessage="来源于最大长度为50")]
		public string  Source { get; set; }
		      
       
        
        /// <summary>
        /// 阅读量
        /// </summary>  
				[DisplayName("阅读量")]
				public int  ReadCount { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 展示在首页？
        /// </summary>  
				[DisplayName("展示在首页？")]
				public bool?  IsShowHome { get; set; }
		      
       
        
        /// <summary>
        /// 置顶
        /// </summary>  
				[DisplayName("置顶")]
				public bool?  IsTop { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsNotice")]
				public bool?  IsNotice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ClassPath")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ClassPath { get; set; }
		      
       
        
        /// <summary>
        /// 副标题
        /// </summary>  
				[DisplayName("副标题")]
		        [MaxLength(250,ErrorMessage="副标题最大长度为250")]
		public string  SubTitle { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Article()
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
    public interface IArticleService :IServiceBase<Article> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Article entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class ArticleService :  ServiceBase<Article>,IArticleService
    {


        public ArticleService(ISysDbFactory dbfactory) : base(dbfactory) { DataContext.Configuration.ValidateOnSaveEnabled = false; }
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Article entity)
        {
           
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
