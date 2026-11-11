using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;

namespace BicoTucano.Controllers
{
    [ClienteAutorizacao]
    public class EnderecoController : Controller
    {
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly LoginCliente _loginCliente;

        public EnderecoController(IEnderecoRepository enderecoRepository, LoginCliente loginCliente)
        {
            _enderecoRepository = enderecoRepository;
            _loginCliente = loginCliente;
        }

        public IActionResult Index()
        {
            int idUsuario = _loginCliente.GetCliente().ID_Usuario;
            return View(_enderecoRepository.ObterEnderecoDoUsuario(idUsuario));
        }

        public IActionResult Cadastrar()
        {
            return View(new Endereco());
        }

        [HttpPost]
        public IActionResult Cadastrar([FromForm] Endereco endereco)
        {
            endereco.ID_Usuario = _loginCliente.GetCliente().ID_Usuario;
            PrepararCep(endereco);

            if (!ModelState.IsValid)
                return View(endereco);

            _enderecoRepository.Cadastrar(endereco);
            TempData["MSG_S"] = "Endereço Cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));

        }
        [HttpGet]
        public IActionResult Atualizar (int id)
        {
            Endereco? endereco = ObterDoCliente(id);
            if (endereco == null)
                return RedirectToAction(nameof(Index));

            return View(endereco);
        }

        [HttpPost]
        public IActionResult Atualizar(int id, [FromForm] Endereco endereco)
        {
            if (ObterDoCliente(id) == null)
                return RedirectToAction(nameof(Index));

            endereco.ID_UsuarioEndereco = id;
            endereco.ID_Usuario = _loginCliente.GetCliente().ID_Usuario;
            PrepararCep(endereco);

            if (!ModelState.IsValid)
                return View(endereco);

            _enderecoRepository.Atualizar(endereco);
            TempData["MSG_S"] = "Endereço Atualizado com sucesso.";
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public IActionResult Excluir(int id)
        {
            if(ObterDoCliente(id) != null)
            {
                _enderecoRepository.Excluir(id);
                TempData["MSG_S"] = "Endereço excluído.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Só devolve o endereço se ele existir e for do cliente logado
        private Endereco? ObterDoCliente(int id)
        {
            Endereco endereco = _enderecoRepository.ObterEndereco(id);

            bool EndCliente = endereco.ID_UsuarioEndereco != 0 &&
                endereco.ID_Usuario == _loginCliente.GetCliente().ID_Usuario;

            return EndCliente ? endereco : null;
        }

        private void PrepararCep(Endereco endereco)
        {
            string digitos = new string((endereco.CEP ?? "").Where(char.IsDigit).ToArray());

            if(digitos.Length != 8)
            {
                ModelState.AddModelError(nameof(Endereco.CEP), "Informe um CEP com 8 número.");
                return;
            }

            endereco.CEP = digitos.Insert(5,"-");
        }
    }
}
