using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class RetiraCategoriasImportadasGenericas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Receitas/Despesas Importadas" (14/15) duplicavam o que
            // "Outras Receitas/Despesas" (4/13) já resolviam — o import passa
            // a usar as categorias existentes do usuário como fallback, e
            // mercadores conhecidos (iFood, Uber, Amazon, assinaturas...) vão
            // direto pra categoria real em vez de cair num balaio genérico.
            migrationBuilder.Sql(@"
                UPDATE Transacao SET IdCategoria = 5
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15
                AND ([Desc] LIKE '%ifood%' OR [Desc] LIKE '%sra cantina%' OR [Desc] LIKE '%grupo madero%'
                     OR [Desc] LIKE '%farofa%' OR [Desc] LIKE '%sorveteria%' OR [Desc] LIKE '%emporio do pao%');

                UPDATE Transacao SET IdCategoria = 7
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15 AND [Desc] LIKE '%uber%';

                UPDATE Transacao SET IdCategoria = 11
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15
                AND ([Desc] LIKE '%amazon%' OR [Desc] LIKE '%oboticario%');

                UPDATE Transacao SET IdCategoria = 8
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15
                AND ([Desc] LIKE '%totalpass%' OR [Desc] LIKE '%drogaria%');

                UPDATE Transacao SET IdCategoria = 10
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15
                AND ([Desc] LIKE '%barbearia%' OR [Desc] LIKE '%ingresso.com%');

                UPDATE Transacao SET IdCategoria = 12
                WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15
                AND ([Desc] LIKE '%claude%' OR [Desc] LIKE '%anthropic%' OR [Desc] LIKE '%apple.com%'
                     OR [Desc] LIKE '%discord%' OR [Desc] LIKE '%nucel%' OR [Desc] LIKE '%livepix%' OR [Desc] LIKE '%steam%');

                -- O que sobrou em 15 (sem match de mercador) e tudo que estava em 14 vira Outras.
                UPDATE Transacao SET IdCategoria = 13 WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 15;
                UPDATE Transacao SET IdCategoria = 4 WHERE PluggyTransactionId IS NOT NULL AND IdCategoria = 14;

                DELETE FROM Categoria WHERE Id IN (14, 15);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Não dá pra restaurar com fidelidade quais linhas eram
            // originalmente 14/15 antes da categorização por mercador — só
            // recria as categorias vazias. Aceitável: dado de uso pessoal,
            // não um sistema com exigência de rollback perfeito.
            migrationBuilder.InsertData(
                table: "Categoria",
                columns: new[] { "Id", "Tipo", "Nome", "IdUser" },
                values: new object[,]
                {
                    { 14, false, "Receitas Importadas", null },
                    { 15, true, "Despesas Importadas", null }
                });
        }
    }
}
