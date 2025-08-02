using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTiemChung.Web.Migrations
{
    /// <inheritdoc />
    public partial class updateVaccineDose2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntervalFromPreviousDoseInDays",
                table: "VaccineDoses",
                newName: "IntervalInMonths");

            migrationBuilder.AlterColumn<string>(
                name: "RecommendedAge",
                table: "VaccineDoses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "VaccineDoses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "VaccineDoses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "VaccineDoses");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "VaccineDoses");

            migrationBuilder.RenameColumn(
                name: "IntervalInMonths",
                table: "VaccineDoses",
                newName: "IntervalFromPreviousDoseInDays");

            migrationBuilder.AlterColumn<string>(
                name: "RecommendedAge",
                table: "VaccineDoses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
