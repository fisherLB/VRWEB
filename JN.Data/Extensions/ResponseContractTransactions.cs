using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Extensions
{
    public class ResponseContractTransactions
    {
        public string Address { get; set; }
        public float Quantity { get; set; }
        public float Balance { get; set; }

        public string GuidNo { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
