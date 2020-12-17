using System;
using System.Collections.Generic;
using System.Linq;

namespace JN.Data.Extensions
{
    public class ResponseEOS_get_transactions
    {
        public int errno { get; set; }
        public string errmsg { get; set; }
        public ResponseEOS_get_transactions_data data { get; set; }
    }

    public class ResponseEOS_get_balance
    {
        public int errno { get; set; }
        public string errmsg { get; set; }
        public ResponseEOS_get_balance_data data { get; set; }
    }
    public class ResponseEOS_get_transactions_data
    {
        public int trace_count { get; set; }
        public List<ResponseEOS_transactions_trace_list> trace_list { get; set; }
    }
    public class ResponseEOS_get_balance_data
    {
        public double balance { get; set; }
    }
    public class ResponseEOS_transactions_trace_list
    {
        public string trx_id { get; set; }
        public string timestamp { get; set; }
        public string receiver { get; set; }
        public string sender { get; set; }
        public string code { get; set; }
        public string quantity { get; set; }
        public string memo { get; set; }
        public string symbol { get; set; }
        public string status { get; set; }
    }
   
}
