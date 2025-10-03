using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grande.Fila.API.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityFlattening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing flattened weekly hours columns for Locations
            migrationBuilder.AddColumn<TimeSpan>(
                name: "MondayOpenTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MondayCloseTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MondayIsClosed",
                table: "Locations",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TuesdayOpenTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TuesdayCloseTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TuesdayIsClosed",
                table: "Locations",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WednesdayOpenTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WednesdayCloseTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WednesdayIsClosed",
                table: "Locations",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ThursdayOpenTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ThursdayCloseTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ThursdayIsClosed",
                table: "Locations",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "FridayOpenTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "FridayCloseTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FridayIsClosed",
                table: "Locations",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SaturdayOpenTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SaturdayCloseTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SaturdayIsClosed",
                table: "Locations",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SundayOpenTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SundayCloseTime",
                table: "Locations",
                type: "TIME",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SundayIsClosed",
                table: "Locations",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            // Remove the old conflicting Price column from SubscriptionPlans
            migrationBuilder.DropColumn(
                name: "Price",
                table: "SubscriptionPlans");

            // Remove the old WeeklyBusinessHours JSON column from Locations
            migrationBuilder.DropColumn(
                name: "WeeklyBusinessHours",
                table: "Locations");

            // Remove the old BusinessHoursStart and BusinessHoursEnd columns from Locations
            migrationBuilder.DropColumn(
                name: "BusinessHoursStart",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "BusinessHoursEnd",
                table: "Locations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back the old columns for rollback
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "WeeklyBusinessHours",
                table: "Locations",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BusinessHoursStart",
                table: "Locations",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BusinessHoursEnd",
                table: "Locations",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            // Remove the flattened weekly hours columns
            migrationBuilder.DropColumn(
                name: "MondayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MondayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MondayIsClosed",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TuesdayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TuesdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TuesdayIsClosed",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "WednesdayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "WednesdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "WednesdayIsClosed",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ThursdayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ThursdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ThursdayIsClosed",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "FridayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "FridayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "FridayIsClosed",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SaturdayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SaturdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SaturdayIsClosed",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SundayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SundayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SundayIsClosed",
                table: "Locations");
        }
    }
}
