# Simple PowerShell script to test the service directly
Write-Host "Creating a simple test to debug GetPublicSalonsService..." -ForegroundColor Green

# Create a simple C# test program
$testCode = @'
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Grande.Fila.API.Application.Public;
using Grande.Fila.API.Infrastructure;

var builder = Host.CreateApplicationBuilder();

// Add configuration
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// Add infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add logging
builder.Services.AddLogging(logging => {
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

var app = builder.Build();

try {
    WriteLine("Testing GetPublicSalonsService...");
    
    using var scope = app.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<GetPublicSalonsService>();
    
    var result = await service.ExecuteAsync();
    
    if (result.Success) {
        WriteLine($"Success! Found {result.Salons.Count} salons");
        foreach (var salon in result.Salons) {
            WriteLine($"  - {salon.Name} (ID: {salon.Id})");
        }
    } else {
        WriteLine("Failed!");
        foreach (var error in result.Errors) {
            WriteLine($"  Error: {error}");
        }
    }
} catch (Exception ex) {
    WriteLine($"Exception: {ex.Message}");
    WriteLine($"Stack Trace: {ex.StackTrace}");
}
'@

$testCode | Out-File -FilePath "test-service.cs" -Encoding UTF8

Write-Host "Test code created. Now let's run it..." -ForegroundColor Yellow

# Run the test
try {
    dotnet script test-service.cs
} catch {
    Write-Host "Error running test: $_" -ForegroundColor Red
}
