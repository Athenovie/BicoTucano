using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IProdutoRepository
    {
        void Cadastrar(Produto produto);

        void Atualizar(Produto produto);
        IEnumerable<Produto> ObterTodosProdutos();
        Produto? ObterProduto(int Id);
    }
}
