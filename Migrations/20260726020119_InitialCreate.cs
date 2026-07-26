using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banco",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banco", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Senha = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<bool>(type: "bit", nullable: false),
                    Nome = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IdUser = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categoria_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Recorrente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "money", nullable: false),
                    TipoVencimento = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DiaVencimento = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recorrente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recorrente_Categoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categoria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Recorrente_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Desc = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    valor = table.Column<decimal>(type: "money", nullable: false),
                    data = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacao_Categoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categoria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacao_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Divida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    IdBanco = table.Column<int>(type: "int", nullable: true),
                    Descricao = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "money", nullable: false),
                    TaxaJuro = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFinalEstimada = table.Column<DateOnly>(type: "date", nullable: true),
                    DataFinal = table.Column<DateOnly>(type: "date", nullable: true),
                    Pagando = table.Column<bool>(type: "bit", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "money", nullable: true),
                    QntParcelas = table.Column<int>(type: "int", nullable: true),
                    QntParcelasPagas = table.Column<int>(type: "int", nullable: false),
                    ValorVista = table.Column<decimal>(type: "money", nullable: true),
                    IdRecorrente = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Divida_Banco",
                        column: x => x.IdBanco,
                        principalTable: "Banco",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Divida_Recorrente",
                        column: x => x.IdRecorrente,
                        principalTable: "Recorrente",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Divida_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categoria_IdUser",
                table: "Categoria",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Divida_IdBanco",
                table: "Divida",
                column: "IdBanco");

            migrationBuilder.CreateIndex(
                name: "IX_Divida_IdRecorrente",
                table: "Divida",
                column: "IdRecorrente");

            migrationBuilder.CreateIndex(
                name: "IX_Divida_IdUser",
                table: "Divida",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Recorrente_IdCategoria",
                table: "Recorrente",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Recorrente_IdUser",
                table: "Recorrente",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Transacao_IdCategoria",
                table: "Transacao",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Transacao_IdUser",
                table: "Transacao",
                column: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Divida");

            migrationBuilder.DropTable(
                name: "Transacao");

            migrationBuilder.DropTable(
                name: "Banco");

            migrationBuilder.DropTable(
                name: "Recorrente");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
