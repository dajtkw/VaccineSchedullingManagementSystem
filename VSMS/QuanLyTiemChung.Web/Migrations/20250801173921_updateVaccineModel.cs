using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTiemChung.Web.Migrations
{
    /// <inheritdoc />
    public partial class updateVaccineModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contraindications",
                table: "Vaccines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfOrigin",
                table: "Vaccines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Vaccines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Indications",
                table: "Vaccines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Vaccines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxAge",
                table: "Vaccines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinAge",
                table: "Vaccines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Vaccines",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAgeInMonths",
                table: "VaccineDoses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinAgeInMonths",
                table: "VaccineDoses",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contraindications",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "CountryOfOrigin",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Indications",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "MaxAge",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "MinAge",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "MaxAgeInMonths",
                table: "VaccineDoses");

            migrationBuilder.DropColumn(
                name: "MinAgeInMonths",
                table: "VaccineDoses");
        }
    }
}
