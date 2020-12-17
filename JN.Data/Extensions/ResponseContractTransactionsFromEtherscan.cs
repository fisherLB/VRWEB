using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Extensions
{
    public class ResponseContractTransactionsFromEtherscan
    {
        public string Address { get; set; }
        public double Quantity { get; set; }
        public double Balance { get; set; }

        public string GuidNo { get; set; }
        public DateTime CreateTime { get; set; }

        public string UserName { get; set; }
    }
}
