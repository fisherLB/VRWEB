 


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
        public DbSet<UserVerify> UserVerify { get; set; }
    }

	/// <summary>
    /// 用户页面密码级别表
    /// </summary>
	[DisplayName("用户页面密码级别表")]
    public partial class UserVerify
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 页面名
        /// </summary>  
				[DisplayName("页面名")]
		        [MaxLength(100,ErrorMessage="页面名最大长度为100")]
		public string  Name { get; set; }
		      
       
        
        /// <summary>
        /// 控制器名
        /// </summary>  
				[DisplayName("控制器名")]
		        [MaxLength(50,ErrorMessage="控制器名最大长度为50")]
		public string  ControllerName { get; set; }
		      
       
        
        /// <summary>
        /// 操作名
        /// </summary>  
				[DisplayName("操作名")]
		        [MaxLength(50,ErrorMessage="操作名最大长度为50")]
		public string  ActionName { get; set; }
		      
       
        
        /// <summary>
        /// 访问密码级别
        /// </summary>  
				[DisplayName("访问密码级别")]
				public int  VerifyLevel { get; set; }
		      
       
        
        /// <summary>
        /// 描述
        /// </summary>  
				[DisplayName("描述")]
		        [MaxLength(250,ErrorMessage="描述最大长度为250")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LastUpdateTime")]
				public DateTime  LastUpdateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public UserVerify()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 用户页面密码级别表业务接口
    /// </summary>
    public interface IUserVerifyService :IServiceBase<UserVerify> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(UserVerify entity);
	}
    /// <summary>
    /// 用户页面密码级别表业务类
    /// </summary>
    public class UserVerifyService :  ServiceBase<UserVerify>,IUserVerifyService
    {


        public UserVerifyService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(UserVerify entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
