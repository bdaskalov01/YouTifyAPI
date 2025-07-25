using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPIProgram.Migrations
{
    /// <inheritdoc />
    public partial class clients2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "roles",
                table: "OAuthClients",
                newName: "Roles");

            migrationBuilder.RenameColumn(
                name: "allowedScopes",
                table: "OAuthClients",
                newName: "AllowedScopes");

            migrationBuilder.RenameColumn(
                name: "client_secret",
                table: "OAuthClients",
                newName: "ClientSecret");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "OAuthClients",
                newName: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Roles",
                table: "OAuthClients",
                newName: "roles");

            migrationBuilder.RenameColumn(
                name: "AllowedScopes",
                table: "OAuthClients",
                newName: "allowedScopes");

            migrationBuilder.RenameColumn(
                name: "ClientSecret",
                table: "OAuthClients",
                newName: "client_secret");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "OAuthClients",
                newName: "client_id");
        }
    }
}
