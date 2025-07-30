using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPIProgram.Migrations
{
    /// <inheritdoc />
    public partial class refreshtokenrework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthClients",
                table: "OAuthClients");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GrantType",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OAuthClients",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthClients",
                table: "OAuthClients",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthClients",
                table: "OAuthClients");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "GrantType",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "RefreshTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OAuthClients",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthClients",
                table: "OAuthClients",
                column: "ClientId");
        }
    }
}
