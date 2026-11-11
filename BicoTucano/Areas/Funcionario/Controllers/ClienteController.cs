using BicoTucano.Libraries.Filtro;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace BicoTucano.Areas.Funcionario.Controllers
{
    [Area("Funcionario")]
    [FuncionarioAutorizacao]
    public class ClienteController : Controller
    {
        private IClienteRepository _clienteRepository;
        private IEnderecoRepository _enderecoRepository;

        public ClienteController(IClienteRepository clienteRepository, IEnderecoRepository enderecoRepository)
        {
            _clienteRepository = clienteRepository;
            _enderecoRepository = enderecoRepository;
        }

        public IActionResult Index()
        {
            return View(_clienteRepository.ObterTodosClientes());
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar([FromForm] Cliente cliente)
        {
            ModelState.Remove(nameof(cliente.Situacao));
            ModelState.Remove(nameof(cliente.DataCadastro));

            if (cliente.CPF != null)
                cliente.CPF = new string(cliente.CPF.Where(char.IsDigit).ToArray());

            if (!ModelState.IsValid)
                return View(cliente);

            if (!string.IsNullOrWhiteSpace(_clienteRepository.BuscaCpfCliente(cliente.CPF).CPF))
            {
                ViewData["MSG_E"] = "CPF já cadastrado.";
                return View(cliente);
            }
            if (!string.IsNullOrWhiteSpace(_clienteRepository.BuscaEmailCliente(cliente.Email).Email))
            {
                ViewData["MSG_E"] = "Email já cadastrado.";
                return View(cliente);
            }

            cliente.Situacao = SituacaoConstant.Ativo;
            _clienteRepository.Cadastrar(cliente);
            return RedirectToAction("Login", "Home", new { area = "" });
        }

        public IActionResult Atualizar(int id)
        {
            Cliente cliente = _clienteRepository.ObterCliente(id);
            if (cliente.ID_Usuario == 0)
                return RedirectToAction(nameof(Index));

            return View(cliente);
        }

        [HttpPost]
        public IActionResult Atualizar(int id, [FromForm] Cliente cliente)
        {
            cliente.ID_Usuario = id;

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
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ViewData["MSG_E"] = "CPF ou e-mail já pertence a outro cadastro.";
                return View(cliente);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Excluir(int id)
        {
            try
            {
                _clienteRepository.Excluir(id);
            }
            catch (MySqlException)
            {
                TempData["MSG_E"] = "Não foi possível excluir: o cliente tem pedidos ou pagamentos. Use Desativar.";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Enderecos(int id)
        {
            Cliente cliente = _clienteRepository.ObterCliente(id);
            if (cliente.ID_Usuario == 0)
                return RedirectToAction(nameof(Index));

            ViewBag.NomeCliente = cliente.Nome;
            return View(_enderecoRepository.ObterEnderecoDoUsuario(id));
        }

        [HttpPost]
        public IActionResult Ativar(int id)
        {
            _clienteRepository.Ativar(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Desativar(int id)
        {
            _clienteRepository.Desativar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}