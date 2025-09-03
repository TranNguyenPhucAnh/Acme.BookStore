using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Added_Book_History_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppBookHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ISBN = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Publisher = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    PreviousAuditId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBookHistories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
                CREATE TRIGGER `book_after_insert`
                AFTER INSERT ON `Book`
                FOR EACH ROW
                BEGIN
                    INSERT INTO BookHistory (
                        Id, BookId, ISBN, Name, Type, PublishDate, Publisher, AuthorId,
                        ActionType, PreviousAuditId, Version, CreationTime
                    )
                    VALUES (
                        UUID(), NEW.Id, NEW.ISBN, NEW.Name, NEW.Type, NEW.PublishDate, NEW.Publisher, NEW.AuthorId,
                        0, NULL, 0, NOW()
                    );
                END
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER `book_after_update`
                AFTER UPDATE ON `Book`
                FOR EACH ROW
                BEGIN
                    DECLARE lastVersion INT;
                    DECLARE lastAuditId CHAR(36);

                    -- Lấy version và Id của bản ghi history mới nhất
                    SELECT Version, Id
                    INTO lastVersion, lastAuditId
                    FROM BookHistory
                    WHERE BookId = NEW.Id
                    ORDER BY Version DESC
                    LIMIT 1;

                    -- Thêm bản ghi mới
                    INSERT INTO BookHistory (
                        Id, BookId, ISBN, Name, Type, PublishDate, Publisher, AuthorId,
                        ActionType, PreviousAuditId, Version, CreationTime
                    )
                    VALUES (
                        UUID(), NEW.Id, NEW.ISBN, NEW.Name, NEW.Type, NEW.PublishDate, NEW.Publisher, NEW.AuthorId,
                        1, lastAuditId, lastVersion + 1, NOW()
                    );
                END
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER `book_after_delete`
                AFTER DELETE ON `Book`
                FOR EACH ROW
                BEGIN
                    DECLARE lastVersion INT;
                    DECLARE lastAuditId CHAR(36);

                    -- Lấy version và Id của bản ghi history mới nhất
                    SELECT Version, Id
                    INTO lastVersion, lastAuditId
                    FROM BookHistory
                    WHERE BookId = OLD.Id
                    ORDER BY Version DESC
                    LIMIT 1;

                    -- Thêm bản ghi mới để đánh dấu Delete
                    INSERT INTO BookHistory (
                        Id, BookId, ISBN, Name, Type, PublishDate, Publisher, AuthorId,
                        ActionType, PreviousAuditId, Version, CreationTime
                    )
                    VALUES (
                        UUID(), OLD.Id, OLD.ISBN, OLD.Name, OLD.Type, OLD.PublishDate, OLD.Publisher, OLD.AuthorId,
                        2, lastAuditId, COALESCE(lastVersion, -1) + 1, NOW()
                    );
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppBookHistories");
        }
    }
}
