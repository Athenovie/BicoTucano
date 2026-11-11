using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    public interface IProdutoRepository
    {
        IEnumerable<Produto> ObterTodosProdutos();
        Produto? ObterProduto(int Id);
    }
}
