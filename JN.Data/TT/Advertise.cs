 







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
        public DbSet<Advertise> Advertise { get; set; }
    }

	/// <summary>
    /// 广告表
    /// </summary>
	[DisplayName("广告表")]
    public partial class Advertise
    {

		
        

        /// <summary>
        /// 广告ID
        /// </summary>  
		
		[DisplayName("广告ID")]
		


		[Key]
		public int  ID { get; set; }
		      
       
        

        /// <summary>
        /// 订单号，编号
        /// </summary>  
		
		[DisplayName("订单号，编号")]
		

        [MaxLength(50,ErrorMessage="订单号，编号最大长度为50")]


		public string  OrderID { get; set; }
		      
       
        

        /// <summary>
        /// 会员ID号
        /// </summary>  
		
		[DisplayName("会员ID号")]
		


		public int  UID { get; set; }
		      
       
        

        /// <summary>
        /// 会员编号
        /// </summary>  
		
		[DisplayName("会员编号")]
		

        [MaxLength(50,ErrorMessage="会员编号最大长度为50")]


		public string  UserName { get; set; }
		      
       
        

        /// <summary>
        /// 币种ID号
        /// </summary>  
		
		[DisplayName("币种ID号")]
		


		public int  CurID { get; set; }
		      
       
        

        /// <summary>
        /// 币种名称
        /// </summary>  
		
		[DisplayName("币种名称")]
		

        [MaxLength(50,ErrorMessage="币种名称最大长度为50")]


		public string  CurName { get; set; }
		      
       
        

        /// <summary>
        /// 币种英文缩写
        /// </summary>  
		
		[DisplayName("币种英文缩写")]
		

        [MaxLength(50,ErrorMessage="币种英文缩写最大长度为50")]


		public string  CurEnSigns { get; set; }
		      
       
        

        /// <summary>
        /// 数量
        /// </summary>  
		
		[DisplayName("数量")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  Quantity { get; set; }
		      
       
        

        /// <summary>
        /// 剩余数量
        /// </summary>  
		
		[DisplayName("剩余数量")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  HaveQuantity { get; set; }
		      
       
        

        /// <summary>
        /// 手续费
        /// </summary>  
		
		[DisplayName("手续费")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  Poundage { get; set; }
		      
       
        

        /// <summary>
        /// 类型（出售0，购买1）
        /// </summary>  
		
		[DisplayName("类型（出售0，购买1）")]
		


		public int  Direction { get; set; }
		      
       
        

        /// <summary>
        /// 所在地ID
        /// </summary>  
		
		[DisplayName("所在地ID")]
		


		public int  LocationID { get; set; }
		      
       
        

        /// <summary>
        /// 地区名称
        /// </summary>  
		
		[DisplayName("地区名称")]
		

        [MaxLength(50,ErrorMessage="地区名称最大长度为50")]


		public string  LocationName { get; set; }
		      
       
        

        /// <summary>
        /// 货币ID
        /// </summary>  
		
		[DisplayName("货币ID")]
		


		public int  CoinID { get; set; }
		      
       
        

        /// <summary>
        /// 付款币种
        /// </summary>  
		
		[DisplayName("付款币种")]
		

        [MaxLength(50,ErrorMessage="付款币种最大长度为50")]


		public string  CoinName { get; set; }
		      
       
        

        /// <summary>
        /// 溢价
        /// </summary>  
		
		[DisplayName("溢价")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  Premium { get; set; }
		      
       
        

        /// <summary>
        /// 价格
        /// </summary>  
		
		[DisplayName("价格")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  Price { get; set; }
		      
       
        

        /// <summary>
        /// 最低价格
        /// </summary>  
		
		[DisplayName("最低价格")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  FloorPrice { get; set; }
		      
       
        

        /// <summary>
        /// 最低限额
        /// </summary>  
		
		[DisplayName("最低限额")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal?  MinimumLimit { get; set; }
		      
       
        

        /// <summary>
        /// 最高限额
        /// </summary>  
		
		[DisplayName("最高限额")]
		


        [Filters.DecimalPrecision(18,8)]

		public decimal  MaximumLimit { get; set; }
		      
       
        

        /// <summary>
        /// 付款方式ID
        /// </summary>  
		
		[DisplayName("付款方式ID")]
		


		public int  PaymentID { get; set; }
		      
       
        

        /// <summary>
        /// 付款方式
        /// </summary>  
		
		[DisplayName("付款方式")]
		

        [MaxLength(50,ErrorMessage="付款方式最大长度为50")]


		public string  PaymentName { get; set; }
		      
       
        

        /// <summary>
        /// 广告留言
        /// </summary>  
		
		[DisplayName("广告留言")]
		

        [MaxLength(350,ErrorMessage="广告留言最大长度为350")]


		public string  AdMessage { get; set; }
		      
       
        

        /// <summary>
        /// 是否开启安全方式（只有验证过的才能交易）
        /// </summary>  
		
		[DisplayName("是否开启安全方式（只有验证过的才能交易）")]
		


		public bool  IsEnableSecurity { get; set; }
		      
       
        

        /// <summary>
        /// 是否只允许我信任的人交易
        /// </summary>  
		
		[DisplayName("是否只允许我信任的人交易")]
		


		public bool  IsTrustOnly { get; set; }
		      
       
        

        /// <summary>
        /// 限制开始时间
        /// </summary>  
		
		[DisplayName("限制开始时间")]
		

        [MaxLength(50,ErrorMessage="限制开始时间最大长度为50")]


		public string  StartLimitedTime { get; set; }
		      
       
        

        /// <summary>
        /// 限制结束时间
        /// </summary>  
		
		[DisplayName("限制结束时间")]
		

        [MaxLength(50,ErrorMessage="限制结束时间最大长度为50")]


		public string  EndLimitedTime { get; set; }
		      
       
        

        /// <summary>
        /// 状态
        /// </summary>  
		
		[DisplayName("状态")]
		


		public int  Status { get; set; }
		      
       
        

        /// <summary>
        /// 创建时间
        /// </summary>  
		
		[DisplayName("创建时间")]
		


		public DateTime  CreateTime { get; set; }
		      
       
        

        /// <summary>
        /// 用户头像
        /// </summary>  
		
		[DisplayName("用户头像")]
		

        [MaxLength(50,ErrorMessage="用户头像最大长度为50")]


		public string  HeadFace { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Advertise()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 广告表业务接口
    /// </summary>
    public interface IAdvertiseService :IServiceBase<Advertise> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Advertise entity);
	}
    /// <summary>
    /// 广告表业务类
    /// </summary>
    public class AdvertiseService :  ServiceBase<Advertise>,IAdvertiseService
    {


        public AdvertiseService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Advertise entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
