using CommandLine;
using System.Collections.Generic;

namespace SvnCITrigger
{
    class Program
    {

        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed<CommandLineOptions>(options => Exec(options.DevOpsUrl, options.ProjectNames, options.PAT, options.SvnUser, options.SvnPassword));

        }

        static void Exec(string url, IEnumerable<string> projects, string pat, string svnUser, string svnPassword)
        {
            AzureSubversionCiController ci = null;

            foreach (string project in projects)
            {
                ci = new AzureSubversionCiController(project, svnUser, svnPassword, url, pat);
                if (ci.RunCI())
                    break;
            }

        }

    }
}
