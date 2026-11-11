using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;

namespace BicoTucano.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoController(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public IActionResult Detalhes(int id)
        {
            Produto? produto = _produtoRepository.ObterProduto(id);

            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
