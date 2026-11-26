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
        bool AtualizarSenha(string email, string novaSenha);

        void Desativar(int id);
        void Ativar(int id);
        Funcionario ObterFuncionario(int Id);

        void AtualizarDadosFuncionario(Funcionario funcionario);
        int ObterIdEndereco(int idUsuario);

        IEnumerable<Funcionario> ObterTodosFuncionarios();
    
    }
}
