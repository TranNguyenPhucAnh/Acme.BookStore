using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Modified_Notification_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Message",
                table: "AppNotifications",
                newName: "LocalizationKey");

            migrationBuilder.AddColumn<string>(
                name: "LocalizationArguments",
                table: "AppNotifications",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalizationArguments",
                table: "AppNotifications");

            migrationBuilder.RenameColumn(
                name: "LocalizationKey",
                table: "AppNotifications",
                newName: "Message");
        }
    }
}
