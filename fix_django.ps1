# PowerShell script to fix Django field __init__.py
$djangoFilePath = "C:\repos\nafila\venv\Lib\site-packages\django\db\models\fields\__init__.py"

Write-Host "Fixing Django file at: $djangoFilePath"

# Read the file content as UTF-8
$content = Get-Content -Path $djangoFilePath -Raw -Encoding UTF8

# Find the line number of the problematic line
$lines = $content -split "`n"
$lineNumber = -1
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -match "selfisinstance") {
        $lineNumber = $i
        Write-Host "Found problematic line at line $($lineNumber + 1): $($lines[$i])"
        break
    }
}

if ($lineNumber -ne -1) {
    # Fix the line
    $lines[$lineNumber] = $lines[$lineNumber] -replace "selfisinstance\(\.name, str\) and \.isinstance\(name, str\)", "self.name and isinstance(self.name, str)"
    
    # Join the lines back together
    $newContent = $lines -join "`n"
    
    # Write the fixed content back
    $newContent | Set-Content -Path $djangoFilePath -Encoding UTF8
    
    Write-Host "Successfully fixed Django field __init__.py"
} else {
    Write-Host "Could not find the problematic line in the file."
} 