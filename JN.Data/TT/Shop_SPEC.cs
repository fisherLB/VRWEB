 


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
        public DbSet<Shop_SPEC> Shop_SPEC { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_SPEC
    {

		
        
        /// <summary>
        /// 商品规格表ID
        /// </summary>  
				[DisplayName("商品规格表ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 商品ID
        /// </summary>  
				[DisplayName("商品ID")]
				public int  PID { get; set; }
		      
       
        
        /// <summary>
        /// 规格名称
        /// </summary>  
				[DisplayName("规格名称")]
		        [MaxLength(1000,ErrorMessage="规格名称最大长度为1000")]
		public string  Value { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsUse")]
				public bool  IsUse { get; set; }
		      
       
        
        /// <summary>
        /// 商品销售价格
        /// </summary>  
				[DisplayName("商品销售价格")]
		        [Filters.DecimalPrecision(18,0)]
		public decimal  Price { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_SPEC()
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
    public interface IShop_SPECService :IServiceBase<Shop_SPEC> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_SPEC entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_SPECService :  ServiceBase<Shop_SPEC>,IShop_SPECService
    {


        public Shop_SPECService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_SPEC entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
