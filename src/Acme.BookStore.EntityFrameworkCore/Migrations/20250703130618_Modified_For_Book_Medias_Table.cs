using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.BookStore.Migrations
{
    /// <inheritdoc />
    public partial class Modified_For_Book_Medias_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "AppBookMedias",
                newName: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AppBookMedias_BookId",
                table: "AppBookMedias",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppBookMedias_AppBooks_BookId",
                table: "AppBookMedias",
                column: "BookId",
                principalTable: "AppBooks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppBookMedias_AppBooks_BookId",
                table: "AppBookMedias");

            migrationBuilder.DropIndex(
                name: "IX_AppBookMedias_BookId",
                table: "AppBookMedias");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "AppBookMedias",
                newName: "FileName");
        }
    }
}
