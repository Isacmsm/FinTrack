using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinTrack.Data;

public class PluggyItemDto
{
    public string Id { get; set; } = "";
    public string? Status { get; set; }
    public string? ExecutionStatus { get; set; }
    public PluggyConnectorDto? Connector { get; set; }
}

public class PluggyConnectorDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
}

public class PluggyAccountDto
{
    public string Id { get; set; } = "";
    public string? Type { get; set; }
    public string? Subtype { get; set; }
    public string Name { get; set; } = "";
    public decimal Balance { get; set; }
    public string? CurrencyCode { get; set; }
    public string ItemId { get; set; } = "";
}

public class PluggyTransactionDto
{
    public string Id { get; set; } = "";
    public string? Description { get; set; }
    public decimal Amount { get; set; }

    /// <summary>
    /// Só vem preenchido quando a transação foi feita em moeda diferente da
    /// conta (compra internacional) — Amount ali é o valor na moeda
    /// original (ex.: USD), não o que realmente saiu da conta em BRL.
    /// </summary>
    public decimal? AmountInAccountCurrency { get; set; }

    /// <summary>Código ISO da moeda de Amount (ex.: "USD"). "BRL" na maioria das transações.</summary>
    public string? CurrencyCode { get; set; }

    public DateTime Date { get; set; }
    public string Type { get; set; } = ""; // DEBIT ou CREDIT
    public string? Category { get; set; }
    public string? CategoryId { get; set; }
    public PluggyMerchantDto? Merchant { get; set; }
    public PluggyCreditCardMetadataDto? CreditCardMetadata { get; set; }
}

public class PluggyMerchantDto
{
    public string? Name { get; set; }
    public string? BusinessName { get; set; }
    public string? Cnpj { get; set; }
}

public class PluggyCreditCardMetadataDto
{
    public int? InstallmentNumber { get; set; }
    public int? TotalInstallments { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? BillId { get; set; }

    /// <summary>Formato YYYY-MM (período da fatura), não uma data completa.</summary>
    public string? BillForecastDate { get; set; }
}

public class PluggyInvestmentDto
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Subtype { get; set; }
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public string? Isin { get; set; }
    public decimal Balance { get; set; }
    public decimal? AmountOriginal { get; set; }
    public decimal? AmountProfit { get; set; }
    public decimal? AmountWithdrawal { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Value { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTime Date { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? Rate { get; set; }
    public string? RateType { get; set; }
    public decimal? FixedAnnualRate { get; set; }
    public string? Issuer { get; set; }
    public string Status { get; set; } = "";
}

public class PluggyBillDto
{
    public string Id { get; set; } = "";
    public DateTime DueDate { get; set; }
    public DateTime? BillClosingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? MinimumPaymentAmount { get; set; }
    public bool AllowsInstallments { get; set; }
    public List<PluggyBillPaymentDto>? Payments { get; set; }
}

public class PluggyBillPaymentDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
}

/// <summary>
/// Cliente pra API da Pluggy (api.pluggy.ai) — primeira integração HTTP de
/// saída do projeto. Não guarda nenhuma credencial: cada chamada recebe o
/// clientId/clientSecret (ou apiKey) do usuário como parâmetro, já que são
/// por usuário, não por app.
/// </summary>
public class PluggyApiClient(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private HttpClient Cliente => httpClientFactory.CreateClient("Pluggy");

    public async Task<string> ObterApiKeyAsync(string clientId, string clientSecret)
    {
        var resposta = await Cliente.PostAsJsonAsync("/auth", new { clientId, clientSecret }, OpcoesJson);

        if (!resposta.IsSuccessStatusCode)
        {
            var mensagem = await LerMensagemDeErroAsync(resposta);
            throw new ErroDeNegocioException($"Não foi possível autenticar com a Pluggy: {mensagem}");
        }

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return corpo.GetProperty("apiKey").GetString()!;
    }

    /// <summary>
    /// Pluggy devolve o motivo do erro em {"message": "..."} ou {"code": "..."}
    /// — sem isso, todo erro vira "não foi possível" genérico e some
    /// informação como "CLIENT_IS_UPDATING_BEFORE_ALLOWED_FREQUENCY" que o
    /// usuário precisa ver pra entender o que fazer.
    /// </summary>
    private static async Task<string> LerMensagemDeErroAsync(HttpResponseMessage resposta)
    {
        var texto = await resposta.Content.ReadAsStringAsync();

        try
        {
            var corpo = JsonSerializer.Deserialize<JsonElement>(texto);

            if (corpo.TryGetProperty("message", out var mensagem) && mensagem.ValueKind == JsonValueKind.String)
                return mensagem.GetString()!;

            if (corpo.TryGetProperty("code", out var codigo) && codigo.ValueKind == JsonValueKind.String)
                return codigo.GetString()!;
        }
        catch (JsonException)
        {
            // corpo não é JSON — cai no fallback abaixo
        }

        return string.IsNullOrWhiteSpace(texto) ? resposta.StatusCode.ToString() : texto;
    }

    private HttpRequestMessage NovaRequisicao(HttpMethod metodo, string caminho, string apiKey)
    {
        var requisicao = new HttpRequestMessage(metodo, caminho);
        requisicao.Headers.Add("X-API-KEY", apiKey);
        return requisicao;
    }

    /// <summary>
    /// Gera o token que o widget Pluggy Connect usa no navegador pra abrir o
    /// fluxo de login no banco. Sem itemId/options — sempre cria um Item
    /// novo, nunca atualiza um existente.
    /// </summary>
    public async Task<string> CriarConnectTokenAsync(string apiKey)
    {
        var requisicao = NovaRequisicao(HttpMethod.Post, "/connect_token", apiKey);
        requisicao.Content = JsonContent.Create(new { }, options: OpcoesJson);

        var resposta = await Cliente.SendAsync(requisicao);

        if (!resposta.IsSuccessStatusCode)
        {
            var mensagem = await LerMensagemDeErroAsync(resposta);
            throw new ErroDeNegocioException($"Não foi possível gerar o connect token na Pluggy: {mensagem}");
        }

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>(OpcoesJson);
        return corpo.GetProperty("accessToken").GetString()!;
    }

    public async Task<PluggyItemDto?> ObterItemAsync(string apiKey, string itemId)
    {
        var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Get, $"/items/{itemId}", apiKey));
        if (!resposta.IsSuccessStatusCode) return null;

        return await resposta.Content.ReadFromJsonAsync<PluggyItemDto>(OpcoesJson);
    }

    /// <summary>
    /// Devolve true se a atualização foi de fato solicitada. Um 409
    /// (CLIENT_IS_UPDATING_BEFORE_ALLOWED_FREQUENCY) não é erro — só significa
    /// que já foi atualizado recentemente — então devolve false em vez de
    /// lançar, e quem chamou segue direto pra ler o que já está sincronizado.
    /// </summary>
    public async Task<bool> SolicitarAtualizacaoAsync(string apiKey, string itemId)
    {
        var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Patch, $"/items/{itemId}", apiKey));

        if (resposta.IsSuccessStatusCode) return true;
        if (resposta.StatusCode == HttpStatusCode.Conflict) return false;

        var mensagem = await LerMensagemDeErroAsync(resposta);
        throw new ErroDeNegocioException($"Não foi possível solicitar a atualização deste item na Pluggy: {mensagem}");
    }

    /// <summary>
    /// Um 404 aqui não é erro — o Item já não existe na Pluggy (ex.: exclusão
    /// repetida ou já removido de outra forma), então o objetivo (não existir
    /// mais lá) já está cumprido.
    /// </summary>
    public async Task ExcluirItemAsync(string apiKey, string itemId)
    {
        var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Delete, $"/items/{itemId}", apiKey));

        if (resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NotFound) return;

        var mensagem = await LerMensagemDeErroAsync(resposta);
        throw new ErroDeNegocioException($"Não foi possível excluir este item na Pluggy: {mensagem}");
    }

    /// <summary>
    /// Busca "alike" no diretório de connectors reais da Pluggy (bancos de
    /// verdade, não o "MeuPluggy" agregador) — usada pra achar o ícone
    /// correto quando o Item veio do Meu Pluggy, que só devolve o connector
    /// genérico. Devolve o primeiro resultado; se não achar nada, null.
    /// </summary>
    public async Task<PluggyConnectorDto?> BuscarConectorPorNomeAsync(string apiKey, string nome)
    {
        var caminho = $"/connectors?name={Uri.EscapeDataString(nome)}&countries=BR";
        var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Get, caminho, apiKey));
        if (!resposta.IsSuccessStatusCode) return null;

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>(OpcoesJson);
        var conectores = JsonSerializer.Deserialize<List<PluggyConnectorDto>>(corpo.GetProperty("results").GetRawText(), OpcoesJson) ?? [];
        return conectores.FirstOrDefault();
    }

    /// <summary>
    /// /investments é paginado (default pageSize 500) mas por item raramente
    /// passa disso — pagina mesmo assim pelo total de páginas devolvido, sem
    /// cursor (diferente de /v2/transactions).
    /// </summary>
    public async Task<List<PluggyInvestmentDto>> ListarInvestimentosAsync(string apiKey, string itemId)
    {
        var investimentos = new List<PluggyInvestmentDto>();
        var pagina = 1;
        int totalPaginas;

        do
        {
            var caminho = $"/investments?itemId={itemId}&page={pagina}";
            var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Get, caminho, apiKey));

            if (!resposta.IsSuccessStatusCode)
            {
                var mensagem = await LerMensagemDeErroAsync(resposta);
                throw new ErroDeNegocioException($"Não foi possível listar os investimentos deste item na Pluggy: {mensagem}");
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>(OpcoesJson);
            investimentos.AddRange(JsonSerializer.Deserialize<List<PluggyInvestmentDto>>(corpo.GetProperty("results").GetRawText(), OpcoesJson) ?? []);

            totalPaginas = corpo.TryGetProperty("totalPages", out var tp) ? tp.GetInt32() : 1;
            pagina++;
        } while (pagina <= totalPaginas);

        return investimentos;
    }

    public async Task<List<PluggyAccountDto>> ListarContasAsync(string apiKey, string itemId)
    {
        var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Get, $"/accounts?itemId={itemId}", apiKey));

        if (!resposta.IsSuccessStatusCode)
        {
            var mensagem = await LerMensagemDeErroAsync(resposta);
            throw new ErroDeNegocioException($"Não foi possível listar as contas deste item na Pluggy: {mensagem}");
        }

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>(OpcoesJson);
        return JsonSerializer.Deserialize<List<PluggyAccountDto>>(corpo.GetProperty("results").GetRawText(), OpcoesJson) ?? [];
    }

    /// <summary>
    /// Faturas de verdade (id, vencimento, fechamento, valor total/mínimo) —
    /// fonte diferente de transaction.creditCardMetadata, que documentação e
    /// dados reais confirmaram nula pro conector Meu Pluggy. É essa a fonte
    /// que o próprio dashboard do meu.pluggy.ai usa pra mostrar fatura.
    /// Só existe pra conta CREDIT; não lança em erro/404 pra não derrubar o
    /// resto da sincronização quando a conta não tem fatura (ex.: conta BANK
    /// passada por engano, ou conector sem esse produto habilitado).
    /// </summary>
    public async Task<List<PluggyBillDto>> ListarFaturasAsync(string apiKey, string accountId)
    {
        var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Get, $"/bills?accountId={accountId}", apiKey));
        if (!resposta.IsSuccessStatusCode) return [];

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>(OpcoesJson);
        return JsonSerializer.Deserialize<List<PluggyBillDto>>(corpo.GetProperty("results").GetRawText(), OpcoesJson) ?? [];
    }

    /// <summary>
    /// Pagina pelo cursor da Pluggy (campo "next" na resposta) até ele vir nulo.
    /// A forma exata do cursor ainda não foi validada contra a API real — ver
    /// tarefa de validação do Cenário A/B no plano.
    /// </summary>
    public async Task<List<PluggyTransactionDto>> ListarTransacoesAsync(string apiKey, string accountId, DateTime dataDe)
    {
        var transacoes = new List<PluggyTransactionDto>();
        string? cursor = null;

        do
        {
            var caminho = $"/v2/transactions?accountId={accountId}&dateFrom={dataDe:yyyy-MM-dd}";
            if (cursor is not null) caminho += $"&after={cursor}";

            var resposta = await Cliente.SendAsync(NovaRequisicao(HttpMethod.Get, caminho, apiKey));

            if (!resposta.IsSuccessStatusCode)
            {
                var mensagem = await LerMensagemDeErroAsync(resposta);
                throw new ErroDeNegocioException($"Não foi possível listar as transações desta conta na Pluggy: {mensagem}");
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>(OpcoesJson);
            var pagina = JsonSerializer.Deserialize<List<PluggyTransactionDto>>(corpo.GetProperty("results").GetRawText(), OpcoesJson) ?? [];
            transacoes.AddRange(pagina);

            cursor = corpo.TryGetProperty("next", out var next) && next.ValueKind == JsonValueKind.String
                ? next.GetString()
                : null;
        } while (cursor is not null);

        return transacoes;
    }
}
