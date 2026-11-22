using BicoTucano.Models;
using static BicoTucano.Models.PedidoAdmin;

namespace BicoTucano.Repository.Contract
{
    public interface IPedidoRepository
    {
        int Cadastrar(Pedido pedido, IEnumerable<Item_Pedido> itens);
        Pedido? ObterPedidos(int nf);
        IEnumerable<PedidoLista> ObterTodosPedidosAdmin();
        PedidoDetalhe? ObterDetalhePedido(int nf);
        void AtualizarStatus(int nf, StatusPedido status);

        IEnumerable<Pedido> ObterPedidosDoCliente(int idUsuario);

        Dashboard ObterDashboard();

    }
}
