using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPluggyFaturaPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PluggyFaturaPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    ContaPluggyId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    Valor = table.Column<decimal>(type: "money", nullable: false),
                    Origem = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluggyFaturaPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluggyFaturaPagamento_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PluggyFaturaPagamento_IdUser_ContaPluggyId_Data_Valor",
                table: "PluggyFaturaPagamento",
                columns: new[] { "IdUser", "ContaPluggyId", "Data", "Valor" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PluggyFaturaPagamento");
        }
    }
}
