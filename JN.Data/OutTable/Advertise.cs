using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data
{
    public partial class Advertise
    {
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("UID")]
        public User OutTable_User { get; set; }
    }
}
