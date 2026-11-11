using System.ComponentModel.DataAnnotations;

namespace BicoTucano.Models
{

    public enum TipoPedido
    {
        Entrega = 1,
        Retirada = 2
    }

    public enum StatusPedido
    {
        EmPreparo = 1,
        EmEntrega = 2,
        Concluido = 3
    }
    public class Pedido
    {
       

        public int NF { get; set; }

        [Display(Name = "Data do Pedido")]
        public DateTime DataPedido { get; set; }

        public int ID_Usuario { get; set; }

        [Display(Name = "Tipo do Pedido")]
        [Required(ErrorMessage = "O tipo do pedido é obrigatório")]
        public TipoPedido TipoPedido { get; set; }

        [Display(Name = "Status do Pedido")]
        [Required(ErrorMessage = "O status do pedido é obrigatório")]
        public StatusPedido StatusPedido { get; set; }

        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Taxa de Entrega")]
        public decimal TaxaEntrega { get; set; }

        [Display(Name = "Valor Total")]
        public decimal ValorTotal { get; set; }

        [Display(Name = "CEP")]
        [MaxLength(9)]
        public string? CEP { get; set; }

        [Display(Name = "Detalhes do Pedido")]
        [MaxLength(150)]
        public string? DetalhesPedido { get; set; }

        public int? ID_Funcionario { get; set; }

    }
}
