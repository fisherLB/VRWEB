using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data
{
    public partial class RaCoin
    {
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("CurID")]
        public Currency OutTable_Currency { get; set; }
    }
}
