function Migrate {
  param([string] $Name)
  if (-not $Name)
  {
    write-host "Please provide migration name";
    return;
  }
  $project = Get-Project
  Build-Project($project)
  $installPath = GetInstallPath $project
  
  $toolsPath = Join-Path $installPath tools
  $info = New-AppDomainSetup $project $installPath
  $dllPath = Join-Path $toolsPath DaggerNet.PowerShell.dll
  write-host "Fuck Nuget and PowerShell"
  $domain = [AppDomain]::CreateDomain('Migrations',  $null, $info)
  $domain.SetData("project", $project)
  $parameters = @($Name)
  try
  {
    $domain.CreateInstanceFrom(
      $dllPath,
      'DaggerNet.PowerShell.MigrateCommand',
      $false,
      0,
      $null,
      $parameters,
      $null,
      $null)
  }
  finally
  {
    [AppDomain]::Unload($domain)
  }
  #$error = Get-Error($domain)

}

function New-AppDomainSetup($Project, $InstallPath)
{
  $targetDir = Join-Path $project.Properties.Item("FullPath") $project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").value;
  write-host "Fuck Nuget and PowerShell"
  write-host $targetDir
  $info = New-Object System.AppDomainSetup -Property @{
    ShadowCopyFiles = 'true';
    ApplicationBase = $targetDir;
    PrivateBinPath = Join-Path $InstallPath 'tools';
    ConfigurationFile = ([AppDomain]::CurrentDomain.SetupInformation.ConfigurationFile)
  }
    
  return $info
}

function GetInstallPath ($project) {
  $package = Get-Package -ProjectName $project.FullName | ?{$_.Id -eq 'DaggerNet'}
  $componentModel = Get-VsComponentModel
  $packageInstallerServices = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerServices])
  $vsPackage = $packageInstallerServices.GetInstalledPackages() | ?{ $_.Id -eq $package.Id -and $_.Version -eq $package.Version }
  return $vsPackage.InstallPath
}

function Build-Project($project)
{
  $configuration = $DTE.Solution.SolutionBuild.ActiveConfiguration.Name
  $DTE.Solution.SolutionBuild.BuildProject($configuration, $project.UniqueName, $true)
  if ($DTE.Solution.SolutionBuild.LastBuildInfo)
  {
    $projectName = $project.Name
    throw "The project '$projectName' failed to build."
  }
}
Export-ModuleMember Migrate