using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPIProgram.Migrations
{
    /// <inheritdoc />
    public partial class clients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OAuthClients",
                columns: table => new
                {
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret = table.Column<string>(type: "text", nullable: false),
                    roles = table.Column<List<string>>(type: "text[]", nullable: false),
                    allowedScopes = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthClients", x => x.client_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthClients");
        }
    }
}
