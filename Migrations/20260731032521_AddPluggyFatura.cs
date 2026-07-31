using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPluggyFatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PluggyFatura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    BillId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ContaPluggyId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ContaNome = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "money", nullable: false),
                    ValorMinimo = table.Column<decimal>(type: "money", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluggyFatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluggyFatura_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PluggyFatura_BillId",
                table: "PluggyFatura",
                column: "BillId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluggyFatura_IdUser",
                table: "PluggyFatura",
                column: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PluggyFatura");
        }
    }
}
