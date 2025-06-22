using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Removed_Unused_Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_OrganizationUnitId",
                table: "AbpUsers",
                column: "OrganizationUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AppOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers",
                column: "OrganizationUnitId",
                principalTable: "AppOrganizationUnits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AppOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_OrganizationUnitId",
                table: "AbpUsers");
        }
    }
}
