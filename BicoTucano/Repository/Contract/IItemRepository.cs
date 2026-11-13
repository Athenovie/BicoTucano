using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IItemRepository
    {
        IEnumerable<Item_Pedido> ObterTodosItens(int nf);
    }
}
