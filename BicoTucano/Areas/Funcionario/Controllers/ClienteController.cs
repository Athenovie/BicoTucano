using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BicoTucano.Areas.Funcionario.Controllers
{
    [Area("Funcionario")]
    [FuncionarioAutorizacao]
    public class ClienteController : Controller
    {
        private IClienteRepository _clienteRepository;

        public ClienteController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
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

            if(cliente.CPF != null)
                cliente.CPF = new string(cliente.CPF.Where(char.IsDigit).ToArray());

            if (!ModelState.IsValid)
                return View(cliente);

            if(!string.IsNullOrWhiteSpace(_clienteRepository.BuscaCpfCliente(cliente.CPF).CPF))
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

