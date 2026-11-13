using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IPedidoRepository
    {
        int Cadastrar(Pedido pedido, IEnumerable<Item_Pedido> itens);
        Pedido? ObterPedidos(int nf);
    }
}
