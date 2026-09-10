using System.ComponentModel.DataAnnotations;

namespace BicoTucano.Models
{
    public class Usuario
    {

        [Display(Name = "Codigo", Description = "Codigo.")]
        public int ID_Usuario { get; set; }

        [Display(Name = "Nome Completo", Description = "Nome e Sobrenome.")]
        [Required(ErrorMessage = "O Nome completo é obrigatorio")]
        public string Nome { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "O email é obrigatorio")]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Informe um email válido")]
        public string Email { get; set; }

        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A senha é obrigatorio")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 a 10 caracteres")]
        public string Senha { get; set; }


        [Display(Name = "Nascimento")]
        [Required(ErrorMessage = "A data é obrigatorio")]
        public DateTime DataNasc { get; set; }



        [Display(Name = "Sexo")]
        [Required(ErrorMessage = "O Sexo é obrigatorio")]
        [StringLength(1, ErrorMessage = "Deve ter 1 caracter")]
        public string Sexo { get; set; }

        [Display(Name = "CPF")]
        [Required(ErrorMessage = "O CPF é obrigatorio")]
        public string CPF { get; set; }


        [Display(Name = "Celular")]
        [Required(ErrorMessage = "O celular é obrigatorio")]
        public decimal Telefone { get; set; }
    }
}
