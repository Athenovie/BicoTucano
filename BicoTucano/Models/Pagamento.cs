namespace BicoTucano.Models
{
    public enum FormaPagamento
    {
        Pix = 1,
        CartaoDebito = 2,
        CartaoCredito = 3
    }

    public enum StatusPagamento
    {
        Pago = 1,
        NaoPago = 2
    }

   
    public class Pagamento
    {
        public int ID_Pagamento { get; set; }

        public int NF { get; set; }

        public FormaPagamento FormaPagamento { get; set; }

        public string? ChavePix { get; set; }

        public int? ID_Cartao { get; set; }

        public decimal ValorPago { get; set; }

        public DateTime? DataPagamento { get; set; }

        public StatusPagamento StatusPagamento { get; set; }
    }
}