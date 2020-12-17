 


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
        public DbSet<TeamAchievement> TeamAchievement { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class TeamAchievement
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 个人业绩
        /// </summary>  
				[DisplayName("个人业绩")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Investment { get; set; }
		      
       
        
        /// <summary>
        /// 团队业绩
        /// </summary>  
				[DisplayName("团队业绩")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  TeamInvestment { get; set; }
		      
       
        
        /// <summary>
        /// 类型：天或者月（day\month）
        /// </summary>  
				[DisplayName("类型：天或者月（day、month）")]
		        [MaxLength(10,ErrorMessage="类型：天或者月（day、month）最大长度为10")]
		public string  Type { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 当天、当月
        /// </summary>  
				[DisplayName("当天、当月")]
				public DateTime  CurrentTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public TeamAchievement()
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
    public interface ITeamAchievementService :IServiceBase<TeamAchievement> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(TeamAchievement entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class TeamAchievementService :  ServiceBase<TeamAchievement>,ITeamAchievementService
    {


        public TeamAchievementService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(TeamAchievement entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 

