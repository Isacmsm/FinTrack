namespace FinTrack.Data;

/// <summary>
/// Erros de validação de input do usuário.
/// Substitui o RAISERROR 50001 que ficava dentro das stored procedures —
/// o JSON entregue ao navegador continua exatamente o mesmo, então o
/// site.js não muda.
/// </summary>
public class ErroValidacaoException(List<ErroValidacaoException.ErroCampo> erros) : Exception
{
    public List<ErroCampo> Erros { get; } = erros;

    public record ErroCampo(string NomeInput, string Mensagem);
}

/// <summary>
/// Acumula os erros e lança todos de uma vez, igual ao padrão das procedures
/// (a variável @Erro BIT ia somando as falhas antes do RETURN). Assim o usuário
/// vê todos os problemas do formulário de uma vez, não um por vez.
/// </summary>
public class Validador
{
    private readonly List<ErroValidacaoException.ErroCampo> _erros = [];

    /// <summary>Registra um erro no campo se a condição NÃO for satisfeita.</summary>
    public Validador Exigir(bool condicao, string nomeInput, string mensagem)
    {
        if (!condicao)
        {
            _erros.Add(new ErroValidacaoException.ErroCampo(nomeInput, mensagem));
        }

        return this;
    }

    /// <summary>Lança a exceção se algo falhou. Se estiver tudo certo, não faz nada.</summary>
    public void LancarSeInvalido()
    {
        if (_erros.Count > 0)
        {
            throw new ErroValidacaoException(_erros);
        }
    }
}
