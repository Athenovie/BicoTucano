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
                using (var transacao = conexao.BeginTransaction())
                {

                    MySqlCommand cmdUsuario = new MySqlCommand(
                    "insert into tbUsuario(Nome, DataNasc, Sexo, CPF, Telefone, Email, Senha) " +
                    "values (@Nome, @DataNasc, @Sexo, @CPF, @Telefone, @Email, @Senha)",
                    conexao,
                    transacao
                );

                    cmdUsuario.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = cliente.Nome;
                    cmdUsuario.Parameters.Add("@DataNasc", MySqlDbType.DateTime).Value = cliente.DataNasc.HasValue ? cliente.DataNasc.Value : DBNull.Value;
                    // HasValue verifica se a data foi preenchida
                    // se sim, envia a data; caso contrário, envia NULL ao banco
                    // foi necessário porque DataNasc é do tipo DateTime? (nullable) (alterei pra DateTime? por conta da msg de erro)
                    cmdUsuario.Parameters.Add("@Sexo", MySqlDbType.VarChar).Value = cliente.Sexo;
                    cmdUsuario.Parameters.Add("@CPF", MySqlDbType.VarChar).Value = cliente.CPF;
                    cmdUsuario.Parameters.Add("@Telefone", MySqlDbType.Decimal).Value = cliente.Telefone;
                    cmdUsuario.Parameters.Add("@Email", MySqlDbType.VarChar).Value = cliente.Email;
                    cmdUsuario.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = cliente.Senha;

                    cmdUsuario.ExecuteNonQuery();

                    long idUsuario = cmdUsuario.LastInsertedId;

                    MySqlCommand cmdCliente = new MySqlCommand(
                        "insert into tbCliente(ID_Cliente, DataCadastro, Situacao) " +
                        "values (@ID_Cliente, @DataCadastro, @Situacao)",
                        conexao,
                        transacao
                    );

                    cmdCliente.Parameters.Add("@ID_Cliente", MySqlDbType.Int32).Value = idUsuario;
                    cmdCliente.Parameters.Add("@DataCadastro", MySqlDbType.DateTime).Value = DateTime.Now;
                    cmdCliente.Parameters.Add("@Situacao", MySqlDbType.VarChar).Value = Situacao;

                    cmdCliente.ExecuteNonQuery();

                    transacao.Commit();
                }
                conexao.Close();
                
            }
        }

        public void Atualizar(Cliente cliente)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmdUsuario = new MySqlCommand(
                    "update tbUsuario set Nome=@Nome, DataNasc=@DataNasc, Sexo=@Sexo, CPF=@CPF," +
                    "Telefone=@Telefone, Email=@Email where ID_Usuario=@Id", conexao);

                cmdUsuario.Parameters.Add("@Id", MySqlDbType.Int32).Value = cliente.ID_Usuario;
                cmdUsuario.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = cliente.Nome;
                cmdUsuario.Parameters.Add("@DataNasc", MySqlDbType.DateTime).Value = cliente.DataNasc.HasValue ? cliente.DataNasc.Value : DBNull.Value;
                cmdUsuario.Parameters.Add("@Sexo", MySqlDbType.VarChar).Value = cliente.Sexo;
                cmdUsuario.Parameters.Add("@CPF", MySqlDbType.VarChar).Value = cliente.CPF;
                cmdUsuario.Parameters.Add("@Telefone", MySqlDbType.Decimal).Value = cliente.Telefone;
                cmdUsuario.Parameters.Add("@Email", MySqlDbType.VarChar).Value = cliente.Email;

                cmdUsuario.ExecuteNonQuery();
                
            }
        }

        

        public void Excluir(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    string[] comandos =
                    {
                "delete from tbUsuarioEndereco where ID_Usuario=@Id",
                "delete from tbCartao where ID_Usuario=@Id",
                "delete from tbCliente where ID_Cliente=@Id",
                "delete from tbUsuario where ID_Usuario=@Id"
            };

                    foreach (string sql in comandos)
                    {
                        MySqlCommand cmd = new MySqlCommand(sql, conexao, transacao);
                        cmd.Parameters.AddWithValue("@Id", Id);
                        cmd.ExecuteNonQuery();
                    }

                    transacao.Commit();
                }
            }
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
                    cliente.Telefone = Convert.ToDecimal(dr["Telefone"]);
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

        public Cliente BuscaCpfCliente(string CPF)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                MySqlCommand cmd = new MySqlCommand("select CPF from tbUsuario WHERE CPF=@CPF", conexao);
                cmd.Parameters.AddWithValue("@CPF", CPF);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlDataReader dr;

                Cliente cliente = new Cliente();
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    cliente.CPF = (string)(dr["CPF"]);
                }
                return cliente;
            }
        }

        public Cliente BuscaEmailCliente(string email)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                MySqlCommand cmd = new MySqlCommand("select Email from tbUsuario WHERE Email=@Email", conexao);
                cmd.Parameters.AddWithValue("@Email", email);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlDataReader dr;

                Cliente cliente = new Cliente();
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    cliente.Email = (string)(dr["Email"]);
                }
                return cliente;
            }
        }
    }
}
