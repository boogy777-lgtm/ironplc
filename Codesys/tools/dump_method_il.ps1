param([string]$Dll, [string]$TypeLike, [string]$Method)
Add-Type -Path ('C:\Codesys\tools\dnlib.dll')
$m = [dnlib.DotNet.ModuleDefMD]::Load($Dll)
foreach ($t in $m.GetTypes()) {
    if ($t.FullName -notlike "*$TypeLike*") { continue }
    foreach ($mm in $t.Methods) {
        if ($Method -ne '' -and $mm.Name -ne $Method) { continue }
        "=== $($t.FullName)::$($mm.Name) token=$($mm.MDToken.Raw.ToString('X8')) ==="
        try {
            $b = $mm.Body
            if ($b -eq $null) { "  body=null"; continue }
            foreach ($i in $b.Instructions) { "  $i" }
            foreach ($eh in $b.ExceptionHandlers) { "  EH $($eh.HandlerType) try=($($eh.TryStart))..($($eh.TryEnd)) h=($($eh.HandlerStart))..($($eh.HandlerEnd)) catch=$($eh.CatchType)" }
        } catch { "  EX: $($_.Exception.Message)" }
    }
}

