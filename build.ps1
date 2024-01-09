param($Configuration = 'Debug')

Push-Location $PSScriptRoot

$sourcePath = Join-Path $PSScriptRoot 'src/CodeConversion'
$destPath = Join-Path $PSScriptRoot 'module/CodeConversion'

Remove-Item -Path $destPath -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path $destPath -ItemType Directory

Copy-Item "CodeConversion.psm1" $destPath

$parameters = @{
    Path               = "$destPath\CodeConversion.psd1"
    Author             = ''
    CompanyName        = ''
    ModuleVersion      = '1.0.0'
    Description        = 'Convert C# to PowerShell'
    RootModule         = "CodeConversion.psm1"
    FunctionsToExport  = @("Invoke-CSharpConversion")
    RequiredAssemblies = 'CodeConversion.dll'
    ProjectUri         = 'https://github.com/fmichaleczek/code-conversion'
    LicenseUri         = 'https://github.com/fmichaleczek/code-conversion/blob/main/LICENSE'

}
New-ModuleManifest @parameters

Set-Location $sourcePath
dotnet publish -c $Configuration -o $destPath

Pop-Location