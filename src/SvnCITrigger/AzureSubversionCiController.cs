using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SvnCITrigger
{
    public class AzureSubversionCiController
    {
        internal string collectionUri = "";
        internal string projectName = "";
        internal string pat = "";
        internal string svnuser = "";
        internal string svnpasssword = "";

        public AzureSubversionCiController(string ProjectName, string svnUser, string svnPassword,string DevOpsUrl="", string PAT="")
        {
            projectName = ProjectName;
            svnuser = svnUser;
            svnpasssword = svnPassword;

            if (!string.IsNullOrEmpty(DevOpsUrl))
                collectionUri = DevOpsUrl;

            if (!string.IsNullOrEmpty(PAT))
                pat = PAT;

        }

        public bool RunCI()
        {

            VssConnection connection = new VssConnection(new Uri(collectionUri), new VssBasicCredential(string.Empty, pat));

            BuildHttpClient buildClient = connection.GetClient<BuildHttpClient>();

            Task<List<BuildDefinitionReference>> buildDefs = Task.Run(() => buildClient.GetDefinitionsAsync(projectName, null, null, null, null, null, null, null, null, null, null, null));
            buildDefs.Wait();

            List<BuildDefinition> buildDefinitions = new List<BuildDefinition>();

            foreach (BuildDefinitionReference bdr in buildDefs.Result)
            {
                Task<BuildDefinition> bd = Task.Run(() => buildClient.GetDefinitionAsync(projectName, bdr.Id));
                bd.Wait();

                buildDefinitions.Add(bd.Result);

            }

            SortedList<int, BuildDefinition> buildIsNeeded = new SortedList<int, BuildDefinition>();
           

            foreach (BuildDefinition buildDef in buildDefinitions)
            {
                if (buildDef.Repository != null && buildDef.Repository.Type == "Svn")
                {
                    // get last version from the repository
                    long lastRepositoryVersion = GetLatestCheckinVersion(buildDef.Repository.Id, buildDef.Repository.DefaultBranch);

                    int buildOrder = 0;
                    if (buildDef.Variables.ContainsKey("buildOrder"))
                    {
                        BuildDefinitionVariable bvBuildOrder = null;
                        if (buildDef.Variables.TryGetValue("buildOrder", out bvBuildOrder))
                        {
                            buildOrder = Convert.ToInt32(bvBuildOrder.Value);

                        }

                    }

                    // get last version from builds
                    Task<List<Microsoft.TeamFoundation.Build.WebApi.Build>> taskBuildList = Task.Run(() => buildClient.GetBuildsAsync(projectName, new List<int> { buildDef.Id }));
                    taskBuildList.Wait();

                    Microsoft.TeamFoundation.Build.WebApi.Build latestBuild = taskBuildList.Result[0] as Microsoft.TeamFoundation.Build.WebApi.Build;
                    if (latestBuild != null)
                    {
                        if (Convert.ToInt64(latestBuild.SourceVersion) < lastRepositoryVersion)
                        {
                            Console.WriteLine(string.Format("Build is required for {0} - last built version {1} last checkin version {2}", buildDef.Name, latestBuild.SourceVersion, lastRepositoryVersion));
                            buildIsNeeded.Add(buildOrder,buildDef);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Build is up to date for {0} - last built version {1} last checkin version {2}", buildDef.Name, latestBuild.SourceVersion, lastRepositoryVersion));
                        }
                    }

                }

            }

            // now we know what needs to be build, we must trigger any builds...
            foreach (BuildDefinition buildDef in buildIsNeeded.Values)
            {
                Console.WriteLine(string.Format("Triggering build for {0}", buildDef.Name));

                Build build = new Build() { Definition = buildDef, Project = buildDef.Project };
                Task<Build> taskBuild = Task.Run(() => buildClient.QueueBuildAsync(build));
                taskBuild.Wait();

                return true; // ONLY ALLOW **ONE** BUILD PER TIME
            }

            return false;

        }

        private long GetLatestCheckinVersion(string serverUrl, string branch)
        {
            try
            {
                using (SharpSvn.SvnClient client = new SharpSvn.SvnClient())
                {
                    client.Authentication.Clear(); // Clear a previous authentication
                    client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(svnuser, svnpasssword);
                    client.Authentication.SslServerTrustHandlers += Authentication_SslServerTrustHandlers;

                    SvnInfoEventArgs info;

                    string endpoint = serverUrl.TrimEnd('/') + @"/" + branch;

                    client.GetInfo(endpoint, out info);
                    long lastRevision = info.LastChangeRevision;

                    return lastRevision;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION - " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return -1;
            }

        }

        private void Authentication_SslServerTrustHandlers(object sender, SharpSvn.Security.SvnSslServerTrustEventArgs e)
        {
            e.AcceptedFailures = e.Failures;
            e.Save = true;

        }
    }
}
