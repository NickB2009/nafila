# Test the GetPublicSalonsService directly to verify WeeklyBusinessHours fix
Write-Host "Testing GetPublicSalonsService directly..." -ForegroundColor Green

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

var host = builder.Build();

try
{
    Console.WriteLine("Testing GetPublicSalonsService...");
    
    var service = host.Services.GetRequiredService<GetPublicSalonsService>();
    var result = await service.ExecuteAsync(CancellationToken.None);
    
    Console.WriteLine($"Success! Found {result.Salons.Count} salons");
    
    foreach (var salon in result.Salons)
    {
        Console.WriteLine($"- {salon.Name} (ID: {salon.Id})");
        Console.WriteLine($"  Is Open: {salon.IsOpen}");
        Console.WriteLine($"  Business Hours: {string.Join(", ", salon.BusinessHours.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}
'@

# Write the test code to a file
$testCode | Out-File -FilePath "TestService.cs" -Encoding UTF8

# Compile and run the test
Write-Host "Compiling test..." -ForegroundColor Yellow
dotnet run --project . --configuration Debug -- TestService.cs

# Clean up
Remove-Item "TestService.cs" -ErrorAction SilentlyContinue
