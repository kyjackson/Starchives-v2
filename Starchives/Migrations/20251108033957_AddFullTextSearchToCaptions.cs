using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Starchives.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchToCaptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration was applied manually via SQL script
            // due to the large dataset (1.28M rows) requiring batched processing
            // The following operations were completed:
            // 1. Added text_search tsvector column (generated)
            // 2. Created GIN index on text_search
            // 3. Created index on VideoId
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Captions_text_search",
                table: "Captions");

            migrationBuilder.DropIndex(
                name: "IX_Captions_VideoId",
                table: "Captions");

            migrationBuilder.DropColumn(
                name: "text_search",
                table: "Captions");
        }
    }
}
