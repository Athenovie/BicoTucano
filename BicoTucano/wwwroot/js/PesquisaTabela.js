(function () {
    var campo = document.getElementById('pesquisa');
    var tabela = document.querySelector('table[data-pesquisavel]');
    if (!campo || !tabela) return;

    var aviso = document.getElementById('pesquisa-vazia');

    // tira acento e maiúscula
    function normalizar(texto) {
        return (texto || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
    }

    function soDigitos(texto) {
        return (texto || '').replace(/\D/g, '');
    }

    var itens = Array.prototype.map.call(tabela.querySelectorAll('tbody tr'), function (linha) {
        var trechos = Array.prototype.filter.call(linha.cells, function (celula) {
            return !celula.hasAttribute('data-sem-busca');
        }).map(function (celula) {
            return celula.textContent;
        });

        var extra = linha.getAttribute('data-busca');
        if (extra) trechos = trechos.concat(extra.split(/\s+/));

        return {
            linha: linha,
            texto: normalizar(trechos.join(' ')),
            numeros: trechos.map(soDigitos)
        };
    });

    function filtrar() {
        var digitado = campo.value.trim();
        var palavras = normalizar(digitado).split(/\s+/).filter(Boolean);

        var ehNumero = /^[\d\s.\-()]+$/.test(digitado);
        var digitos = soDigitos(digitado);

        var visiveis = 0;

        itens.forEach(function (item) {
            var combina;

            if (palavras.length === 0) {
                combina = true;
            } else if (ehNumero && digitos.length > 0) {
                combina = item.numeros.some(function (n) { return n.indexOf(digitos) > -1; });
            } else {
                combina = palavras.every(function (p) { return item.texto.indexOf(p) > -1; });
            }

            item.linha.style.display = combina ? '' : 'none';
            if (combina) visiveis++;
        });

        if (aviso) {
            aviso.style.display = (visiveis === 0 && itens.length > 0) ? 'block' : 'none';
        }
    }

    campo.addEventListener('input', filtrar);
    filtrar();
})();