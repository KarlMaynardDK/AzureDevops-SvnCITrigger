# AzureDevops-SvnCITrigger
A windows CLI program that connects to AzureDevops and will trigger a build when you check files into a SVN repository.

Having failed to get the CI trigger for my Azure Devops builds to work with my VisualSVN repository I wrote this small utility to help document the issue for Microsoft.

So, whilst triggering manual builds works perfectly, scheduled builds work fine too - it is just the CI trigger in Azure DevOps that does nothing. After much help from the great team at Microsoft, the problem is caused by the structure of my SVN respository - this utility saves me from restructuring the complete repository.

## SvnCI-Trigger

To user the SvnCI-Trigger you will need to create a PAT (Personal Access Token) in your Azure DevOps account. 

To get help for the program, simply run at the command line with the --help option.

  --url            Required. Base url to Azure DevOps instance

  --projects       Required. Project name within Azure DevOps, to process multiple projects specify them in a ,
                   seperated list

  --pat            Required. PAT (Personal Access Token)

  --svnuser        Required.

  --svnpassword    Required.

  --help           Display this help screen.

  --version        Display version information.

## Build Order

If you have multiple builds defined for a project, you can set the build order by adding a variable to each build called *buildOrder* (Lowest number build first)
