using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTiemChung.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteVaccineInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteVaccineInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VaccinationSiteId = table.Column<int>(type: "int", nullable: false),
                    VaccineId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteVaccineInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteVaccineInventories_VaccinationSites_VaccinationSiteId",
                        column: x => x.VaccinationSiteId,
                        principalTable: "VaccinationSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteVaccineInventories_Vaccines_VaccineId",
                        column: x => x.VaccineId,
                        principalTable: "Vaccines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteVaccineInventories_VaccinationSiteId",
                table: "SiteVaccineInventories",
                column: "VaccinationSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteVaccineInventories_VaccineId",
                table: "SiteVaccineInventories",
                column: "VaccineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteVaccineInventories");
        }
    }
}
