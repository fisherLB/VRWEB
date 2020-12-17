 







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
        public DbSet<UserBankCard> UserBankCard { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class UserBankCard
    {

		
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("ID")]
		


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
        /// 银行名称
        /// </summary>  
		
		[DisplayName("银行名称")]
		

        [MaxLength(50,ErrorMessage="银行名称最大长度为50")]


		public string  BankName { get; set; }
		      
       
        

        /// <summary>
        /// 银行名称ID号
        /// </summary>  
		
		[DisplayName("银行名称ID号")]
		


		public int  BankNameID { get; set; }
		      
       
        

        /// <summary>
        /// 银行卡号
        /// </summary>  
		
		[DisplayName("银行卡号")]
		

        [MaxLength(50,ErrorMessage="银行卡号最大长度为50")]


		public string  BankCard { get; set; }
		      
       
        

        /// <summary>
        /// 开户行
        /// </summary>  
		
		[DisplayName("开户行")]
		

        [MaxLength(50,ErrorMessage="开户行最大长度为50")]


		public string  BankOfDeposit { get; set; }
		      
       
        

        /// <summary>
        /// 银行地址
        /// </summary>  
		
		[DisplayName("银行地址")]
		

        [MaxLength(150,ErrorMessage="银行地址最大长度为150")]


		public string  BankAddress { get; set; }
		      
       
        

        /// <summary>
        /// 银行户名
        /// </summary>  
		
		[DisplayName("银行户名")]
		

        [MaxLength(50,ErrorMessage="银行户名最大长度为50")]


		public string  BankUser { get; set; }
		      
       
        

        /// <summary>
        /// 
        /// </summary>  
		
		[DisplayName("CreateTime")]
		


		public DateTime  CreateTime { get; set; }
		      
       
        

        /// <summary>
        /// 是否默认
        /// </summary>  
		
		[DisplayName("是否默认")]
		


		public bool  IsDefault { get; set; }
		      
       
        

        /// <summary>
        /// 银行图标
        /// </summary>  
		
		[DisplayName("银行图标")]
		

        [MaxLength(80,ErrorMessage="银行图标最大长度为80")]


		public string  BankIcon { get; set; }
		      
       
        

        /// <summary>
        /// 付款信息图片
        /// </summary>  
		
		[DisplayName("付款信息图片")]
		

        [MaxLength(250,ErrorMessage="付款信息图片最大长度为250")]


		public string  BankImgUrl { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public UserBankCard()
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
    public interface IUserBankCardService :IServiceBase<UserBankCard> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(UserBankCard entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class UserBankCardService :  ServiceBase<UserBankCard>,IUserBankCardService
    {


        public UserBankCardService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(UserBankCard entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
