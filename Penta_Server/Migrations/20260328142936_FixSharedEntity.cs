using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Penta_Server.Migrations
{
    /// <inheritdoc />
    public partial class FixSharedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SharedMarker",
                table: "SharedDatas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_SharedDatas_SharedMarker",
                table: "SharedDatas",
                column: "SharedMarker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SharedDatas_SharedMarker",
                table: "SharedDatas");

            migrationBuilder.DropColumn(
                name: "SharedMarker",
                table: "SharedDatas");
        }
    }
}
