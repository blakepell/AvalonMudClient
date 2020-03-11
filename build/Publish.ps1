# Set the build variables.
$project = "..\src\Avalon.Client\Avalon.Client.csproj"
$x64output = ".\x64"
$x86output = ".\x86"
$dotnet = "C:\Program Files\dotnet\dotnet.exe"

# Make the build folders if they don't exist.
New-Item -ItemType Directory -Force -Path $x64output
New-Item -ItemType Directory -Force -Path $x86output

# Clear the previous releases if it exists.
Remove-Item "$x64output\*" -Recurse -Confirm:$true
Remove-Item "$x86output\*" -Recurse -Confirm:$true

# Build the 64 & 32 bit releases
& $dotnet publish $project -c Release -r win-x64 -o $x64output
& $dotnet publish $project -c Release -r win-x86 -o $x86output

# Execute the Inno scripts to build the installers.
$installer = '"C:\Users\bpell\AppData\Local\Programs\Inno Setup 6\iscc.exe" ".\x64Installer.iss"'
Invoke-Expression "& $installer"

$installer = '"C:\Users\bpell\AppData\Local\Programs\Inno Setup 6\iscc.exe" ".\x86Installer.iss"'
Invoke-Expression "& $installer"
