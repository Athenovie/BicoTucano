using System.ComponentModel.DataAnnotations;

namespace BicoTucano.Models
{
    public class Cartao
    {
        public int ID_Cartao { get; set; }

        public int ID_Usuario { get; set; }

        [Display(Name = "Número do cartão")]
        [Required(ErrorMessage = "Informe o número do cartão.")]
        [MaxLength(100)]
        public string NumeroCartao { get; set; }

        [Display(Name = "Nome do titular")]
        [Required(ErrorMessage = "Informe o nome impresso no cartão.")]
        [MaxLength(75, ErrorMessage = "Máximo de 75 caracteres.")]
        public string NomeTitular { get; set; }

        [Display(Name = "Apelido do cartão")]
        [Required(ErrorMessage = "Dê um apelido ao cartão (ex.: Meu Nubank).")]
        [MaxLength(50, ErrorMessage = "Máximo de 50 caracteres.")]
        public string Apelido { get; set; }

        [Display(Name = "Validade")]
        public DateTime DataValidade { get; set; }

        [Display(Name = "Bandeira")]
        [MaxLength(100)]
        public string? Bandeira { get; set; }
    }
}
