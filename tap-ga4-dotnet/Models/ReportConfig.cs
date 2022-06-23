using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingerTapGA4.Models
{
    public class ReportConfig
    {
        public string Name { get; set; }
        public List<string> Dimensions { get; set; }
        public List<string> Metrics { get; set; }
    }
}
