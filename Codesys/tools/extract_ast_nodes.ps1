# Regenerate tables\ast_nodes.csv from the Core language-model interfaces.
#
# The AST map is a 1:1 dump of every interface declared in
# `_3S.CoDeSys.Core.LanguageModel` (Compiler.dll, 691 interfaces):
#   kind       - coarse classification (statement/expression/type/token/other)
#   base_type  - first CODESYS base interface (Core.* prefix stripped)
#   full_name  - fully qualified interface name
#   key_fields - declared properties, joined with '|'
# The concrete_impl / red_factory_method / producer columns are filled later by
# tools\build_concrete_map.py.
#
# Usage:  powershell -File tools\extract_ast_nodes.ps1
param(
    [string]$CompilerDll = "C:\Codesys\binaries\Compiler.dll",
    [string]$Out = "C:\Codesys\tables\ast_nodes.csv"
)
$ErrorActionPreference = 'Stop'
Add-Type -Path "C:\Codesys\tools\dnlib.dll"

$NS = '_3S.CoDeSys.Core.LanguageModel'
$module = [dnlib.DotNet.ModuleDefMD]::Load($CompilerDll)
$types = @($module.GetTypes() | Where-Object { $_.IsInterface -and $_.Namespace -eq $NS })

function Base-Names($t) {
    @($t.Interfaces | ForEach-Object { $_.Interface } | Where-Object { $_ -and ([string]$_.FullName) -like '_3S.*' })
}
function Has-Base($t, [string]$target, $seen) {
    if ($seen.Contains([string]$t.Name)) { return $false }
    [void]$seen.Add([string]$t.Name)
    foreach ($b in (Base-Names $t)) {
        if ([string]$b.Name -eq $target) { return $true }
        if (Has-Base $b $target $seen) { return $true }
    }
    return $false
}
function Kind-Of($t) {
    $n = ([string]$t.Name) -replace '\d+$', ''
    if ($n -like '*Token') { return 'token' }
    if ($n -eq 'IElseIf' -or $n -like '*Statement') { return 'statement' }
    if ((Has-Base $t 'IStatement' (New-Object 'System.Collections.Generic.HashSet[string]'))) { return 'statement' }
    if ($n -like '*Expression' -or $n -like '*Exprement') { return 'expression' }
    if ((Has-Base $t 'IExpression' (New-Object 'System.Collections.Generic.HashSet[string]'))) { return 'expression' }
    if ((Has-Base $t 'IExprement' (New-Object 'System.Collections.Generic.HashSet[string]'))) { return 'expression' }
    if ($n -like '*Type') { return 'type' }
    return 'other'
}
function Base-Type($t) {
    $b = @(Base-Names $t | Select-Object -First 1)
    if ($b.Count -eq 0) { return '' }
    $fn = [string]$b[0].FullName
    return $fn.Replace('_3S.CoDeSys.Core.LanguageModel.', '')
}

$rows = New-Object System.Collections.Generic.List[object]
foreach ($t in $types) {
    $kf = (@($t.Properties | ForEach-Object { [string]$_.Name }) -join '|')
    $rows.Add([pscustomobject]@{
        kind            = (Kind-Of $t)
        base_type       = (Base-Type $t)
        full_name       = ("$NS." + [string]$t.Name)
        key_fields      = $kf
        builder_interface = ''
        concrete_impl   = ''
        red_factory_method = ''
        producer        = ''
    })
}
$rows | Export-Csv -Path $Out -NoTypeInformation -Encoding UTF8
Write-Output ("ast_nodes.csv: {0} interfaces written" -f $rows.Count)
