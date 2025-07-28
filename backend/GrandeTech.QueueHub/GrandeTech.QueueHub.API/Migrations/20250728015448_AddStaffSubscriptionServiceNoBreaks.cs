using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grande.Fila.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffSubscriptionServiceNoBreaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicesOffered",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TimesProvided = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ActualAverageDurationMinutes = table.Column<double>(type: "float", nullable: false, defaultValue: 30.0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesOffered", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsOnDuty = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    StaffStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "available"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AverageServiceTimeInMinutes = table.Column<double>(type: "float", nullable: false, defaultValue: 30.0),
                    CompletedServicesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EmployeeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SpecialtyServiceTypeIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MonthlyPriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    YearlyPriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YearlyPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxLocations = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxStaffPerLocation = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    IncludesAnalytics = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IncludesAdvancedReporting = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IncludesCustomBranding = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IncludesAdvertising = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IncludesMultipleLocations = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxQueueEntriesPerDay = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicesOffered_EstimatedDuration",
                table: "ServicesOffered",
                column: "EstimatedDurationMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesOffered_IsActive",
                table: "ServicesOffered",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesOffered_Location_Active",
                table: "ServicesOffered",
                columns: new[] { "LocationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ServicesOffered_LocationId",
                table: "ServicesOffered",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesOffered_Name",
                table: "ServicesOffered",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_Email",
                table: "StaffMembers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_EmployeeCode",
                table: "StaffMembers",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_IsActive",
                table: "StaffMembers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_LocationId",
                table: "StaffMembers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_Name",
                table: "StaffMembers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_PhoneNumber",
                table: "StaffMembers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_UserId",
                table: "StaffMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_Username",
                table: "StaffMembers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsActive",
                table: "SubscriptionPlans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsDefault",
                table: "SubscriptionPlans",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsFeatured",
                table: "SubscriptionPlans",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Price",
                table: "SubscriptionPlans",
                column: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicesOffered");

            migrationBuilder.DropTable(
                name: "StaffMembers");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");
        }
    }
}
