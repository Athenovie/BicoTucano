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
        public void Cadastrar(Produto produto)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                // 1) Insere o produto 
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO tbProduto (Nome, Preco, Descricao, Disponibilidade, ID_Categoria)" +
                    " VALUES (@Nome, @Preco, @Descricao, @Disponibilidade, @ID_Categoria)",
                    conexao);

                cmd.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = produto.Nome;
                cmd.Parameters.Add("@Preco", MySqlDbType.Decimal).Value = produto.Preco;
                cmd.Parameters.Add("@Descricao", MySqlDbType.VarChar).Value = produto.Descricao;
                cmd.Parameters.Add("@Disponibilidade", MySqlDbType.Bit).Value = produto.Disponibilidade;
                cmd.Parameters.Add("@ID_Categoria", MySqlDbType.Int32).Value = produto.ID_Categoria;

                cmd.ExecuteNonQuery();

                // 2) Pega o ID que o banco acabou de gerar
                int idNovo = Convert.ToInt32(cmd.LastInsertedId);

                // 3) Insere a imagem ligada a esse produto
                MySqlCommand cmdImg = new MySqlCommand(
                    "INSERT INTO tbImagem (ID_Produto, NomeImagem) VALUES (@ID_Produto, @NomeImagem)",
                    conexao);

                cmdImg.Parameters.Add("@ID_Produto", MySqlDbType.Int32).Value = idNovo;
                cmdImg.Parameters.Add("@NomeImagem", MySqlDbType.VarChar).Value = produto.NomeImagem;

                cmdImg.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public void Atualizar(Produto produto)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                // 1) Atualiza os dados do produto
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE tbProduto SET Nome = @Nome, Preco = @Preco, Descricao = @Descricao, " +
                    "Disponibilidade = @Disponibilidade, ID_Categoria = @ID_Categoria " +
                    "WHERE ID_Produto = @ID_Produto",
                    conexao);

                cmd.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = produto.Nome;
                cmd.Parameters.Add("@Preco", MySqlDbType.Decimal).Value = produto.Preco;
                cmd.Parameters.Add("@Descricao", MySqlDbType.VarChar).Value = produto.Descricao;
                cmd.Parameters.Add("@Disponibilidade", MySqlDbType.Bit).Value = produto.Disponibilidade;
                cmd.Parameters.Add("@ID_Categoria", MySqlDbType.Int32).Value = produto.ID_Categoria;
                cmd.Parameters.Add("@ID_Produto", MySqlDbType.Int32).Value = produto.ID_Produto;

                cmd.ExecuteNonQuery();

                // 2) Troca a imagem: apaga a antiga e insere a nova
                MySqlCommand cmdDel = new MySqlCommand(
                    "DELETE FROM tbImagem WHERE ID_Produto = @ID_Produto", conexao);
                cmdDel.Parameters.Add("@ID_Produto", MySqlDbType.Int32).Value = produto.ID_Produto;
                cmdDel.ExecuteNonQuery();

                MySqlCommand cmdImg = new MySqlCommand(
                    "INSERT INTO tbImagem (ID_Produto, NomeImagem) VALUES (@ID_Produto, @NomeImagem)",
                    conexao);
                cmdImg.Parameters.Add("@ID_Produto", MySqlDbType.Int32).Value = produto.ID_Produto;
                cmdImg.Parameters.Add("@NomeImagem", MySqlDbType.VarChar).Value = produto.NomeImagem;
                cmdImg.ExecuteNonQuery();

                conexao.Close();
            }
        }


        public Produto? ObterProduto(int Id)
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


    

        // Usado pelos botões DESATIVAR / ATIVAR da listagem
        public void AlterarDisponibilidade(int id, bool disponibilidade)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE tbProduto SET Disponibilidade = @Disponibilidade WHERE ID_Produto = @Id",
                    conexao);

                cmd.Parameters.Add("@Disponibilidade", MySqlDbType.Bit).Value = disponibilidade;
                cmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

                cmd.ExecuteNonQuery();
                conexao.Close();
            }
        }

        public IEnumerable<Produto> ObterProdutosAleatorios(int idProduto)
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
                    "WHERE p.ID_Produto <> @Id " +
                    "ORDER BY RAND() " +
                    "LIMIT 3",
                    conexao
                );

                cmd.Parameters.AddWithValue("@Id", idProduto);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

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
                            NomeImagem = dr["NomeImagem"] == DBNull.Value
                                ? ""
                                : Convert.ToString(dr["NomeImagem"]) ?? ""
                        }
                    );
                }

                conexao.Close();
            }

            return prodList;
        }
    }
}
