// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Exibe/esconde com a classe 'd-none' (Bootstrap). No Tailwind era 'hidden'.
// Os campos de erro são selecionados por '.erro-input', uma classe semântica —
// antes era '.text-red-500', que é classe de COR: se você mudasse a cor do
// erro, o JS parava de achar os campos.

function mostrarErros(xhr) {
    // 400 é o status dos três destinos do Util.TratarErro. 429 vem do rate
    // limiter do login (Program.cs), que também responde com {mensagem}.
    if ((xhr.status === 400 || xhr.status === 429) && xhr.responseJSON) {
        const erros = xhr.responseJSON;

        if (erros.errosInput) {
            erros.errosInput.forEach(function(erro) {
                const span = $('#erro' + erro.nomeInput.charAt(0).toUpperCase() + erro.nomeInput.slice(1));
                span.text(erro.mensagem).removeClass('d-none');
            });
        }

        if (erros.mensagem) {
            $('#erroGeral').text(erros.mensagem).removeClass('d-none');
        }
    } else {
        $('#erroGeral').text('Erro ao conectar. Tente novamente.').removeClass('d-none');
    }
}

function limparErros() {
    $('.erro-input').addClass('d-none').text('');
    $('#erroGeral').addClass('d-none').text('');
}
