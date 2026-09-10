using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;
using System.Data;
using X.PagedList;

namespace BicoTucano.Repository
{
    public class ClienteRepository : IClienteRepository
    {
        
        private readonly string _conexaoMySQL;
        IConfiguration _config;

        public ClienteRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
            _config = conf;
        }



        public Cliente Login(string Email, string Senha)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT u.ID_Usuario, u.Nome, u.DataNasc, u.Sexo, u.CPF, u.Telefone,
                 u.Email, u.Senha, c.DataCadastro, c.Situacao
          FROM tbUsuario u
          INNER JOIN tbCliente c ON c.ID_Cliente = u.ID_Usuario
          WHERE u.Email = @Email AND u.Senha = @Senha", conexao);

                cmd.Parameters.Add("@Email", MySqlDbType.VarChar).Value = Email;
                cmd.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = Senha;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlDataReader dr;

                Cliente cliente = new Cliente();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                    {
                        cliente.ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]);
                        cliente.Nome = Convert.ToString(dr["Nome"]);
                        cliente.DataNasc = Convert.ToDateTime(dr["DataNasc"]);
                        cliente.Sexo = Convert.ToString(dr["Sexo"]);
                        cliente.CPF = Convert.ToString(dr["CPF"]);
                        cliente.Telefone = Convert.ToDecimal(dr["Telefone"]);
                        cliente.Situacao = Convert.ToString(dr["Situacao"]);
                        cliente.Email = Convert.ToString(dr["Email"]);
                        cliente.Senha = Convert.ToString(dr["Senha"]);
                    }
                

                return cliente;
            }
        }

        public void Cadastrar(Cliente cliente)
        {
            string Situacao = SituacaoConstant.Ativo;

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmdUsuario = new MySqlCommand(
                    "insert into tbUsuario(Nome, DataNasc, Sexo, CPF, Telefone, Email, Senha) " +
                    "values (@Nome, @DataNasc, @Sexo, @CPF, @Telefone, @Email, @Senha)",
                    conexao
                ); 

                cmdUsuario.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = cliente.Nome;
                cmdUsuario.Parameters.Add("@DataNasc", MySqlDbType.DateTime).Value = cliente.DataNasc.ToString("yyyy/MM/dd");
                cmdUsuario.Parameters.Add("@Sexo", MySqlDbType.VarChar).Value = cliente.Sexo;
                cmdUsuario.Parameters.Add("@CPF", MySqlDbType.VarChar).Value = cliente.CPF;
                cmdUsuario.Parameters.Add("@Telefone", MySqlDbType.VarChar).Value = cliente.Telefone;
                cmdUsuario.Parameters.Add("@Email", MySqlDbType.VarChar).Value = cliente.Email;
                cmdUsuario.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = cliente.Senha;

                cmdUsuario.ExecuteNonQuery();

                long idUsuario = cmdUsuario.LastInsertedId;

                MySqlCommand cmdCliente = new MySqlCommand(
                    "insert into tbCliente(ID_Cliente, DataCadastro, Situacao) " +
                    "values (@ID_Cliente, @DataCadastro, @Situacao)",
                    conexao
                ); 

                cmdCliente.Parameters.Add("@ID_Cliente", MySqlDbType.Int32).Value = idUsuario;
                cmdCliente.Parameters.Add("@DataCadastro", MySqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy/MM/dd");
                cmdCliente.Parameters.Add("@Situacao", MySqlDbType.VarChar).Value = Situacao;

                cmdCliente.ExecuteNonQuery();
                conexao.Close();
            }
        }

        public void Atualizar(Cliente cliente)
        {
            throw new NotImplementedException();
        }

        

        public void Excluir(int Id)
        {
            throw new NotImplementedException();
        }

       

        public Cliente ObterCliente(int Id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Cliente> ObterTodosClientes()
        {
            throw new NotImplementedException();
        }

        public IPagedList<Cliente> ObterTodosClientes(int? pagina, string pesquisa)
        {
            throw new NotImplementedException();
        }
    }
}
