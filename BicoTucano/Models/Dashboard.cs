namespace BicoTucano.Models
{
    public class Dashboard
    {
        public int PedidosEmAndamento { get; set; }
        public int PedidosHoje { get; set; }
        public decimal FaturamentoMes { get; set; }
        public int TotalClientes { get; set; }
        public decimal TotalSemana { get; set; }

        public List<VendaDia> VendasSemana { get; set; } = new List<VendaDia>();
        public List<PratoRanking> MaisVendidos { get; set; } = new List<PratoRanking>();
        public List<PedidoDashboard> UltimosPedidos { get; set; } = new List<PedidoDashboard>();
    }

    public class VendaDia
    {
        public DateTime Data { get; set; }
        public decimal Total { get; set; }
    }

    public class PratoRanking
    {
        public string Nome { get; set; } = "";
        public string NomeImagem { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal Total { get; set; }
    }

    public class PedidoDashboard
    {
        public int NF { get; set; }
        public string Cliente { get; set; } = "";
        public StatusPedido StatusPedido { get; set; }
        public decimal ValorTotal { get; set; }
    }
}