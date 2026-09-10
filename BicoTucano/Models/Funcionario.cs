using System.ComponentModel.DataAnnotations;

namespace BicoTucano.Models
{


    public enum NivelAcesso
    {
        Comum = 2,
        Administrador = 1
    }

    public class Funcionario : Usuario
    {
        [Display(Name = "Nível de acesso")]
        [Required(ErrorMessage = "O Nível de acesso  é obrigatório")]
        public NivelAcesso NivelAcesso { get; set; }

        [Display(Name = "Cargo")]
        [Required(ErrorMessage = "O cargo é obrigatório")]
        public string Cargo { get; set; }

        [Display(Name = "Data de Admissão")]
        [Required(ErrorMessage = "A data é obrigatorio")]
        public DateTime DataAdmissao { get; set; }


        [Display(Name = "Data de demissão")]
        public DateTime DataDemissao { get; set; }
    }
}
