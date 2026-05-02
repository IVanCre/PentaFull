using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Penta_Server.Migrations
{
    /// <inheritdoc />
    public partial class Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdminGroupID = table.Column<int>(type: "int", nullable: false),
                    UsersInGroup = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSended = table.Column<bool>(type: "bit", nullable: false),
                    FromUserID = table.Column<int>(type: "int", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    ToUserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SharedDataID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UtcTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SharedDatas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SharedMarker = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CopyCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedDatas", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MaskedPassword = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RegistrationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastConnectDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

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

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    AccessHash = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    RefreshHash = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tokens_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ID",
                table: "Groups",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ID",
                table: "Messages",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ToUserID_IsSended",
                table: "Messages",
                columns: new[] { "ToUserID", "IsSended" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Type",
                table: "Messages",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UtcTimestamp",
                table: "Messages",
                column: "UtcTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SharedDatas_SharedMarker",
                table: "SharedDatas",
                column: "SharedMarker");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccessHash",
                table: "Tokens",
                column: "AccessHash");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ID",
                table: "Tokens",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_RefreshHash",
                table: "Tokens",
                column: "RefreshHash");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_UserID",
                table: "Tokens",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ID",
                table: "Users",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name_MaskedPassword",
                table: "Users",
                columns: new[] { "Name", "MaskedPassword" });

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
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "SharedDatas");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "UsersDevices");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
