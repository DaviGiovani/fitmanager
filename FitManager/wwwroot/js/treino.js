// Formulário da ficha de treino: adiciona e remove exercícios e mantém os
// índices dos campos em sequência (Itens[0], Itens[1], ...), como o
// model binding do ASP.NET Core espera.
(function () {
    var corpo = document.getElementById('itensTreino');
    var modelo = document.getElementById('modeloItem');
    var botaoAdicionar = document.getElementById('adicionarItem');
    var avisoSemItens = document.getElementById('semItens');

    if (!corpo || !modelo || !botaoAdicionar) {
        return;
    }

    function reindexar() {
        var linhas = corpo.querySelectorAll('tr.item-treino');

        linhas.forEach(function (linha, indice) {
            linha.querySelector('.ordem').textContent = indice + 1;

            linha.querySelectorAll('[name]').forEach(function (campo) {
                campo.name = campo.name.replace(/^Itens\[\d+\]/, 'Itens[' + indice + ']');
            });
        });

        if (avisoSemItens) {
            avisoSemItens.classList.toggle('d-none', linhas.length > 0);
        }
    }

    botaoAdicionar.addEventListener('click', function () {
        var novaLinha = modelo.content.cloneNode(true);
        corpo.appendChild(novaLinha);
        reindexar();

        var selects = corpo.querySelectorAll('tr.item-treino select');
        selects[selects.length - 1].focus();
    });

    corpo.addEventListener('click', function (evento) {
        var botao = evento.target.closest('.remover-item');
        if (!botao) {
            return;
        }

        botao.closest('tr.item-treino').remove();
        reindexar();
    });

    reindexar();
})();
