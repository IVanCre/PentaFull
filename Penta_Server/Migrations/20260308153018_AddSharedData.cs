using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Penta_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Messages");

            migrationBuilder.AddColumn<long>(
                name: "SharedDataID",
                table: "Messages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "SharedDatas",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CopyCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedDatas", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedDatas");

            migrationBuilder.DropColumn(
                name: "SharedDataID",
                table: "Messages");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "Messages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrdersToSend",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageID = table.Column<long>(type: "bigint", nullable: false),
                    RecieverID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersToSend", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdersToSend_ID",
                table: "OrdersToSend",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdersToSend_RecieverID_MessageID",
                table: "OrdersToSend",
                columns: new[] { "RecieverID", "MessageID" });
        }
    }
}
