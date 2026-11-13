namespace BicoTucano.Models
{
    public class Item_Pedido
    {

        public int ID_Item { get; set; }
        public int NF { get; set; }
        public int ID_Produto { get; set; }
        public int Quantidade { get; set; }
        public Decimal Preco { get; set; }

    }

}
