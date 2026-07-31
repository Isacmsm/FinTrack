using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriaMovimentacaoFatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categoria",
                columns: new[] { "Id", "Tipo", "Nome", "IdUser", "EhMovimentacaoInterna" },
                values: new object[,]
                {
                    { 18, true, "Pagamento de Fatura", null, true },
                    { 19, false, "Recebimento de Fatura", null, true }
                });

            // Pagar a própria fatura do cartão não é despesa nova (você já
            // contou a compra quando ela aconteceu no cartão) nem receita —
            // reaponta o que já foi importado com essas descrições.
            migrationBuilder.Sql(@"
                UPDATE Transacao SET IdCategoria = 18
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15
                AND [Desc] = 'Pagamento de fatura';

                UPDATE Transacao SET IdCategoria = 19
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 14
                AND [Desc] = 'Pagamento recebido';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Transacao SET IdCategoria = 15 WHERE IdCategoria = 18;
                UPDATE Transacao SET IdCategoria = 14 WHERE IdCategoria = 19;
            ");

            migrationBuilder.DeleteData(
                table: "Categoria",
                keyColumn: "Id",
                keyValues: new object[] { 18, 19 });
        }
    }
}
