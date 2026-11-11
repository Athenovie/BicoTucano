namespace BicoTucano.Models
{
    public class Item_Pedido
    {

        public Guid ID_Item { get; set; }
        public int ID_Produto { get; set; }
        public string Nome { get; set; }
        public Decimal Quantidade { get; set; }
        public Decimal Preco { get; set; }
        public string Imagem { get; set; }

    }

}
