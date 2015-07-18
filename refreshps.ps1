#
# refreshps.ps1
#
$currentProject = Get-Project
if ($currentProject.Name -ne 'DaggerNet.Tests')
{
  return write-host 'Please set to DaggerNet.Tests project';
}

del *.nupkg

$project = Get-Project DaggerNet.PowerShell
$DTE.Solution.SolutionBuild.BuildProject("Release", $project.UniqueName, $true)
if ($DTE.Solution.SolutionBuild.LastBuildInfo)
{
	$projectName = $project.Name
	throw "The project '$projectName' failed to build."
}

$now = Get-Date
$v = "{0}.{1}.{2}" -f $now.Hour, $now.Minute, $now.Second
nuget pack .\src\DaggerNet.PowerShell\nuget\DaggerPS.nuspec -Version $v
Uninstall-Package DaggerNet
Install-Package DaggerNet -s .\