using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoriasSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categoria",
                columns: new[] { "Id", "Tipo", "Nome", "IdUser" },
                values: new object[,]
                {
                    { 1, false, "Salário", null },
                    { 2, false, "Freelance / Renda Extra", null },
                    { 3, false, "Investimentos", null },
                    { 4, false, "Outras Receitas", null },
                    { 5, true, "Alimentação", null },
                    { 6, true, "Moradia", null },
                    { 7, true, "Transporte", null },
                    { 8, true, "Saúde", null },
                    { 9, true, "Educação", null },
                    { 10, true, "Lazer", null },
                    { 11, true, "Compras", null },
                    { 12, true, "Contas e Assinaturas", null },
                    { 13, true, "Outras Despesas", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categoria",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
        }
    }
}
