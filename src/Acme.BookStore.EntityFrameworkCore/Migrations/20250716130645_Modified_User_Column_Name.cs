using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Modified_User_Column_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers");

            migrationBuilder.RenameColumn(
                name: "OrganizationUnitId",
                table: "AbpUsers",
                newName: "EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_AbpUsers_OrganizationUnitId",
                table: "AbpUsers",
                newName: "IX_AbpUsers_EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_EntityId",
                table: "AbpUsers",
                column: "EntityId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_EntityId",
                table: "AbpUsers");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "AbpUsers",
                newName: "OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_AbpUsers_EntityId",
                table: "AbpUsers",
                newName: "IX_AbpUsers_OrganizationUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
