using System.ComponentModel.DataAnnotations;

namespace BicoTucano.Models
{
    public class Endereco
    {
        [Display(Name = "Código", Description = "Código.")]
        public int ID_UsuarioEndereco { get; set; }

        [Display(Name = "Usuário", Description = "Usuário.")]
        public int ID_Usuario { get; set; }

        [Display(Name = "CEP", Description = "CEP.")]
        [MaxLength(9, ErrorMessage = "O CEP deve ter no máximo 9 caracteres.")]
        [Required(ErrorMessage = "O CEP é obrigatório.")]
        public string CEP { get; set; }

        [Display(Name = "Estado", Description = "Estado.")]
        [Required(ErrorMessage = "O Estado é obrigatório.")]
        public int Estado { get; set; }

        [Display(Name = "Cidade", Description = "Cidade.")]
        [Required(ErrorMessage = "A Cidade é obrigatória.")]
        public int Cidade { get; set; }

        [Display(Name = "Bairro", Description = "Bairro.")]
        [Required(ErrorMessage = "O Bairro é obrigatório.")]
        public int Bairro { get; set; }

        [Display(Name = "Logradouro", Description = "Logradouro.")]
        [Required(ErrorMessage = "O Logradouro é obrigatório.")]
        [MaxLength(75)]
        public string Logradouro { get; set; }

        [Display(Name = "Tipo de Endereço", Description = "Tipo de endereço.")]
        [Required(ErrorMessage = "O tipo de endereço é obrigatório.")]
        [MaxLength(50)]
        public string NomeEndereco{ get; set; }

        [Display(Name = "Número", Description = "Número.")]
        [Required(ErrorMessage = "O número é obrigatório.")]
        [MaxLength(10)]
        public string Numero { get; set; }

        [Display(Name = "Complemento", Description = "Complemento.")]
        [MaxLength(200)]
        public string Complemento { get; set; }
    }
}