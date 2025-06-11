# Fix namespace references in test files
$testFile = "c:\git\eutonafila\nafila\backend\GrandeTech.QueueHub.API\GrandeTech.QueueHub.Tests\Integration\ServiceTypesControllerTests.cs"

# Read the file content
$content = Get-Content $testFile -Raw

# Replace the references
$content = $content -replace '\bnew AddServiceTypeRequest\b', 'new ApplicationServices.AddServiceTypeRequest'
$content = $content -replace '\bnew UpdateServiceTypeRequest\b', 'new ApplicationServices.UpdateServiceTypeRequest'  
$content = $content -replace '\bAddServiceTypeResult\b', 'ApplicationServices.AddServiceTypeResult'
$content = $content -replace '\bServiceTypeDto\b', 'ApplicationServices.ServiceTypeDto'

# Write back the content
Set-Content $testFile -Value $content -NoNewline

Write-Host "Fixed references in ServiceTypesControllerTests.cs"
