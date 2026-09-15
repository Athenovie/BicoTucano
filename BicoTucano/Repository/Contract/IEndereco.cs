using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IEndereco
    {
        void Cadastrar(Endereco endereco);
        void Atualizar(Endereco endereco);

        void Excluir(int Id);

        Endereco ObterEndereco(int Id);

        IEnumerable<Endereco> ObterTodosEnderecos();
    }
}
