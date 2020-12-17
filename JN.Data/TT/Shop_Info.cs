 







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
        public DbSet<Shop_Info> Shop_Info { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Shop_Info
    {

		
        

        /// <summary>
        /// 店铺ID
        /// </summary>  
		
		[DisplayName("店铺ID")]
		


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
        /// 创建时间
        /// </summary>  
		
		[DisplayName("创建时间")]
		


		public DateTime  CreateTime { get; set; }
		      
       
        

        /// <summary>
        /// 是否激活
        /// </summary>  
		
		[DisplayName("是否激活")]
		


		public bool  IsActivation { get; set; }
		      
       
        

        /// <summary>
        /// 激活时间
        /// </summary>  
		
		[DisplayName("激活时间")]
		


		public DateTime?  ActivationTime { get; set; }
		      
       
        

        /// <summary>
        /// 店铺名称
        /// </summary>  
		
		[DisplayName("店铺名称")]
		

        [MaxLength(50,ErrorMessage="店铺名称最大长度为50")]


		public string  ShopName { get; set; }
		      
       
        

        /// <summary>
        /// 店铺经营分类
        /// </summary>  
		
		[DisplayName("店铺经营分类")]
		


		public int  ShopClass { get; set; }
		      
       
        

        /// <summary>
        /// 电话
        /// </summary>  
		
		[DisplayName("电话")]
		

        [MaxLength(50,ErrorMessage="电话最大长度为50")]


		public string  Tel { get; set; }
		      
       
        

        /// <summary>
        /// 店铺地址
        /// </summary>  
		
		[DisplayName("店铺地址")]
		


		public string  Address { get; set; }
		      
       
        

        /// <summary>
        /// LOGO
        /// </summary>  
		
		[DisplayName("LOGO")]
		


		public string  LogoImg { get; set; }
		      
       
        

        /// <summary>
        /// 店铺级别
        /// </summary>  
		
		[DisplayName("店铺级别")]
		


		public int  ShopLevel { get; set; }
		      
       
        

        /// <summary>
        /// 省
        /// </summary>  
		
		[DisplayName("省")]
		

        [MaxLength(50,ErrorMessage="省最大长度为50")]


		public string  Province { get; set; }
		      
       
        

        /// <summary>
        /// 市
        /// </summary>  
		
		[DisplayName("市")]
		

        [MaxLength(50,ErrorMessage="市最大长度为50")]


		public string  City { get; set; }
		      
       
        

        /// <summary>
        /// 县
        /// </summary>  
		
		[DisplayName("县")]
		

        [MaxLength(50,ErrorMessage="县最大长度为50")]


		public string  Town { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("IsLock")]
		


		public bool  IsLock { get; set; }
		      
       
        

        /// <summary>
        /// 店铺简介
        /// </summary>  
		
		[DisplayName("店铺简介")]
		


		public string  ShopIntro { get; set; }
		      
       
        

        /// <summary>
        /// 店铺QQ1
        /// </summary>  
		
		[DisplayName("店铺QQ1")]
		

        [MaxLength(50,ErrorMessage="店铺QQ1最大长度为50")]


		public string  ShopQQ { get; set; }
		      
       
        

        /// <summary>
        /// 广告图 banner
        /// </summary>  
		
		[DisplayName("广告图 banner")]
		


		public string  BanerImg { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ReserveStr1")]
		

        [MaxLength(250,ErrorMessage="最大长度为250")]


		public string  ReserveStr1 { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ReserveStr2")]
		

        [MaxLength(250,ErrorMessage="最大长度为250")]


		public string  ReserveStr2 { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ReserveStr3")]
		

        [MaxLength(250,ErrorMessage="最大长度为250")]


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
        /// -1:审核不通过；0:申请开店；1：营业中；-2：冻结
        /// </summary>  
		
		[DisplayName("-1:审核不通过；0:申请开店；1：营业中；-2：冻结")]
		


		public int  Status { get; set; }
		      
       
        

        /// <summary>
        /// 营业资质
        /// </summary>  
		
		[DisplayName("营业资质")]
		


		public string  ShopInfoImg { get; set; }
		      
       
        

        /// <summary>
        /// 是否为线下店铺
        /// </summary>  
		
		[DisplayName("是否为线下店铺")]
		


		public bool  IsOfflineShop { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Shop_Info()
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
    public interface IShop_InfoService :IServiceBase<Shop_Info> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Shop_Info entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class Shop_InfoService :  ServiceBase<Shop_Info>,IShop_InfoService
    {


        public Shop_InfoService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Shop_Info entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
