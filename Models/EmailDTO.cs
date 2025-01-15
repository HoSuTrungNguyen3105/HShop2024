using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HShop2024.Models
{
    public class EmailDTO
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
    }
}
