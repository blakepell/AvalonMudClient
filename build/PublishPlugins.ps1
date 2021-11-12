# Set the build variables.
$project = "..\src\Avalon.Plugins.DarkAndShatteredLands\Avalon.Plugins.DarkAndShatteredLands.csproj"
$output = ".\Plugins"
$dll = ".\Plugins\Avalon.Plugins.DarkAndShatteredLands.dll"
$dotnet = "C:\Program Files\dotnet\dotnet.exe"

# Make the build folders if they don't exist.
New-Item -ItemType Directory -Force -Path $output

# Clear the previous releases if it exists.
Remove-Item "$output\*" -Recurse -Confirm:$false

# Build the 64 & 32 bit releases
& $dotnet publish $project -c Release -r AnyCpu -o $output

Copy-Item -Path $dll -Destination ".\" -force