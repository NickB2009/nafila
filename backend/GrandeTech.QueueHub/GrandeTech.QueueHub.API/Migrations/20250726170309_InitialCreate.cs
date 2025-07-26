using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grande.Fila.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Queues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxSize = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    LateClientCapTimeInMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
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
                    table.PrimaryKey("PK_Queues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueueEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StaffMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckedInAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    EstimatedStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StaffNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TokenNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotificationSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NotificationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationChannel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_QueueEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueEntries_Queues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "Queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_CustomerId",
                table: "QueueEntries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_EnteredAt",
                table: "QueueEntries",
                column: "EnteredAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_QueueId",
                table: "QueueEntries",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_QueueId_Position",
                table: "QueueEntries",
                columns: new[] { "QueueId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_QueueId_Status",
                table: "QueueEntries",
                columns: new[] { "QueueId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_ServiceTypeId",
                table: "QueueEntries",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_StaffMemberId",
                table: "QueueEntries",
                column: "StaffMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_Status",
                table: "QueueEntries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_IsActive",
                table: "Queues",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_LocationId",
                table: "Queues",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_LocationId_QueueDate",
                table: "Queues",
                columns: new[] { "LocationId", "QueueDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Queues_QueueDate",
                table: "Queues",
                column: "QueueDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueEntries");

            migrationBuilder.DropTable(
                name: "Queues");
        }
    }
}
