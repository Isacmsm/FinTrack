using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class CriaSessaoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessaoUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    InicioEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FimEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Motivo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessaoUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessaoUsuario_Usuario",
                        column: x => x.IdUser,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessaoUsuario_IdUser",
                table: "SessaoUsuario",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_SessaoUsuario_Token",
                table: "SessaoUsuario",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessaoUsuario");
        }
    }
}
