namespace BicoTucano.Models
{
    public class Produto
    {
        public int ID_Produto { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public string Descricao { get; set; }
        public bool Disponibilidade { get; set; }
        public int ID_Categoria { get; set; }
        public string Categoria { get; set; }
        public string NomeImagem { get; set; }
    }
}
