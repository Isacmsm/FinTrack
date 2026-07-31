using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPluggyIntegracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PluggyTransactionId",
                table: "Transacao",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PluggyConexao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    ClientIdProtegido = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    ClientSecretProtegido = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluggyConexao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluggyConexao_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PluggyItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    NomeConector = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    UltimaSincronizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluggyItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluggyItem_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transacao_PluggyTransactionId",
                table: "Transacao",
                column: "PluggyTransactionId",
                unique: true,
                filter: "[PluggyTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PluggyConexao_IdUser",
                table: "PluggyConexao",
                column: "IdUser",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluggyItem_IdUser",
                table: "PluggyItem",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_PluggyItem_ItemId",
                table: "PluggyItem",
                column: "ItemId",
                unique: true);

            migrationBuilder.InsertData(
                table: "Categoria",
                columns: new[] { "Id", "Tipo", "Nome", "IdUser" },
                values: new object[,]
                {
                    { 14, false, "Receitas Importadas", null },
                    { 15, true, "Despesas Importadas", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categoria",
                keyColumn: "Id",
                keyValues: new object[] { 14, 15 });

            migrationBuilder.DropTable(
                name: "PluggyConexao");

            migrationBuilder.DropTable(
                name: "PluggyItem");

            migrationBuilder.DropIndex(
                name: "IX_Transacao_PluggyTransactionId",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "PluggyTransactionId",
                table: "Transacao");
        }
    }
}
