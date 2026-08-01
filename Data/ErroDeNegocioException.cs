namespace FinTrack.Data;

/// <summary>
/// Erro de regra de negócio cuja mensagem foi escrita para ser lida pelo
/// usuário ("Categoria não encontrada", "Este Item já está registrado").
///
/// Existe para separar isso de uma exceção qualquer do framework. Antes tudo
/// era <see cref="Exception"/>, então o <c>Util.ExecutarHandlerAsync</c> não
/// tinha como distinguir uma mensagem escrita para o usuário de uma
/// <c>SqlException</c> — e acabava mandando as duas para o navegador.
///
/// Regra: se a mensagem foi escrita pensando em quem vai ler na tela, use esta
/// exceção. Se o erro é imprevisto, deixe estourar — vira log + mensagem
/// genérica.
/// </summary>
public class ErroDeNegocioException(string mensagem) : Exception(mensagem);
