using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
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

                return new RedirectResult(Url.Action(nameof(Painel)));
            }
            else
            {
                ViewData["MSG_E"] = "Usuário não encontrado, verifique o e-mail e senha digitado!";
                return View();
            }
        }


        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar([FromForm] Models.Funcionario funcionario)
        {
           
            _repositoryFuncionario.Cadastrar(funcionario);
            return RedirectToAction(nameof(Cadastrar));


        }

        public IActionResult Index()
        {
            return View(_repositoryFuncionario.ObterTodosFuncionarios());
        }
       
     


        public IActionResult Painel()
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
