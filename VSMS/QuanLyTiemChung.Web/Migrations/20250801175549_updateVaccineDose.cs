using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTiemChung.Web.Migrations
{
    /// <inheritdoc />
    public partial class updateVaccineDose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAgeInMonths",
                table: "VaccineDoses");

            migrationBuilder.DropColumn(
                name: "MinAgeInMonths",
                table: "VaccineDoses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
