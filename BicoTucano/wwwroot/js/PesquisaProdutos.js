
(function () {
    var campo = document.getElementById('pesquisa');
    var produtos = document.querySelectorAll('.produto-pesquisavel');
    var aviso = document.getElementById('pesquisa-vazia');

    if (!campo || produtos.length === 0) return;

    function normalizar(texto) {
        return (texto || '')
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .toLowerCase();
    }

    function filtrarProdutos() {
        var digitado = normalizar(campo.value.trim());
        var encontrados = 0;

        produtos.forEach(function (produto) {
            var texto = normalizar(
                produto.getAttribute('data-busca')
            );

            var corresponde = texto.includes(digitado);

            produto.style.display = corresponde ? '' : 'none';

            if (corresponde) {
                encontrados++;
            }
        });

        if (aviso) {
            aviso.style.display =
                encontrados === 0 ? 'block' : 'none';
        }
    }

    campo.addEventListener('input', filtrarProdutos);

    filtrarProdutos();
})();
