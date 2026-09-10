using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BicoTucano.Controllers
{
    public class ClienteController : Controller
    {
        // Injeção de dependência
        private IClienteRepository _clienteRepository;
        private LoginCliente _loginCliente;

        public ClienteController(
            IClienteRepository clienteRepository,
            LoginCliente loginCliente)
        {
            _clienteRepository = clienteRepository;
            _loginCliente = loginCliente;
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

                return new RedirectResult(
                    Url.Action(nameof(PainelCliente))
                );
            }
            else
            {
                // Erro na sessão
                ViewData["MSG_E"] =
                    "Usuário não localizado, por favor verifique e-mail e senha digitado";

                return View();
            }
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Cadastrar([FromForm] Cliente cliente)
        {

            _clienteRepository.Cadastrar(cliente);

            return RedirectToAction(nameof(Login));
        }


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
            return RedirectToAction("Index", "Home");
        }
        

        public IActionResult Index()
        {
            return View();
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
