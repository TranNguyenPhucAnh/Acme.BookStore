using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Changed_Organization_Unit_Table_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpOrganizationUnitRoles_AppOrganizationUnits_OrganizationUn~",
                table: "AbpOrganizationUnitRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AbpUserOrganizationUnits_AppOrganizationUnits_OrganizationUn~",
                table: "AbpUserOrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AppOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AppOrganizationUnits_AppOrganizationUnits_ParentId",
                table: "AppOrganizationUnits");

            migrationBuilder.DropTable(
                name: "AbpSettingDefinitions");

            migrationBuilder.DropTable(
                name: "AbpSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppOrganizationUnits",
                table: "AppOrganizationUnits");

            migrationBuilder.RenameTable(
                name: "AppOrganizationUnits",
                newName: "AbpOrganizationUnits");

            migrationBuilder.RenameIndex(
                name: "IX_AppOrganizationUnits_ParentId",
                table: "AbpOrganizationUnits",
                newName: "IX_AbpOrganizationUnits_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_AppOrganizationUnits_Code",
                table: "AbpOrganizationUnits",
                newName: "IX_AbpOrganizationUnits_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AbpOrganizationUnits",
                table: "AbpOrganizationUnits",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpOrganizationUnitRoles_AbpOrganizationUnits_OrganizationUn~",
                table: "AbpOrganizationUnitRoles",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbpOrganizationUnits_AbpOrganizationUnits_ParentId",
                table: "AbpOrganizationUnits",
                column: "ParentId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUserOrganizationUnits_AbpOrganizationUnits_OrganizationUn~",
                table: "AbpUserOrganizationUnits",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpOrganizationUnitRoles_AbpOrganizationUnits_OrganizationUn~",
                table: "AbpOrganizationUnitRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AbpOrganizationUnits_AbpOrganizationUnits_ParentId",
                table: "AbpOrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_AbpUserOrganizationUnits_AbpOrganizationUnits_OrganizationUn~",
                table: "AbpUserOrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_AbpOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AbpOrganizationUnits",
                table: "AbpOrganizationUnits");

            migrationBuilder.RenameTable(
                name: "AbpOrganizationUnits",
                newName: "AppOrganizationUnits");

            migrationBuilder.RenameIndex(
                name: "IX_AbpOrganizationUnits_ParentId",
                table: "AppOrganizationUnits",
                newName: "IX_AppOrganizationUnits_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_AbpOrganizationUnits_Code",
                table: "AppOrganizationUnits",
                newName: "IX_AppOrganizationUnits_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppOrganizationUnits",
                table: "AppOrganizationUnits",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AbpSettingDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DefaultValue = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExtraProperties = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEncrypted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsInherited = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVisibleToClients = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Providers = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettingDefinitions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AbpSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettingDefinitions_Name",
                table: "AbpSettingDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettings_Name_ProviderName_ProviderKey",
                table: "AbpSettings",
                columns: new[] { "Name", "ProviderName", "ProviderKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AbpOrganizationUnitRoles_AppOrganizationUnits_OrganizationUn~",
                table: "AbpOrganizationUnitRoles",
                column: "OrganizationUnitId",
                principalTable: "AppOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUserOrganizationUnits_AppOrganizationUnits_OrganizationUn~",
                table: "AbpUserOrganizationUnits",
                column: "OrganizationUnitId",
                principalTable: "AppOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_AppOrganizationUnits_OrganizationUnitId",
                table: "AbpUsers",
                column: "OrganizationUnitId",
                principalTable: "AppOrganizationUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppOrganizationUnits_AppOrganizationUnits_ParentId",
                table: "AppOrganizationUnits",
                column: "ParentId",
                principalTable: "AppOrganizationUnits",
                principalColumn: "Id");
        }
    }
}
