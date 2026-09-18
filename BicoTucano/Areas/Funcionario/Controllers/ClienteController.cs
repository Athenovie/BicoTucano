using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Models.Constants;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BicoTucano.Areas.Funcionario.Controllers
{
    [Area("Funcionario")]
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
            cliente.Situacao = SituacaoConstant.Ativo;
            _clienteRepository.Cadastrar(cliente);
            return RedirectToAction(nameof(Cadastrar));


        }
     
        public IActionResult Ativar(int id)
        {
            _clienteRepository.Ativar(id);
            return RedirectToAction(nameof(Index));
        }
       
        public IActionResult Desativar(int id)
        {
            _clienteRepository.Desativar(id);
            return RedirectToAction(nameof(Index));
        }

    }
}

