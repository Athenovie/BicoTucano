using BicoTucano.GerenciaArquivo;
using BicoTucano.Libraries.Filtro;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BicoTucano.Areas.Funcionario.Controllers
{
    [Area("Funcionario")]
    [FuncionarioAutorizacao]
    public class ProdutoController : Controller
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public ProdutoController(IProdutoRepository produtoRepository, ICategoriaRepository CategoriaRepository)
        {
            _produtoRepository = produtoRepository;
            _categoriaRepository = CategoriaRepository;
        }


        // LISTAGEM
        public IActionResult Index()
        {
            return View(_produtoRepository.ObterTodosProdutos());
        }

        // Preenche o dropdown de categorias
        private void CarregarCategorias()
        {
            ViewBag.Categorias = new SelectList(
                _categoriaRepository.ObterTodasCategorias(),
                "ID_Categoria",
                "NomeCategoria");
        }


        // CADASTRAR
        [HttpGet]
        public IActionResult Cadastrar()
        {
            CarregarCategorias();
            return View(new Produto { Disponibilidade = true });
        }

        [HttpPost]
        public IActionResult Cadastrar(Produto produto, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("NomeImagem", "Selecione uma imagem.");
            }

            if (ModelState.IsValid)
            {
                produto.NomeImagem = GerenciadorArquivo.CadastrarImagemProduto(file);
                _produtoRepository.Cadastrar(produto);
                return RedirectToAction("Index");
            }

            CarregarCategorias();
            return View(produto);
        }
        // ALTERAR
        [HttpGet]
        public IActionResult Alterar(int id)
        {
            Produto? produto = _produtoRepository.ObterProduto(id);

            if (produto == null)
            {
                return RedirectToAction("Index");
            }

            CarregarCategorias();
            return View(produto);
        }

        [HttpPost]
        public IActionResult Alterar(Produto produto, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // Só troca a imagem se o funcionário escolheu uma nova.
                // Se não escolheu, o NomeImagem antigo vem no campo hidden do formulário.
                if (file != null && file.Length > 0)
                {
                    produto.NomeImagem = GerenciadorArquivo.CadastrarImagemProduto(file);
                }

                _produtoRepository.Atualizar(produto);
                return RedirectToAction("Index");
            }

            CarregarCategorias();
            return View(produto);
        }

        // ATIVAR / DESATIVAR
        [HttpPost]
        public IActionResult AlterarDisponibilidade(int id, bool disponibilidade)
        {
            _produtoRepository.AlterarDisponibilidade(id, disponibilidade);
            return RedirectToAction("Index");
        }

    
    }
}
