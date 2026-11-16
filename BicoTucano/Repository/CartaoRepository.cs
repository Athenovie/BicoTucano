using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;

namespace BicoTucano.Repository
{
    public class CartaoRepository : ICartaoRepository
    {
        private readonly string _conexaoMySQL;

        public CartaoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
        }

        private const string SelectBase =
            "SELECT ID_Cartao, ID_Usuario, NumeroCartao, NomeTitular, Apelido, DataValidade, Bandeira FROM tbCartao ";

        public void Cadastrar(Cartao cartao)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO tbCartao (ID_Usuario, NumeroCartao, NomeTitular, Apelido, DataValidade, Bandeira) " +
                    "VALUES (@ID_Usuario, @NumeroCartao, @NomeTitular, @Apelido, @DataValidade, @Bandeira)",
                    conexao);

                cmd.Parameters.AddWithValue("@ID_Usuario", cartao.ID_Usuario);
                cmd.Parameters.AddWithValue("@NumeroCartao", cartao.NumeroCartao);
                cmd.Parameters.AddWithValue("@NomeTitular", cartao.NomeTitular);
                cmd.Parameters.AddWithValue("@Apelido", cartao.Apelido);
                cmd.Parameters.AddWithValue("@DataValidade", cartao.DataValidade);
                cmd.Parameters.AddWithValue("@Bandeira", (object?)cartao.Bandeira ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<Cartao> ObterCartoesDoUsuario(int idUsuario)
        {
            return Consultar(SelectBase + "WHERE ID_Usuario=@Valor ORDER BY ID_Cartao", idUsuario);
        }

        public Cartao? ObterCartao(int id)
        {
            return Consultar(SelectBase + "WHERE ID_Cartao=@Valor", id).FirstOrDefault();
        }

        
        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand("DELETE FROM tbCartao WHERE ID_Cartao=@ID_Cartao", conexao);
                cmd.Parameters.AddWithValue("@ID_Cartao", id);
                cmd.ExecuteNonQuery();
            }
        }

        private List<Cartao> Consultar(string sql, int valor)
        {
            var lista = new List<Cartao>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Valor", valor);

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Cartao
                        {
                            ID_Cartao = Convert.ToInt32(dr["ID_Cartao"]),
                            ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]),
                            NumeroCartao = Convert.ToString(dr["NumeroCartao"]) ?? "",
                            NomeTitular = Convert.ToString(dr["NomeTitular"]) ?? "",
                            Apelido = Convert.ToString(dr["Apelido"]) ?? "",
                            DataValidade = Convert.ToDateTime(dr["DataValidade"]),
                            Bandeira = dr["Bandeira"] as string
                        });
                    }
                }
            }
            return lista;
        }
    }
}