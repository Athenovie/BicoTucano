using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Identity;
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

            if (funcionarioDB != null && !string.IsNullOrEmpty(funcionarioDB.Senha))
            {
                // funcionário desativado (com data de demissão) não entra
                if (funcionarioDB.DataDemissao.Year > 1)
                {
                    ViewData["MSG_E"] = "Este funcionário está desativado. Procure o gerente.";
                    return View();
                }
                // Verificando a senha digitada com o hash do banco
                var hasher = new PasswordHasher<Models.Funcionario>();

                var resultado = hasher.VerifyHashedPassword(
                    funcionarioDB,
                    funcionarioDB.Senha,
                    funcionario.Senha
                );

                if (resultado != PasswordVerificationResult.Failed)
                {
                    _loginFuncionario.Login(funcionarioDB);

                    return RedirectToAction(nameof(Painel));
                }
            }

            ViewData["MSG_E"] =
                "Usuário não localizado, por favor verifique e-mail e senha digitado";

            return View();
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

                TempData["MSG_S"] = "Cadastro realizado com sucesso!";
                return RedirectToAction(nameof(Index));

            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ViewData["MSG_E"] = "E=mail ou CPF já cadastrado";

                return View(funcionario);
            }


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


        [FuncionarioAutorizacao(FuncionarioTipoConstant.Gerente)]
        [HttpPost]
        public IActionResult Ativar(int id)
        {
            _repositoryFuncionario.Ativar(id);
            return RedirectToAction(nameof(Index));
        }

        [FuncionarioAutorizacao(FuncionarioTipoConstant.Gerente)]
        [HttpPost]
        public IActionResult Desativar(int id)
        {
            // o gerente não pode desativar a si mesmo
            if (id != _loginFuncionario.GetFuncionario().ID_Usuario)
                _repositoryFuncionario.Desativar(id);

            return RedirectToAction(nameof(Index));
        }

        [FuncionarioAutorizacao]
        public IActionResult Perfil()
        {
            Models.Funcionario logado = _loginFuncionario.GetFuncionario();
            return View(_repositoryFuncionario.ObterFuncionario(logado.ID_Usuario));
        }

        [FuncionarioAutorizacao]
        public IActionResult EditarPerfil()
        {
            Models.Funcionario logado = _loginFuncionario.GetFuncionario();
            return View(_repositoryFuncionario.ObterFuncionario(logado.ID_Usuario));
        }

        [FuncionarioAutorizacao]
        [HttpPost]
        public IActionResult EditarPerfil([FromForm] Models.Funcionario funcionario)
        {
            Models.Funcionario logado = _loginFuncionario.GetFuncionario();
            funcionario.ID_Usuario = logado.ID_Usuario;

            ModelState.Remove(nameof(funcionario.Senha));
            ModelState.Remove(nameof(funcionario.NivelAcesso));
            ModelState.Remove(nameof(funcionario.Cargo));
            ModelState.Remove(nameof(funcionario.DataAdmissao));

            if (funcionario.CPF != null)
                funcionario.CPF = new string(funcionario.CPF.Where(char.IsDigit).ToArray());

            if (!ModelState.IsValid)
                return View(funcionario);

            try
            {
                _repositoryFuncionario.Atualizar(funcionario);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ViewData["MSG_E"] = "CPF ou e-mail já pertence a outro cadastro.";
                return View(funcionario);
            }

            TempData["MSG_S"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Perfil));
        }

        public IActionResult Logout()
        {
            _loginFuncionario.logout();
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        [HttpGet]
        public IActionResult RedefinirSenha()
        {
            return View(new Models.Funcionario());
        }

        [HttpPost]
        public IActionResult RedefinirSenha([FromForm] Models.Funcionario funcionario, string confirmarSenha)
        {
            if (funcionario.Senha != confirmarSenha)
                ModelState.AddModelError("confirmarSenha", "As senhas não coincidem");

            bool invalido = ModelState["Email"]?.Errors.Count > 0
                         || ModelState["Senha"]?.Errors.Count > 0
                         || ModelState["confirmarSenha"]?.Errors.Count > 0;

            if (invalido)
                return View(funcionario);

            if (!_repositoryFuncionario.AtualizarSenha(funcionario.Email, funcionario.Senha))
            {
                ViewData["MSG_E"] = "E-mail não encontrado, por favor verifique o e-mail digitado";
                return View(funcionario);
            }

            TempData["MSG_S"] = "Senha alterada com sucesso! Entre com a nova senha.";
            return RedirectToAction(nameof(Login));
        }
    }
}