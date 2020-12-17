 


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
        public DbSet<Chating> Chating { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Chating
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 订单号
        /// </summary>  
				[DisplayName("订单号")]
		        [MaxLength(50,ErrorMessage="订单号最大长度为50")]
		public string  OrderNo { get; set; }
		      
       
        
        /// <summary>
        /// 订单号id
        /// </summary>  
				[DisplayName("订单号id")]
				public int  AdOrderID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MsgContent")]
		        [MaxLength(350,ErrorMessage="最大长度为350")]
		public string  MsgContent { get; set; }
		      
       
        
        /// <summary>
        /// 发送者UID
        /// </summary>  
				[DisplayName("发送者UID")]
				public int  SendUID { get; set; }
		      
       
        
        /// <summary>
        /// 发送者编号
        /// </summary>  
				[DisplayName("发送者编号")]
		        [MaxLength(50,ErrorMessage="发送者编号最大长度为50")]
		public string  SendUserName { get; set; }
		      
       
        
        /// <summary>
        /// 发送者头像
        /// </summary>  
				[DisplayName("发送者头像")]
		        [MaxLength(250,ErrorMessage="发送者头像最大长度为250")]
		public string  SendFace { get; set; }
		      
       
        
        /// <summary>
        /// 接收者id号
        /// </summary>  
				[DisplayName("接收者id号")]
				public int  RecUID { get; set; }
		      
       
        
        /// <summary>
        /// 接收者编号
        /// </summary>  
				[DisplayName("接收者编号")]
		        [MaxLength(50,ErrorMessage="接收者编号最大长度为50")]
		public string  RecUserName { get; set; }
		      
       
        
        /// <summary>
        /// 接收者头像
        /// </summary>  
				[DisplayName("接收者头像")]
		        [MaxLength(250,ErrorMessage="接收者头像最大长度为250")]
		public string  RecFace { get; set; }
		      
       
        
        /// <summary>
        /// 附件
        /// </summary>  
				[DisplayName("附件")]
		        [MaxLength(350,ErrorMessage="附件最大长度为350")]
		public string  Attachment { get; set; }
		      
       
        
        /// <summary>
        /// 聊天类型
        /// </summary>  
				[DisplayName("聊天类型")]
		        [MaxLength(50,ErrorMessage="聊天类型最大长度为50")]
		public string  MsgType { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Chating()
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
    public interface IChatingService :IServiceBase<Chating> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Chating entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class ChatingService :  ServiceBase<Chating>,IChatingService
    {


        public ChatingService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Chating entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
