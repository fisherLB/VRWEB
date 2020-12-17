 


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
        public DbSet<ShopProduct> ShopProduct { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class ShopProduct
    {

		
        
        /// <summary>
        /// 商品表ID
        /// </summary>  
				[DisplayName("商品表ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 会员ID(店铺)
        /// </summary>  
				[DisplayName("会员ID(店铺)")]
				public int  ShopID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ShopName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ShopName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 矿机名称
        /// </summary>  
				[DisplayName("矿机名称")]
		        [MaxLength(250,ErrorMessage="矿机名称最大长度为250")]
		public string  ProductName { get; set; }
		      
       
        
        /// <summary>
        /// 商品简称
        /// </summary>  
				[DisplayName("商品简称")]
		        [MaxLength(64,ErrorMessage="商品简称最大长度为64")]
		public string  ShortName { get; set; }
		      
       
        
        /// <summary>
        /// 产品编码
        /// </summary>  
				[DisplayName("产品编码")]
		        [MaxLength(250,ErrorMessage="产品编码最大长度为250")]
		public string  ProductCode { get; set; }
		      
       
        
        /// <summary>
        /// 规格
        /// </summary>  
				[DisplayName("规格")]
		        [MaxLength(50,ErrorMessage="规格最大长度为50")]
		public string  Spec { get; set; }
		      
       
        
        /// <summary>
        /// 单位
        /// </summary>  
				[DisplayName("单位")]
		        [MaxLength(50,ErrorMessage="单位最大长度为50")]
		public string  Unit { get; set; }
		      
       
        
        /// <summary>
        /// 图片地址
        /// </summary>  
				[DisplayName("图片地址")]
		        [MaxLength(250,ErrorMessage="图片地址最大长度为250")]
		public string  ImageUrl { get; set; }
		      
       
        
        /// <summary>
        /// 产品分类
        /// </summary>  
				[DisplayName("产品分类")]
				public int  ClassId { get; set; }
		      
       
        
        /// <summary>
        /// 原价
        /// </summary>  
				[DisplayName("原价")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  CostPrice { get; set; }
		      
       
        
        /// <summary>
        /// 现价
        /// </summary>  
				[DisplayName("现价")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  RealPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("FreightPrice")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  FreightPrice { get; set; }
		      
       
        
        /// <summary>
        /// 封顶收益
        /// </summary>  
				[DisplayName("封顶收益")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TopBonus { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Rate")]
		        [Filters.DecimalPrecision(18,4)]
		public decimal  Rate { get; set; }
		      
       
        
        /// <summary>
        /// 库存
        /// </summary>  
				[DisplayName("库存")]
				public int  Stock { get; set; }
		      
       
        
        /// <summary>
        /// 产品简介
        /// </summary>  
				[DisplayName("产品简介")]
		        [MaxLength(250,ErrorMessage="产品简介最大长度为250")]
		public string  ShortContent { get; set; }
		      
       
        
        /// <summary>
        /// 商品详细介绍
        /// </summary>  
				[DisplayName("商品详细介绍")]
				public string  Content { get; set; }
		      
       
        
        /// <summary>
        /// 1上架，0下架
        /// </summary>  
				[DisplayName("1上架，0下架")]
				public bool  IsSales { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SaleCount")]
				public int  SaleCount { get; set; }
		      
       
        
        /// <summary>
        /// 1明星产品，0不是
        /// </summary>  
				[DisplayName("1明星产品，0不是")]
				public bool?  IsStar { get; set; }
		      
       
        
        /// <summary>
        /// 1网友都喜欢，0不是
        /// </summary>  
				[DisplayName("1网友都喜欢，0不是")]
				public bool?  IsHot { get; set; }
		      
       
        
        /// <summary>
        /// 1是热评，0不是
        /// </summary>  
				[DisplayName("1是热评，0不是")]
				public bool?  IsElite { get; set; }
		      
       
        
        /// <summary>
        /// 1是爱看图书，0不是
        /// </summary>  
				[DisplayName("1是爱看图书，0不是")]
				public bool?  IsTop { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Hits")]
				public int  Hits { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr1")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr2")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr3")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveInt1")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveInt2")]
				public int?  ReserveInt2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDate")]
				public DateTime?  ReserveDate { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal { get; set; }
		      
       
        
        /// <summary>
        /// 注册是否赠送
        /// </summary>  
				[DisplayName("注册是否赠送")]
				public bool?  IsPresent { get; set; }
		      
       
        
        /// <summary>
        /// 矿机对应业绩
        /// </summary>  
				[DisplayName("矿机对应业绩")]
		        [Filters.DecimalPrecision(18,4)]
		public decimal?  Performance { get; set; }
		      
       
        
        /// <summary>
        /// 转换比例
        /// </summary>  
				[DisplayName("转换比例")]
		        [Filters.DecimalPrecision(18,4)]
		public decimal?  Conversion { get; set; }
		      
       
        
        /// <summary>
        /// 每天/时/分钟收益
        /// </summary>  
				[DisplayName("每天/时/分钟收益")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  TimesType { get; set; }
		      
       
        
        /// <summary>
        /// 收益时长
        /// </summary>  
				[DisplayName("收益时长")]
				public int?  Duration { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyTop1001")]
				public int?  BuyTop1001 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyTop1002")]
				public int?  BuyTop1002 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyTop1003")]
				public int?  BuyTop1003 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyTop1004")]
				public int?  BuyTop1004 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyTop1005")]
				public int?  BuyTop1005 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyTop1006")]
				public int?  BuyTop1006 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyTop")]
				public int?  BuyTop { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ShopProduct()
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
    public interface IShopProductService :IServiceBase<ShopProduct> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ShopProduct entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class ShopProductService :  ServiceBase<ShopProduct>,IShopProductService
    {


        public ShopProductService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ShopProduct entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
