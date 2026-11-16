using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IPagamentoRepository
    {
        int Cadastrar(Pagamento pagamento);
        void Atualizar(Pagamento pagamento);
        Pagamento? ObterPagamentoDoPedido(int nf);
    }
}
