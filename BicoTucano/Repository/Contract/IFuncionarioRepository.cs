using BicoTucano.Models;
using X.PagedList;

namespace BicoTucano.Repository.Contract
{
    public interface IFuncionarioRepository
    {
        Funcionario Login(string Email, string Senha);

        //CRUD
        void Cadastrar(Funcionario funcionario);
        void Atualizar(Funcionario funcionario);
        void AtualizarSenha(Funcionario funcionario);
        void Excluir(int Id);
        Funcionario ObterFuncionario(int Id);


        IEnumerable<Funcionario> ObterFuncionarioPorEmail();
        IEnumerable<Funcionario> ObterTodosFuncionarios();
        IPagedList<Funcionario> ObterTodosFuncionarios(int? pagina, string pesquisa);
    }
}
