# PowerShell script to fix Django query.py
$queryFilePath = "C:\repos\nafila\venv\Lib\site-packages\django\db\models\sql\query.py"

Write-Host "Fixing Django query.py file at: $queryFilePath"

if (Test-Path $queryFilePath) {
    $content = Get-Content -Path $queryFilePath -Raw -Encoding UTF8
    
    # Fix the problematic line with the "childisinstance" syntax error
    $fixedContent = $content -replace "childisinstance\(\.name, str\) and \.isinstance\(name, str\) and name\.replace", "child.name and isinstance(child.name, str) and child.name.replace"
    
    # Write the fixed content back
    $fixedContent | Set-Content -Path $queryFilePath -Encoding UTF8
    
    Write-Host "Successfully fixed Django query.py"
} else {
    Write-Host "File does not exist: $queryFilePath"
} 