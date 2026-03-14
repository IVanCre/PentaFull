using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Penta_Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTokenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_AccessToken",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_RefreshToken",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Tokens");

            migrationBuilder.AddColumn<string>(
                name: "AccessHash",
                table: "Tokens",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshHash",
                table: "Tokens",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccessHash",
                table: "Tokens",
                column: "AccessHash");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_RefreshHash",
                table: "Tokens",
                column: "RefreshHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_AccessHash",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_RefreshHash",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "AccessHash",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "RefreshHash",
                table: "Tokens");

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Tokens",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccessToken",
                table: "Tokens",
                column: "AccessToken");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_RefreshToken",
                table: "Tokens",
                column: "RefreshToken");
        }
    }
}
