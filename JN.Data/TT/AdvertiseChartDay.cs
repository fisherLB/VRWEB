 







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
        public DbSet<AdvertiseChartDay> AdvertiseChartDay { get; set; }
    }

	/// <summary>
    /// 点对点交易日线
    /// </summary>
	[DisplayName("点对点交易日线")]
    public partial class AdvertiseChartDay
    {

		
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ID")]
		


		[Key]
		public int  ID { get; set; }
		      
       
        

        /// <summary>
        /// 开盘价
        /// </summary>  
		
		[DisplayName("开盘价")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  OpenPrice { get; set; }
		      
       
        

        /// <summary>
        /// 收盘价
        /// </summary>  
		
		[DisplayName("收盘价")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  ClosePrice { get; set; }
		      
       
        

        /// <summary>
        /// 最高价
        /// </summary>  
		
		[DisplayName("最高价")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  HightPrice { get; set; }
		      
       
        

        /// <summary>
        /// 最低价
        /// </summary>  
		
		[DisplayName("最低价")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  LowPrice { get; set; }
		      
       
        

        /// <summary>
        /// 成交量
        /// </summary>  
		
		[DisplayName("成交量")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  Volume { get; set; }
		      
       
        

        /// <summary>
        /// 成交额
        /// </summary>  
		
		[DisplayName("成交额")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  Turnover { get; set; }
		      
       
        

        /// <summary>
        /// 币种ID号
        /// </summary>  
		
		[DisplayName("币种ID号")]
		


		public int  CurID { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("StockDate")]
		


		public DateTime  StockDate { get; set; }
		      
       
        

        /// <summary>
        /// 涨跌幅
        /// </summary>  
		
		[DisplayName("涨跌幅")]
		


		public double  UpsAndDownsScale { get; set; }
		      
       
        

        /// <summary>
        /// 涨跌额
        /// </summary>  
		
		[DisplayName("涨跌额")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  UpsAndDownsPrice { get; set; }
		      
       
        

        /// <summary>
        /// 昨日收盘价
        /// </summary>  
		
		[DisplayName("昨日收盘价")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  YesterdayClosePrice { get; set; }
		      
       
        

        /// <summary>
        /// 价格涨跌箭头
        /// </summary>  
		
		[DisplayName("价格涨跌箭头")]
		

        [MaxLength(50,ErrorMessage="价格涨跌箭头最大长度为50")]


		public string  UpOrDown { get; set; }
		      
       
        

        /// <summary>
        /// 区域ID
        /// </summary>  
		
		[DisplayName("区域ID")]
		


		public int?  RegionID { get; set; }
		      
       
        

        /// <summary>
        /// 图标
        /// </summary>  
		
		[DisplayName("图标")]
		

        [MaxLength(250,ErrorMessage="图标最大长度为250")]


		public string  CurImages { get; set; }
		      
       
        

        /// <summary>
        /// 币种名称
        /// </summary>  
		
		[DisplayName("币种名称")]
		

        [MaxLength(50,ErrorMessage="币种名称最大长度为50")]


		public string  CurName { get; set; }
		      
       
        

        /// <summary>
        /// 币种英文
        /// </summary>  
		
		[DisplayName("币种英文")]
		

        [MaxLength(50,ErrorMessage="币种英文最大长度为50")]


		public string  CurEnglish { get; set; }
		      
       
        

        /// <summary>
        /// 市值
        /// </summary>  
		
		[DisplayName("市值")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal?  TotalValue { get; set; }
		      
       
        

        /// <summary>
        /// 5日均线
        /// </summary>  
		
		[DisplayName("5日均线")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  MA5 { get; set; }
		      
       
        

        /// <summary>
        /// 10日均线
        /// </summary>  
		
		[DisplayName("10日均线")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  MA10 { get; set; }
		      
       
        

        /// <summary>
        /// 30日均线
        /// </summary>  
		
		[DisplayName("30日均线")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  MA30 { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("CreateTime")]
		


		public DateTime  CreateTime { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("TotalStock")]
		


        [Filters.DecimalPrecision(18,6)]

		public decimal  TotalStock { get; set; }
		      
       
        

        /// <summary>
        /// 主交易区ID
        /// </summary>  
		
		[DisplayName("主交易区ID")]
		


		public int  AreaID { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public AdvertiseChartDay()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 点对点交易日线业务接口
    /// </summary>
    public interface IAdvertiseChartDayService :IServiceBase<AdvertiseChartDay> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(AdvertiseChartDay entity);
	}
    /// <summary>
    /// 点对点交易日线业务类
    /// </summary>
    public class AdvertiseChartDayService :  ServiceBase<AdvertiseChartDay>,IAdvertiseChartDayService
    {


        public AdvertiseChartDayService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(AdvertiseChartDay entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
