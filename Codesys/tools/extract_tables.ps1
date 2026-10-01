param(
    [string]$ParserDll = "C:\Codesys\binaries\Parser35220.plugin.dll",
    [string]$CompilerDll = "C:\Codesys\binaries\Compiler.dll",
    [string]$OutDir = "C:\Codesys\tables"
)
$ErrorActionPreference = 'Stop'
Add-Type -Path "C:\Codesys\tools\dnlib.dll"
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$U32 = [uint32]
$ALL_LANGS = [uint32]4294901760

function Get-IntPushValue($ins) {
    switch ($ins.OpCode.Name) {
        'ldc.i4.m1' { return -1 } 'ldc.i4.0' { return 0 } 'ldc.i4.1' { return 1 }
        'ldc.i4.2' { return 2 } 'ldc.i4.3' { return 3 } 'ldc.i4.4' { return 4 }
        'ldc.i4.5' { return 5 } 'ldc.i4.6' { return 6 } 'ldc.i4.7' { return 7 }
        'ldc.i4.8' { return 8 }
        default { return [int]$ins.Operand }
    }
}

function Get-U32([long]$v) { return [uint32]([int64]$v -band 0xFFFFFFFFL) }

function Test-Bit([uint32]$u, [uint32]$mask) { return (($u -band $mask) -ne 0) }

function Decode-Flags([long]$value) {
    $u = Get-U32 $value
    $names = New-Object System.Collections.Generic.List[string]
    if (Test-Bit $u ([uint32]0x1))   { $names.Add('DataType') }
    if (Test-Bit $u ([uint32]0x8))   { $names.Add('NumericDataType') }
    if (Test-Bit $u ([uint32]0x2))   { $names.Add('Operator') }
    if (Test-Bit $u ([uint32]0x4))   { $names.Add('Keyword') }
    if (Test-Bit $u ([uint32]0x10))  { $names.Add('SafetyDataType') }
    if (Test-Bit $u ([uint32]0x20))  { $names.Add('Contextual') }
    if (($u -band $ALL_LANGS) -eq $ALL_LANGS) {
        $names.Add('AllLanguages')
    } else {
        if (Test-Bit $u ([uint32]0x10000))  { $names.Add('StructuredText') }
        if (Test-Bit $u ([uint32]0x20000))  { $names.Add('InstructionList') }
        if (Test-Bit $u ([uint32]0x40000))  { $names.Add('FunctionBlockDiagram') }
        if (Test-Bit $u ([uint32]0x80000))  { $names.Add('Declaration') }
        if (Test-Bit $u ([uint32]0x100000)) { $names.Add('Internal') }
    }
    return ($names -join '|')
}

function Decode-Language([long]$value) {
    $u = Get-U32 $value
    if (($u -band $ALL_LANGS) -eq $ALL_LANGS) { return 'All' }
    $l = New-Object System.Collections.Generic.List[string]
    if (Test-Bit $u ([uint32]0x10000))  { $l.Add('ST') }
    if (Test-Bit $u ([uint32]0x20000))  { $l.Add('IL') }
    if (Test-Bit $u ([uint32]0x40000))  { $l.Add('FBD') }
    if (Test-Bit $u ([uint32]0x80000))  { $l.Add('Declaration') }
    if (Test-Bit $u ([uint32]0x100000)) { $l.Add('Internal') }
    return ($l -join '|')
}

function Extract-OperatorDescEntries($method) {
    $res = New-Object System.Collections.Generic.List[object]
    if ($method -eq $null -or $method.Body -eq $null) { return ,$res }
    $ints = New-Object System.Collections.Generic.List[int]
    $lastStr = $null
    foreach ($i in $method.Body.Instructions) {
        $n = $i.OpCode.Name
        if ($n -eq 'ldstr') { $lastStr = [string]$i.Operand }
        elseif ($n -like 'ldc.i4*') { $ints.Add((Get-IntPushValue $i)) }
        elseif ($n -eq 'newobj') {
            $decl = $i.Operand.DeclaringType.FullName
            if ($decl -like '*OperatorDesc' -and $i.Operand.Name -eq '.ctor') {
                if ($ints.Count -ge 2) {
                    $res.Add([pscustomobject]@{
                        Text = $lastStr
                        Operator = [int64]$ints[$ints.Count - 2]
                        Flags = [int64]$ints[$ints.Count - 1]
                    })
                }
                $ints.Clear(); $lastStr = $null
            }
        }
    }
    return ,$res
}

function Save-Rows($path, $rows) { $rows | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8 }

# ---------- enums ----------
$cm = [dnlib.DotNet.ModuleDefMD]::Load($CompilerDll)
$pm = [dnlib.DotNet.ModuleDefMD]::Load($ParserDll)

function Get-EnumRows($module, $fullName) {
    $t = $module.Find($fullName, $true)
    if ($t -eq $null) { throw "enum not found: $fullName" }
    $rows = New-Object System.Collections.Generic.List[object]
    foreach ($f in $t.Fields) { if ($f.IsLiteral) { $rows.Add([pscustomobject]@{ name=$f.Name; value=[int64]$f.Constant.Value }) } }
    return ,$rows
}
$opRows = Get-EnumRows $cm "_3S.CoDeSys.Core.LanguageModel.Operator"
$ttRows = Get-EnumRows $cm "_3S.CoDeSys.Core.LanguageModel.TokenType"
$iecRows = Get-EnumRows $cm "_3S.CoDeSys.Core.LanguageModel.IECLanguage"
Save-Rows (Join-Path $OutDir 'operators.csv')    $opRows
Save-Rows (Join-Path $OutDir 'token_types.csv')  $ttRows
Save-Rows (Join-Path $OutDir 'ieclanguage.csv')  $iecRows

$ofT = $pm.Find("CODESYS.Parser35220.Scanner.OperatorFlags", $true)
$ofRows = New-Object System.Collections.Generic.List[object]
foreach ($f in $ofT.Fields) {
    if ($f.IsLiteral) {
        $v = [int64]$f.Constant.Value
        $ofRows.Add([pscustomobject]@{ name=$f.Name; value=$v; hex=('0x{0:X8}' -f (Get-U32 $v)) })
    }
}
Save-Rows (Join-Path $OutDir 'operator_flags.csv') $ofRows

# ---------- OperatorTable.Add* ----------
$ot = $pm.Find("CODESYS.Parser35220.Scanner.OperatorTable", $true)
function Get-Method($type, $name) { $type.Methods | Where-Object { $_.Name -eq $name } | Select-Object -First 1 }

function Save-AddTable($csvName, $methodName) {
    $md = Get-Method $ot $methodName
    $entries = Extract-OperatorDescEntries $md
    $rows = New-Object System.Collections.Generic.List[object]
    foreach ($e in $entries) {
        $rows.Add([pscustomobject]@{
            text = $e.Text; operator_ordinal = $e.Operator; flags = $e.Flags
            flags_decoded = (Decode-Flags $e.Flags); language = (Decode-Language $e.Flags)
        })
    }
    Save-Rows (Join-Path $OutDir $csvName) $rows
    return $entries.Count
}

$counts = [ordered]@{}
$counts['st_keywords.csv']          = Save-AddTable 'st_keywords.csv'          'AddKeywords'
$counts['standard_operators.csv']   = Save-AddTable 'standard_operators.csv'   'AddStandardOperators'
$counts['operator_symbols.csv']     = Save-AddTable 'operator_symbols.csv'     'AddOperatorSymbols'
$counts['datatype_names.csv']       = Save-AddTable 'datatype_names.csv'       'AddDataTypeNames'
$counts['special_operators.csv']    = Save-AddTable 'special_operators.csv'    'AddSpecialOperators'
$counts['ilo_operators.csv']        = Save-AddTable 'ilo_operators.csv'        'AddILOperators'
$counts['oo_keywords.csv']          = Save-AddTable 'oo_keywords.csv'          'AddOOKeywords'
$counts['vector_operators.csv']     = Save-AddTable 'vector_operators.csv'     'AddVectorOperatos'

# ---------- conversion operators (merged, dynamic names) ----------
$convRows = New-Object System.Collections.Generic.List[object]
foreach ($pair in @(@('AddConversionOperators','CONV'), @('AddOverloadedConversions','OVERLOAD'))) {
    $md = Get-Method $ot $pair[0]
    $entries = Extract-OperatorDescEntries $md
    foreach ($e in $entries) {
        $convRows.Add([pscustomobject]@{
            text_prefix = $e.Text; operator_ordinal = $e.Operator; flags = $e.Flags
            flags_decoded = (Decode-Flags $e.Flags); language = (Decode-Language $e.Flags)
            dynamic = 'true'; source_method = $pair[0]; name_template = ''
        })
    }
}
# annotate known templates (in IL order per method)
$tmpl = @('_TO_<T>','ANY_NUM_TO_<T>','ANY_TO_<T>','TO_<T>','<T>_TO_<U>')
for ($i = 0; $i -lt $convRows.Count; $i++) { $convRows[$i].name_template = $tmpl[$i] }
Save-Rows (Join-Path $OutDir 'conversion_operators.csv') $convRows
$counts['conversion_operators.csv'] = $convRows.Count

# ---------- reserved unused keywords ----------
$ruk = $pm.Find("CODESYS.Parser35220.Scanner.ReservedUnusedKeywords", $true)
$cctor = Get-Method $ruk '.cctor'
$kw = New-Object System.Collections.Generic.List[string]
foreach ($i in $cctor.Body.Instructions) { if ($i.OpCode.Name -eq 'ldstr') { $kw.Add([string]$i.Operand) } }
Set-Content -Path (Join-Path $OutDir 'reserved_unused_keywords.txt') -Value $kw -Encoding UTF8

# ---------- summary ----------
Write-Output "ENUMS: Operator=$($opRows.Count) TokenType=$($ttRows.Count) IECLanguage=$($iecRows.Count) OperatorFlags=$($ofRows.Count)"
foreach ($k in $counts.Keys) { Write-Output ("TABLE {0} = {1}" -f $k, $counts[$k]) }
Write-Output "reserved_unused_keywords.txt = $($kw.Count)"
