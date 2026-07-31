using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantEContaTransacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContaNome",
                table: "Transacao",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContaPluggyId",
                table: "Transacao",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContaTipo",
                table: "Transacao",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantCnpj",
                table: "Transacao",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantNome",
                table: "Transacao",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContaNome",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "ContaPluggyId",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "ContaTipo",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "MerchantCnpj",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "MerchantNome",
                table: "Transacao");
        }
    }
}
