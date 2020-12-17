using System;
using System.Collections.Generic;
using System.Linq;

namespace JN.Data.Extensions
{
    public class ResponseXRPTransactions
    {
        public string result { get; set; }
        public int count { get; set; }
        public List<paymentslist> payments { get; set; }
        public string message { get; set; }
    }

    public class paymentslist
    {
        public string amount { get; set; }
        public string delivered_amount { get; set; }
        public List<changes> destination_balance_changes { get; set; }
        public List<changes> source_balance_changes { get; set; }
        public int tx_index { get; set; }
        public string currency { get; set; }
        public string destination { get; set; }
        public int destination_tag { get; set; }
        public string executed_time { get; set; }
        public long ledger_index { get; set; }
        public string source { get; set; }
        public string source_currency { get; set; }
        public string tx_hash { get; set; }
        public string transaction_cost { get; set; }
    }

    public class changes
    {
        public string counterparty { get; set; }
        public string currency { get; set; }
        public string value { get; set; }
    }
    }
