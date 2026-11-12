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

        public void Cadastrar(Item_Pedido item)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO tbItem_Pedido VALUES (@ID_Item, @ID_Produto, @Nome, @Quantidade, @Preco, @Imagem)",
                    conexao
                );

                cmd.Parameters.Add("@ID_Item", MySqlDbType.Guid).Value = item.ID_Item;
                cmd.Parameters.Add("@ID_Produto", MySqlDbType.Int32).Value = item.ID_Produto;
                cmd.Parameters.Add("@Nome", MySqlDbType.VarChar).Value = item.Nome;
                cmd.Parameters.Add("@Quantidade", MySqlDbType.Decimal).Value = item.Quantidade;
                cmd.Parameters.Add("@Preco", MySqlDbType.Decimal).Value = item.Preco;
                cmd.Parameters.Add("@Imagem", MySqlDbType.VarChar).Value = item.Imagem;

                cmd.ExecuteNonQuery();
                conexao.Close();
            }
        }

        public void Atualizar(Item_Pedido itens)
        {
            throw new NotImplementedException();
        }

        public void Excluir(int id)
        {
            throw new NotImplementedException();
        }

        public Item_Pedido ObterItens(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Item_Pedido> ObterTodosItens()
        {
            throw new NotImplementedException();
        }
    }
}
