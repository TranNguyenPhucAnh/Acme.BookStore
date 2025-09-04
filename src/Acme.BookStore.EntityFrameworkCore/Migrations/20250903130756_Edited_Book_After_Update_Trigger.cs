using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Edited_Book_After_Update_Trigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS `book_after_update`;");

            migrationBuilder.Sql(@"
                CREATE TRIGGER `book_after_update`
                AFTER UPDATE ON `AppBooks`
                FOR EACH ROW
                BEGIN
                    DECLARE lastVersion INT;
                    DECLARE lastAuditId CHAR(36);

                    -- Lấy version và Id của bản ghi history mới nhất
                    SELECT Version, Id
                    INTO lastVersion, lastAuditId
                    FROM AppBookHistories
                    WHERE BookId = NEW.Id
                    ORDER BY Version DESC
                    LIMIT 1;

                    -- Thêm bản ghi mới
                    INSERT INTO AppBookHistories (
                        Id, BookId, ISBN, Name, Type, PublishDate, Publisher, AuthorId,
                        ActionType, PreviousAuditId, Version, CreationTime
                    )
                    VALUES (
                        UUID(), NEW.Id, NEW.ISBN, NEW.Name, NEW.Type, NEW.PublishDate, NEW.Publisher, NEW.AuthorId,
                        1, lastAuditId, COALESCE(lastVersion, -1) + 1, NOW()
                    );
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS `book_after_update`;");

            migrationBuilder.Sql(@"
                CREATE TRIGGER `book_after_update`
                AFTER UPDATE ON `AppBooks`
                FOR EACH ROW
                BEGIN
                    DECLARE lastVersion INT;
                    DECLARE lastAuditId CHAR(36);

                    -- Lấy version và Id của bản ghi history mới nhất
                    SELECT Version, Id
                    INTO lastVersion, lastAuditId
                    FROM AppBookHistories
                    WHERE BookId = NEW.Id
                    ORDER BY Version DESC
                    LIMIT 1;

                    -- Thêm bản ghi mới
                    INSERT INTO AppBookHistories (
                        Id, BookId, ISBN, Name, Type, PublishDate, Publisher, AuthorId,
                        ActionType, PreviousAuditId, Version, CreationTime
                    )
                    VALUES (
                        UUID(), NEW.Id, NEW.ISBN, NEW.Name, NEW.Type, NEW.PublishDate, NEW.Publisher, NEW.AuthorId,
                        1, lastAuditId, lastVersion + 1, NOW()
                    );
                END
            ");
        }
    }
}
