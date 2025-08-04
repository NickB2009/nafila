using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grande.Fila.API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToJsonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, add the new JSON column
            migrationBuilder.AddColumn<string>(
                name: "WeeklyBusinessHours",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Migrate existing data from 21 columns to JSON format using string concatenation
            migrationBuilder.Sql(@"
                UPDATE Locations SET WeeklyBusinessHours = 
                '{""monday"":{""isOpen"":' + 
                    CASE WHEN MondayIsOpen = 1 THEN 'true' ELSE 'false' END + 
                    CASE WHEN MondayIsOpen = 1 
                        THEN ',""openTime"":""' + FORMAT(MondayOpenTime, 'hh\:mm\:ss') + '"",""closeTime"":""' + FORMAT(MondayCloseTime, 'hh\:mm\:ss') + '""'
                        ELSE ',""openTime"":null,""closeTime"":null'
                    END +
                '},""tuesday"":{""isOpen"":' + 
                    CASE WHEN TuesdayIsOpen = 1 THEN 'true' ELSE 'false' END + 
                    CASE WHEN TuesdayIsOpen = 1 
                        THEN ',""openTime"":""' + FORMAT(TuesdayOpenTime, 'hh\:mm\:ss') + '"",""closeTime"":""' + FORMAT(TuesdayCloseTime, 'hh\:mm\:ss') + '""'
                        ELSE ',""openTime"":null,""closeTime"":null'
                    END +
                '},""wednesday"":{""isOpen"":' + 
                    CASE WHEN WednesdayIsOpen = 1 THEN 'true' ELSE 'false' END + 
                    CASE WHEN WednesdayIsOpen = 1 
                        THEN ',""openTime"":""' + FORMAT(WednesdayOpenTime, 'hh\:mm\:ss') + '"",""closeTime"":""' + FORMAT(WednesdayCloseTime, 'hh\:mm\:ss') + '""'
                        ELSE ',""openTime"":null,""closeTime"":null'
                    END +
                '},""thursday"":{""isOpen"":' + 
                    CASE WHEN ThursdayIsOpen = 1 THEN 'true' ELSE 'false' END + 
                    CASE WHEN ThursdayIsOpen = 1 
                        THEN ',""openTime"":""' + FORMAT(ThursdayOpenTime, 'hh\:mm\:ss') + '"",""closeTime"":""' + FORMAT(ThursdayCloseTime, 'hh\:mm\:ss') + '""'
                        ELSE ',""openTime"":null,""closeTime"":null'
                    END +
                '},""friday"":{""isOpen"":' + 
                    CASE WHEN FridayIsOpen = 1 THEN 'true' ELSE 'false' END + 
                    CASE WHEN FridayIsOpen = 1 
                        THEN ',""openTime"":""' + FORMAT(FridayOpenTime, 'hh\:mm\:ss') + '"",""closeTime"":""' + FORMAT(FridayCloseTime, 'hh\:mm\:ss') + '""'
                        ELSE ',""openTime"":null,""closeTime"":null'
                    END +
                '},""saturday"":{""isOpen"":' + 
                    CASE WHEN SaturdayIsOpen = 1 THEN 'true' ELSE 'false' END + 
                    CASE WHEN SaturdayIsOpen = 1 
                        THEN ',""openTime"":""' + FORMAT(SaturdayOpenTime, 'hh\:mm\:ss') + '"",""closeTime"":""' + FORMAT(SaturdayCloseTime, 'hh\:mm\:ss') + '""'
                        ELSE ',""openTime"":null,""closeTime"":null'
                    END +
                '},""sunday"":{""isOpen"":' + 
                    CASE WHEN SundayIsOpen = 1 THEN 'true' ELSE 'false' END + 
                    CASE WHEN SundayIsOpen = 1 
                        THEN ',""openTime"":""' + FORMAT(SundayOpenTime, 'hh\:mm\:ss') + '"",""closeTime"":""' + FORMAT(SundayCloseTime, 'hh\:mm\:ss') + '""'
                        ELSE ',""openTime"":null,""closeTime"":null'
                    END +
                '}}'
                WHERE MondayIsOpen IS NOT NULL  -- Only update rows that have the old columns
            ");

            // Now drop the old columns
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeeklyBusinessHours",
                table: "Locations");

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
        }
    }
}
