using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicrobloggingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGameTitleToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameTitle",
                table: "Posts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameTitle",
                table: "Posts");
        }
    }
}
