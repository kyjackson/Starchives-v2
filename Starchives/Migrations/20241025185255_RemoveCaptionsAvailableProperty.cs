using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Starchives.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCaptionsAvailableProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaptionsAvailable",
                table: "Videos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CaptionsAvailable",
                table: "Videos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
