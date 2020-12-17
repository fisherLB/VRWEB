 


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
using System.Data.SqlClient;
using System.Data;
namespace JN.Data
{
    public partial class SysDbContext : FrameworkContext
    {
        /// <summary>
        /// 把实体添加到EF上下文
        /// </summary>
        public DbSet<WalletLog> WalletLog { get; set; }
    }

	/// <summary>
    /// 帐户明细表
    /// </summary>
	[DisplayName("帐户明细表")]
    public partial class WalletLog
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
		        [MaxLength(20,ErrorMessage="用户名最大长度为20")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 币种ID
        /// </summary>  
				[DisplayName("币种ID")]
				public int  CoinID { get; set; }
		      
       
        
        /// <summary>
        /// 币种名称
        /// </summary>  
				[DisplayName("币种名称")]
		        [MaxLength(10,ErrorMessage="币种名称最大长度为10")]
		public string  CoinName { get; set; }
		      
       
        
        /// <summary>
        /// 变更金额
        /// </summary>  
				[DisplayName("变更金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  ChangeMoney { get; set; }
		      
       
        
        /// <summary>
        /// 余额
        /// </summary>  
				[DisplayName("余额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Balance { get; set; }
		      
       
        
        /// <summary>
        /// 描述
        /// </summary>  
				[DisplayName("描述")]
		        [MaxLength(150,ErrorMessage="描述最大长度为150")]
		public string  Description { get; set; }
		      
       
        
        /// <summary>
        /// 变更时间
        /// </summary>  
				[DisplayName("变更时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public WalletLog()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 帐户明细表业务接口
    /// </summary>
    public interface IWalletLogService :IServiceBase<WalletLog> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(WalletLog entity);
        void BulkInsert<T>(IList<T> list, string conn = null, string tableName = null);
    }
    /// <summary>
    /// 帐户明细表业务类
    /// </summary>
    public class WalletLogService :  ServiceBase<WalletLog>,IWalletLogService
    {


        public WalletLogService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(WalletLog entity)
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
 
