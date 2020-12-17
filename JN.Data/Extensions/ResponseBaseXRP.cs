using System;
using System.Collections.Generic;
using System.Linq;

namespace JN.Data.Extensions
{
    public class ResponseBaseXRP<T>
    {
        public T result { get; set; }

    }
    public class ResponseXRP_account_info_account_data
    {
        public string Account { get; set; }
        public string Balance { get; set; }
        public long Flags { get; set; }
        public string status { get; set; }
        public string LedgerEntryType { get; set; }
        public int OwnerCount { get; set; }
        public string PreviousTxnID { get; set; }
        public int PreviousTxnLgrSeq { get; set; }
        public int Sequence { get; set; }
        public string index { get; set; }
    }
    public class ResponseXRP_account_info
    {
        public string status { get; set; }
        public bool validated { get; set; }
        public ResponseXRP_account_info_account_data account_data { get; set; }
        public int ledger_current_index { get; set; }
        //public string queue_data { get; set; }

        public string error { get; set; }
    }

    public class ResponseXRP_tx_json
    {
        public string Account { get; set; }
        public string Amount { get; set; }
        public string Destination { get; set; }
        public string Fee { get; set; }
        public long Flags { get; set; }
        public int Sequence { get; set; }
        public string SigningPubKey { get; set; }
        public string TransactionType { get; set; }
        public string TxnSignature { get; set; }
        public string hash { get; set; }
    }
    //public class ResponseXRP_Amount
    //{
    //    public string currency { get; set; }
    //    public string issuer { get; set; }
    //    public string value { get; set; }
    //}

    public class ResponseXRP_sign
    {
        public string status { get; set; }
        public string tx_blob { get; set; }
        public ResponseXRP_tx_json tx_json { get; set; }

        public string error { get; set; }
    }

    public class ResponseXRP_submit
    {
        public string engine_result { get; set; }
        public int engine_result_code { get; set; }
        public string engine_result_message { get; set; }

        public string status { get; set; }
        public string tx_blob { get; set; }
        public ResponseXRP_tx_json tx_json { get; set; }

        public string error { get; set; }
    }
    
}
