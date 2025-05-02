# PowerShell script to fix Django query.py with verbose output
$queryFilePath = "C:\repos\nafila\venv\Lib\site-packages\django\db\models\sql\query.py"

Write-Host "Fixing Django query.py file at: $queryFilePath"

if (Test-Path $queryFilePath) {
    Write-Host "File exists. Reading content..."
    try {
        $content = Get-Content -Path $queryFilePath -Raw -Encoding UTF8
        Write-Host "Content read successfully. Length: $($content.Length) characters"
        
        # Search for the problematic line
        $lines = $content -split "`n"
        $lineFound = $false
        
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match "childisinstance") {
                Write-Host "Found problematic line at line $($i+1): $($lines[$i])"
                $lineFound = $true
                break
            }
        }
        
        if (-not $lineFound) {
            Write-Host "Could not find the problematic line with 'childisinstance'"
            # Let's check 20 lines around line 109
            Write-Host "Checking lines around line 109:"
            for ($i = [Math]::Max(0, 109-10); $i -lt [Math]::Min($lines.Length, 109+10); $i++) {
                Write-Host "Line $($i+1): $($lines[$i])"
            }
        } else {
            # Fix the problematic line
            Write-Host "Replacing problematic text pattern..."
            $fixedContent = $content -replace "childisinstance\(\.name, str\) and \.isinstance\(name, str\) and name\.replace", "child.name and isinstance(child.name, str) and child.name.replace"
            
            if ($fixedContent -eq $content) {
                Write-Host "Warning: No changes were made to the content. Pattern might not match."
            } else {
                # Write the fixed content back
                Write-Host "Writing fixed content back to file..."
                $fixedContent | Set-Content -Path $queryFilePath -Encoding UTF8
                Write-Host "Successfully fixed Django query.py"
            }
        }
    } catch {
        Write-Host "Error processing file: $_"
    }
} else {
    Write-Host "File does not exist: $queryFilePath"
} 