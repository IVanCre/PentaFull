using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Penta_Server.Migrations
{
    /// <inheritdoc />
    public partial class DataSourceApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GroupID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_AccessToken",
                table: "DataSources",
                column: "AccessToken");

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_ID",
                table: "DataSources",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_Name",
                table: "DataSources",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSources");
        }
    }
}
