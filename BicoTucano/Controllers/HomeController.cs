using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace BicoTucano.Controllers
{
    public class HomeController : Controller
    {

        private IClienteRepository _clienteRepository;
        private LoginCliente _loginCliente;
        private IProdutoRepository _produtoRepository;
        private IEnderecoRepository _enderecoRepository;

        public HomeController(
            IClienteRepository clienteRepository,
            LoginCliente loginCliente,
            IProdutoRepository produtoRepository,
            IEnderecoRepository enderecoRepository)
        {
            _clienteRepository = clienteRepository;
            _loginCliente = loginCliente;
            _produtoRepository = produtoRepository;
            _enderecoRepository = enderecoRepository;
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar([FromForm] Cliente cliente)
        {
            // Estes campos não vêm do formulário
            ModelState.Remove(nameof(Cliente.Situacao));
            ModelState.Remove(nameof(Cliente.DataCadastro));

            if (cliente.CPF != null)
                cliente.CPF = new string(cliente.CPF.Where(char.IsDigit).ToArray());

            if (cliente.CPF != null && cliente.CPF.Length != 11)
                ModelState.AddModelError(nameof(Cliente.CPF), "Informe um CPF com 11 números");

            if (!ModelState.IsValid)
                return View(cliente);          // devolve o model para não apagar o que foi digitado

            if (!string.IsNullOrWhiteSpace(_clienteRepository.BuscaCpfCliente(cliente.CPF).CPF))
            {
                ViewData["MSG_CPF"] = "CPF já cadastrado, por favor verifique os dados digitados";
                return View(cliente);
            }
            if (!string.IsNullOrWhiteSpace(_clienteRepository.BuscaEmailCliente(cliente.Email).Email))
            {
                ViewData["MSG_Email"] = "E-mail já cadastrado, por favor verifique os dados digitados";
                return View(cliente);
            }

            cliente.Situacao = SituacaoConstant.Ativo;
            _clienteRepository.Cadastrar(cliente);
            return RedirectToAction("Login");
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login([FromForm] Cliente cliente)
        {
            Cliente clienteDB = _clienteRepository.Login(cliente.Email, cliente.Senha);

            if (clienteDB != null && !string.IsNullOrEmpty(clienteDB.Senha))
            {
                // cliente desativado não entra
                if (clienteDB.Situacao == SituacaoConstant.Desativado)
                {
                    ViewData["MSG_E"] = "Sua conta está desativada. Entre em contato conosco.";
                    return View();
                }

                // Verificando a senha digitada com o hash do banco
                var hasher = new PasswordHasher<Cliente>();

                var resultado = hasher.VerifyHashedPassword(
                    clienteDB,
                    clienteDB.Senha,
                    cliente.Senha
                );

                if (resultado != PasswordVerificationResult.Failed)
                {
                    _loginCliente.Login(clienteDB);

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["MSG_E"] =
                "Usuário não localizado, por favor verifique e-mail e senha digitado";

            return View();
        }


        [ClienteAutorizacao]
        public IActionResult Perfil()
        {
            Cliente logado = _loginCliente.GetCliente();
            Cliente cliente = _clienteRepository.ObterCliente(logado.ID_Usuario);

            ViewBag.Enderecos = _enderecoRepository.ObterEnderecoDoUsuario(logado.ID_Usuario).ToList();
            return View(cliente);
        }

        [ClienteAutorizacao]
        public IActionResult EditarPerfil()
        {
            Cliente logado = _loginCliente.GetCliente();
            return View(_clienteRepository.ObterCliente(logado.ID_Usuario));
        }

        [ClienteAutorizacao]
        [HttpPost]
        public IActionResult EditarPerfil([FromForm] Cliente cliente)
        {
            Cliente logado = _loginCliente.GetCliente();
            cliente.ID_Usuario = logado.ID_Usuario;

            ModelState.Remove(nameof(cliente.Senha));
            ModelState.Remove(nameof(cliente.Situacao));
            ModelState.Remove(nameof(cliente.DataCadastro));

            if (cliente.CPF != null)
                cliente.CPF = new string(cliente.CPF.Where(char.IsDigit).ToArray());

            if (!ModelState.IsValid)
                return View(cliente);

            try
            {
                _clienteRepository.Atualizar(cliente);
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1062)
            {
                ViewData["MSG_E"] = "CPF ou e-mail já pertence a outro cadastro.";
                return View(cliente);
            }

            TempData["MSG_S"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Perfil));
        }

        public IActionResult LogoutCliente()
        {
            _loginCliente.Logout();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            List<Produto> produtos = _produtoRepository.ObterTodosProdutos().ToList();

            return View(produtos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult RedefinirSenha()
        {
            return View(new Cliente()); 
        }

        [HttpPost]
        public IActionResult RedefinirSenha([FromForm] Cliente cliente, string confirmarSenha)
        {
            if (cliente.Senha != confirmarSenha)
                ModelState.AddModelError("confirmarSenha", "As senhas não coincidem");

            bool invalido = ModelState["Email"]?.Errors.Count > 0
                         || ModelState["Senha"]?.Errors.Count > 0
                         || ModelState["confirmarSenha"]?.Errors.Count > 0;

            if (invalido)
                return View(cliente);

            if (!_clienteRepository.AtualizarSenha(cliente.Email, cliente.Senha))
            {
                ViewData["MSG_E"] = "E-mail não encontrado, por favor verifique o e-mail digitado";
                return View(cliente);
            }

            TempData["MSG_S"] = "Senha alterada com sucesso! Entre com a nova senha.";
            return RedirectToAction(nameof(Login));
        }
    }

}
