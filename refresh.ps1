

$currentProject = Get-Project
if ($currentProject.Name -ne 'DaggerNet.Tests')
{
  return write-host 'Please set to DaggerNet.Tests project';
}

.\buildpkg.ps1

Uninstall-Package DaggerNet.PostgresSQL
Uninstall-Package DaggerNet
Install-Package DaggerNet.PostgresSQL -s .\

