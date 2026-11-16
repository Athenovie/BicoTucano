using BicoTucano.Models;
namespace BicoTucano.Repository.Contract
{
    public interface ICartaoRepository
    {
        void Cadastrar(Cartao cartao);
        IEnumerable<Cartao> ObterCartoesDoUsuario(int idUsuario);
        Cartao? ObterCartao(int id);
        void Excluir(int id);
    }
}
