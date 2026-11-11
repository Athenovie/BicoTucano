using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;
using System.Data;

namespace BicoTucano.Repository
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly string _conexaoMySQL;

        public ProdutoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
        }

        public Produto ObterProduto(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT p.ID_Produto, p.Nome, p.Preco, p.Descricao, " +
                    "p.Disponibilidade, p.ID_Categoria, c.Categoria, i.NomeImagem " +
                    "FROM tbProduto p " +
                    "INNER JOIN tbCategoria c ON p.ID_Categoria = c.ID_Categoria " +
                    "LEFT JOIN tbImagem i ON p.ID_Produto = i.ID_Produto " +
                    "WHERE p.ID_Produto = @Id",
                    conexao
                );

                cmd.Parameters.AddWithValue("@Id", Id);

                MySqlDataReader dr = cmd.ExecuteReader();

                Produto? produto = null;

                while (dr.Read())
                {
                    produto = new Produto
                    {
                        ID_Produto = Convert.ToInt32(dr["ID_Produto"]),
                        Nome = Convert.ToString(dr["Nome"]) ?? "",
                        Preco = Convert.ToDecimal(dr["Preco"]),
                        Descricao = Convert.ToString(dr["Descricao"]) ?? "",
                        Disponibilidade = Convert.ToBoolean(dr["Disponibilidade"]),
                        ID_Categoria = Convert.ToInt32(dr["ID_Categoria"]),
                        Categoria = Convert.ToString(dr["Categoria"]) ?? "",
                        NomeImagem = dr["NomeImagem"] == DBNull.Value ? "" : Convert.ToString(dr["NomeImagem"]) ?? ""
                    };
                }

                dr.Close();

                return produto;
            }
        }

        public IEnumerable<Produto> ObterTodosProdutos()
        {
            List<Produto> prodList = new List<Produto>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT p.ID_Produto, p.Nome, p.Preco, p.Descricao, " +
                    "p.Disponibilidade, p.ID_Categoria, c.Categoria, i.NomeImagem " +
                    "FROM tbProduto p " +
                    "INNER JOIN tbCategoria c ON p.ID_Categoria = c.ID_Categoria " +
                    "LEFT JOIN tbImagem i ON p.ID_Produto = i.ID_Produto " +
                    "ORDER BY p.ID_Produto",
                    conexao
                );

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();

                da.Fill(dt);

                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    prodList.Add(
                        new Produto
                        {
                            ID_Produto = Convert.ToInt32(dr["ID_Produto"]),
                            Nome = Convert.ToString(dr["Nome"]) ?? "",
                            Preco = Convert.ToDecimal(dr["Preco"]),
                            Descricao = Convert.ToString(dr["Descricao"]) ?? "",
                            Disponibilidade = Convert.ToBoolean(dr["Disponibilidade"]),
                            ID_Categoria = Convert.ToInt32(dr["ID_Categoria"]),
                            Categoria = Convert.ToString(dr["Categoria"]) ?? "",
                            NomeImagem = dr["NomeImagem"] == DBNull.Value ? "" : Convert.ToString(dr["NomeImagem"]) ?? ""
                        }
                    );
                }
            }

            return prodList;
        }
    }
}
