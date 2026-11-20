using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IProdutoRepository
    {
        void Cadastrar(Produto produto);

        void Atualizar(Produto produto);
        void AlterarDisponibilidade(int id, bool disponibilidade);
        IEnumerable<Produto> ObterTodosProdutos();
        IEnumerable<Produto> ObterProdutosAleatorios(int idProduto);
        Produto? ObterProduto(int Id);
    }
}
