# Get Azure Logs for QueueHub API
# This script retrieves various types of logs from Azure for troubleshooting

param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-p-queuehub-core-001",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServiceName = "app-p-queuehub-api-001",
    
    [Parameter(Mandatory=$false)]
    [string]$AppInsightsName = "appi-p-queuehub-api-001",
    
    [Parameter(Mandatory=$false)]
    [int]$Hours = 24,
    
    [Parameter(Mandatory=$false)]
    [string]$LogType = "all",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\logs"
)

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Retrieving Azure logs for QueueHub API..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "App Service: $AppServiceName" -ForegroundColor Yellow
Write-Host "App Insights: $AppInsightsName" -ForegroundColor Yellow
Write-Host "Time Range: Last $Hours hours" -ForegroundColor Yellow
Write-Host "Log Type: $LogType" -ForegroundColor Yellow

# Create output directory if it doesn't exist
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-Host "Created output directory: $OutputPath" -ForegroundColor Yellow
}

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$logPrefix = "queuehub-logs-$timestamp"

# Function to get App Service logs
function Get-AppServiceLogs {
    Write-Host "`n=== Retrieving App Service Logs ===" -ForegroundColor Cyan
    
    # Get application logs
    $appLogFile = "$OutputPath\$logPrefix-app.log"
    Write-Host "Getting application logs..." -ForegroundColor Yellow
    az webapp log download --name $AppServiceName --resource-group $ResourceGroupName --log-file $appLogFile
    
    if (Test-Path $appLogFile) {
        Write-Host "Application logs saved to: $appLogFile" -ForegroundColor Green
    } else {
        Write-Host "Warning: No application logs found" -ForegroundColor Yellow
    }
    
    # Get container logs
    $containerLogFile = "$OutputPath\$logPrefix-container.log"
    Write-Host "Getting container logs..." -ForegroundColor Yellow
    az webapp log container show --name $AppServiceName --resource-group $ResourceGroupName --output table > $containerLogFile
    
    if (Test-Path $containerLogFile) {
        Write-Host "Container logs saved to: $containerLogFile" -ForegroundColor Green
    } else {
        Write-Host "Warning: No container logs found" -ForegroundColor Yellow
    }
}

# Function to get Application Insights logs
function Get-AppInsightsLogs {
    Write-Host "`n=== Retrieving Application Insights Logs ===" -ForegroundColor Cyan
    
    $endTime = Get-Date
    $startTime = $endTime.AddHours(-$Hours)
    $startTimeStr = $startTime.ToString("yyyy-MM-ddTHH:mm:ssZ")
    $endTimeStr = $endTime.ToString("yyyy-MM-ddTHH:mm:ssZ")
    
    # Get exceptions
    $exceptionsFile = "$OutputPath\$logPrefix-exceptions.json"
    Write-Host "Getting exceptions..." -ForegroundColor Yellow
    $exceptionsQuery = "exceptions | where timestamp >= datetime('$startTimeStr') and timestamp <= datetime('$endTimeStr') | order by timestamp desc"
    az monitor app-insights query --app $AppInsightsName --resource-group $ResourceGroupName --analytics-query $exceptionsQuery --output json > $exceptionsFile
    
    if ((Get-Item $exceptionsFile).Length -gt 0) {
        Write-Host "Exceptions saved to: $exceptionsFile" -ForegroundColor Green
    } else {
        Write-Host "No exceptions found in the last $Hours hours" -ForegroundColor Yellow
    }
    
    # Get requests
    $requestsFile = "$OutputPath\$logPrefix-requests.json"
    Write-Host "Getting requests..." -ForegroundColor Yellow
    $requestsQuery = "requests | where timestamp >= datetime('$startTimeStr') and timestamp <= datetime('$endTimeStr') | order by timestamp desc"
    az monitor app-insights query --app $AppInsightsName --resource-group $ResourceGroupName --analytics-query $requestsQuery --output json > $requestsFile
    
    if ((Get-Item $requestsFile).Length -gt 0) {
        Write-Host "Requests saved to: $requestsFile" -ForegroundColor Green
    } else {
        Write-Host "No requests found in the last $Hours hours" -ForegroundColor Yellow
    }
    
    # Get traces
    $tracesFile = "$OutputPath\$logPrefix-traces.json"
    Write-Host "Getting traces..." -ForegroundColor Yellow
    $tracesQuery = "traces | where timestamp >= datetime('$startTimeStr') and timestamp <= datetime('$endTimeStr') | order by timestamp desc"
    az monitor app-insights query --app $AppInsightsName --resource-group $ResourceGroupName --analytics-query $tracesQuery --output json > $tracesFile
    
    if ((Get-Item $tracesFile).Length -gt 0) {
        Write-Host "Traces saved to: $tracesFile" -ForegroundColor Green
    } else {
        Write-Host "No traces found in the last $Hours hours" -ForegroundColor Yellow
    }
    
    # Get custom events
    $eventsFile = "$OutputPath\$logPrefix-events.json"
    Write-Host "Getting custom events..." -ForegroundColor Yellow
    $eventsQuery = "customEvents | where timestamp >= datetime('$startTimeStr') and timestamp <= datetime('$endTimeStr') | order by timestamp desc"
    az monitor app-insights query --app $AppInsightsName --resource-group $ResourceGroupName --analytics-query $eventsQuery --output json > $eventsFile
    
    if ((Get-Item $eventsFile).Length -gt 0) {
        Write-Host "Custom events saved to: $eventsFile" -ForegroundColor Green
    } else {
        Write-Host "No custom events found in the last $Hours hours" -ForegroundColor Yellow
    }
}

# Function to get diagnostic settings
function Get-DiagnosticLogs {
    Write-Host "`n=== Retrieving Diagnostic Logs ===" -ForegroundColor Cyan
    
    # Get App Service diagnostic settings
    $diagnosticFile = "$OutputPath\$logPrefix-diagnostic.json"
    Write-Host "Getting diagnostic settings..." -ForegroundColor Yellow
    az monitor diagnostic-settings list --resource "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Web/sites/$AppServiceName" --output json > $diagnosticFile
    
    if ((Get-Item $diagnosticFile).Length -gt 0) {
        Write-Host "Diagnostic settings saved to: $diagnosticFile" -ForegroundColor Green
    } else {
        Write-Host "No diagnostic settings found" -ForegroundColor Yellow
    }
}

# Function to get real-time logs
function Get-RealTimeLogs {
    Write-Host "`n=== Starting Real-Time Log Stream ===" -ForegroundColor Cyan
    Write-Host "Press Ctrl+C to stop the log stream" -ForegroundColor Yellow
    Write-Host "Starting log stream..." -ForegroundColor Yellow
    
    try {
        az webapp log tail --name $AppServiceName --resource-group $ResourceGroupName
    } catch {
        Write-Host "Log stream stopped or failed to start" -ForegroundColor Yellow
    }
}

# Main execution based on log type
switch ($LogType.ToLower()) {
    "appservice" {
        Get-AppServiceLogs
    }
    "appinsights" {
        Get-AppInsightsLogs
    }
    "diagnostic" {
        Get-DiagnosticLogs
    }
    "realtime" {
        Get-RealTimeLogs
    }
    "all" {
        Get-AppServiceLogs
        Get-AppInsightsLogs
        Get-DiagnosticLogs
    }
    default {
        Write-Host "Invalid log type. Use: appservice, appinsights, diagnostic, realtime, or all" -ForegroundColor Red
        exit 1
    }
}

# Create summary file
$summaryFile = "$OutputPath\$logPrefix-summary.txt"
$summaryContent = @"
QueueHub API Log Retrieval Summary
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Resource Group: $ResourceGroupName
App Service: $AppServiceName
App Insights: $AppInsightsName
Time Range: Last $Hours hours
Log Type: $LogType

Files Generated:
$(Get-ChildItem $OutputPath -Filter "$logPrefix*" | ForEach-Object { "- $($_.Name)" })

Next Steps:
1. Review the log files for errors and issues
2. Check Application Insights portal for detailed analytics
3. Set up alerts for critical errors
4. Monitor application performance metrics

Log Analysis Tips:
- Look for 5xx errors in requests logs
- Check for exceptions with high frequency
- Review traces for performance bottlenecks
- Monitor custom events for business logic issues
"@

$summaryContent | Out-File -FilePath $summaryFile -Encoding UTF8

Write-Host "`n=== LOG RETRIEVAL COMPLETE ===" -ForegroundColor Green
Write-Host "All logs saved to: $OutputPath" -ForegroundColor Cyan
Write-Host "Summary file: $summaryFile" -ForegroundColor Cyan
Write-Host "`nTo view logs in Azure Portal:" -ForegroundColor Yellow
Write-Host "App Service: https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Web/sites/$AppServiceName" -ForegroundColor White
Write-Host "App Insights: https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Insights/components/$AppInsightsName" -ForegroundColor White 