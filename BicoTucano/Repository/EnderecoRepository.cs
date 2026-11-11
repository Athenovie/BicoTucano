using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using MySql.Data.MySqlClient;
using System.Transactions;

namespace BicoTucano.Repository
{
    public class EnderecoRepository : IEnderecoRepository
    {

        private readonly string _conexaoMySQL;

        public EnderecoRepository(IConfiguration conf)
        {
            _conexaoMySQL = conf.GetConnectionString("ConexaoMySQL");
        }

        private const string SelectBase = "select ue.ID_UsuarioEndereco, ue.ID_Usuario, ue.CEP, ue.NomeEndereco, ue.Numero, ue.Complemento, " +
         "e.Logradouro, b.Bairro, c.Cidade, uf.UF " +
         "from tbUsuarioEndereco ue " +
         "inner join tbEndereco e on e.CEP = ue.CEP " +
         "inner join tbBairro b on b.ID_Bairro = e.Bairro " +
         "inner join tbCidade c on c.ID_Cidade = e.Cidade " + 
         "inner join tbEstado uf on uf.ID_Uf = e.Estado";   


        public void Atualizar(Endereco endereco)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    SalvarCep(conexao, transacao, endereco);

                    MySqlCommand cmd = new MySqlCommand(
                         "update tbUsuarioEndereco set CEP=@CEP, NomeEndereco=@NomeEndereco, " +
                         "Numero=@Numero, Complemento=@Complemento " +                          
                         "where ID_UsuarioEndereco=@Id and ID_Usuario=@ID_Usuario",
                         conexao, transacao);

                    cmd.Parameters.AddWithValue("@Id", endereco.ID_UsuarioEndereco);
                    cmd.Parameters.AddWithValue("@ID_Usuario", endereco.ID_Usuario);
                    cmd.Parameters.AddWithValue("@CEP", endereco.CEP);
                    cmd.Parameters.AddWithValue("@NomeEndereco", endereco.NomeEndereco);
                    cmd.Parameters.AddWithValue("@Numero", endereco.Numero);
                    cmd.Parameters.AddWithValue("@Complemento", (object?)endereco.Complemento ?? DBNull.Value);

                    cmd.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }


        public void Cadastrar(Endereco endereco)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                using (var transacao = conexao.BeginTransaction())
                {
                    SalvarCep(conexao, transacao, endereco);

                    MySqlCommand cmd = new MySqlCommand(
                        "insert into tbUsuarioEndereco(ID_Usuario, CEP, NomeEndereco, Numero, Complemento)" +
                        "values (@ID_Usuario,@CEP,@NomeEndereco, @Numero, @Complemento)",
                        conexao, transacao);

                    cmd.Parameters.AddWithValue("@Id", endereco.ID_UsuarioEndereco);
                    cmd.Parameters.AddWithValue("@ID_Usuario", endereco.ID_Usuario);
                    cmd.Parameters.AddWithValue("@CEP", endereco.CEP);
                    cmd.Parameters.AddWithValue("@NomeEndereco", endereco.NomeEndereco);
                    cmd.Parameters.AddWithValue("@Numero", endereco.Numero);
                    cmd.Parameters.AddWithValue("@Complemento", (object?)endereco.Complemento ?? DBNull.Value);
                    cmd.ExecuteNonQuery();

                    transacao.Commit();
                }
            }
        }

    

        public void Excluir(int Id)
        {
            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "delete from tbUsuarioEndereco where ID_UsuarioEndereco=@Id", conexao);
                cmd.Parameters.AddWithValue("@Id", Id);
                cmd.ExecuteNonQuery();
            }
        }

        public Endereco ObterEndereco(int Id)
        {
            return Consultar(SelectBase + " where ue.ID_UsuarioEndereco=@Valor", Id).FirstOrDefault()
                ?? new Endereco();
        }

        public IEnumerable<Endereco> ObterEnderecoDoUsuario(int idUsuario)
        {
            return Consultar(SelectBase + " where ue.ID_Usuario=@Valor order by ue.ID_UsuarioEndereco", idUsuario);

        }

        public IEnumerable<Endereco> ObterTodosEnderecos()
        {
            return Consultar(SelectBase + " order by ue.ID_Usuario", null);

        }
        private List<Endereco> Consultar(string sql, int? valor)
        {
            var lista = new List<Endereco>();

            using (var conexao = new MySqlConnection(_conexaoMySQL))
            {
                conexao.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                if (valor.HasValue)
                    cmd.Parameters.AddWithValue("@Valor", valor.Value);

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Endereco
                        {
                            ID_UsuarioEndereco = Convert.ToInt32(dr["ID_UsuarioEndereco"]),
                            ID_Usuario = Convert.ToInt32(dr["ID_Usuario"]),
                            CEP = Convert.ToString(dr["CEP"]),
                            NomeEndereco = Convert.ToString(dr["NomeEndereco"]),
                            Numero = Convert.ToString(dr["Numero"]),
                            Complemento = Convert.ToString(dr["Complemento"]),
                            Logradouro = Convert.ToString(dr["Logradouro"]),
                            Bairro = Convert.ToString(dr["Bairro"]),
                            Cidade = Convert.ToString(dr["Cidade"]),
                            Estado = Convert.ToString(dr["UF"])
                        });
                    }
                }
            }
            return lista;
        }

        private void SalvarCep(MySqlConnection conexao, MySqlTransaction transacao, Endereco endereco)
        {
            // 1. Converte TODOS os textos em IDs numéricos (incluindo o Estado agora!)
            int idEstado = ObterOuCriarId(conexao, transacao, "tbEstado", "ID_Uf", "UF", endereco.Estado.ToUpper());
            int idCidade = ObterOuCriarId(conexao, transacao, "tbCidade", "ID_Cidade", "Cidade", endereco.Cidade);
            int idBairro = ObterOuCriarId(conexao, transacao, "tbBairro", "ID_Bairro", "Bairro", endereco.Bairro);

            MySqlCommand cmd = new MySqlCommand(
                "insert into tbEndereco(CEP, Bairro, Cidade, Estado, Logradouro) " +
                "values (@CEP, @Bairro, @Cidade, @Estado, @Logradouro) " +
                "on duplicate key update Bairro=@Bairro, Cidade=@Cidade, Estado=@Estado, Logradouro=@Logradouro",
                conexao, transacao);

            // 2. Passe as variáveis numéricas corretas para todos os parâmetros de IDs
            cmd.Parameters.AddWithValue("@CEP", endereco.CEP);
            cmd.Parameters.AddWithValue("@Bairro", idBairro);      // Passa o ID (int)
            cmd.Parameters.AddWithValue("@Cidade", idCidade);      // Passa o ID (int)
            cmd.Parameters.AddWithValue("@Estado", idEstado);      // Passa o ID (int) convertido de 'SP'
            cmd.Parameters.AddWithValue("@Logradouro", endereco.Logradouro.Trim());

            cmd.ExecuteNonQuery();
        }

        private int ObterOuCriarId(MySqlConnection conexao, MySqlTransaction transacao,
                                          string tabela, string colunaId, string colunaTexto, string valor)
        {
            MySqlCommand busca = new MySqlCommand(
                $"select {colunaId} from {tabela} where {colunaTexto}=@Valor", conexao, transacao);
            busca.Parameters.AddWithValue("@Valor", valor);

            object? achou = busca.ExecuteScalar();
            if (achou != null)
                return Convert.ToInt32(achou);

            MySqlCommand insere = new MySqlCommand(
                $"insert into {tabela}({colunaTexto}) values (@Valor)", conexao, transacao);
            insere.Parameters.AddWithValue("@Valor", valor);
            insere.ExecuteNonQuery();

            return (int)insere.LastInsertedId;
        }

        
    }
}
