using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Migrated_Book_Snapshot_For_Audit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "AppBookHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.Sql(@"
                INSERT INTO AppBookHistories (
                    Id, BookId, ISBN, Name, Type, PublishDate, Publisher, AuthorId,
                    ActionType, PreviousAuditId, Version, CreationTime
                )
                SELECT 
                    UUID(), b.Id, b.ISBN, b.Name, b.Type, b.PublishDate, b.Publisher, b.AuthorId,
                    3, NULL, 0, NOW()
                FROM AppBooks b
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM AppBookHistories h 
                    WHERE h.BookId = b.Id
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "AppBookHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
