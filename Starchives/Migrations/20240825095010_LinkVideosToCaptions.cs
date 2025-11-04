using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Starchives.Migrations
{
    /// <inheritdoc />
    public partial class LinkVideosToCaptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VideoId",
                table: "Captions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Captions_VideoId",
                table: "Captions",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Captions_Videos_VideoId",
                table: "Captions",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "VideoId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Captions_Videos_VideoId",
                table: "Captions");

            migrationBuilder.DropIndex(
                name: "IX_Captions_VideoId",
                table: "Captions");

            migrationBuilder.AlterColumn<string>(
                name: "VideoId",
                table: "Captions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
