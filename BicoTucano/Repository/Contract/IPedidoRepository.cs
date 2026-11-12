using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IPedidoRepository
    {
        IEnumerable<Pedido> ObterTodosItens();

        void Cadastrar(Pedido pedido);
        void Atualizar(Pedido pedido);

        Pedido ObterPedidos(int id);

        void buscaIdPedido(Pedido pedido);

        void Excluir(int id);
    }
}
