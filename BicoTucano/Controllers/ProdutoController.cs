using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace BicoTucano.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public ProdutoController(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository)
        {
            _produtoRepository = produtoRepository;
            _categoriaRepository = categoriaRepository;
        }

        public IActionResult Detalhes(int id)
        {
            Produto? produto = _produtoRepository.ObterProduto(id);

            if (produto == null)
            {
                return NotFound();
            }

            ViewBag.ProdutosRelacionados = _produtoRepository.ObterProdutosAleatorios(id).ToList();
    
            return View(produto);
        }

        public IActionResult Index(List<int>? categorias, decimal? precoMin, decimal? precoMax, string? categoria, string? busca)
        {
            List<Categoria> todasCategorias = _categoriaRepository.ObterTodasCategorias().ToList();
            List<Produto> todos = _produtoRepository.ObterTodosProdutos().ToList();

            categorias ??= new List<int>();

            // faz os links do rodapé funcionarem
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                Categoria? porNome = todasCategorias.FirstOrDefault(c =>
                    string.Equals(c.NomeCategoria, categoria, StringComparison.OrdinalIgnoreCase));

                if (porNome != null && !categorias.Contains(porNome.ID_Categoria))
                    categorias.Add(porNome.ID_Categoria);
            }

            if (precoMin.HasValue && precoMax.HasValue && precoMin > precoMax)
                (precoMin, precoMax) = (precoMax, precoMin);

            IEnumerable<Produto> filtrados = todos;

            if (categorias.Count > 0)
                filtrados = filtrados.Where(p => categorias.Contains(p.ID_Categoria));

            if (precoMin.HasValue)
                filtrados = filtrados.Where(p => p.Preco >= precoMin.Value);

            if (precoMax.HasValue)
                filtrados = filtrados.Where(p => p.Preco <= precoMax.Value);

            // barra de pesquisa
            busca = busca?.Trim();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                string[] palavras = SemAcento(busca)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Length > 3 && w.EndsWith("s") ? w.Substring(0, w.Length - 1) : w)   // "lanches" acha "lanche"
                    .ToArray();

                filtrados = filtrados.Where(p =>
                {
                    string nomeCategoria = todasCategorias
                        .FirstOrDefault(c => c.ID_Categoria == p.ID_Categoria)?.NomeCategoria ?? "";

                    string texto = SemAcento($"{p.Nome} {p.Descricao} {nomeCategoria}");
                    return palavras.All(palavra => texto.Contains(palavra));
                });
            }

            ViewBag.Busca = busca;
            ViewBag.Categorias = todasCategorias;
            ViewBag.CategoriasSelecionadas = categorias;
            ViewBag.PrecoMin = precoMin;
            ViewBag.PrecoMax = precoMax;
            ViewBag.PrecoMaximoCatalogo = todos.Count > 0 ? todos.Max(p => p.Preco) : 0m;

            return View(filtrados.ToList());
        }

        private static string SemAcento(string texto)
        {
            string decomposto = texto.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in decomposto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }
    }
}