param([Parameter(Mandatory=$true)][string[]]$Dlls)
$ErrorActionPreference='Stop'
Add-Type -Path "C:\Codesys\tools\dnlib.dll"
foreach ($d in $Dlls) {
    $m = [dnlib.DotNet.ModuleDefMD]::Load($d)
    $asm = $m.Assembly
    Write-Output "===== $([System.IO.Path]::GetFileName($d)) ====="
    Write-Output "FullName: $($asm.FullName)"
    if ($asm.CustomAttributes) {
        foreach ($ca in $asm.CustomAttributes) {
            $t = "$($ca.TypeFullName)"
            if ($t -match "AssemblyProduct|AssemblyTitle|AssemblyDescription|AssemblyCompany|AssemblyCopyright|AssemblyInformationalVersion") {
                $vals = @()
                if ($ca.ConstructorArguments) { foreach ($a in $ca.ConstructorArguments) { $vals += "$($a.Value)" } }
                Write-Output ("  {0} = {1}" -f ($t -replace '.*\.',''), ($vals -join ','))
            }
        }
    }
}

