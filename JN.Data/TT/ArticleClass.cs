 


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
        public DbSet<ArticleClass> ArticleClass { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class ArticleClass
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 父级分类ID
        /// </summary>  
				[DisplayName("父级分类ID")]
				public int  Pid { get; set; }
		      
       
        
        /// <summary>
        /// 分类路径
        /// </summary>  
				[DisplayName("分类路径")]
		        [MaxLength(50,ErrorMessage="分类路径最大长度为50")]
		public string  Ppach { get; set; }
		      
       
        
        /// <summary>
        /// 分类名称
        /// </summary>  
				[DisplayName("分类名称")]
		        [MaxLength(50,ErrorMessage="分类名称最大长度为50")]
		public string  ClassName { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 排序
        /// </summary>  
				[DisplayName("排序")]
				public int  Sort { get; set; }
		      
       
        
        /// <summary>
        /// 显示在底部
        /// </summary>  
				[DisplayName("显示在底部")]
				public bool?  IsShowBottom { get; set; }
		      
       
        
        /// <summary>
        /// 文章推广
        /// </summary>  
				[DisplayName("文章推广")]
				public bool?  IsNotice { get; set; }
		      
       
        
        /// <summary>
        /// 是否二级分类文章可重复
        /// </summary>  
				[DisplayName("是否二级分类文章可重复")]
				public bool  IsAlone { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ArticleClass()
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
    public interface IArticleClassService :IServiceBase<ArticleClass> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ArticleClass entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class ArticleClassService :  ServiceBase<ArticleClass>,IArticleClassService
    {


        public ArticleClassService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ArticleClass entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
