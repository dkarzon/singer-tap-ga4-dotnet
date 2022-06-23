using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingerTapGA4.Models
{
    public class Config
    {
        public string CredentialsJsonPath { get; set; }

        public string Reports { get; set; }

        public string PropertyId { get; set; }

        public string StartDate { get; set; }

        public string? EndDate { get; set; }
    }
}
