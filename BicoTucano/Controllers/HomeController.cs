using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BicoTucano.Controllers
{
    public class HomeController : Controller
    {

        // Injeção de dependência
        private IClienteRepository _clienteRepository;
        private LoginCliente _loginCliente;
        private IProdutoRepository _produtoRepository;

        public HomeController(
            IClienteRepository clienteRepository,
            LoginCliente loginCliente,
            IProdutoRepository produtoRepository)
        {
            _clienteRepository = clienteRepository;
            _loginCliente = loginCliente;
            _produtoRepository = produtoRepository;
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

            if (clienteDB.Email != null && clienteDB.Senha != null)
            {
                _loginCliente.Login(clienteDB);

                return RedirectToAction(nameof(PainelCliente));
               
            }
            else
            {
                // Erro na sessão
                ViewData["MSG_E"] =
                    "Usuário não localizado, por favor verifique e-mail e senha digitado";

                return View();
            }
        }

        [ClienteAutorizacao]
        public IActionResult PainelCliente()
        {
            ViewBag.Nome = _loginCliente.GetCliente().Nome;
            ViewBag.CPF = _loginCliente.GetCliente().CPF;
            ViewBag.Email = _loginCliente.GetCliente().Email;
            return View();

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
    }

}
