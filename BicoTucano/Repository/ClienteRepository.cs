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
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "select u.ID_Usuario, u.Nome, u.DataNasc, u.Sexo, u.CPF, u.Telefone, u.Email, c.Situacao " +
                    "from tbUsuario u inner join tbCliente c on c.ID_Cliente = u.ID_Usuario " +
                    "WHERE u.ID_Usuario=@Id",
                    conexao
                );

                cmd.Parameters.AddWithValue("@Id", Id);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlDataReader dr;

                Cliente cliente = new Cliente();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    cliente.ID_Usuario = (Int32)(dr["ID_Usuario"]);
                    cliente.Nome = (string)(dr["Nome"]);
                    cliente.DataNasc = (DateTime)(dr["DataNasc"]);
                    cliente.Sexo = (string)(dr["Sexo"]);
                    cliente.CPF = (string)(dr["CPF"]);
                    cliente.Telefone = (Decimal)(dr["Telefone"]);
                    cliente.Email = (string)(dr["Email"]);
                    cliente.Situacao = (string)(dr["Situacao"]);
                }

                return cliente;
            }
        }

        public IEnumerable<Cliente> ObterTodosClientes()
        {
            List<Cliente> cliList = new List<Cliente>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "select u.ID_Usuario, u.Nome, u.DataNasc, u.Sexo, u.CPF, u.Telefone, u.Email,  c.Situacao " +
                    "from tbUsuario u inner join tbCliente c on c.ID_Cliente = u.ID_Usuario", conexao);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    cliList.Add(
                        new Cliente
                        {
                            ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]),
                            Nome = (string)(dr["Nome"]),
                            DataNasc = Convert.ToDateTime(dr["DataNasc"]),
                            Sexo = Convert.ToString(dr["Sexo"]),
                            CPF = Convert.ToString(dr["CPF"]),
                            Telefone = Convert.ToDecimal(dr["Telefone"]),
                            Email = Convert.ToString(dr["Email"]),
                            Situacao = Convert.ToString(dr["Situacao"])
                        });
                }
            }
            return cliList;
        }

        public IPagedList<Cliente> ObterTodosClientes(int? pagina, string pesquisa)
        {
            throw new NotImplementedException();
        }



        public void Ativar(int Id)
        {
            string Situacao = SituacaoConstant.Ativo;
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                MySqlCommand cmd = new MySqlCommand("update tbCliente set Situacao=@Situacao WHERE ID_Cliente=@Id", conexao);

                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = Id;
                cmd.Parameters.Add("@Situacao", MySqlDbType.VarChar).Value = Situacao;
                cmd.ExecuteNonQuery();
                conexao.Close();
            }
        }
        public void Desativar(int id)
        {
            string Situacao = SituacaoConstant.Desativado;
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                MySqlCommand cmd = new MySqlCommand("update tbCliente set Situacao=@Situacao WHERE ID_Cliente=@Id", conexao);

                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
                cmd.Parameters.Add("@Situacao", MySqlDbType.VarChar).Value = Situacao;
                cmd.ExecuteNonQuery();
                conexao.Close();
            }
        }
    }
}
