using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IItemRepository
    {
        IEnumerable<Item_Pedido> ObterTodosItens();

        void Cadastrar(Item_Pedido item);
        void Atualizar(Item_Pedido item);

        Item_Pedido ObterItens(int id);

        void Excluir(int id);
    }
}
