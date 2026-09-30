param(
    [Parameter(Mandatory=$true)][string]$Dll,
    [Parameter(Mandatory=$true)][string]$Pattern,
    [int]$MinLen = 2
)
$bytes = [System.IO.File]::ReadAllBytes($Dll)
$strings = New-Object System.Collections.Generic.List[string]
# ASCII pass
$sb = New-Object System.Text.StringBuilder
foreach ($b in $bytes) {
    if (($b -ge 32 -and $b -le 126)) { [void]$sb.Append([char]$b) }
    else { if ($sb.Length -ge $MinLen) { $strings.Add($sb.ToString()) }; [void]$sb.Clear() }
}
if ($sb.Length -ge $MinLen) { $strings.Add($sb.ToString()) }
# UTF-16LE pass
$sb2 = New-Object System.Text.StringBuilder
for ($i = 0; $i + 1 -lt $bytes.Length; $i += 2) {
    $lo = $bytes[$i]; $hi = $bytes[$i+1]
    if ($hi -eq 0 -and $lo -ge 32 -and $lo -le 126) { [void]$sb2.Append([char]$lo) }
    else { if ($sb2.Length -ge $MinLen) { $strings.Add($sb2.ToString()) }; [void]$sb2.Clear() }
}
if ($sb2.Length -ge $MinLen) { $strings.Add($sb2.ToString()) }
$re = [regex]$Pattern
$hits = $strings | Where-Object { $re.IsMatch($_) } | Select-Object -Unique
"### $([System.IO.Path]::GetFileName($Dll)) : $($hits.Count) unique matches for /$Pattern/"
$hits | ForEach-Object { $_ }
