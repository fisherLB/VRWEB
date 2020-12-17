


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
using System.Data;
using System.Data.SqlClient;

namespace JN.Data
{
    public partial class SysDbContext : FrameworkContext
    {
        /// <summary>
        /// 把实体添加到EF上下文
        /// </summary>
        public DbSet<BonusDetail> BonusDetail { get; set; }
    }

	/// <summary>
    /// 奖金明细表
    /// </summary>
	[DisplayName("奖金明细表")]
    public partial class BonusDetail
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 奖金来自用户ID
        /// </summary>  
				[DisplayName("奖金来自用户ID")]
				public int?  FromUID { get; set; }
		      
       
        
        /// <summary>
        /// 奖金来自用户名
        /// </summary>  
				[DisplayName("奖金来自用户名")]
		        [MaxLength(50,ErrorMessage="奖金来自用户名最大长度为50")]
		public string  FromUserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CoinID")]
				public int?  CoinID { get; set; }
		      
       
        
        /// <summary>
        /// 奖金ID
        /// </summary>  
				[DisplayName("奖金ID")]
				public int  BonusID { get; set; }
		      
       
        
        /// <summary>
        /// 奖金名称
        /// </summary>  
				[DisplayName("奖金名称")]
		        [MaxLength(50,ErrorMessage="奖金名称最大长度为50")]
		public string  BonusName { get; set; }
		      
       
        
        /// <summary>
        /// 奖金金额
        /// </summary>  
				[DisplayName("奖金金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  BonusMoney { get; set; }
		      
       
        
        /// <summary>
        /// 描述
        /// </summary>  
				[DisplayName("描述")]
		        [MaxLength(250,ErrorMessage="描述最大长度为250")]
		public string  Description { get; set; }
		      
       
        
        /// <summary>
        /// 是否结算
        /// </summary>  
				[DisplayName("是否结算")]
				public bool  IsBalance { get; set; }
		      
       
        
        /// <summary>
        /// 结算时间
        /// </summary>  
				[DisplayName("结算时间")]
				public DateTime  BalanceTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 理财奖金
        /// </summary>  
				[DisplayName("理财奖金")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  BonusMoneyCF { get; set; }
		      
       
        
        /// <summary>
        /// 理财奖金可路转余额
        /// </summary>  
				[DisplayName("理财奖金可路转余额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  BonusMoneyCFBA { get; set; }
		      
       
        
        /// <summary>
        /// 结算期数
        /// </summary>  
				[DisplayName("结算期数")]
				public int  Period { get; set; }
		      
       
        
        /// <summary>
        /// 提供帮助单号
        /// </summary>  
				[DisplayName("提供帮助单号")]
		        [MaxLength(50,ErrorMessage="提供帮助单号最大长度为50")]
		public string  SupplyNo { get; set; }
		      
       
        
        /// <summary>
        /// 是否解冻
        /// </summary>  
				[DisplayName("是否解冻")]
				public bool?  IsEffective { get; set; }
		      
       
        
        /// <summary>
        /// 解冻时间
        /// </summary>  
				[DisplayName("解冻时间")]
				public DateTime?  EffectiveTime { get; set; }
		      
       
        
        /// <summary>
        /// 匹配单号
        /// </summary>  
				[DisplayName("匹配单号")]
		        [MaxLength(50,ErrorMessage="匹配单号最大长度为50")]
		public string  MatchNo { get; set; }
		      
       
        
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
				[DisplayName("ReserveBoolean")]
				public bool?  ReserveBoolean { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public BonusDetail()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 奖金明细表业务接口
    /// </summary>
    public interface IBonusDetailService :IServiceBase<BonusDetail> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(BonusDetail entity);

        void BulkInsert<T>(IList<T> list, string conn = null, string tableName = null);
    }
    /// <summary>
    /// 奖金明细表业务类
    /// </summary>
    public class BonusDetailService :  ServiceBase<BonusDetail>,IBonusDetailService
    {


        public BonusDetailService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(BonusDetail entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }

        /// <summary>  
        /// 批量插入  
        /// </summary>
        /// <param name="list">要插入大泛型集合</param>  
        /// <typeparam name="T">泛型集合的类型</typeparam>  
        /// <param name="conn">连接对象</param>  
        /// <param name="tableName">将泛型集合插入到本地数据库表的表名</param>         
        public void BulkInsert<T>(IList<T> list, string conn = null, string tableName = null)
        {

            if (conn == null)
            {
                conn = this.DataContext.Database.Connection.ConnectionString;
            }

            if (tableName == null)
            {
                tableName = typeof(T).Name;
            }
            using (var bulkCopy = new SqlBulkCopy(conn))
            {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof(T))

                    .Cast<PropertyDescriptor>()
                    .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                    .ToArray();

                foreach (var propertyInfo in props)
                {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }
    }

}   
 