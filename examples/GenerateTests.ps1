Import-Module "$PSScriptRoot/../module/CodeConversion/CodeConversion.psd1" -Force

$inputFolder = @(
    "$PSScriptRoot/../tests/CodeConversion.Tests/Languages/CSharpToPowerShell/CSharp"
    "$PSScriptRoot/../tests/CodeConversion.Tests/Languages/CSharpToPowerShell5/CSharp"
)

$outputTypeFolder = "$PSScriptRoot/generated/type"
if (-not (Test-Path $outputTypeFolder)) { New-Item -ItemType Directory -Path $outputTypeFolder }

Get-ChildItem -Path $inputFolder -File -Filter *.cs | ForEach-Object {
    $outputTypeModule = $outputTypeFolder | Join-Path -ChildPath $_.Name.Replace('.cs', '.psm1')
    Invoke-CSharpConversion -InputFile $_.FullName -OutputFile $outputTypeModule -As Type
}

<#
if (-not (Test-Path $outputFuncFolder)) { New-Item -ItemType Directory -Path $outputFuncFolder }
$outputFuncFolder = "$PSScriptRoot/generated/function"
Get-ChildItem -Path $inputFolder -File -Filter *.cs | ForEach-Object {
    $outputFuncScript = $outputFuncFolder | Join-Path -ChildPath $_.Name.Replace('.cs', '.ps1')
    Invoke-CSharpConversion -InputFile $_.FullName -OutputFile $outputFuncScript 
}
#>