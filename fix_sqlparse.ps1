# PowerShell script to fix SQLParse output.py
$sqlparseFilePath = "C:\repos\nafila\venv\Lib\site-packages\sqlparse\filters\output.py"

Write-Host "Fixing SQLParse output.py file at: $sqlparseFilePath"

if (Test-Path $sqlparseFilePath) {
    $content = Get-Content -Path $sqlparseFilePath -Raw -Encoding UTF8
    
    # Fix the problematic line
    $fixedContent = $content -replace "tokenisinstance\(\.value, str\) and \.isinstance\(value, str\) and value\.replace", "token.value and isinstance(token.value, str) and token.value.replace"
    
    # Write the fixed content back
    $fixedContent | Set-Content -Path $sqlparseFilePath -Encoding UTF8
    
    Write-Host "Successfully fixed SQLParse output.py"
} else {
    Write-Host "File does not exist: $sqlparseFilePath"
} 