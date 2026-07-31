using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimentacaoInternaECategoriaPluggy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PluggyCategoria",
                table: "Transacao",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PluggyCategoriaId",
                table: "Transacao",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EhMovimentacaoInterna",
                table: "Categoria",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Categoria",
                columns: new[] { "Id", "Tipo", "Nome", "IdUser", "EhMovimentacaoInterna" },
                values: new object[,]
                {
                    { 16, true, "Investimento - Aplicação", null, true },
                    { 17, false, "Investimento - Resgate", null, true }
                });

            // Reaponta transações já importadas que na verdade são movimentação
            // interna (aplicação/resgate automático) pras novas categorias —
            // sem isso, o dashboard continuaria inflado com dados antigos
            // mesmo depois do código novo entrar em vigor.
            migrationBuilder.Sql(@"
                UPDATE Transacao SET IdCategoria = 16
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15
                AND [Desc] IN ('Aplicação RDB','Resgate RDB','Aplicação em Tesouro Direto','Valor recebido de Investimentos','Valor transferido para Investimentos');

                UPDATE Transacao SET IdCategoria = 17
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 14
                AND [Desc] IN ('Aplicação RDB','Resgate RDB','Aplicação em Tesouro Direto','Valor recebido de Investimentos','Valor transferido para Investimentos');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Transacao SET IdCategoria = 15 WHERE IdCategoria = 16;
                UPDATE Transacao SET IdCategoria = 14 WHERE IdCategoria = 17;
            ");

            migrationBuilder.DeleteData(
                table: "Categoria",
                keyColumn: "Id",
                keyValues: new object[] { 16, 17 });

            migrationBuilder.DropColumn(
                name: "PluggyCategoria",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "PluggyCategoriaId",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "EhMovimentacaoInterna",
                table: "Categoria");
        }
    }
}
