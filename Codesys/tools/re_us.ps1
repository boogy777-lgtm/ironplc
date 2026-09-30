param(
    [Parameter(Mandatory=$true)][string]$Dll,
    [Parameter(Mandatory=$true)][string]$Pattern
)
$ErrorActionPreference='Stop'
Add-Type -Path "C:\Codesys\tools\dnlib.dll"
$m = [dnlib.DotNet.ModuleDefMD]::Load($Dll)
$all = @($m.GetUserStrings() | ForEach-Object { $_.ToString() })
$re = [regex]$Pattern
$hits = $all | Where-Object { $re.IsMatch($_) } | Select-Object -Unique
"### $([System.IO.Path]::GetFileName($Dll)) : US total=$($all.Count), matches=$($hits.Count) /$Pattern/"
$hits | ForEach-Object { $_ }

