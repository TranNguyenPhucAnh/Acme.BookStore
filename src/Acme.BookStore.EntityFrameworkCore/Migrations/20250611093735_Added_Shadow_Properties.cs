using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Added_Shadow_Properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AbpUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "varchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id");
        }
    }
}
