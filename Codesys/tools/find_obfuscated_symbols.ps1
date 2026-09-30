<#
.SYNOPSIS
  Сканер SmartAssembly-обфусцированных имён (\uXXXX) в декомпилированных .cs.
.DESCRIPTION
  Находит эскейп-имена вида \u001F.\u0010, считает частоты и печатает объявления
  (class/interface/enum/struct/delegate) с обфусцированными именами и их namespace.
  KISS: только чтение, без dnlib.
.EXAMPLE
  .\find_obfuscated_symbols.ps1 -Root C:\Codesys\decompiled\Compiler35220.plugin
  .\find_obfuscated_symbols.ps1 -Root C:\Codesys\decompiled\LanguageModelManager.plugin -Top 20
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$Root,
  [int]$Top = 30
)

if (-not (Test-Path -LiteralPath $Root)) { throw "Not found: $Root" }

$refRx  = [regex]'\\u[0-9A-Fa-f]{4}\.\\u[0-9A-Fa-f]{4}'
$nameRx = [regex]'\\u[0-9A-Fa-f]{4}'
$nsRx   = [regex]'namespace\s+(\\u[0-9A-Fa-f]{4})'
$declRx = [regex]'(?m)^\s*(?:internal|public|private|protected)?\s*(?:sealed |static |abstract |partial )*(class|interface|enum|struct|delegate)\s+(\\u[0-9A-Fa-f]{4})\b'
$files  = Get-ChildItem -LiteralPath $Root -Recurse -File -Filter *.cs

$refs = @{}; $names = @{}; $decls = New-Object System.Collections.Generic.List[object]
foreach ($f in $files) {
  $t = Get-Content -LiteralPath $f.FullName -Raw
  foreach ($m in $nameRx.Matches($t)) { $names[$m.Value] = 1 }
  foreach ($m in $refRx.Matches($t)) { $k = $m.Value; $refs[$k] = 1 + [int]$refs[$k] }
  $ns = if ($t -match $nsRx) { $matches[1] } else { '' }
  foreach ($m in $declRx.Matches($t)) {
    $decls.Add([pscustomobject]@{ ns = $ns; kind = $m.Groups[1].Value; name = $m.Groups[2].Value; file = $f.Name })
  }
}

Write-Host "Root          : $Root"
Write-Host "Files (.cs)   : $($files.Count)"
Write-Host "Distinct names: $($names.Count)"
Write-Host ''
Write-Host "== Top obfuscated type refs =="
$refs.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First $Top |
  Format-Table @{n = 'ref'; e = { $_.Name }}, Value -AutoSize
Write-Host ''
Write-Host "== Declarations ($($decls.Count)) =="
$decls | Sort-Object name | Format-Table ns, kind, name, file -AutoSize
