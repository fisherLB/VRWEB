using System;
using System.Collections.Generic;
using System.Linq;

namespace JN.Data.Extensions
{
    public class ResponseXRPTransactionsForTX
    {
        public string result { get; set; }
        public XRPTransaction transaction { get; set; }
        public string message { get; set; }
    }

    public class XRPTransaction
    {
        public string hash { get; set; }
        public string date { get; set; }
        public XRPTransactionTx tx { get; set; }
        public XRPTransactionMeta meta { get; set; }
    }

    public class XRPTransactionMeta
    {
        public string TransactionResult { get; set; }
        public List<XRPTransactionAffectedNodes> AffectedNodes { get; set; }
    }
    public class XRPTransactionTx
    {
        public string TransactionType { get; set; }
        public long Flags { get; set; }
        public string Account { get; set; }
        public string Destination { get; set; }
        public long DestinationTag { get; set; }
        public string Amount { get; set; }
        public string Fee { get; set; }
    }
    public class XRPTransactionAffectedNodes
    {
    }
}
