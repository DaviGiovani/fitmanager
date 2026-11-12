// FitManager – scripts gerais

// Máscara de CPF: 000.000.000-00
document.querySelectorAll('[data-mascara="cpf"]').forEach(function (campo) {
    campo.addEventListener('input', function () {
        var d = campo.value.replace(/\D/g, '').slice(0, 11);
        var texto = d;
        if (d.length > 9) {
            texto = d.slice(0, 3) + '.' + d.slice(3, 6) + '.' + d.slice(6, 9) + '-' + d.slice(9);
        } else if (d.length > 6) {
            texto = d.slice(0, 3) + '.' + d.slice(3, 6) + '.' + d.slice(6);
        } else if (d.length > 3) {
            texto = d.slice(0, 3) + '.' + d.slice(3);
        }
        campo.value = texto;
    });
});

// Máscara de telefone: (00) 00000-0000
document.querySelectorAll('[data-mascara="telefone"]').forEach(function (campo) {
    campo.addEventListener('input', function () {
        var d = campo.value.replace(/\D/g, '').slice(0, 11);
        var texto = d;
        if (d.length > 10) {
            texto = '(' + d.slice(0, 2) + ') ' + d.slice(2, 7) + '-' + d.slice(7);
        } else if (d.length > 6) {
            texto = '(' + d.slice(0, 2) + ') ' + d.slice(2, 6) + '-' + d.slice(6);
        } else if (d.length > 2) {
            texto = '(' + d.slice(0, 2) + ') ' + d.slice(2);
        }
        campo.value = texto;
    });
});

// Confirmação antes de ações irreversíveis (RNF04): <form data-confirmar="mensagem">
document.querySelectorAll('form[data-confirmar]').forEach(function (formulario) {
    formulario.addEventListener('submit', function (evento) {
        if (!window.confirm(formulario.getAttribute('data-confirmar'))) {
            evento.preventDefault();
        }
    });
});

// Campos de valor: quem digitar ponto como separador decimal (89.90) tem o
// valor convertido para o formato brasileiro (89,90).
document.addEventListener('change', function (evento) {
    var campo = evento.target;
    if (campo && campo.matches && campo.matches('input[inputmode="decimal"]')) {
        var texto = campo.value.trim();
        if (/^\d+\.\d{1,2}$/.test(texto)) {
            campo.value = texto.replace('.', ',');
        }
    }
});

// Botão de olho nos campos de senha: mostra ou esconde os caracteres digitados.
document.querySelectorAll('input[type="password"]').forEach(function (campo) {
    var grupo = document.createElement('div');
    grupo.className = 'input-group';
    campo.parentNode.insertBefore(grupo, campo);
    grupo.appendChild(campo);

    var botao = document.createElement('button');
    botao.type = 'button';
    botao.className = 'btn btn-outline-secondary btn-senha';
    botao.tabIndex = -1;
    grupo.appendChild(botao);

    function atualizar() {
        var visivel = campo.type === 'text';
        var texto = visivel ? 'Esconder senha' : 'Mostrar senha';
        botao.innerHTML = visivel ? '<i class="bi bi-eye-slash"></i>' : '<i class="bi bi-eye"></i>';
        botao.title = texto;
        botao.setAttribute('aria-label', texto);
    }

    botao.addEventListener('click', function () {
        campo.type = campo.type === 'password' ? 'text' : 'password';
        atualizar();
        campo.focus();
    });

    // Volta a esconder a senha antes de enviar o formulário.
    if (campo.form) {
        campo.form.addEventListener('submit', function () {
            campo.type = 'password';
            atualizar();
        });
    }

    atualizar();
});
