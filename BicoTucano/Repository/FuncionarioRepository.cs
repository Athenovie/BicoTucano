using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using X.PagedList;

namespace BicoTucano.Repository
{
    public class FuncionarioRepository : IFuncionarioRepository
    {
        public void Atualizar(Funcionario funcionario)
        {
            throw new NotImplementedException();
        }

        public void AtualizarSenha(Funcionario funcionario)
        {
            throw new NotImplementedException();
        }

        public void Cadastrar(Funcionario funcionario)
        {
            throw new NotImplementedException();
        }

        public void Excluir(int Id)
        {
            throw new NotImplementedException();
        }

        public Funcionario Login(string Email, string Senha)
        {
            throw new NotImplementedException();
        }

        public Funcionario ObterFuncionario(int Id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Funcionario> ObterFuncionarioPorEmail()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Funcionario> ObterTodosFuncionarios()
        {
            throw new NotImplementedException();
        }

        public IPagedList<Funcionario> ObterTodosFuncionarios(int? pagina, string pesquisa)
        {
            throw new NotImplementedException();
        }
    }
}
