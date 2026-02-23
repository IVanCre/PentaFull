using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Penta_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDeviceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsersDevices",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DeviceToken = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersDevices", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersDevices_ID",
                table: "UsersDevices",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersDevices_UserID_DeviceToken",
                table: "UsersDevices",
                columns: new[] { "UserID", "DeviceToken" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersDevices");
        }
    }
}
