#!/usr/bin/env pwsh
# Export data from local Docker SQL Server to Azure SQL Database

Write-Host "📤 Exporting data from local Docker SQL Server..." -ForegroundColor Green

# Export data using sqlcmd
$exportPath = ".\local-data-export.sql"

Write-Host "Exporting schema and data..." -ForegroundColor Yellow

# Create the SQL query as a properly escaped string
$sqlQuery = @"
SET NOCOUNT ON;
GO

-- Export Organizations
SELECT 'INSERT INTO Organizations (Id, Name, Slug, ContactInfo_Phone, ContactInfo_Email, ContactInfo_Address, Branding_PrimaryColor, Branding_LogoUrl, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + REPLACE(Name, '''', '''''') + ''', ' +
       '''' + REPLACE(Slug, '''', '''''') + ''', ' +
       '''' + REPLACE(ContactInfo_Phone, '''', '''''') + ''', ' +
       '''' + REPLACE(ContactInfo_Email, '''', '''''') + ''', ' +
       '''' + REPLACE(ContactInfo_Address, '''', '''''') + ''', ' +
       '''' + REPLACE(Branding_PrimaryColor, '''', '''''') + ''', ' +
       '''' + REPLACE(Branding_LogoUrl, '''', '''''') + ''', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM Organizations
WHERE IsDeleted = 0;

-- Export Users
SELECT 'INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + REPLACE(Username, '''', '''''') + ''', ' +
       '''' + REPLACE(Email, '''', '''''') + ''', ' +
       '''' + REPLACE(PasswordHash, '''', '''''') + ''', ' +
       '''' + REPLACE(Role, '''', '''''') + ''', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM Users
WHERE IsDeleted = 0;

-- Export Locations
SELECT 'INSERT INTO Locations (Id, OrganizationId, Name, Address_Street, Address_City, Address_State, Address_ZipCode, Address_Country, BusinessHours_OpenTime, BusinessHours_CloseTime, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + CAST(OrganizationId AS NVARCHAR(36)) + ''', ' +
       '''' + REPLACE(Name, '''', '''''') + ''', ' +
       '''' + REPLACE(Address_Street, '''', '''''') + ''', ' +
       '''' + REPLACE(Address_City, '''', '''''') + ''', ' +
       '''' + REPLACE(Address_State, '''', '''''') + ''', ' +
       '''' + REPLACE(Address_ZipCode, '''', '''''') + ''', ' +
       '''' + REPLACE(Address_Country, '''', '''''') + ''', ' +
       '''' + CONVERT(NVARCHAR, BusinessHours_OpenTime, 108) + ''', ' +
       '''' + CONVERT(NVARCHAR, BusinessHours_CloseTime, 108) + ''', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM Locations
WHERE IsDeleted = 0;

-- Export SubscriptionPlans
SELECT 'INSERT INTO SubscriptionPlans (Id, Name, Description, Price_Amount, Price_Currency, Features, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + REPLACE(Name, '''', '''''') + ''', ' +
       '''' + REPLACE(Description, '''', '''''') + ''', ' +
       CAST(Price_Amount AS NVARCHAR(10)) + ', ' +
       '''' + REPLACE(Price_Currency, '''', '''''') + ''', ' +
       '''' + REPLACE(Features, '''', '''''') + ''', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM SubscriptionPlans
WHERE IsDeleted = 0;

-- Export ServicesOffered
SELECT 'INSERT INTO ServicesOffered (Id, OrganizationId, Name, Description, Duration, Price_Amount, Price_Currency, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + CAST(OrganizationId AS NVARCHAR(36)) + ''', ' +
       '''' + REPLACE(Name, '''', '''''') + ''', ' +
       '''' + REPLACE(Description, '''', '''''') + ''', ' +
       CAST(Duration AS NVARCHAR(10)) + ', ' +
       CAST(Price_Amount AS NVARCHAR(10)) + ', ' +
       '''' + REPLACE(Price_Currency, '''', '''''') + ''', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM ServicesOffered
WHERE IsDeleted = 0;

-- Export StaffMembers
SELECT 'INSERT INTO StaffMembers (Id, OrganizationId, UserId, Name, Email, Phone, Skills, IsAvailable, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + CAST(OrganizationId AS NVARCHAR(36)) + ''', ' +
       '''' + CAST(UserId AS NVARCHAR(36)) + ''', ' +
       '''' + REPLACE(Name, '''', '''''') + ''', ' +
       '''' + REPLACE(Email, '''', '''''') + ''', ' +
       '''' + REPLACE(Phone, '''', '''''') + ''', ' +
       '''' + REPLACE(Skills, '''', '''''') + ''', ' +
       CAST(IsAvailable AS NVARCHAR(1)) + ', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM StaffMembers
WHERE IsDeleted = 0;

-- Export Queues
SELECT 'INSERT INTO Queues (Id, LocationId, Date, Status, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + CAST(LocationId AS NVARCHAR(36)) + ''', ' +
       '''' + CONVERT(NVARCHAR, Date, 23) + ''', ' +
       '''' + REPLACE(Status, '''', '''''') + ''', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM Queues
WHERE IsDeleted = 0;

-- Export QueueEntries
SELECT 'INSERT INTO QueueEntries (Id, QueueId, CustomerId, Position, Status, EstimatedWaitTime, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy) VALUES (' +
       '''' + CAST(Id AS NVARCHAR(36)) + ''', ' +
       '''' + CAST(QueueId AS NVARCHAR(36)) + ''', ' +
       '''' + CAST(CustomerId AS NVARCHAR(36)) + ''', ' +
       CAST(Position AS NVARCHAR(10)) + ', ' +
       '''' + REPLACE(Status, '''', '''''') + ''', ' +
       CAST(EstimatedWaitTime AS NVARCHAR(10)) + ', ' +
       '''' + CONVERT(NVARCHAR, CreatedAt, 121) + ''', ' +
       '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
       CASE WHEN LastModifiedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, LastModifiedAt, 121) + '''' END + ', ' +
       CASE WHEN LastModifiedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(LastModifiedBy, '''', '''''') + '''' END + ', ' +
       CAST(IsDeleted AS NVARCHAR(1)) + ', ' +
       CASE WHEN DeletedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(NVARCHAR, DeletedAt, 121) + '''' END + ', ' +
       CASE WHEN DeletedBy IS NULL THEN 'NULL' ELSE '''' + REPLACE(DeletedBy, '''', '''''') + '''' END + ');'
FROM QueueEntries
WHERE IsDeleted = 0;
"@

# Execute the SQL query and save to file
docker exec nafila-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!" -d GrandeTechQueueHub -Q $sqlQuery > $exportPath

Write-Host "✅ Data exported to $exportPath" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "1. Connect to Azure SQL Database using SQL Server Management Studio" -ForegroundColor White
Write-Host "2. Run the generated SQL script: $exportPath" -ForegroundColor White
Write-Host "3. Or use the Azure CLI to import the data" -ForegroundColor White