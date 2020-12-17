using System;
using System.Runtime.Serialization;

namespace APICore
{
    /// <summary>
    /// API客户端异常。
    /// </summary>
    public class APIException : Exception
    {
        private string errorCode;
        private string errorMsg;

        public APIException()
            : base()
        {
        }

        public APIException(string message)
            : base(message)
        {
        }

        protected APIException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public APIException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public APIException(string errorCode, string errorMsg)
            : base(errorCode + ":" + errorMsg)
        {
            this.errorCode = errorCode;
            this.errorMsg = errorMsg;
        }

        public string ErrorCode
        {
            get { return this.errorCode; }
        }

        public string ErrorMsg
        {
            get { return this.errorMsg; }
        }
    }
}
