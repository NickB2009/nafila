using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grande.Fila.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyBusinessHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "FridayCloseTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FridayIsOpen",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "FridayOpenTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MondayCloseTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MondayIsOpen",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MondayOpenTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SaturdayCloseTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SaturdayIsOpen",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SaturdayOpenTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SundayCloseTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SundayIsOpen",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SundayOpenTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ThursdayCloseTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ThursdayIsOpen",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ThursdayOpenTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TuesdayCloseTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TuesdayIsOpen",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TuesdayOpenTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WednesdayCloseTime",
                table: "Locations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WednesdayIsOpen",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WednesdayOpenTime",
                table: "Locations",
                type: "time",
                nullable: true);

            // Populate new weekly hours columns with existing BusinessHours data
            // Monday through Saturday: use existing business hours
            // Sunday: closed (matching current hardcoded behavior)
            migrationBuilder.Sql(@"
                UPDATE Locations SET
                    MondayIsOpen = 1,
                    MondayOpenTime = BusinessHoursStart,
                    MondayCloseTime = BusinessHoursEnd,
                    TuesdayIsOpen = 1,
                    TuesdayOpenTime = BusinessHoursStart,
                    TuesdayCloseTime = BusinessHoursEnd,
                    WednesdayIsOpen = 1,
                    WednesdayOpenTime = BusinessHoursStart,
                    WednesdayCloseTime = BusinessHoursEnd,
                    ThursdayIsOpen = 1,
                    ThursdayOpenTime = BusinessHoursStart,
                    ThursdayCloseTime = BusinessHoursEnd,
                    FridayIsOpen = 1,
                    FridayOpenTime = BusinessHoursStart,
                    FridayCloseTime = BusinessHoursEnd,
                    SaturdayIsOpen = 1,
                    SaturdayOpenTime = BusinessHoursStart,
                    SaturdayCloseTime = BusinessHoursEnd,
                    SundayIsOpen = 0,
                    SundayOpenTime = NULL,
                    SundayCloseTime = NULL
                WHERE BusinessHoursStart IS NOT NULL AND BusinessHoursEnd IS NOT NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FridayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "FridayIsOpen",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "FridayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MondayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MondayIsOpen",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MondayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SaturdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SaturdayIsOpen",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SaturdayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SundayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SundayIsOpen",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SundayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ThursdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ThursdayIsOpen",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ThursdayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TuesdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TuesdayIsOpen",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TuesdayOpenTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "WednesdayCloseTime",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "WednesdayIsOpen",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "WednesdayOpenTime",
                table: "Locations");
        }
    }
}
