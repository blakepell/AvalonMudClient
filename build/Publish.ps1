Clear-Host
Write-Host -ForegroundColor Green "Avalon Mud Client Install Build"
Write-Host -ForegroundColor Green "-------------------------------"
Write-Host

#Set the build variables
$project = "..\src\Avalon.Client\Avalon.Client.csproj"
$project_common = "..\src\Avalon.Common\Avalon.Common.csproj"
$project_moonsharp = "..\src\Avalon.MoonSharp\Avalon.MoonSharp.csproj"
$project_dsl = "..\src\Avalon.Plugins.DarkAndShatteredLands\Avalon.Plugins.DarkAndShatteredLands.csproj"
$project_sqlite = "..\src\Avalon.Sqlite\Avalon.Sqlite.csproj"
$project_issx86 = ".\x86Installer.iss"
$project_issx64 = ".\x64Installer.iss"

$x64output = ".\x64"
$x86output = ".\x86"
#$x64outputStandAlone = ".\x64-stand-alone"
$dotnet = "C:\Program Files\dotnet\dotnet.exe"

# Get the version we are going to build and update the csproj files and InnoSetup scripts before the build.
$newVersion = Read-Host -Prompt 'Enter the new version number'
Write-Host "Updating csproj and iss files to version: '$NewVersion'"

& $PSScriptRoot\UpdateVersion.ps1 -project $project -version $newVersion
& $PSScriptRoot\UpdateVersion.ps1 -project $project_common -version $newVersion
& $PSScriptRoot\UpdateVersion.ps1 -project $project_moonsharp -version $newVersion
& $PSScriptRoot\UpdateVersion.ps1 -project $project_dsl -version $newVersion
& $PSScriptRoot\UpdateVersion.ps1 -project $project_sqlite -version $newVersion
& $PSScriptRoot\UpdateVersion.ps1 -project $project_issx86 -version $newVersion
& $PSScriptRoot\UpdateVersion.ps1 -project $project_issx64 -version $newVersion

# Optional plugin build
$pluginsBuild = Read-Host "Do you want to publish any plugin projects (y/n)"

# Make the build folders if they don't exist.
New-Item -ItemType Directory -Force -Path $x64output
New-Item -ItemType Directory -Force -Path $x86output
#New-Item -ItemType Directory -Force -Path $x64outputStandAlone

# Clear the previous releases if it exists.
Remove-Item "$x64output\*" -Recurse -Confirm:$false
Remove-Item "$x86output\*" -Recurse -Confirm:$false
#Remove-Item "$x86outputStandAlone\*" -Recurse -Confirm:$true

# Build the 64 & 32 bit releases
& $dotnet publish $project -c Release -r win-x64 -o $x64output
& $dotnet publish $project -c Release -r win-x86 -o $x86output
#& $dotnet publish $project -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true

# Execute the Inno scripts to build the installers.
$installer = '"C:\Program Files (x86)\Inno Setup 6\iscc.exe" ".\x64Installer.iss"'
Invoke-Expression "& $installer"

$installer = '"C:\Program Files (x86)\Inno Setup 6\iscc.exe" ".\x86Installer.iss"'
Invoke-Expression "& $installer"

# If they wanted to build the plugins also then do that.
if ($pluginsBuild -eq 'y') {
    & $PSScriptRoot\PublishPlugins.ps1
}