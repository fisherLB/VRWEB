 


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
        public DbSet<User> User { get; set; }
    }

	/// <summary>
    /// 会员表
    /// </summary>
	[DisplayName("会员表")]
    public partial class User
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 主帐号名称
        /// </summary>  
				[DisplayName("主帐号名称")]
				public int?  MainAccountID { get; set; }
		      
       
        
        /// <summary>
        /// 生成子帐号的的根主帐号
        /// </summary>  
				[DisplayName("生成子帐号的的根主帐号")]
				public int?  RootAccountID { get; set; }
		      
       
        
        /// <summary>
        /// 是否子帐号
        /// </summary>  
				[DisplayName("是否子帐号")]
				public bool?  IsSubAccount { get; set; }
		      
       
        
        /// <summary>
        /// 安置人ID
        /// </summary>  
				[DisplayName("安置人ID")]
				public int  ParentID { get; set; }
		      
       
        
        /// <summary>
        /// 安置用户名
        /// </summary>  
				[DisplayName("安置用户名")]
		        [MaxLength(50,ErrorMessage="安置用户名最大长度为50")]
		public string  ParentUser { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系路径
        /// </summary>  
				[DisplayName("安置关系路径")]
				public string  ParentPath { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系层
        /// </summary>  
				[DisplayName("安置关系层")]
				public int  Depth { get; set; }
		      
       
        
        /// <summary>
        /// 安置节点数
        /// </summary>  
				[DisplayName("安置节点数")]
				public int  Child { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系根ID
        /// </summary>  
				[DisplayName("安置关系根ID")]
				public int  RootID { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系层排位
        /// </summary>  
				[DisplayName("安置关系层排位")]
				public int  DepthSort { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系节点排位
        /// </summary>  
				[DisplayName("安置关系节点排位")]
				public int  ChildPlace { get; set; }
		      
       
        
        /// <summary>
        /// 推荐人ID
        /// </summary>  
				[DisplayName("推荐人ID")]
				public int  RefereeID { get; set; }
		      
       
        
        /// <summary>
        /// 推荐人
        /// </summary>  
				[DisplayName("推荐人")]
		        [MaxLength(50,ErrorMessage="推荐人最大长度为50")]
		public string  RefereeUser { get; set; }
		      
       
        
        /// <summary>
        /// 推荐关系路径
        /// </summary>  
				[DisplayName("推荐关系路径")]
				public string  RefereePath { get; set; }
		      
       
        
        /// <summary>
        /// 推荐关系层
        /// </summary>  
				[DisplayName("推荐关系层")]
				public int  RefereeDepth { get; set; }
		      
       
        
        /// <summary>
        /// 所属商务中心ID
        /// </summary>  
				[DisplayName("所属商务中心ID")]
				public int?  AgentID { get; set; }
		      
       
        
        /// <summary>
        /// 所属商务中心
        /// </summary>  
				[DisplayName("所属商务中心")]
		        [MaxLength(50,ErrorMessage="所属商务中心最大长度为50")]
		public string  AgentUser { get; set; }
		      
       
        
        /// <summary>
        /// 商务中心推荐人
        /// </summary>  
				[DisplayName("商务中心推荐人")]
		        [MaxLength(50,ErrorMessage="商务中心推荐人最大长度为50")]
		public string  AgentRefereeUser { get; set; }
		      
       
        
        /// <summary>
        /// 登录密码
        /// </summary>  
				[DisplayName("登录密码")]
		        [MaxLength(50,ErrorMessage="登录密码最大长度为50")]
		public string  Password { get; set; }
		      
       
        
        /// <summary>
        /// 二级密码
        /// </summary>  
				[DisplayName("二级密码")]
		        [MaxLength(50,ErrorMessage="二级密码最大长度为50")]
		public string  Password2 { get; set; }
		      
       
        
        /// <summary>
        /// 三级密码
        /// </summary>  
				[DisplayName("三级密码")]
		        [MaxLength(50,ErrorMessage="三级密码最大长度为50")]
		public string  Password3 { get; set; }
		      
       
        
        /// <summary>
        /// 昵称
        /// </summary>  
				[DisplayName("昵称")]
		        [MaxLength(20,ErrorMessage="昵称最大长度为20")]
		public string  NickName { get; set; }
		      
       
        
        /// <summary>
        /// 真实姓名
        /// </summary>  
				[DisplayName("真实姓名")]
		        [MaxLength(50,ErrorMessage="真实姓名最大长度为50")]
		public string  RealName { get; set; }
		      
       
        
        /// <summary>
        /// 性别
        /// </summary>  
				[DisplayName("性别")]
		        [MaxLength(2,ErrorMessage="性别最大长度为2")]
		public string  Sex { get; set; }
		      
       
        
        /// <summary>
        /// 生日
        /// </summary>  
				[DisplayName("生日")]
				public DateTime?  Birthday { get; set; }
		      
       
        
        /// <summary>
        /// 邮箱
        /// </summary>  
				[DisplayName("邮箱")]
		        [MaxLength(30,ErrorMessage="邮箱最大长度为30")]
		public string  Email { get; set; }
		      
       
        
        /// <summary>
        /// 是否邮箱验证
        /// </summary>  
				[DisplayName("是否邮箱验证")]
				public bool?  IsEmail { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("QQ")]
		        [MaxLength(20,ErrorMessage="最大长度为20")]
		public string  QQ { get; set; }
		      
       
        
        /// <summary>
        /// 银行名称
        /// </summary>  
				[DisplayName("银行名称")]
		        [MaxLength(50,ErrorMessage="银行名称最大长度为50")]
		public string  BankName { get; set; }
		      
       
        
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
        /// 银行户名
        /// </summary>  
				[DisplayName("银行户名")]
		        [MaxLength(50,ErrorMessage="银行户名最大长度为50")]
		public string  BankUser { get; set; }
		      
       
        
        /// <summary>
        /// 国家
        /// </summary>  
				[DisplayName("国家")]
		        [MaxLength(50,ErrorMessage="国家最大长度为50")]
		public string  Country { get; set; }
		      
       
        
        /// <summary>
        /// 省份
        /// </summary>  
				[DisplayName("省份")]
		        [MaxLength(50,ErrorMessage="省份最大长度为50")]
		public string  Province { get; set; }
		      
       
        
        /// <summary>
        /// 城市
        /// </summary>  
				[DisplayName("城市")]
		        [MaxLength(50,ErrorMessage="城市最大长度为50")]
		public string  City { get; set; }
		      
       
        
        /// <summary>
        /// 区县
        /// </summary>  
				[DisplayName("区县")]
		        [MaxLength(50,ErrorMessage="区县最大长度为50")]
		public string  County { get; set; }
		      
       
        
        /// <summary>
        /// 地址
        /// </summary>  
				[DisplayName("地址")]
		        [MaxLength(50,ErrorMessage="地址最大长度为50")]
		public string  Address { get; set; }
		      
       
        
        /// <summary>
        /// 手机
        /// </summary>  
				[DisplayName("手机")]
		        [MaxLength(50,ErrorMessage="手机最大长度为50")]
		public string  Mobile { get; set; }
		      
       
        
        /// <summary>
        /// 是否手机验证
        /// </summary>  
				[DisplayName("是否手机验证")]
				public bool?  IsMobile { get; set; }
		      
       
        
        /// <summary>
        /// 电话
        /// </summary>  
				[DisplayName("电话")]
		        [MaxLength(50,ErrorMessage="电话最大长度为50")]
		public string  Tel { get; set; }
		      
       
        
        /// <summary>
        /// 头像
        /// </summary>  
				[DisplayName("头像")]
		        [MaxLength(250,ErrorMessage="头像最大长度为250")]
		public string  HeadFace { get; set; }
		      
       
        
        /// <summary>
        /// 是否激活
        /// </summary>  
				[DisplayName("是否激活")]
				public bool  IsActivation { get; set; }
		      
       
        
        /// <summary>
        /// 是否锁定
        /// </summary>  
				[DisplayName("是否锁定")]
				public bool  IsLock { get; set; }
		      
       
        
        /// <summary>
        /// 是否代理商（商务中心）
        /// </summary>  
				[DisplayName("是否代理商（商务中心）")]
				public bool?  IsAgent { get; set; }
		      
       
        
        /// <summary>
        /// 商务中心名称
        /// </summary>  
				[DisplayName("商务中心名称")]
		        [MaxLength(50,ErrorMessage="商务中心名称最大长度为50")]
		public string  AgentName { get; set; }
		      
       
        
        /// <summary>
        /// 申请商务中心备注
        /// </summary>  
				[DisplayName("申请商务中心备注")]
		        [MaxLength(250,ErrorMessage="申请商务中心备注最大长度为250")]
		public string  ApplyAgentRemark { get; set; }
		      
       
        
        /// <summary>
        /// 申请商务中心时间
        /// </summary>  
				[DisplayName("申请商务中心时间")]
				public DateTime?  ApplyAgentTime { get; set; }
		      
       
        
        /// <summary>
        /// 身份证号码
        /// </summary>  
				[DisplayName("身份证号码")]
		        [MaxLength(50,ErrorMessage="身份证号码最大长度为50")]
		public string  IDCard { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IDCardPic1")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  IDCardPic1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IDCardPic2")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  IDCardPic2 { get; set; }
		      
       
        
        /// <summary>
        /// 会员申请认证？
        /// </summary>  
				[DisplayName("会员申请认证？")]
				public int?  AuthenticationStatus { get; set; }
		      
       
        
        /// <summary>
        /// 是否实名认证
        /// </summary>  
				[DisplayName("是否实名认证")]
				public bool?  IsAuthentication { get; set; }
		      
       
        
        /// <summary>
        /// 实名认证时间
        /// </summary>  
				[DisplayName("实名认证时间")]
				public DateTime?  AuthenticationTime { get; set; }
		      
       
        
        /// <summary>
        /// 未认证原因
        /// </summary>  
				[DisplayName("未认证原因")]
		        [MaxLength(250,ErrorMessage="未认证原因最大长度为250")]
		public string  NoAuthenticationReason { get; set; }
		      
       
        
        /// <summary>
        /// 拒绝实名认证时间
        /// </summary>  
				[DisplayName("拒绝实名认证时间")]
				public DateTime?  NoAuthenticationTime { get; set; }
		      
       
        
        /// <summary>
        /// 密保问题
        /// </summary>  
				[DisplayName("密保问题")]
		        [MaxLength(50,ErrorMessage="密保问题最大长度为50")]
		public string  Question { get; set; }
		      
       
        
        /// <summary>
        /// 密保答案
        /// </summary>  
				[DisplayName("密保答案")]
		        [MaxLength(50,ErrorMessage="密保答案最大长度为50")]
		public string  Answer { get; set; }
		      
       
        
        /// <summary>
        /// 激活时间
        /// </summary>  
				[DisplayName("激活时间")]
				public DateTime?  ActivationTime { get; set; }
		      
       
        
        /// <summary>
        /// 冻结原因
        /// </summary>  
				[DisplayName("冻结原因")]
		        [MaxLength(250,ErrorMessage="冻结原因最大长度为250")]
		public string  LockReason { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LockTime")]
				public DateTime?  LockTime { get; set; }
		      
       
        
        /// <summary>
        /// 用户级别
        /// </summary>  
				[DisplayName("用户级别")]
				public int  UserLevel { get; set; }
		      
       
        
        /// <summary>
        /// 注册时间
        /// </summary>  
				[DisplayName("注册时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 登录错误次数
        /// </summary>  
				[DisplayName("登录错误次数")]
				public int  InputWrongPwdTimes { get; set; }
		      
       
        
        /// <summary>
        /// 最后登录IP
        /// </summary>  
				[DisplayName("最后登录IP")]
		        [MaxLength(50,ErrorMessage="最后登录IP最大长度为50")]
		public string  LastLoginIP { get; set; }
		      
       
        
        /// <summary>
        /// 最后登录时间
        /// </summary>  
				[DisplayName("最后登录时间")]
				public DateTime?  LastLoginTime { get; set; }
		      
       
        
        /// <summary>
        /// 最后修改时间
        /// </summary>  
				[DisplayName("最后修改时间")]
				public DateTime?  LastUpdateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Wallet2001")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Wallet2001 { get; set; }
		      
       
        
        /// <summary>
        /// 现金币
        /// </summary>  
				[DisplayName("现金币")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Wallet2002 { get; set; }
		      
       
        
        /// <summary>
        /// 虚拟币
        /// </summary>  
				[DisplayName("虚拟币")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Wallet2003 { get; set; }
		      
       
        
        /// <summary>
        /// 拆分币
        /// </summary>  
				[DisplayName("拆分币")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Wallet2004 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Wallet2005")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Wallet2005 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Wallet2006")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Wallet2006 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金1
        /// </summary>  
				[DisplayName("累计奖金1")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1101 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金2
        /// </summary>  
				[DisplayName("累计奖金2")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1102 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金3
        /// </summary>  
				[DisplayName("累计奖金3")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1103 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金4
        /// </summary>  
				[DisplayName("累计奖金4")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1104 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金5
        /// </summary>  
				[DisplayName("累计奖金5")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1105 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金6
        /// </summary>  
				[DisplayName("累计奖金6")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1106 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金7
        /// </summary>  
				[DisplayName("累计奖金7")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1107 { get; set; }



        /// <summary>
        /// 累计奖金8
        /// </summary>  
        [DisplayName("累计奖金8")]
        [Filters.DecimalPrecision(18, 8)]
        public decimal Addup1108 { get; set; }



        /// <summary>
        /// 累计奖金9
        /// </summary>  
        [DisplayName("累计奖金9")]
        [Filters.DecimalPrecision(18, 8)]
        public decimal Addup1109 { get; set; }



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("Addup1802")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Addup1802 { get; set; }
		      
       
        
        /// <summary>
        /// 个人业绩（累计提供订单的业绩）
        /// </summary>  
				[DisplayName("个人业绩（累计提供订单的业绩）")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  AddupSupplyAmount { get; set; }
		      
       
        
        /// <summary>
        /// 币种1
        /// </summary>  
				[DisplayName("币种1")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3001 { get; set; }
		      
       
        
        /// <summary>
        /// 币种2
        /// </summary>  
				[DisplayName("币种2")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3002 { get; set; }
		      
       
        
        /// <summary>
        /// 币种3
        /// </summary>  
				[DisplayName("币种3")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3003 { get; set; }
		      
       
        
        /// <summary>
        /// 币种4
        /// </summary>  
				[DisplayName("币种4")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3004 { get; set; }
		      
       
        
        /// <summary>
        /// 币种5
        /// </summary>  
				[DisplayName("币种5")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3005 { get; set; }
		      
       
        
        /// <summary>
        /// 币种6
        /// </summary>  
				[DisplayName("币种6")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3006 { get; set; }
		      
       
        
        /// <summary>
        /// 币种7
        /// </summary>  
				[DisplayName("币种7")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3007 { get; set; }
		      
       
        
        /// <summary>
        /// 币种8
        /// </summary>  
				[DisplayName("币种8")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3008 { get; set; }
		      
       
        
        /// <summary>
        /// 币种9
        /// </summary>  
				[DisplayName("币种9")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3009 { get; set; }
		      
       
        
        /// <summary>
        /// 币种10
        /// </summary>  
				[DisplayName("币种10")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3010 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包11
        /// </summary>  
				[DisplayName("钱包11")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3011 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包12
        /// </summary>  
				[DisplayName("钱包12")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3012 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包13
        /// </summary>  
				[DisplayName("钱包13")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3013 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包14
        /// </summary>  
				[DisplayName("钱包14")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3014 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包15
        /// </summary>  
				[DisplayName("钱包15")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Cur3015 { get; set; }
		      
       
        
        /// <summary>
        /// 冻结钱包3001
        /// </summary>  
				[DisplayName("冻结钱包3001")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3001 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3002")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3002 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3003")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3003 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3004")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3004 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3005")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3005 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3006")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3006 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3007")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3007 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3008")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3008 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3009")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3009 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3010")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3010 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3011")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3011 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3012")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3012 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3013")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3013 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3014")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3014 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurFro3015")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  CurFro3015 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3001 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3002 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3003 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3004 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3005 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3006 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3007 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3008 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3009 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3010 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3011 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3012 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3013 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3014 { get; set; }
		      
       
        
        /// <summary>
        /// 钱包地址
        /// </summary>  
				[DisplayName("钱包地址")]
		        [MaxLength(50,ErrorMessage="钱包地址最大长度为50")]
		public string  R3015 { get; set; }
		      
       
        
        /// <summary>
        /// 累计提现
        /// </summary>  
				[DisplayName("累计提现")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  AddupTakeCash { get; set; }
		      
       
        
        /// <summary>
        /// 投资金额
        /// </summary>  
				[DisplayName("投资金额")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal  Investment { get; set; }



        /// <summary>
        /// 团队业绩
        /// </summary>  
        [DisplayName("团队业绩")]
        [Filters.DecimalPrecision(18, 8)]
        public decimal TeamInvestment { get; set; }



        /// <summary>
        /// 已分红期数
        /// </summary>  
        [DisplayName("已分红期数")]
				public int?  BounsPeriod { get; set; }
		      
       
        
        /// <summary>
        /// 是否可分红
        /// </summary>  
				[DisplayName("是否可分红")]
				public bool?  IsShareBouns { get; set; }
		      
       
        
        /// <summary>
        /// 分红结束原因
        /// </summary>  
				[DisplayName("分红结束原因")]
		        [MaxLength(50,ErrorMessage="分红结束原因最大长度为50")]
		public string  ShareBounsStopReason { get; set; }
		      
       
        
        /// <summary>
        /// 分红结束时间
        /// </summary>  
				[DisplayName("分红结束时间")]
				public DateTime?  ShareBounsStopTime { get; set; }
		      
       
        
        /// <summary>
        /// 左对碰余量
        /// </summary>  
				[DisplayName("左对碰余量")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  LeftDpMargin { get; set; }
		      
       
        
        /// <summary>
        /// 右对碰余量
        /// </summary>  
				[DisplayName("右对碰余量")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  RightDpMargin { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LeftAchievement")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  LeftAchievement { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RightAchievement")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  RightAchievement { get; set; }
		      
       
        
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
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  ReserveDecamal1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal2")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  ReserveDecamal2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AgentLevel")]
				public int?  AgentLevel { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateBy")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  CreateBy { get; set; }
		      
       
        
        /// <summary>
        /// 钱包锁
        /// </summary>  
				[DisplayName("钱包锁")]
				public bool?  WalletLock { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsShop")]
				public bool?  IsShop { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ShopLevel")]
				public int?  ShopLevel { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ProductID")]
				public int?  ProductID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ProductName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ProductName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AliPay")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  AliPay { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RefereeCount")]
				public int  RefereeCount { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Racoin")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Racoin { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Bitcoin")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Bitcoin { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("WeiXin")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  WeiXin { get; set; }
		      
       
        
        /// <summary>
        /// 好评
        /// </summary>  
				[DisplayName("好评")]
				public int?  Positive { get; set; }
		      
       
        
        /// <summary>
        /// 中评
        /// </summary>  
				[DisplayName("中评")]
				public int?  Neutral { get; set; }
		      
       
        
        /// <summary>
        /// 差评
        /// </summary>  
				[DisplayName("差评")]
				public int?  Negative { get; set; }
		      
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IDCardPic3")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  IDCardPic3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ApplyAuthenticationNum")]
				public int?  ApplyAuthenticationNum { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  Poundage { get; set; }
		      
       
        
        /// <summary>
        /// 会员介绍
        /// </summary>  
				[DisplayName("会员介绍")]
		        [MaxLength(250,ErrorMessage="会员介绍最大长度为250")]
		public string  UserInfo { get; set; }
		      
       
        
        /// <summary>
        /// 信任的人
        /// </summary>  
				[DisplayName("信任的人")]
				public string  TrustPath { get; set; }
		      
       
        
        /// <summary>
        /// 屏蔽的人
        /// </summary>  
				[DisplayName("屏蔽的人")]
				public string  IgnorePath { get; set; }
		      
       
        
        /// <summary>
        /// 交易次数
        /// </summary>  
				[DisplayName("交易次数")]
				public int?  TransactionNum { get; set; }
		      
       
        
        /// <summary>
        /// 绑定状态
        /// </summary>  
				[DisplayName("绑定状态")]
				public bool  BindStatus { get; set; }
		      
       
        
        /// <summary>
        /// 绑定系统用户ID
        /// </summary>  
				[DisplayName("绑定系统用户ID")]
				public int  BindUserId { get; set; }
		      
       
        
        /// <summary>
        /// 绑定系统用户名
        /// </summary>  
				[DisplayName("绑定系统用户名")]
		        [MaxLength(50,ErrorMessage="绑定系统用户名最大长度为50")]
		public string  BindUserName { get; set; }
		      
       
        
        /// <summary>
        /// 绑定会员路径
        /// </summary>  
				[DisplayName("绑定会员路径")]
				public string  BindUserPath { get; set; }
		      
       
        
        /// <summary>
        /// 信用值
        /// </summary>  
				[DisplayName("信用值")]
				public int  CreditValue { get; set; }
		      
       
        
        /// <summary>
        /// 好评分
        /// </summary>  
				[DisplayName("好评分")]
				public int  GoodScore { get; set; }
		      
       
        
        /// <summary>
        /// 区号
        /// </summary>  
				[DisplayName("区号")]
		        [MaxLength(50,ErrorMessage="区号最大长度为50")]
		public string  CountryCode { get; set; }
		      
       
        
        /// <summary>
        /// 出售开关
        /// </summary>  
				[DisplayName("出售开关")]
				public bool?  SellSwitch { get; set; }
		      
       
        
        /// <summary>
        /// 购买开关
        /// </summary>  
				[DisplayName("购买开关")]
				public bool?  BuySwitch { get; set; }
		      
       
        
        /// <summary>
        /// 在wp18091401中只有显示作用
        /// </summary>  
				[DisplayName("在wp18091401中只有显示作用")]
				public int?  DisplayID { get; set; }
		      
       
        
        /// <summary>
        /// 转账转入累计
        /// </summary>  
				[DisplayName("转账转入累计")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  TransferInto { get; set; }
		      
       
        
        /// <summary>
        /// 买入交易量
        /// </summary>  
				[DisplayName("买入交易量")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  BuyTradeNum { get; set; }
		      
       
        
        /// <summary>
        /// 卖出交易量
        /// </summary>  
				[DisplayName("卖出交易量")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  SellTradeNum { get; set; }
		      
       
        
        /// <summary>
        /// 显示买入数
        /// </summary>  
				[DisplayName("显示买入数")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  ShowBuyTradeNum { get; set; }
		      
       
        
        /// <summary>
        /// 显示卖出数
        /// </summary>  
				[DisplayName("显示卖出数")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  ShowSellTradeNum { get; set; }
		      
       
        
        /// <summary>
        /// 累计买入
        /// </summary>  
				[DisplayName("累计买入")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  AddBuyTradeNum { get; set; }
		      
       
        
        /// <summary>
        /// 累计卖出
        /// </summary>  
				[DisplayName("累计卖出")]
		        [Filters.DecimalPrecision(18,8)]
		public decimal?  AddSellTradeNum { get; set; }



        /// <summary>
        /// 业绩达标次数
        /// </summary>  
        [DisplayName("业绩达标次数")]
        public int ReachingNum { get; set; }



        /// <summary>
        /// 业绩达标时间
        /// </summary>  
        [DisplayName("业绩达标时间")]
        public DateTime ReachingTime { get; set; }




        /// <summary>
        /// 时间截
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public User()
        {
           //  = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 会员表业务接口
    /// </summary>
    public interface IUserService :IServiceBase<User> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(User entity);
	}
    /// <summary>
    /// 会员表业务类
    /// </summary>
    public class UserService :  ServiceBase<User>,IUserService
    {


        public UserService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(User entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
