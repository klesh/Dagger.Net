function Migrate {
  param([string] $Name)
  if (-not $Name)
  {
    write-host "Please provide migration name";
    return;
  }
  $project = Get-Project
  $installPath = GetInstallPath $project
  $toolsPath = Join-Path $installPath tools
  $info = New-AppDomainSetup $project $installPath
  $dllPath = Join-Path $toolsPath DaggerNet.PowerShell.dll
  $domain = [AppDomain]::CreateDomain('Migrations',  $null, $info)
  $domain.SetData("project", $project)
  $parameters = @($Name)
  try
  {

    $domain.CreateInstanceFrom(
      $dllPath,
      'NugetCommandTest.MigrateCommand',
      $false,
      0,
      $null,
      $parameters,
      $null,
      $null)
    $error = Get-Error($domain)
    write-host $error.StackTrace
  } 
  finally
  {
    [AppDomain]::Unload($domain)
  }
}

function New-AppDomainSetup($Project, $InstallPath)
{
    $info = New-Object System.AppDomainSetup -Property @{
            ShadowCopyFiles = 'true';
            ApplicationBase = $InstallPath;
            PrivateBinPath = 'tools;lib';
            ConfigurationFile = ([AppDomain]::CurrentDomain.SetupInformation.ConfigurationFile)
        }
    return $info
}

function GetInstallPath ($project) {
  $package = Get-Package -ProjectName $project.FullName | ?{$_.Id -eq 'KleshPackage'}
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