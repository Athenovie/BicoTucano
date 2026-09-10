using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Repository;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;

namespace BicoTucano.Areas.Funcionario.Controllers
{
    [Area("Funcionario")]
    public class HomeController : Controller
    {
        private IFuncionarioRepository _repositoryFuncionario;
        private LoginFuncionario _loginFuncionario;
        public HomeController(IFuncionarioRepository repositoryFuncionario, LoginFuncionario loginFuncionario)
        {
            _repositoryFuncionario = repositoryFuncionario;
            _loginFuncionario = loginFuncionario;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login([FromForm] Models.Funcionario funcionario)
        {
            Models.Funcionario funcionarioDB = _repositoryFuncionario.Login(funcionario.Email, funcionario.Senha);

            if (funcionarioDB.Email != null && funcionarioDB.Senha != null)
            {
                _loginFuncionario.Login(funcionarioDB);

                if (funcionarioDB.NivelAcesso == Models.NivelAcesso.Administrador)
                {
                    return new RedirectResult(Url.Action(nameof(PainelAdministrador)));
                }
                else
                {
                    return new RedirectResult(Url.Action(nameof(PainelComum)));
                }
            }
            else
            {
                ViewData["MSG_E"] = "Usuário não encontrado, verifique o e-mail e senha digitado!";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Cadastrar([FromForm] Models.Funcionario funcionario)
        {

            _repositoryFuncionario.Cadastrar(funcionario);

            return RedirectToAction(nameof(Login));
        }

        public IActionResult PainelAdministrador()
        {
            var funcionario = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionario.Nome;
            ViewBag.NivelAcesso = funcionario.NivelAcesso;
            ViewBag.Email = funcionario.Email;
            return View();
        }

        public IActionResult PainelComum()
        {
            var funcionario = _loginFuncionario.GetFuncionario();
            ViewBag.Nome = funcionario.Nome;
            ViewBag.NivelAcesso = funcionario.NivelAcesso;
            ViewBag.Email = funcionario.Email;
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            _loginFuncionario.logout();
            return RedirectToAction("Login", "Home");
        }
    }
}
