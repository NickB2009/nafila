# PowerShell script to fix Django models/__init__.py
$djangoFilePath = "C:\repos\nafila\venv\Lib\site-packages\django\db\models\__init__.py"

Write-Host "Fixing Django models/__init__.py file at: $djangoFilePath"

if (Test-Path $djangoFilePath) {
    $content = Get-Content -Path $djangoFilePath -Raw -Encoding UTF8
    $lines = $content -split "`n"
    
    # Check if we need to add the missing lines
    if ($lines.Count -gt 2 -and $lines[0] -match "from django.core.exceptions import ObjectDoesNotExist" -and 
        $lines[1] -notmatch "from django.db.models import signals") {
        
        Write-Host "Lines 2 and 3 are missing, adding them now"
        
        # Construct the new file content with the missing lines
        $newContent = $lines[0] + "`n" +
                      "from django.db.models import signals`n" +
                      "from django.db.models.aggregates import *  # NOQA`n"
        
        # Add the rest of the lines starting from index 1 (skipping the aggregates import which is now line 3)
        for ($i = 1; $i -lt $lines.Length; $i++) {
            if ($i -eq 3) {
                # We've already added the aggregates import, so skip it
                continue
            }
            $newContent += $lines[$i] + "`n"
        }
        
        # Write the fixed content back
        $newContent | Set-Content -Path $djangoFilePath -Encoding UTF8
        
        Write-Host "Successfully fixed Django models/__init__.py"
    } else {
        Write-Host "File structure doesn't match the expected pattern or lines already exist"
    }
} else {
    Write-Host "File does not exist: $djangoFilePath"
} 