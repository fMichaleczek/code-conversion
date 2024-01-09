function Invoke-CSharpConversion
{
    [OutputType(ParameterSetName = 'Code', [string])]
    [OutputType(ParameterSetName = 'FileSystem', [void])]
    [CmdletBinding(DefaultParameterSetName = 'Code')]
    param
    (
        [Parameter(ParameterSetName = 'Code', Position = 0, Mandatory)]
        [Alias('Code')]
        [string]
        $InputCode,
        
        [Parameter(ParameterSetName = 'FileSystem', Position = 0, Mandatory)]
        [Alias('InFile')]
        [string]
        $InputFile,
        
        [Parameter(Position = 1, ParameterSetName = 'Code')]
        [Parameter(Position = 1, ParameterSetName = 'FileSystem')]
        [Alias('OutFile')]
        [string]
        $OutputFile,
        
        [Parameter(Position = 2)]
        [ValidateSet('Function', 'Type')]
        [string] 
        $As
    )
    
    begin
    {
        $sep = '-' * 80
        
        $parser = [CodeConversion.CSharpSyntaxTreeVisitor]::new()
    
        if ($PSBoundParameters.ContainsKey('As') -and $As -eq 'Type')
        {
            $writer = [CodeConversion.PowerShell5CodeWriter]::new()
        }
        else
        {
            $writer = [CodeConversion.PowerShellCodeWriter]::new()
        }
        
    }
    
    process
    {
        if ($PSBoundParameters.ContainsKey('InputFile'))
        {
            $InputCode = Get-Content -Path $InputFile -Encoding UTF8 -Raw
        }
        
        Write-Verbose "C# Input:`n$sep`n$InputCode`n$sep`n"
        
        try 
        {
            $ast = $parser.Visit($InputCode)
        }
        catch 
        {
            $PSCmdlet.ThrowTerminatingError($_)
        }
        
        try 
        {
            $output = $writer.Write($ast)
        
        }
        catch 
        {
            $PSCmdlet.ThrowTerminatingError($_)
        }
                
        if ($PSBoundParameters.ContainsKey('OutputFile'))
        {
            Set-Content -Path $OutputFile -Value $output -Encoding UTF8 -Force
        }
        else
        {
            $output 
        }
        
        Write-Verbose "PowerShell Output:`n$sep`n$output`n$sep"
    }
    
    end
    {
        
    }
}
