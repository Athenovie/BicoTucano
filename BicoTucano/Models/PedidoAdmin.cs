namespace BicoTucano.Models
{
    public class PedidoAdmin
    {
        // Uma linha da tabela "Pedidos"
        public class PedidoLista
        {
            public int NF { get; set; }
            public DateTime DataPedido { get; set; }
            public decimal ValorTotal { get; set; }
            public StatusPedido StatusPedido { get; set; }
            public string Cliente { get; set; } = "";
            public string Email { get; set; } = "";
            public string CPF { get; set; } = "";
            public string Produtos { get; set; } = "";  
        }

        // Uma linha da tabela "Itens do pedido"
        public class PedidoItemDetalhe
        {
            public string Produto { get; set; } = "";
            public int Quantidade { get; set; }
            public decimal Preco { get; set; }
            public decimal Subtotal { get { return Preco * Quantidade; } }
        }

        // Tela "Detalhes do pedido"
        public class PedidoDetalhe
        {
            public Pedido Pedido { get; set; } = new Pedido();
            public List<PedidoItemDetalhe> Itens { get; set; } = new List<PedidoItemDetalhe>();

            // Dados do cliente
            public string NomeCliente { get; set; } = "";
            public string Email { get; set; } = "";
            public string Telefone { get; set; } = "";
            public string CPF { get; set; } = "";

            // Endereço de entrega (vazio quando é retirada)
            public string Logradouro { get; set; } = "";
            public string Bairro { get; set; } = "";
            public string CEP { get; set; } = "";
            public string CidadeUF { get; set; } = "";

            // Resumo financeiro
            public string MetodoPagamento { get; set; } = "Não informado";
        }
    }
}
