 


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
        public DbSet<VRUsers> VRUsers { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class VRUsers
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
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  Name { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Password")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  Password { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime?  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("VRTime")]
				public int  VRTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public VRUsers()
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
    public interface IVRUsersService :IServiceBase<VRUsers> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(VRUsers entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class VRUsersService :  ServiceBase<VRUsers>,IVRUsersService
    {


        public VRUsersService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(VRUsers entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
