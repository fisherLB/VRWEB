using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Extensions
{
    public class ResponseContractTransfer
    {
            public string OrderNumber { get; set; }
            public string FromAddress { get; set; }

            public string ToUserName { get; set; }

            public string ToAddress { get; set; }

            public double Quantity { get; set; }

            public int? ConfirmationNumber { get; set; }

            public string TransactionHash { get; set; }
            public int Status { get; set; }

            public DateTime CreateTime { get; set; }

            public DateTime? ExecTime { get; set; }

            public string PostUrl { get; set; }
            public string Remark { get; set; }

            public string ContractName { get; set; }


    }
}
