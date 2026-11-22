using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;
using static BicoTucano.Models.PedidoAdmin;

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

        public IEnumerable<Pedido> ObterPedidosDoCliente(int idUsuario)
        {
            List<Pedido> lista = new List<Pedido>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT NF, DataPedido, StatusPedido, ValorTotal " +
                    "FROM tbPedido WHERE ID_Usuario = @ID_Usuario ORDER BY NF DESC", conexao);
                cmd.Parameters.AddWithValue("@ID_Usuario", idUsuario);

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Pedido
                        {
                            NF = Convert.ToInt32(dr["NF"]),
                            DataPedido = Convert.ToDateTime(dr["DataPedido"]),
                            StatusPedido = TextoParaStatus(Convert.ToString(dr["StatusPedido"])),
                            ValorTotal = Convert.ToDecimal(dr["ValorTotal"])
                        });
                    }
                }
            }

            return lista;
        }

        public Dashboard ObterDashboard()
        {
            Dashboard dash = new Dashboard();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                // Cards
                dash.PedidosEmAndamento = Convert.ToInt32(Escalar(conexao,
                    "SELECT COUNT(*) FROM tbPedido WHERE StatusPedido IN ('Em preparo','Em Entrega')"));

                dash.PedidosHoje = Convert.ToInt32(Escalar(conexao,
                    "SELECT COUNT(*) FROM tbPedido WHERE DATE(DataPedido) = CURDATE()"));

                dash.FaturamentoMes = Convert.ToDecimal(Escalar(conexao,
                    "SELECT COALESCE(SUM(ValorTotal),0) FROM tbPedido " +
                    "WHERE YEAR(DataPedido) = YEAR(CURDATE()) AND MONTH(DataPedido) = MONTH(CURDATE())"));

                dash.TotalClientes = Convert.ToInt32(Escalar(conexao, "SELECT COUNT(*) FROM tbCliente"));

                // Vendas dos últimos 7 dias (um dia sem pedido fica com 0)
                Dictionary<DateTime, decimal> vendas = new Dictionary<DateTime, decimal>();

                MySqlCommand cmdSemana = new MySqlCommand(
                    "SELECT DATE(DataPedido) AS Dia, SUM(ValorTotal) AS Total FROM tbPedido " +
                    "WHERE DATE(DataPedido) >= DATE_SUB(CURDATE(), INTERVAL 6 DAY) " +
                    "GROUP BY DATE(DataPedido)", conexao);

                using (MySqlDataReader dr = cmdSemana.ExecuteReader())
                {
                    while (dr.Read())
                        vendas[Convert.ToDateTime(dr["Dia"]).Date] = Convert.ToDecimal(dr["Total"]);
                }

                for (int i = 6; i >= 0; i--)
                {
                    DateTime dia = DateTime.Today.AddDays(-i);
                    decimal total = vendas.ContainsKey(dia) ? vendas[dia] : 0m;

                    dash.VendasSemana.Add(new VendaDia { Data = dia, Total = total });
                    dash.TotalSemana += total;
                }

                // Pratos mais vendidos
                MySqlCommand cmdTop = new MySqlCommand(
                    "SELECT p.Nome, i.NomeImagem, SUM(ip.Quantidade) AS Qtd, SUM(ip.Quantidade * ip.Preco) AS Total " +
                    "FROM tbItem_Pedido ip " +
                    "INNER JOIN tbProduto p ON ip.ID_Produto = p.ID_Produto " +
                    "LEFT JOIN tbImagem i ON p.ID_Produto = i.ID_Produto " +
                    "GROUP BY p.ID_Produto, p.Nome, i.NomeImagem " +
                    "ORDER BY Qtd DESC LIMIT 5", conexao);

                using (MySqlDataReader dr = cmdTop.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dash.MaisVendidos.Add(new PratoRanking
                        {
                            Nome = Convert.ToString(dr["Nome"]) ?? "",
                            NomeImagem = dr["NomeImagem"] == DBNull.Value ? "" : Convert.ToString(dr["NomeImagem"]) ?? "",
                            Quantidade = Convert.ToInt32(dr["Qtd"]),
                            Total = Convert.ToDecimal(dr["Total"])
                        });
                    }
                }

                // Últimos pedidos em andamento
                MySqlCommand cmdUlt = new MySqlCommand(
                    "SELECT p.NF, u.Nome, p.StatusPedido, p.ValorTotal " +
                    "FROM tbPedido p INNER JOIN tbUsuario u ON p.ID_Usuario = u.ID_Usuario " +
                    "WHERE p.StatusPedido IN ('Em preparo','Em Entrega') " +
                    "ORDER BY p.NF DESC LIMIT 5", conexao);

                using (MySqlDataReader dr = cmdUlt.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dash.UltimosPedidos.Add(new PedidoDashboard
                        {
                            NF = Convert.ToInt32(dr["NF"]),
                            Cliente = Convert.ToString(dr["Nome"]) ?? "",
                            StatusPedido = TextoParaStatus(Convert.ToString(dr["StatusPedido"])),
                            ValorTotal = Convert.ToDecimal(dr["ValorTotal"])
                        });
                    }
                }
            }

            return dash;
        }

        // Executa um SELECT que devolve um número só
        private static object Escalar(MySqlConnection conexao, string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, conexao);
            object? valor = cmd.ExecuteScalar();
            return valor == null || valor == DBNull.Value ? 0 : valor;
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

        // LISTAGEM: uma linha por pedido
        public IEnumerable<PedidoLista> ObterTodosPedidosAdmin()
        {
            List<PedidoLista> lista = new List<PedidoLista>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT p.NF, p.DataPedido, p.ValorTotal, p.StatusPedido, u.Nome, u.Email, u.CPF, " +
                    "GROUP_CONCAT(CONCAT(pr.Nome, ' x', ip.Quantidade) SEPARATOR '|') AS Produtos " +
                    "FROM tbPedido p " +
                    "INNER JOIN tbUsuario u ON p.ID_Usuario = u.ID_Usuario " +
                    "INNER JOIN tbItem_Pedido ip ON ip.NF = p.NF " +
                    "INNER JOIN tbProduto pr ON pr.ID_Produto = ip.ID_Produto " +
                    "GROUP BY p.NF, p.DataPedido, p.ValorTotal, p.StatusPedido, u.Nome, u.Email, u.CPF " +
                    "ORDER BY p.NF DESC",
                    conexao);

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new PedidoLista
                        {
                            NF = Convert.ToInt32(dr["NF"]),
                            DataPedido = Convert.ToDateTime(dr["DataPedido"]),
                            ValorTotal = Convert.ToDecimal(dr["ValorTotal"]),
                            StatusPedido = TextoParaStatus(Convert.ToString(dr["StatusPedido"])),
                            Cliente = Convert.ToString(dr["Nome"]) ?? "",
                            Email = Convert.ToString(dr["Email"]) ?? "",
                            CPF = Convert.ToString(dr["CPF"]) ?? "",
                            Produtos = Convert.ToString(dr["Produtos"]) ?? ""
                        });
                    }
                }
            }

            return lista;
        }


        // DETALHES: pedido + itens + cliente + endereço + forma de pagamento
        public PedidoDetalhe? ObterDetalhePedido(int nf)
        {
            Pedido? pedido = ObterPedidos(nf);

            if (pedido == null)
                return null;

            PedidoDetalhe detalhe = new PedidoDetalhe();
            detalhe.Pedido = pedido;

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                // 1) Itens do pedido (nome do produto vem da tbProduto)
                MySqlCommand cmdItens = new MySqlCommand(
                    "SELECT pr.Nome, ip.Quantidade, ip.Preco " +
                    "FROM tbItem_Pedido ip " +
                    "INNER JOIN tbProduto pr ON ip.ID_Produto = pr.ID_Produto " +
                    "WHERE ip.NF = @NF", conexao);
                cmdItens.Parameters.AddWithValue("@NF", nf);

                using (MySqlDataReader dr = cmdItens.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        detalhe.Itens.Add(new PedidoItemDetalhe
                        {
                            Produto = Convert.ToString(dr["Nome"]) ?? "",
                            Quantidade = Convert.ToInt32(dr["Quantidade"]),
                            Preco = Convert.ToDecimal(dr["Preco"])
                        });
                    }
                }

                // 2) Dados do cliente
                MySqlCommand cmdCliente = new MySqlCommand(
                    "SELECT Nome, Email, Telefone, CPF FROM tbUsuario WHERE ID_Usuario = @Id", conexao);
                cmdCliente.Parameters.AddWithValue("@Id", pedido.ID_Usuario);

                using (MySqlDataReader dr = cmdCliente.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        detalhe.NomeCliente = Convert.ToString(dr["Nome"]) ?? "";
                        detalhe.Email = Convert.ToString(dr["Email"]) ?? "";
                        detalhe.Telefone = Convert.ToString(dr["Telefone"]) ?? "";
                        detalhe.CPF = Convert.ToString(dr["CPF"]) ?? "";
                    }
                }

                // 3) Endereço (só existe quando o pedido tem CEP, ou seja, entrega)
                if (!string.IsNullOrEmpty(pedido.CEP))
                {
                    MySqlCommand cmdEnd = new MySqlCommand(
                        "SELECT e.CEP, e.Logradouro, b.Bairro, c.Cidade, es.UF " +
                        "FROM tbEndereco e " +
                        "LEFT JOIN tbBairro b ON e.Bairro = b.ID_Bairro " +
                        "LEFT JOIN tbCidade c ON e.Cidade = c.ID_Cidade " +
                        "LEFT JOIN tbEstado es ON e.Estado = es.ID_Uf " +
                        "WHERE e.CEP = @CEP", conexao);
                    cmdEnd.Parameters.AddWithValue("@CEP", pedido.CEP);

                    using (MySqlDataReader dr = cmdEnd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            detalhe.CEP = Convert.ToString(dr["CEP"]) ?? "";
                            detalhe.Logradouro = Convert.ToString(dr["Logradouro"]) ?? "";
                            detalhe.Bairro = Convert.ToString(dr["Bairro"]) ?? "";
                            detalhe.CidadeUF = Convert.ToString(dr["Cidade"]) + " - " + Convert.ToString(dr["UF"]);
                        }
                    }
                }

                // 4) Forma de pagamento (último pagamento do pedido)
                MySqlCommand cmdPag = new MySqlCommand(
                    "SELECT FormaPagamento FROM tbPagamento WHERE NF = @NF ORDER BY ID_Pagamento DESC LIMIT 1",
                    conexao);
                cmdPag.Parameters.AddWithValue("@NF", nf);

                object? forma = cmdPag.ExecuteScalar();
                if (forma != null && forma != DBNull.Value)
                    detalhe.MetodoPagamento = Convert.ToString(forma) ?? "Não informado";
            }

            return detalhe;
        }


        // ALTERAR STATUS
        public void AtualizarStatus(int nf, StatusPedido status)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE tbPedido SET StatusPedido = @Status WHERE NF = @NF", conexao);

                cmd.Parameters.AddWithValue("@Status", StatusParaTexto(status));
                cmd.Parameters.AddWithValue("@NF", nf);

                cmd.ExecuteNonQuery();
            }
        }




      
    }
}
