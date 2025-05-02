# PowerShell script to check Django models/__init__.py
$djangoFilePath = "C:\repos\nafila\venv\Lib\site-packages\django\db\models\__init__.py"

Write-Host "Checking Django models/__init__.py file at: $djangoFilePath"

if (Test-Path $djangoFilePath) {
    $content = Get-Content -Path $djangoFilePath -Raw -Encoding UTF8
    Write-Host "File exists and contains $($content.Length) characters"
    
    # Display the file content with line numbers
    $lines = $content -split "`n"
    for ($i = 0; $i -lt $lines.Length; $i++) {
        Write-Host "$($i+1): $($lines[$i])"
    }
} else {
    Write-Host "File does not exist: $djangoFilePath"
} 