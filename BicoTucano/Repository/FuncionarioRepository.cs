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
                    funcionario.NivelAcesso = (NivelAcesso)Enum.Parse(typeof(NivelAcesso), dr["NivelAcesso"].ToString());
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
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmdUsuario = new MySqlCommand(
                    "insert into tbUsuario(Nome, DataNasc, Sexo, CPF, Telefone, Email, Senha) " +
                    "values (@Nome, @DataNasc, @Sexo, @CPF, @Telefone, @Email, @Senha)",
                    conexao
                );

                cmdUsuario.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = funcionario.Nome;
                cmdUsuario.Parameters.Add("@DataNasc", MySqlDbType.DateTime).Value = funcionario.DataNasc.ToString("yyyy/MM/dd");
                cmdUsuario.Parameters.Add("@Sexo", MySqlDbType.VarChar).Value = funcionario.Sexo;
                cmdUsuario.Parameters.Add("@CPF", MySqlDbType.VarChar).Value = funcionario.CPF;
                cmdUsuario.Parameters.Add("@Telefone", MySqlDbType.VarChar).Value = funcionario.Telefone;
                cmdUsuario.Parameters.Add("@Email", MySqlDbType.VarChar).Value = funcionario.Email;
                cmdUsuario.Parameters.Add("@Senha", MySqlDbType.VarChar).Value = funcionario.Senha;

                cmdUsuario.ExecuteNonQuery();

                long idUsuario = cmdUsuario.LastInsertedId;

                MySqlCommand cmdFuncionario = new MySqlCommand(
                    "insert into tbFuncionario(ID_Funcionario, DataAdmissao, NivelAcesso, Cargo) " +
                    "values (@ID_Funcionario, @DataAdmissao, @NivelAcesso, @Cargo)",
                    conexao
                );

                cmdFuncionario.Parameters.Add("@ID_Funcionario", MySqlDbType.Int32).Value = idUsuario;
                cmdFuncionario.Parameters.Add("@DataAdmissao", MySqlDbType.DateTime).Value = funcionario.DataAdmissao.ToString("yyyy/MM/dd");
                cmdFuncionario.Parameters.Add("@NivelAcesso", MySqlDbType.VarChar).Value = funcionario.NivelAcesso;
                cmdFuncionario.Parameters.Add("@Cargo", MySqlDbType.VarChar).Value = funcionario.Cargo;

                cmdFuncionario.ExecuteNonQuery();
                conexao.Close();
            }
        }

        public void Excluir(int Id)
        {
            throw new NotImplementedException();
        }



        public Funcionario ObterFuncionario(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "select u.ID_Usuario, u.Nome, u.DataNasc, u.Sexo, u.CPF, u.Telefone, u.Email," +
                    "f.NivelAcesso, f.Cargo, f.DataAdmissao, f.DataDemissao " +
                    "from tbUsuario u inner join tbFuncionario f on f.ID_Funcionario = u.ID_Usuario " +
                    "WHERE u.ID_Usuario=@Id",
                    conexao
                );

                cmd.Parameters.AddWithValue("@Id", Id);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlDataReader dr;

                Funcionario funcionario = new Funcionario();

                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    funcionario.ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]);
                    funcionario.Nome = (string)(dr["Nome"]);
                    funcionario.DataNasc = (DateTime)(dr["DataNasc"]);
                    funcionario.Sexo = (string)(dr["Sexo"]);
                    funcionario.CPF = (string)(dr["CPF"]);
                    funcionario.Telefone = (Decimal)(dr["Telefone"]);
                    funcionario.Email = (string)(dr["Email"]);
                    funcionario.NivelAcesso = (NivelAcesso)Enum.Parse(typeof(NivelAcesso), dr["NivelAcesso"].ToString());
                    funcionario.Cargo = (string)(dr["Cargo"]);
                    funcionario.DataAdmissao = (DateTime)(dr["DataAdmissao"]);

                    if (dr["DataDemissao"] != DBNull.Value)
                    {
                        funcionario.DataDemissao = (DateTime)(dr["DataDemissao"]);
                    }
                }

                return funcionario;
            }
        }

        public IEnumerable<Funcionario> ObterFuncionarioPorEmail()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Funcionario> ObterTodosFuncionarios()
        {
            List<Funcionario> funcList = new List<Funcionario>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "select u.ID_Usuario, u.Nome, u.DataNasc, u.Sexo, u.CPF, u.Telefone, u.Email,  " +
                    "f.NivelAcesso, f.Cargo, f.DataAdmissao, f.DataDemissao " +
                    "from tbUsuario u inner join tbFuncionario f on f.ID_Funcionario = u.ID_Usuario",
                    conexao
                );

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    Funcionario funcionario = new Funcionario
                    {
                        ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]),
                        Nome = (string)(dr["Nome"]),
                        DataNasc = Convert.ToDateTime(dr["DataNasc"]),
                        Sexo = Convert.ToString(dr["Sexo"]),
                        CPF = Convert.ToString(dr["CPF"]),
                        Telefone = Convert.ToDecimal(dr["Telefone"]),
                        Email = Convert.ToString(dr["Email"]),
                        
                        NivelAcesso = (NivelAcesso)Enum.Parse(typeof(NivelAcesso), dr["NivelAcesso"].ToString()),
                        Cargo = Convert.ToString(dr["Cargo"]),
                        DataAdmissao = Convert.ToDateTime(dr["DataAdmissao"])
                    };

                    if (dr["DataDemissao"] != DBNull.Value)
                    {
                        funcionario.DataDemissao = Convert.ToDateTime(dr["DataDemissao"]);
                    }

                    funcList.Add(funcionario);
                }
            }
            return funcList;
        }

        public IPagedList<Funcionario> ObterTodosFuncionarios(int? pagina, string pesquisa)
        {
            throw new NotImplementedException();
        }

        public void Desativar(Funcionario funcionario)
        {
            throw new NotImplementedException();
        }

        public void Ativar(Funcionario funcionario)
        {
            throw new NotImplementedException();
        }
    }
}
