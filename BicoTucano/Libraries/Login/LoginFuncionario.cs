using BicoTucano.Models;
using Newtonsoft.Json;

namespace BicoTucano.Libraries.Login
{
    public class LoginFuncionario
    {
        private string Key = "Login.Funcionario-";
        private Sessao.Sessao _sessao;

        public LoginFuncionario(Sessao.Sessao sesssao)
        {
            _sessao = sesssao;
        }

        public void Login(Funcionario funcionario)
        {
            string funcionarioJSONString = JsonConvert.SerializeObject(funcionario);
            _sessao.Cadastrar(Key, funcionarioJSONString);
        }

        public Funcionario GetFuncionario()
        {
            if (_sessao.Existe(Key))
            {
                string funcionarioJSONString = _sessao.Consultar(Key);
                return JsonConvert.DeserializeObject<Funcionario>(funcionarioJSONString);
            }
            else
            {
                return null;
            }
        }

        public void logout()
        {
            _sessao.RemoverTodos();
        }
    }
}
