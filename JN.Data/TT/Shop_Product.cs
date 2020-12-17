 







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
        public DbSet<Shop_Product> Shop_Product { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Product
    {

		
        

        /// <summary>
        /// 商品ID
        /// </summary>  
		
		[DisplayName("商品ID")]
		


		[Key]
		public long  Id { get; set; }
		      
       
        

        /// <summary>
        /// 创建时间
        /// </summary>  
		
		[DisplayName("创建时间")]
		


		public DateTime  CreateTime { get; set; }
		      
       
        

        /// <summary>
        /// 产品名称
        /// </summary>  
		
		[DisplayName("产品名称")]
		

        [MaxLength(250,ErrorMessage="产品名称最大长度为250")]


		public string  ProductName { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ShortName")]
		

        [MaxLength(64,ErrorMessage="最大长度为64")]


		public string  ShortName { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ProductCode")]
		

        [MaxLength(250,ErrorMessage="最大长度为250")]


		public string  ProductCode { get; set; }
		      
       
        

        /// <summary>
        /// 图片路径
        /// </summary>  
		
		[DisplayName("图片路径")]


		public string  ImageUrl { get; set; }
		      
       
        

        /// <summary>
        /// 所属商品分类ID
        /// </summary>  
		
		[DisplayName("所属商品分类ID")]
		


		public int  GoodsClassId { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("PrentClass")]
		


		public int?  PrentClass { get; set; }
		      
       
        

        /// <summary>
        /// 商品类别路径
        /// </summary>  
		
		[DisplayName("商品类别路径")]
		


		public string  ClassPath { get; set; }
		      
       
        

        /// <summary>
        /// 成本价
        /// </summary>  
		
		[DisplayName("成本价")]
		


        [Filters.DecimalPrecision(18,2)]

		public decimal?  CostPrice { get; set; }
		      
       
        

        /// <summary>
        /// 销售价格
        /// </summary>  
		
		[DisplayName("销售价格")]
		


        [Filters.DecimalPrecision(18,2)]

		public decimal  RealPrice { get; set; }
		      
       
        

        /// <summary>
        /// 库存
        /// </summary>  
		
		[DisplayName("库存")]
		


		public int  Stock { get; set; }
		      
       
        

        /// <summary>
        /// 店铺ID
        /// </summary>  
		
		[DisplayName("店铺ID")]
		


		public long  SId { get; set; }
		      
       
        

        /// <summary>
        /// 店铺名称
        /// </summary>  
		
		[DisplayName("店铺名称")]
		

        [MaxLength(250,ErrorMessage="店铺名称最大长度为250")]


		public string  ShopName { get; set; }
		      
       
        

        /// <summary>
        /// 商品简介
        /// </summary>  
		
		[DisplayName("商品简介")]
		

        [MaxLength(16,ErrorMessage="商品简介最大长度为16")]


		public string  Info { get; set; }
		      
       
        

        /// <summary>
        /// 详细信息
        /// </summary>  
		
		[DisplayName("详细信息")]
		


		public string  InfoMation { get; set; }
		      
       
        

        /// <summary>
        /// 售后服务
        /// </summary>  
		
		[DisplayName("售后服务")]
		


		public string  AfterSsales { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("AddUP")]
		


		public int  AddUP { get; set; }
		      
       
        

        /// <summary>
        /// 是否上架
        /// </summary>  
		
		[DisplayName("是否上架")]
		


		public bool  Status { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsLock")]
		


		public bool?  IsLock { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("Inclueding")]
		


		public bool  Inclueding { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ReviewCount")]
		


		public int?  ReviewCount { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsNew")]
		


		public bool?  IsNew { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsDiscount")]
		


		public bool?  IsDiscount { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsStar")]
		


		public bool?  IsStar { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsLike")]
		


		public bool?  IsLike { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsF5")]
		


		public bool?  IsF5 { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsF6")]
		


		public bool?  IsF6 { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsF7")]
		


		public bool?  IsF7 { get; set; }
		      
       
        

        /// <summary>
        /// 是否通过后台审核
        /// </summary>  
		
		[DisplayName("是否通过后台审核")]
		


		public bool?  IsPass { get; set; }
		      
       
        

        /// <summary>
        /// 是否为线下商品
        /// </summary>  
		
		[DisplayName("是否为线下商品")]
		


		public bool  IsOfflineProduct { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Product()
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
    public interface IShop_ProductService :IServiceBase<Shop_Product> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Product entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_ProductService :  ServiceBase<Shop_Product>,IShop_ProductService
    {


        public Shop_ProductService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Product entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
