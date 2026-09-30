param(
    [Parameter(Mandatory=$true)][string]$Dll,
    [string]$OutDir = "C:\Users\HALLIBURTON\Desktop\1.1.0.0\_dump_st",
    [string]$TypeFilter = "",
    [switch]$NoTypes
)
$ErrorActionPreference = 'Stop'
$dnlib = "C:\Codesys\tools\dnlib.dll"
Add-Type -Path $dnlib
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$m = [dnlib.DotNet.ModuleDefMD]::Load($Dll)
$asm = $m.Assembly
$name = [System.IO.Path]::GetFileNameWithoutExtension($Dll)
$out = Join-Path $OutDir "$name.types.txt"

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("FILE`t$Dll")
$lines.Add("ASM`t$($asm.FullName)")
$lines.Add("MODULE`t$($m.Name)")
foreach ($ar in $m.GetAssemblyRefs()) {
    $lines.Add("REF`t$($ar.Name)`t$($ar.Version)")
}
$types = @($m.GetTypes())
$lines.Add("TYPECOUNT`t$($types.Count)")
$nsSet = New-Object System.Collections.Generic.SortedSet[string]
foreach ($t in $types) { [void]$nsSet.Add($t.Namespace) }
foreach ($ns in $nsSet) { $lines.Add("NAMESPACE`t$ns") }

if (-not $NoTypes) {
    foreach ($t in $types) {
        if ($TypeFilter -ne "" -and $t.FullName -notmatch $TypeFilter) { continue }
        $kind = 'class'
        if ($t.IsInterface) { $kind = 'interface' }
        elseif ($t.IsEnum) { $kind = 'enum' }
        elseif ($t.IsValueType) { $kind = 'struct' }
        elseif ($t.IsDelegate) { $kind = 'delegate' }
        $lines.Add("TYPE`t$kind`t$($t.FullName)`tbase=$($t.BaseType)")
        if ($t.IsEnum) {
            foreach ($f in $t.Fields) { if ($f.IsLiteral) { $lines.Add("`tENUM`t$($f.Name)`t=$($f.Constant.Value)") } }
            continue
        }
        foreach ($p in $t.Properties) {
            $g = if ($p.GetMethod) { 'get' } else { '' }
            $s = if ($p.SetMethod) { 'set' } else { '' }
            $lines.Add("`tPROP`t$($p.PropertySig)`t$g/$s")
        }
        foreach ($f in $t.Fields) {
            if ($f.IsLiteral) { continue }
            $lines.Add("`tFIELD`t$($f.FieldType) $($f.Name)")
        }
        foreach ($md in $t.Methods) {
            if ($md.IsGetter -or $md.IsSetter -or $md.IsAddOn -or $md.IsRemoveOn) { continue }
            $lines.Add("`tMETHOD`t$($md.MethodSig) $($md.Name)$($md.GenericParameters.Count)")
        }
    }
}
Set-Content -Path $out -Value $lines -Encoding UTF8
"$out`t($($lines.Count) lines, types=$($types.Count))"

