using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Data;
using X.PagedList;

namespace BicoTucano.Repository
{
    public class FuncionarioRepository : IFuncionarioRepository
    {

        private readonly string _conexaoMySQL;
        private IConfiguration _conf;

        public FuncionarioRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
            _conf = conf;
        }


        public Funcionario Login(string Email, string Senha)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "select u.ID_Usuario, u.Nome, u.Email, u.Senha, f.NivelAcesso, f.Cargo " +
                    "from tbUsuario u inner join tbFuncionario f on f.ID_Funcionario = u.ID_Usuario " +
                    "where u.Email = @Email and u.Senha = @Senha", conexao);

                cmd.Parameters.Add("@Email", MySqlDbType.VarChar).Value = Email;
                cmd.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = Senha;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlDataReader dr;

                Funcionario funcionario = new Funcionario();
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    funcionario.ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]);
                    funcionario.Nome = (string)(dr["Nome"]);
                    funcionario.NivelAcesso = (string)(dr["NivelAcesso"]);
                    funcionario.Email = (string)(dr["Email"]);
                    funcionario.Senha = (string)(dr["Senha"]);
                }
                return funcionario;
            }
        }

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
