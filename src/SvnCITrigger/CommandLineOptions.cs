using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace SvnCITrigger
{
    public class CommandLineOptions
    {

        [Option("url", HelpText ="Base url to Azure DevOps instance", Required =true)]
        public string DevOpsUrl { get; set; }

        [Option("projects", HelpText ="Project name within Azure DevOps, to process multiple projects specify them in a , seperated list", Required = true, Separator =',', Min =1)]
        public IEnumerable<string> ProjectNames { get; set; }

        [Option("pat", HelpText = "PAT (Personal Access Token) ", Required = true)]
        public string PAT { get; set; }

        [Option("svnuser", Required=true)]
        public string SvnUser { get; set; }

        [Option("svnpassword", Required = true)]
        public string SvnPassword { get; set; }
    }
}
