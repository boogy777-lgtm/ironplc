param(
    [Parameter(Mandatory=$true)][string]$Dll,
    [Parameter(Mandatory=$true)][string]$Tokens
)
Add-Type -Path 'C:\Codesys\tools\dnlib.dll'
$wanted = @{}
foreach ($t in ($Tokens -split '[,; ]+')) {
    if ($t -eq '') { continue }
    $wanted[[uint32]::Parse($t, [System.Globalization.NumberStyles]::HexNumber)] = $true
}
$m = [dnlib.DotNet.ModuleDefMD]::Load($Dll)
foreach ($td in $m.GetTypes()) {
    foreach ($mm in $td.Methods) {
        if (-not $wanted.ContainsKey($mm.MDToken.Raw)) { continue }
        "=== [$($td.FullName)] $($mm.Name) token=$($mm.MDToken.Raw.ToString('X8')) sig=$($mm.MethodSig) ==="
        $b = $mm.Body
        if ($b -eq $null) { "  body=null"; continue }
        foreach ($i in $b.Instructions) { "  $i" }
    }
}
