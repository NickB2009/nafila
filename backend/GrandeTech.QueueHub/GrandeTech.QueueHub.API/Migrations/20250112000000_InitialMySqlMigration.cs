using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grande.Fila.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySqlMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MonthlyPriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPriceCurrency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    YearlyPriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YearlyPriceCurrency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    MaxLocations = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxStaffPerLocation = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    IncludesAnalytics = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IncludesAdvancedReporting = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IncludesCustomBranding = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IncludesAdvertising = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IncludesMultipleLocations = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    MaxQueueEntriesPerDay = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsFeatured = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: false, defaultValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsLocked = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RequiresTwoFactor = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: false, defaultValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: true),
                    ContactPhone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    BrandingPrimaryColor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    BrandingSecondaryColor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    BrandingLogoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    BrandingFaviconUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    BrandingCompanyName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    BrandingTagLine = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    BrandingFontFamily = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    SubscriptionPlanId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SharesDataForAnalytics = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    LocationIds = table.Column<string>(type: "json", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: false, defaultValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    NotificationsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    PreferredNotificationChannel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    UserId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    FavoriteLocationIds = table.Column<string>(type: "json", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    AddressStreet = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    AddressNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    AddressComplement = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    AddressNeighborhood = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    AddressCity = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    AddressState = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    AddressCountry = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    AddressPostalCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    AddressLatitude = table.Column<double>(type: "double", nullable: true),
                    AddressLongitude = table.Column<double>(type: "double", nullable: true),
                    ContactPhone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ContactEmail = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: true),
                    CustomBrandingPrimaryColor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CustomBrandingSecondaryColor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CustomBrandingLogoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CustomBrandingFaviconUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CustomBrandingCompanyName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CustomBrandingTagLine = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CustomBrandingFontFamily = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    BusinessHoursStart = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    BusinessHoursEnd = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    IsQueueEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    MaxQueueSize = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    LateClientCapTimeInMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    AverageServiceTimeInMinutes = table.Column<double>(type: "double", nullable: false, defaultValue: 30.0),
                    LastAverageTimeReset = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    StaffMemberIds = table.Column<string>(type: "json", nullable: false),
                    ServiceTypeIds = table.Column<string>(type: "json", nullable: false),
                    AdvertisementIds = table.Column<string>(type: "json", nullable: false),
                    WeeklyBusinessHours = table.Column<string>(type: "longtext", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: false, defaultValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "ServicesOffered",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LocationId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PriceCurrency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    TimesProvided = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ActualAverageDurationMinutes = table.Column<double>(type: "double", nullable: false, defaultValue: 30.0),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: false, defaultValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesOffered", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicesOffered_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "StaffMembers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    LocationId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    Email = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    IsOnDuty = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    StaffStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "available"),
                    UserId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    AverageServiceTimeInMinutes = table.Column<double>(type: "double", nullable: false, defaultValue: 30.0),
                    CompletedServicesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EmployeeCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    SpecialtyServiceTypeIds = table.Column<string>(type: "json", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: false, defaultValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffMembers_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Queues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    LocationId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    QueueDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    MaxSize = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    LateClientCapTimeInMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queues_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "QueueEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    QueueId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    CustomerId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    CustomerName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    StaffMemberId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true),
                    ServiceTypeId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true),
                    Notes = table.Column<string>(type: "text", maxLength: 1000, nullable: true),
                    EnteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CalledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CheckedInAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ServiceDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    EstimatedStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CustomerNotes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    StaffNotes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    TokenNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    NotificationSent = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    NotificationSentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NotificationChannel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Queues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "Queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QueueEntries_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "CustomerServiceHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    CustomerId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    LocationId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    StaffMemberId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    ServiceTypeId = table.Column<string>(type: "char(36)", maxLength: 36, nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Feedback = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerServiceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerServiceHistory_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsAnonymous",
                table: "Customers",
                column: "IsAnonymous");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerServiceHistory_CustomerId",
                table: "CustomerServiceHistory",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerServiceHistory_LocationId",
                table: "CustomerServiceHistory",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerServiceHistory_ServiceDate",
                table: "CustomerServiceHistory",
                column: "ServiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ContactEmail",
                table: "Locations",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_IsActive",
                table: "Locations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_IsQueueEnabled",
                table: "Locations",
                column: "IsQueueEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_OrganizationId",
                table: "Locations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Slug",
                table: "Locations",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ContactEmail",
                table: "Organizations",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_IsActive",
                table: "Organizations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Slug",
                table: "Organizations",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_SubscriptionPlanId",
                table: "Organizations",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_CustomerId",
                table: "QueueEntries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_EnteredAt",
                table: "QueueEntries",
                column: "EnteredAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_Position",
                table: "QueueEntries",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_QueueId",
                table: "QueueEntries",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_Status",
                table: "QueueEntries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_Status_Position",
                table: "QueueEntries",
                columns: new[] { "Status", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Queues_IsActive",
                table: "Queues",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_LocationId",
                table: "Queues",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_QueueDate",
                table: "Queues",
                column: "QueueDate");

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
                name: "IX_StaffMembers_EmployeeCode",
                table: "StaffMembers",
                column: "EmployeeCode",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerServiceHistory");

            migrationBuilder.DropTable(
                name: "QueueEntries");

            migrationBuilder.DropTable(
                name: "ServicesOffered");

            migrationBuilder.DropTable(
                name: "StaffMembers");

            migrationBuilder.DropTable(
                name: "Queues");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");
        }
    }
}
