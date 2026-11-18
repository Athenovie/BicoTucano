document.addEventListener("DOMContentLoaded", function () {

    const $ = (id) => document.getElementById(id);

    // CAMPOS DO FORMULÁRIO (IDs gerados pelo asp-for)
    const numeroInput = $("NumeroCartao");
    const nomeInput = $("NomeTitular");
    const apelidoInput = $("Apelido");
    const validadeInput = $("validade");

    // PRÉVIA DO CARTÃO
    const cartao = $("cartao");
    const bandeira = $("bandeira");
    const numeroVisu = $("numeroCartaoVisu");
    const nomeVisu = $("nomeCartaoVisu");
    const apelidoVisu = $("apelidoCartaoVisu");
    const validadeVisu = $("validadeCartaoVisu");
    const tipoVisu = $("tipo-cartao-visu");
    const seloBandeira = $("seloBandeira");

    const LOGO_PADRAO = "/img/Logo.png";

    const bandeiras = {
        visa: { nome: "Visa", imagem: "/img/Logo_Visa.png", classe: "cartao-visa" },
        mastercard: { nome: "Mastercard", imagem: "/img/Logo_Mastercard.svg", classe: "cartao-mastercard" },
        elo: { nome: "Elo", imagem: "/img/Logo_Elo.png", classe: "cartao-elo" },
        amex: { nome: "Amex", imagem: "/img/Logo_Amex.png", classe: "cartao-amex" }
    };

    // A ORDEM IMPORTA: Elo vem antes de Visa e Mastercard
    const REGEX_BANDEIRAS = [
        ["elo", /^(401178|401179|431274|438935|451416|457393|457631|457632|504175|5066|5067|509|627780|636297|636368|650[0-9]|651[67]|655[0-9])/],
        ["amex", /^3[47]/],
        ["visa", /^4/],
        ["mastercard", /^(5[1-5]|2(2[2-9]|[3-6]\d|7[01]|720))/]
    ];

    function detectarBandeira(numero) {
        numero = String(numero).replace(/\D/g, "");
        const achou = REGEX_BANDEIRAS.find(([, regex]) => regex.test(numero));
        return achou ? achou[0] : null;
    }

    function removerClassesBandeira() {
        if (!cartao) return;
        Object.values(bandeiras).forEach(b => cartao.classList.remove(b.classe));
    }

    function alterarBandeira(tipo) {
        const dados = bandeiras[tipo];
        if (!dados) { resetarBandeira(); return; }

        if (bandeira) {
            bandeira.src = dados.imagem;
            bandeira.alt = `Bandeira ${dados.nome}`;
            bandeira.classList.remove("cartao-bandeira-logo");   // volta o fundo branco
        }
        removerClassesBandeira();
        if (cartao) cartao.classList.add(dados.classe);
        if (tipoVisu) tipoVisu.textContent = dados.nome;
        if (seloBandeira) seloBandeira.textContent = dados.nome;
    }

    function resetarBandeira() {
        if (bandeira) {
            bandeira.src = LOGO_PADRAO;
            bandeira.alt = "Logo do Cartão";
            bandeira.classList.add("cartao-bandeira-logo");      // tira o fundo branco
        }
        removerClassesBandeira();
        if (tipoVisu) tipoVisu.textContent = "Tipo";
        if (seloBandeira) seloBandeira.textContent = "";
    }

    // NÚMERO (Amex: 15 dígitos em 4-6-5; demais: 16 em 4-4-4-4)
    function formatarNumero(digitos) {
        if (detectarBandeira(digitos) === "amex") {
            return digitos.substring(0, 15)
                .replace(/^(\d{4})(\d{1,6})/, "$1 $2")
                .replace(/^(\d{4}) (\d{6})(\d{1,5})/, "$1 $2 $3");
        }
        return digitos.substring(0, 16).replace(/(\d{4})(?=\d)/g, "$1 ");
    }

    if (numeroInput) {
        numeroInput.addEventListener("input", function () {
            const digitos = this.value.replace(/\D/g, "");
            const formatado = formatarNumero(digitos);

            this.value = formatado;
            if (numeroVisu) numeroVisu.textContent = formatado || "0000 0000 0000 0000";

            const tipo = digitos.length >= 6 ? detectarBandeira(digitos) : null;
            tipo ? alterarBandeira(tipo) : resetarBandeira();
        });
    }

    // NOME DO TITULAR
    if (nomeInput) {
        nomeInput.addEventListener("input", function () {
            const valor = this.value.replace(/[^a-zA-ZÀ-ÿ\s]/g, "")
                .replace(/\s{2,}/g, " ")
                .substring(0, 30);
            this.value = valor;
            if (nomeVisu) nomeVisu.textContent = valor.toUpperCase() || "NOME IMPRESSO";
        });
    }

    // APELIDO
    if (apelidoInput) {
        apelidoInput.addEventListener("input", function () {
            const valor = this.value.replace(/[^a-zA-ZÀ-ÿ0-9\s]/g, "")
                .replace(/\s{2,}/g, " ")
                .substring(0, 20);
            this.value = valor;
            if (apelidoVisu) apelidoVisu.textContent = valor || "APELIDO";
        });
    }

    // VALIDADE (MM/AA)
    if (validadeInput) {
        validadeInput.addEventListener("input", function () {
            let valor = this.value.replace(/\D/g, "").substring(0, 4);

            if (valor.length === 1 && Number(valor) > 1) valor = "0" + valor;

            if (valor.length >= 2) {
                const mes = Math.min(Math.max(Number(valor.substring(0, 2)), 1), 12);
                valor = String(mes).padStart(2, "0") + valor.substring(2);
            }

            if (valor.length > 2) valor = valor.substring(0, 2) + "/" + valor.substring(2);

            this.value = valor;
            if (validadeVisu) validadeVisu.textContent = valor || "MM/AA";

            if (valor.length === 5) {
                const mes = Number(valor.substring(0, 2));
                const ano = 2000 + Number(valor.substring(3, 5));
                const hoje = new Date();
                const venceu = ano < hoje.getFullYear() ||
                    (ano === hoje.getFullYear() && mes < hoje.getMonth() + 1);
                this.setCustomValidity(venceu ? "Cartão vencido" : "");
            } else {
                this.setCustomValidity("Informe a validade completa (MM/AA)");
            }
        });
    }

    // Se a página voltou com valores (erro do servidor), atualiza a prévia
    [numeroInput, nomeInput, apelidoInput, validadeInput].forEach(el => {
        if (el && el.value) el.dispatchEvent(new Event("input"));
    });
});