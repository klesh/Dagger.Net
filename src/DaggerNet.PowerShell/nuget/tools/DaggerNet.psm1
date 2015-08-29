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
      $null) | Out-Null
    $created = $domain.GetData('created');
	if ($created)
	{
		write-host "Migration file created: $created"
		$project.DTE.ItemOperations.OpenFile($created) | Out-Null
	}
  }
  finally
  {
    $error = Get-Error($domain)
    if ($error)
    {
      Write-Verbose $error.StackTrace
    }    
    [AppDomain]::Unload($domain)
  }

}

function New-AppDomainSetup($Project, $InstallPath)
{
  $targetDir = Join-Path $project.Properties.Item("FullPath").Value $project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value;
  $info = New-Object System.AppDomainSetup -Property @{
    ShadowCopyFiles = 'true';
    ApplicationBase = $targetDir;
    PrivateBinPath = Join-Path $targetDir tools;
    ConfigurationFile = ([AppDomain]::CurrentDomain.SetupInformation.ConfigurationFile);
    LoaderOptimization = 1; # crucial, release the dlls while appdomain is unloaded.
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

function Get-Error($domain)
{

    if (!$domain.GetData('wasError'))
    {
        return $null
    }

    return @{
            Message = $domain.GetData('error.Message');
            TypeName = $domain.GetData('error.TypeName');
            StackTrace = $domain.GetData('error.StackTrace')
    }
}
Export-ModuleMember Migrate