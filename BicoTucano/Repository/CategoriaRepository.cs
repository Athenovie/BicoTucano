using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;
using System.Data;

namespace BicoTucano.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly string _conexaoMySQL;

        public CategoriaRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
        }
        public IEnumerable<Categoria> ObterTodasCategorias()
        {
            List<Categoria> lista = new List<Categoria>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT ID_Categoria, Categoria FROM tbCategoria ORDER BY ID_Categoria", conexao);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                conexao.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    lista.Add(new Categoria
                    {
                        ID_Categoria = Convert.ToInt32(dr["ID_Categoria"]),
                        NomeCategoria = Convert.ToString(dr["Categoria"]) ?? ""
                    });
                }
            }

            return lista;
        }
    }
}

