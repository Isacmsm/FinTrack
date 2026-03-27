// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function mostrarErros(xhr) {
    if (xhr.status === 400 && xhr.responseJSON) {
        const erros = xhr.responseJSON;

        if (erros.errosInput) {
            erros.errosInput.forEach(function(erro) {
                const span = $('#erro' + erro.nomeInput.charAt(0).toUpperCase() + erro.nomeInput.slice(1));
                span.text(erro.mensagem).removeClass('hidden');
            });
        }

        if (erros.mensagem) {
            $('#erroGeral').text(erros.mensagem).removeClass('hidden');
        }
    } else {
        $('#erroGeral').text('Erro ao conectar. Tente novamente.').removeClass('hidden');
    }
}

function limparErros() {
    $('.text-red-500').addClass('hidden').text('');
    $('#erroGeral').addClass('hidden').text('');
}
