using System.ComponentModel.DataAnnotations;

namespace BicoTucano.Models
{
    public class Cliente : Usuario
    {
        [Display(Name = "Data de Cadastro")]
        [Required(ErrorMessage = "A data é obrigatoria")]
        public DateTime DataCadastro { get; set; }

        [Display(Name = "Situação")]
        [Required(ErrorMessage = "A situação é obrigatoria")]
        public string Situacao { get; set; }
    }
}
