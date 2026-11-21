using BicoTucano.Libraries.Filtro;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using static BicoTucano.Models.PedidoAdmin;

namespace BicoTucano.Areas.Funcionario.Controllers
{
    [Area("Funcionario")]
    [FuncionarioAutorizacao]
    public class PedidoController : Controller
    {
        private readonly IPedidoRepository _pedidoRepository;

        public PedidoController(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        // LISTAGEM
        public IActionResult Index()
        {
            return View(_pedidoRepository.ObterTodosPedidosAdmin());
        }

        // DETALHES
        public IActionResult Detalhes(int id)
        {
            PedidoDetalhe? detalhe = _pedidoRepository.ObterDetalhePedido(id);

            if (detalhe == null)
            {
                return RedirectToAction("Index");
            }

            return View(detalhe);
        }

        // ALTERAR STATUS
        [HttpPost]
        public IActionResult AlterarStatus(int id, StatusPedido status)
        {
            if (Enum.IsDefined(typeof(StatusPedido), status))
            {
                _pedidoRepository.AtualizarStatus(id, status);
                TempData["MSG_S"] = "Status do pedido atualizado.";
            }
            else
            {
                TempData["MSG_E"] = "Status inválido.";
            }

            return RedirectToAction("Detalhes", new { id = id });
        }
    }
}