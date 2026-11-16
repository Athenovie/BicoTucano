using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;

namespace BicoTucano.Repository
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly string _conexaoMySQL;

        public PagamentoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
        }

        public int Cadastrar(Pagamento pagamento)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO tbPagamento (NF, FormaPagamento, ChavePix, ID_Cartao, ValorPago, DataPagamento, StatusPagamento) " +
                    "VALUES (@NF, @FormaPagamento, @ChavePix, @ID_Cartao, @ValorPago, @DataPagamento, @StatusPagamento)",
                    conexao);

                PreencherParametros(cmd, pagamento);
                cmd.ExecuteNonQuery();

                return (int)cmd.LastInsertedId;
            }
        }

        public void Atualizar(Pagamento pagamento)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE tbPagamento SET NF=@NF, FormaPagamento=@FormaPagamento, ChavePix=@ChavePix, " +
                    "ID_Cartao=@ID_Cartao, ValorPago=@ValorPago, DataPagamento=@DataPagamento, " +
                    "StatusPagamento=@StatusPagamento WHERE ID_Pagamento=@ID_Pagamento",
                    conexao);

                PreencherParametros(cmd, pagamento);
                cmd.Parameters.AddWithValue("@ID_Pagamento", pagamento.ID_Pagamento);
                cmd.ExecuteNonQuery();
            }
        }

        public Pagamento? ObterPagamentoDoPedido(int nf)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT ID_Pagamento, NF, FormaPagamento, ChavePix, ID_Cartao, ValorPago, DataPagamento, StatusPagamento " +
                    "FROM tbPagamento WHERE NF=@NF ORDER BY ID_Pagamento DESC LIMIT 1",
                    conexao);
                cmd.Parameters.AddWithValue("@NF", nf);

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        return null;

                    return new Pagamento
                    {
                        ID_Pagamento = Convert.ToInt32(dr["ID_Pagamento"]),
                        NF = Convert.ToInt32(dr["NF"]),
                        FormaPagamento = TextoParaForma(Convert.ToString(dr["FormaPagamento"])),
                        ChavePix = dr["ChavePix"] as string,
                        ID_Cartao = dr["ID_Cartao"] == DBNull.Value ? null : Convert.ToInt32(dr["ID_Cartao"]),
                        ValorPago = dr["ValorPago"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["ValorPago"]),
                        DataPagamento = dr["DataPagamento"] == DBNull.Value ? null : Convert.ToDateTime(dr["DataPagamento"]),
                        StatusPagamento = Convert.ToString(dr["StatusPagamento"]) == "Pago"
                                          ? StatusPagamento.Pago
                                          : StatusPagamento.NaoPago
                    };
                }
            }
        }

        private static void PreencherParametros(MySqlCommand cmd, Pagamento pagamento)
        {
            cmd.Parameters.AddWithValue("@NF", pagamento.NF);
            cmd.Parameters.AddWithValue("@FormaPagamento", FormaParaTexto(pagamento.FormaPagamento));
            cmd.Parameters.AddWithValue("@ChavePix", (object?)pagamento.ChavePix ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ID_Cartao", (object?)pagamento.ID_Cartao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ValorPago", pagamento.ValorPago);
            cmd.Parameters.AddWithValue("@DataPagamento", (object?)pagamento.DataPagamento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusPagamento", pagamento.StatusPagamento == StatusPagamento.Pago ? "Pago" : "Não Pago");
        }


        private static string FormaParaTexto(FormaPagamento forma)
        {
            return forma switch
            {
                FormaPagamento.Pix => "Pix",
                FormaPagamento.CartaoDebito => "Cartão Débito",
                FormaPagamento.CartaoCredito => "Cartão Crédito",
                _ => throw new ArgumentException("Forma de pagamento inválida")
            };
        }

        private static FormaPagamento TextoParaForma(string? texto)
        {
            return texto switch
            {
                "Cartão Débito" => FormaPagamento.CartaoDebito,
                "Cartão Crédito" => FormaPagamento.CartaoCredito,
                _ => FormaPagamento.Pix
            };
        }
    }
}