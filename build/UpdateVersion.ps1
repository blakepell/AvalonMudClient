# Updates the AssemblyVersion, FileVersion and Version attributes in a .NET Core csproj file.
Param(
    [string]$project,
    [string]$version
)

$assemblyVersionPattern = '\<AssemblyVersion\>(.*)\<\/AssemblyVersion\>'
$fileVersionPattern = '\<FileVersion\>(.*)\<\/FileVersion\>'
$versionPattern = '\<Version\>(.*)\<\/Version\>'
$issPattern = '\#define MyAppVersion \"(.*)\"'

(Get-Content $project) | ForEach-Object{
     if($_ -match $assemblyVersionPattern){
           '    <AssemblyVersion>{0}</AssemblyVersion>' -f $version
     }
     elseif($_ -match $fileVersionPattern){
           '    <FileVersion>{0}</FileVersion>' -f $version
     }
     elseif($_ -match $versionPattern){
           '    <Version>{0}</Version>' -f $version
     }
     elseif($_ -match $issPattern){
           '#define MyAppVersion "{0}"' -f $version
     }
     else{
        $_
     }
} | Set-Content $project