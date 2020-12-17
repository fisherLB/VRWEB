 


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
        public DbSet<MachineOrder> MachineOrder { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class MachineOrder
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 订单编号
        /// </summary>  
				[DisplayName("订单编号")]
		        [MaxLength(50,ErrorMessage="订单编号最大长度为50")]
		public string  InvestmentNo { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("InvestmentType")]
				public int  InvestmentType { get; set; }
		      
       
        
        /// <summary>
        /// 订单用户ID
        /// </summary>  
				[DisplayName("订单用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 订单用户名
        /// </summary>  
				[DisplayName("订单用户名")]
		        [MaxLength(50,ErrorMessage="订单用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 订单总额
        /// </summary>  
				[DisplayName("订单总额")]
		        [Filters.DecimalPrecision(18,5)]
		public decimal  ApplyInvestment { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SettlementMoney")]
		        [Filters.DecimalPrecision(18,5)]
		public decimal  SettlementMoney { get; set; }
		      
       
        
        /// <summary>
        /// 订单数量
        /// </summary>  
				[DisplayName("订单数量")]
				public int  BuyNum { get; set; }
		      
       
        
        /// <summary>
        /// 是否可分红
        /// </summary>  
				[DisplayName("是否可分红")]
				public bool  IsBalance { get; set; }
		      
       
        
        /// <summary>
        /// 算力ID 
        /// </summary>  
				[DisplayName("算力ID ")]
				public int  HashID { get; set; }
		      
       
        
        /// <summary>
        /// 算力名称
        /// </summary>  
				[DisplayName("算力名称")]
		        [MaxLength(250,ErrorMessage="算力名称最大长度为250")]
		public string  HashnestName { get; set; }
		      
       
        
        /// <summary>
        /// 算力数量
        /// </summary>  
				[DisplayName("算力数量")]
				public int  HashnestNum { get; set; }
		      
       
        
        /// <summary>
        /// 订单状态
        /// </summary>  
				[DisplayName("订单状态")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 备注
        /// </summary>  
				[DisplayName("备注")]
		        [MaxLength(250,ErrorMessage="备注最大长度为250")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 累计收益
        /// </summary>  
				[DisplayName("累计收益")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  AddupInterest { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ShopID")]
				public int?  ShopID { get; set; }
		      
       
        
        /// <summary>
        /// 矿机ID
        /// </summary>  
				[DisplayName("矿机ID")]
				public int  ProductID { get; set; }
		      
       
        
        /// <summary>
        /// 矿机名称
        /// </summary>  
				[DisplayName("矿机名称")]
		        [MaxLength(50,ErrorMessage="矿机名称最大长度为50")]
		public string  ProductName { get; set; }
		      
       
        
        /// <summary>
        /// 结算方式
        /// </summary>  
				[DisplayName("结算方式")]
		        [MaxLength(50,ErrorMessage="结算方式最大长度为50")]
		public string  PayWay { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 待提取收益
        /// </summary>  
				[DisplayName("待提取收益")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  WaitExtractIncome { get; set; }
		      
       
        
        /// <summary>
        /// 最后收益时间
        /// </summary>  
				[DisplayName("最后收益时间")]
				public DateTime?  LastProfitTime { get; set; }
		      
       
        
        /// <summary>
        /// 图片路径
        /// </summary>  
				[DisplayName("图片路径")]
		        [MaxLength(250,ErrorMessage="图片路径最大长度为250")]
		public string  ImageUrl { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr1")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr2")]
		        [MaxLength(500,ErrorMessage="最大长度为500")]
		public string  ReserveStr2 { get; set; }
		      
       
        
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
				[DisplayName("ReserveDate1")]
				public DateTime?  ReserveDate1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDate2")]
				public DateTime?  ReserveDate2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveBoolean1")]
				public bool?  ReserveBoolean1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveBoolean2")]
				public bool?  ReserveBoolean2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal1")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal2")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal2 { get; set; }
		      
       
        
        /// <summary>
        /// 每天/时/分钟收益
        /// </summary>  
				[DisplayName("每天/时/分钟收益")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  TimesType { get; set; }
		      
       
        
        /// <summary>
        /// 封顶收益
        /// </summary>  
				[DisplayName("封顶收益")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TopBonus { get; set; }
		      
       
        
        /// <summary>
        /// 今日产矿
        /// </summary>  
				[DisplayName("今日产矿")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  TodaySettlement { get; set; }
		      
       
        
        /// <summary>
        /// 收益时长
        /// </summary>  
				[DisplayName("收益时长")]
				public int  Duration { get; set; }
		      
       
        
        /// <summary>
        /// 今日产矿时长
        /// </summary>  
				[DisplayName("今日产矿时长")]
				public int  TodayMiningTime { get; set; }



        /// <summary>
        /// 支付金额
        /// </summary>  
        [DisplayName("支付金额")]
        [Filters.DecimalPrecision(18, 8)]
        public decimal PayMoney { get; set; }



        /// <summary>
        /// 激活次数
        /// </summary>  
        [DisplayName("激活次数")]
        public int ActivationNums { get; set; }



        /// <summary>
        /// 激活时间
        /// </summary>  
        [DisplayName("激活时间")]
        public DateTime? ActivationTime { get; set; }




        /// <summary>
        /// 构造函数
        /// </summary>

        public MachineOrder()
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
    public interface IMachineOrderService :IServiceBase<MachineOrder> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(MachineOrder entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class MachineOrderService :  ServiceBase<MachineOrder>,IMachineOrderService
    {


        public MachineOrderService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(MachineOrder entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
