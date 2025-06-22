using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Edited_Book_Author_Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "AppBooks");

            migrationBuilder.DropColumn(
                name: "PublishDate",
                table: "AppBooks");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AppAuthors");

            migrationBuilder.DropColumn(
                name: "ShortBio",
                table: "AppAuthors");

            migrationBuilder.AddColumn<string>(
                name: "ISBN",
                table: "AppBooks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PublishYear",
                table: "AppBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "AppBooks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                table: "AppAuthors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISBN",
                table: "AppBooks");

            migrationBuilder.DropColumn(
                name: "PublishYear",
                table: "AppBooks");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "AppBooks");

            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "AppAuthors");

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "AppBooks",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

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

            migrationBuilder.AddColumn<string>(
                name: "ShortBio",
                table: "AppAuthors",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
