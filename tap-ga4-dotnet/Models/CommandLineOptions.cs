using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingerTapGA4.Models
{
    public class CommandLineOptions
    {
        [Option('c', "config", Required = true, HelpText = "A required argument that points to a JSON file containing the transformation configuration.")]
        public string Config { get; set; }
    }
}
