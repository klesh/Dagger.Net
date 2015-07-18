function Build-Project($project)
{
#  $configuration = $DTE.Solution.SolutionBuild.ActiveConfiguration.Name
  $DTE.Solution.SolutionBuild.BuildProject("Release", $project.UniqueName, $true)
  if ($DTE.Solution.SolutionBuild.LastBuildInfo)
  {
    $projectName = $project.Name
    throw "The project '$projectName' failed to build."
  }
}

Build-Project (Get-Project DaggerNet.Postgres)
Build-Project (Get-Project DaggerNet.PowerShell)
del .\*.nupkg
nuget pack .\src\DaggerNet\nuget\DaggerNet.nuspec
nuget pack .\src\DaggerNet.Postgres\nuget\DaggerNet.Postgres.nuspec
