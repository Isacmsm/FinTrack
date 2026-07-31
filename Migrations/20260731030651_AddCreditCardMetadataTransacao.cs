using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCardMetadataTransacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PluggyBillId",
                table: "Transacao",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PluggyDataCompra",
                table: "Transacao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PluggyFaturaPrevista",
                table: "Transacao",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PluggyParcelaAtual",
                table: "Transacao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PluggyParcelaTotal",
                table: "Transacao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PluggyValorCompraTotal",
                table: "Transacao",
                type: "money",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PluggyBillId",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "PluggyDataCompra",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "PluggyFaturaPrevista",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "PluggyParcelaAtual",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "PluggyParcelaTotal",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "PluggyValorCompraTotal",
                table: "Transacao");
        }
    }
}
