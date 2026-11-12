using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;

namespace BicoTucano.Repository
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly string _conexaoMySQL;
        public PedidoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
        }
        public void Atualizar(Pedido pedido)
        {
            throw new NotImplementedException();
        }

        public void buscaIdPedido(Pedido pedido)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlDataReader dr;

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT NF FROM tbPedido ORDER BY NF DESC LIMIT 1",
                    conexao
                );

                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    pedido.NF = Convert.ToInt32(dr[0]);
                }

                conexao.Close();
            }
        }

        public void Cadastrar(Pedido pedido)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO tbPedido VALUES" +
                    " (default, @DataPedido, @ID_Usuario, @TipoPedido, @StatusPedido, @Subtotal, @TaxaEntrega, @ValorTotal, @CEP, @DetalhesPedido, @ID_Funcionario)",
                    conexao
                );

                cmd.Parameters.Add("@DataPedido", MySqlDbType.DateTime).Value = pedido.DataPedido;
                cmd.Parameters.Add("@ID_Usuario", MySqlDbType.Int32).Value = pedido.ID_Usuario;
                cmd.Parameters.Add("@TipoPedido", MySqlDbType.VarChar).Value = pedido.TipoPedido == TipoPedido.Entrega ? "Entrega" : "Retirada";
                cmd.Parameters.Add("@StatusPedido", MySqlDbType.VarChar).Value = pedido.StatusPedido switch
                {
                    StatusPedido.EmPreparo => "Em preparo",
                    StatusPedido.EmEntrega => "Em Entrega",
                    StatusPedido.Concluido => "Concluido",
                    _ => throw new ArgumentException("Status do pedido inválido")
                };
                cmd.Parameters.Add("@Subtotal", MySqlDbType.Decimal).Value = pedido.Subtotal;
                cmd.Parameters.Add("@TaxaEntrega", MySqlDbType.Decimal).Value = pedido.TaxaEntrega;
                cmd.Parameters.Add("@ValorTotal", MySqlDbType.Decimal).Value = pedido.ValorTotal;
                cmd.Parameters.Add("@CEP", MySqlDbType.VarChar).Value = (object?)pedido.CEP;
                cmd.Parameters.Add("@DetalhesPedido", MySqlDbType.VarChar).Value = (object?)pedido.DetalhesPedido;
                cmd.Parameters.Add("@ID_Funcionario", MySqlDbType.Int32).Value = (object?)pedido.ID_Funcionario;

                cmd.ExecuteNonQuery();

                conexao.Close();
            }
        }

        public void Excluir(int id)
        {
            throw new NotImplementedException();
        }

        public Pedido ObterPedidos(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Pedido> ObterTodosItens()
        {
            throw new NotImplementedException();
        }
    }
}
