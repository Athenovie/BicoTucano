using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;

namespace BicoTucano.Repository
{
    public class ItemRepository : IItemRepository
    {
        private readonly string _conexaoMySQL;

        public ItemRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
        }

        public IEnumerable<Item_Pedido> ObterTodosItens(int nf)
        {
            var lista = new List<Item_Pedido>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT ID_Item, NF, ID_Produto, Quantidade, Preco " +
                    "FROM tbItem_Pedido WHERE NF=@NF ORDER BY ID_Item", conexao);
                cmd.Parameters.AddWithValue("@NF", nf);

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Item_Pedido
                        {
                            ID_Item = Convert.ToInt32(dr["ID_Item"]),
                            NF = Convert.ToInt32(dr["NF"]),
                            ID_Produto = Convert.ToInt32(dr["ID_Produto"]),
                            Quantidade = Convert.ToInt32(dr["Quantidade"]),
                            Preco = Convert.ToDecimal(dr["Preco"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}