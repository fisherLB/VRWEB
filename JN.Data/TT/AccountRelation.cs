 


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
        public DbSet<AccountRelation> AccountRelation { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class AccountRelation
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名称
        /// </summary>  
				[DisplayName("用户名称")]
		        [MaxLength(50,ErrorMessage="用户名称最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 关联帐号ID
        /// </summary>  
				[DisplayName("关联帐号ID")]
				public int  RelationUID { get; set; }
		      
       
        
        /// <summary>
        /// 关联帐号名称
        /// </summary>  
				[DisplayName("关联帐号名称")]
		        [MaxLength(50,ErrorMessage="关联帐号名称最大长度为50")]
		public string  RelationUserName { get; set; }
		      
       
        
        /// <summary>
        /// 关联状态
        /// </summary>  
				[DisplayName("关联状态")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 关联时间
        /// </summary>  
				[DisplayName("关联时间")]
				public DateTime?  RelationTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public AccountRelation()
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
    public interface IAccountRelationService :IServiceBase<AccountRelation> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(AccountRelation entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class AccountRelationService :  ServiceBase<AccountRelation>,IAccountRelationService
    {


        public AccountRelationService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(AccountRelation entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 

