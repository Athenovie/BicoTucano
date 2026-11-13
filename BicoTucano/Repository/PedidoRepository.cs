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

        public int Cadastrar(Pedido pedido, IEnumerable<Item_Pedido> itens)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                using (var transacao = conexao.BeginTransaction())
                {
                    MySqlCommand cmdPedido = new MySqlCommand(
                        "INSERT INTO tbPedido (DataPedido, ID_Usuario, TipoPedido, StatusPedido, Subtotal, " +
                        "TaxaEntrega, ValorTotal, CEP, DetalhesPedido, ID_Funcionario) " +
                        "VALUES (@DataPedido, @ID_Usuario, @TipoPedido, @StatusPedido, @Subtotal, " +
                        "@TaxaEntrega, @ValorTotal, @CEP, @DetalhesPedido, @ID_Funcionario)",
                        conexao, transacao);

                    cmdPedido.Parameters.AddWithValue("@DataPedido", pedido.DataPedido);
                    cmdPedido.Parameters.AddWithValue("@ID_Usuario", pedido.ID_Usuario);
                    cmdPedido.Parameters.AddWithValue("@TipoPedido", TipoParaTexto(pedido.TipoPedido));
                    cmdPedido.Parameters.AddWithValue("@StatusPedido", StatusParaTexto(pedido.StatusPedido));
                    cmdPedido.Parameters.AddWithValue("@Subtotal", pedido.Subtotal);
                    cmdPedido.Parameters.AddWithValue("@TaxaEntrega", pedido.TaxaEntrega);
                    cmdPedido.Parameters.AddWithValue("@ValorTotal", pedido.ValorTotal);
                    cmdPedido.Parameters.AddWithValue("@CEP", (object?)pedido.CEP ?? DBNull.Value);
                    cmdPedido.Parameters.AddWithValue("@DetalhesPedido", (object?)pedido.DetalhesPedido ?? DBNull.Value);
                    cmdPedido.Parameters.AddWithValue("@ID_Funcionario", (object?)pedido.ID_Funcionario ?? DBNull.Value);
                    cmdPedido.ExecuteNonQuery();

                    int nf = (int)cmdPedido.LastInsertedId;

                    foreach (Item_Pedido item in itens)
                    {
                        MySqlCommand cmdItem = new MySqlCommand(
                            "INSERT INTO tbItem_Pedido (NF, ID_Produto, Quantidade, Preco) " +
                            "VALUES (@NF, @ID_Produto, @Quantidade, @Preco)",
                            conexao, transacao);

                        cmdItem.Parameters.AddWithValue("@NF", nf);
                        cmdItem.Parameters.AddWithValue("@ID_Produto", item.ID_Produto);
                        cmdItem.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        cmdItem.Parameters.AddWithValue("@Preco", item.Preco);
                        cmdItem.ExecuteNonQuery();
                    }

                    transacao.Commit();
                    return nf;
                }
            }
        }

        public Pedido? ObterPedidos(int nf)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT NF, DataPedido, ID_Usuario, TipoPedido, StatusPedido, Subtotal, TaxaEntrega, " +
                    "ValorTotal, CEP, DetalhesPedido, ID_Funcionario FROM tbPedido WHERE NF=@NF", conexao);
                cmd.Parameters.AddWithValue("@NF", nf);

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        return null;

                    return new Pedido
                    {
                        NF = Convert.ToInt32(dr["NF"]),
                        DataPedido = Convert.ToDateTime(dr["DataPedido"]),
                        ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]),
                        TipoPedido = TextoParaTipo(Convert.ToString(dr["TipoPedido"])),
                        StatusPedido = TextoParaStatus(Convert.ToString(dr["StatusPedido"])),
                        Subtotal = Convert.ToDecimal(dr["Subtotal"]),
                        TaxaEntrega = Convert.ToDecimal(dr["TaxaEntrega"]),
                        ValorTotal = Convert.ToDecimal(dr["ValorTotal"]),
                        CEP = dr["CEP"] as string,
                        DetalhesPedido = dr["DetalhesPedido"] as string,
                        ID_Funcionario = dr["ID_Funcionario"] == DBNull.Value ? null : Convert.ToInt32(dr["ID_Funcionario"])
                    };
                }
            }
        }

        private static string TipoParaTexto(TipoPedido tipo)
        {
            return tipo == TipoPedido.Entrega ? "Entrega" : "Retirada";
        }

        private static string StatusParaTexto(StatusPedido status)
        {
            return status switch
            {
                StatusPedido.EmPreparo => "Em preparo",
                StatusPedido.EmEntrega => "Em Entrega",
                StatusPedido.Concluido => "Concluido",
                _ => throw new ArgumentException("Status do pedido inválido")
            };
        }

        private static TipoPedido TextoParaTipo(string? texto)
        {
            return texto == "Entrega" ? TipoPedido.Entrega : TipoPedido.Retirada;
        }
        private static StatusPedido TextoParaStatus(string? texto)
        {
            return texto switch
            {
                "Em Entrega" => StatusPedido.EmEntrega,
                "Concluido" => StatusPedido.Concluido,
                _ => StatusPedido.EmPreparo
            };
        }




    }
}
