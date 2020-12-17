using System;
using System.Collections.Generic;
using System.Linq;

namespace JN.Data.Extensions
{
    public class ResponseBaseEOS<T>
    {
        public T result { get; set; }

    }
    public class ResponseEOS_get_info
    {
        public string server_version { get; set; }
        public string chain_id { get; set; }
        public long head_block_num { get; set; }
        public long last_irreversible_block_num { get; set; }
        public string last_irreversible_block_id { get; set; }
        public string head_block_id { get; set; }
        public string head_block_time { get; set; }
        public string head_block_producer { get; set; }
        public long virtual_block_cpu_limit { get; set; }
        public long virtual_block_net_limit { get; set; }
        public long block_cpu_limit { get; set; }
        public long block_net_limit { get; set; }
        public string server_version_string { get; set; }
    }
    public class ResponseEOS_get_block
    {
        public string timestamp { get; set; }
        public string producer { get; set; }
        public int confirmed { get; set; }
        public string previous { get; set; }
        public string transaction_mroot { get; set; }
        public string action_mroot { get; set; }
        public int schedule_version { get; set; }
        public string new_producers { get; set; }
        public string header_extensions { get; set; }
        public string producer_signature { get; set; }
        public string transactions { get; set; }
        public bool block_extensions { get; set; }
        public string id { get; set; }
        public long block_num { get; set; }
        public long ref_block_prefix { get; set; }
    }

    public class json_transactions
    {
        public string Account { get; set; }
    }

        public class Response_actions
    {
        public json_actions actions { get; set; }
    }


    public class json_actions
    {
        public int global_action_seq { get; set; }
        public int account_action_seq { get; set; }
        public int block_num { get; set; }
        public string block_time { get; set; }
        public string action_trace { get; set; }
    }
}
