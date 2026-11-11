using BicoTucano.GerenciaArquivo;
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

       
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Produto produto, IFormFile file)
        {
            var Caminho = GerenciadorArquivo.CadastrarImagemProduto(file);

            produto.NomeImagem = Caminho;

            _produtoRepository.Cadastrar(produto);

            ViewBag.msg = "Cadastro realizado";
            return RedirectToAction("Home", "Index");


        }

        public IActionResult Cadastrar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Cadastrar(Produto produto, IFormFile file)
        {
            var Caminho = GerenciadorArquivo.CadastrarImagemProduto(file);

            produto.NomeImagem = Caminho;

            _produtoRepository.Cadastrar(produto);

            ViewBag.msg = "Cadastro realizado";
            return RedirectToAction("Index", "Home");



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

      
    }
}
