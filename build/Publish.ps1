# Set the build variables.
$project = "..\src\Avalon.Client\Avalon.Client.csproj"
$x64output = ".\x64"
$x86output = ".\x86"
#$x64outputStandAlone = ".\x64-stand-alone"
$dotnet = "C:\Program Files\dotnet\dotnet.exe"

# Make the build folders if they don't exist.
New-Item -ItemType Directory -Force -Path $x64output
New-Item -ItemType Directory -Force -Path $x86output
#New-Item -ItemType Directory -Force -Path $x64outputStandAlone

# Clear the previous releases if it exists.
Remove-Item "$x64output\*" -Recurse -Confirm:$true
Remove-Item "$x86output\*" -Recurse -Confirm:$true
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
