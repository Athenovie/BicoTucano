using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;


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

        [FuncionarioAutorizacao(FuncionarioTipoConstant.Gerente)]
        public IActionResult Cadastrar()
        {
            return View();
        }
        [FuncionarioAutorizacao(FuncionarioTipoConstant.Gerente)]
        [HttpPost]
        public IActionResult Cadastrar([FromForm] Models.Funcionario funcionario)
        {
           if (funcionario.CPF != null)
                funcionario.CPF= new string(funcionario.CPF.Where(char.IsDigit).ToArray());

            if (!ModelState.IsValid)
                return View(funcionario);

            funcionario.DataAdmissao = DateTime.Today;

            try
            {
                _repositoryFuncionario.Cadastrar(funcionario);
                ViewData["MSG_S"] = "Cadastro realizado com sucesso!";
                return View();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ViewData["MSG_E"] = "E=mail ou CPF já cadastrado";
                return View(funcionario);
            }
            
            return RedirectToAction(nameof(Cadastrar));


        }
        [FuncionarioAutorizacao(FuncionarioTipoConstant.Gerente)]
        public IActionResult Index()
        {
            return View(_repositoryFuncionario.ObterTodosFuncionarios());
        }



        [FuncionarioAutorizacao]
        public IActionResult Painel()
        {

            Models.Funcionario funcionario = _loginFuncionario.GetFuncionario();
            ViewBag.Nome= funcionario.Nome;
            ViewBag.Email = funcionario.Email;
            ViewBag.NivelAcesso = funcionario.NivelAcesso;
            
            if(funcionario.NivelAcesso == NivelAcesso.Administrador)
                return View("PainelAdministrador");

            return View("PainelComum");
        }
      
        public IActionResult Logout()
        {
            _loginFuncionario.logout();
            return RedirectToAction("Login", "Home");
        }
    }
}
