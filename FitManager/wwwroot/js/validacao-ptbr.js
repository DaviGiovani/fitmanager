// Ajusta a validação do jQuery Validate para o formato brasileiro:
// mensagens em português e números com vírgula decimal (ex.: 89,90).
(function ($) {
    if (!$ || !$.validator) {
        return;
    }

    // Mensagens padrão em português (usadas nos campos sem mensagem própria).
    $.extend($.validator.messages, {
        required: 'Este campo é obrigatório.',
        email: 'Informe um e-mail válido.',
        number: 'Informe um número válido.',
        digits: 'Informe apenas números.',
        date: 'Informe uma data válida.',
        maxlength: $.validator.format('Informe no máximo {0} caracteres.'),
        minlength: $.validator.format('Informe ao menos {0} caracteres.'),
        min: $.validator.format('Informe um valor maior ou igual a {0}.'),
        max: $.validator.format('Informe um valor menor ou igual a {0}.'),
        range: $.validator.format('Informe um valor entre {0} e {1}.')
    });

    // Só os campos de texto usam vírgula decimal. Campos type="number" já chegam
    // no formato padrão (ponto), e campos de data são comparados como texto.
    function usaVirgula(element) {
        return element.type === 'text' || element.type === 'tel' || element.type === 'search';
    }

    function paraNumero(valor, element) {
        var texto = String(valor);
        if (usaVirgula(element)) {
            texto = texto.replace(/\./g, '').replace(',', '.');
        }
        return parseFloat(texto);
    }

    var numeroOriginal = $.validator.methods.number;
    var minOriginal = $.validator.methods.min;
    var maxOriginal = $.validator.methods.max;
    var rangeOriginal = $.validator.methods.range;

    $.validator.methods.number = function (value, element) {
        if (!usaVirgula(element)) {
            return numeroOriginal.call(this, value, element);
        }
        return this.optional(element) || /^-?\d{1,3}(\.\d{3})*(,\d+)?$/.test(value) || /^-?\d+(,\d+)?$/.test(value);
    };

    $.validator.methods.range = function (value, element, param) {
        if (!usaVirgula(element)) {
            return rangeOriginal.call(this, value, element, param);
        }
        var numero = paraNumero(value, element);
        return this.optional(element) || (numero >= param[0] && numero <= param[1]);
    };

    $.validator.methods.min = function (value, element, param) {
        if (!usaVirgula(element)) {
            return minOriginal.call(this, value, element, param);
        }
        return this.optional(element) || paraNumero(value, element) >= param;
    };

    $.validator.methods.max = function (value, element, param) {
        if (!usaVirgula(element)) {
            return maxOriginal.call(this, value, element, param);
        }
        return this.optional(element) || paraNumero(value, element) <= param;
    };
})(window.jQuery);
