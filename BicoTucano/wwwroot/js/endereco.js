$(function () {
    $(".CEP").mask("00000-000");

    function limparCep() {
        $("#Estado, #Cidade, #Bairro, #Logradouro").val("");
    }

    $("#CEP").on("change", function () {
        var cep = $(this).val().replace(/\D/g, "");

        if (cep === "") { limparCep(); return; }

        if (!/^[0-9]{8}$/.test(cep)) {
            limparCep();
            alert("Formato de CEP inválido.");
            return;
        }

        fetch("https://viacep.com.br/ws/" + cep + "/json/")
            .then(function (r) { return r.json(); })
            .then(function (dados) {
                if (dados.erro) {
                    limparCep();
                    alert("CEP não encontrado.");
                    return;
                }
                $("#Estado").val(dados.uf);
                $("#Cidade").val(dados.localidade);
                $("#Bairro").val(dados.bairro);
                $("#Logradouro").val(dados.logradouro);
                $("#Numero").focus();
            })
            .catch(function () {
                alert("Não foi possível consultar o CEP agora. Preencha os campos manualmente.");
            });
    });
});