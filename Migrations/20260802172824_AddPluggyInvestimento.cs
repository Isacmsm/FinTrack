using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPluggyInvestimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PluggyInvestimento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    InvestmentId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    NomeConector = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Subtipo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Nome = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Codigo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Isin = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Saldo = table.Column<decimal>(type: "money", nullable: false),
                    ValorOriginal = table.Column<decimal>(type: "money", nullable: true),
                    LucroInformado = table.Column<decimal>(type: "money", nullable: true),
                    ValorDisponivelResgate = table.Column<decimal>(type: "money", nullable: true),
                    Quantidade = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    ValorCota = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    MoedaCodigo = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: true),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Taxa = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    TipoTaxa = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    TaxaAnualFixa = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Emissor = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluggyInvestimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluggyInvestimento_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PluggyInvestimento_IdUser",
                table: "PluggyInvestimento",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_PluggyInvestimento_InvestmentId",
                table: "PluggyInvestimento",
                column: "InvestmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PluggyInvestimento");
        }
    }
}
