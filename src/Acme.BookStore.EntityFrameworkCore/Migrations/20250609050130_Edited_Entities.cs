using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Edited_Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishYear",
                table: "AppBooks");

            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "AppAuthors");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishDate",
                table: "AppBooks",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AppAuthors",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishDate",
                table: "AppBooks");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AppAuthors");

            migrationBuilder.AddColumn<int>(
                name: "PublishYear",
                table: "AppBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                table: "AppAuthors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
