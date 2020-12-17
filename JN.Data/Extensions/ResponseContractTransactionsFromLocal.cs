using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Extensions
{
    public class ResponseContractTransactionsFromLocal
    {
        public string ContractAddress { get; set; }

        public int BlockNumber { get; set; }

        public string TransactionHash { get; set; }

        public string FromAddress { get; set; }

        public string ToAddress { get; set; }

        public double Quantity { get; set; }

        public string EventName { get; set; }

        public string Direction { get; set; }

        public DateTime CreateTime { get; set; }

        public string ContractName { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
    }
}
