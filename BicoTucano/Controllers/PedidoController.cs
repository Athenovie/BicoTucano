using BicoTucano.CarrinhoCompra;
using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;

namespace BicoTucano.Controllers
{
    public class PedidoController : Controller
    {
        private const int LimitePorItem = 99;
        private const decimal TaxaEntregaPadrao = 5.00m;   // valor da entrega: ajuste aqui
        private const int TamanhoDetalhes = 150;           // tbPedido.DetalhesPedido é VARCHAR(150)

        private readonly IProdutoRepository _produtoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly CookieCarrinhoCompra _cookieCarrinhoCompra;
        private readonly LoginCliente _loginCliente;

        public PedidoController(IProdutoRepository produtoRepository,
                                IPedidoRepository pedidoRepository,
                                IItemRepository itemRepository,
                                IEnderecoRepository enderecoRepository,
                                CookieCarrinhoCompra cookieCarrinhoCompra,
                                LoginCliente loginCliente)
        {
            _produtoRepository = produtoRepository;
            _pedidoRepository = pedidoRepository;
            _itemRepository = itemRepository;
            _enderecoRepository = enderecoRepository;
            _cookieCarrinhoCompra = cookieCarrinhoCompra;
            _loginCliente = loginCliente;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Carrinho));
        }

        // ---------------------------------------------------------------
        // CARRINHO (não precisa estar logado)
        // ---------------------------------------------------------------

        // Item ID = ID do Produto
        [HttpPost]
        public IActionResult AdicionarItem(int id, int quantidade = 1)
        {
            Produto? produto = _produtoRepository.ObterProduto(id);

            if (produto == null || !produto.Disponibilidade)
            {
                TempData["MSG_E"] = "Este prato não está disponível no momento.";
                return RedirectToAction(nameof(Carrinho));
            }

            quantidade = Math.Clamp(quantidade, 1, LimitePorItem);

            _cookieCarrinhoCompra.Cadastrar(new Item_Pedido
            {
                ID_Produto = id,
                Quantidade = quantidade
            });

            TempData["MSG_S"] = produto.Nome + " foi adicionado ao carrinho.";
            return RedirectToAction(nameof(Carrinho));
        }

        public IActionResult Carrinho()
        {
            var (itens, produtos, alterou) = CarregarCarrinho();

            ViewBag.Produtos = produtos;
            return View(itens);
        }

        [HttpPost]
        public IActionResult AlterarQuantidade(int id, int quantidade)
        {
            if (quantidade <= 0)
                _cookieCarrinhoCompra.Remover(new Item_Pedido { ID_Produto = id });
            else
                _cookieCarrinhoCompra.Atualizar(new Item_Pedido { ID_Produto = id, Quantidade = quantidade });

            return RedirectToAction(nameof(Carrinho));
        }

        // Remover itens do carrinho
        [HttpPost]
        public IActionResult RemoverItem(int id)
        {
            _cookieCarrinhoCompra.Remover(new Item_Pedido { ID_Produto = id });
            return RedirectToAction(nameof(Carrinho));
        }

        [HttpPost]
        public IActionResult LimparCarrinho()
        {
            _cookieCarrinhoCompra.RemoverTodos();
            return RedirectToAction(nameof(Carrinho));
        }

        // FINALIZAR PEDIDO (precisa estar logado como cliente)

        [ClienteAutorizacao]
        public IActionResult Finalizar()
        {
            var (itens, produtos, alterou) = CarregarCarrinho();

            if (itens.Count == 0 || alterou)
                return RedirectToAction(nameof(Carrinho));

            PrepararFinalizacao(produtos);
            return View(itens);
        }

        [ClienteAutorizacao]
        [HttpPost]
        public IActionResult Finalizar(TipoPedido tipoPedido, int? idEndereco, string? observacao)
        {
            Cliente cliente = _loginCliente.GetCliente();
            var (itens, produtos, alterou) = CarregarCarrinho();

            if (itens.Count == 0 || alterou)
                return RedirectToAction(nameof(Carrinho));

            // Entrega exige um endereço que seja do próprio cliente
            Endereco? endereco = null;
            string? erro = null;

            if (!Enum.IsDefined(typeof(TipoPedido), tipoPedido))
            {
                erro = "Escolha entre retirada e entrega.";
            }
            else if (tipoPedido == TipoPedido.Entrega)
            {
                if (idEndereco.HasValue)
                    endereco = _enderecoRepository.ObterEndereco(idEndereco.Value);

                if (endereco == null || endereco.ID_UsuarioEndereco == 0 || endereco.ID_Usuario != cliente.ID_Usuario)
                {
                    endereco = null;
                    erro = "Escolha um dos seus endereços para a entrega.";
                }
            }

            if (erro != null)
            {
                PrepararFinalizacao(produtos);
                ViewBag.Erro = erro;
                ViewBag.TipoSelecionado = tipoPedido;
                ViewBag.EnderecoSelecionado = idEndereco;
                ViewBag.Observacao = observacao;
                return View(itens);
            }

            // Tudo é calculado aqui no servidor, com os preços do banco
            decimal subtotal = itens.Sum(i => i.Preco * i.Quantidade);
            decimal taxa = tipoPedido == TipoPedido.Entrega ? TaxaEntregaPadrao : 0m; // calculo da taxa de entrega, ver depois

            Pedido pedido = new Pedido
            {
                DataPedido = DateTime.Now,
                ID_Usuario = cliente.ID_Usuario,
                TipoPedido = tipoPedido,
                StatusPedido = StatusPedido.EmPreparo,
                Subtotal = subtotal,
                TaxaEntrega = taxa,
                ValorTotal = subtotal + taxa,
                CEP = endereco?.CEP,
                DetalhesPedido = MontarDetalhes(endereco, observacao),
                ID_Funcionario = null // analisar condições 
            };

            int nf = _pedidoRepository.Cadastrar(pedido, itens);

            _cookieCarrinhoCompra.RemoverTodos();
            return RedirectToAction(nameof(Confirmacao), new { id = nf });
        }

        [ClienteAutorizacao]
        public IActionResult Confirmacao(int id)
        {
            Cliente cliente = _loginCliente.GetCliente();
            Pedido? pedido = _pedidoRepository.ObterPedidos(id);

            // só o dono do pedido vê a confirmação
            if (pedido == null || pedido.ID_Usuario != cliente.ID_Usuario)
                return RedirectToAction("Index", "Home");

            List<Item_Pedido> itens = _itemRepository.ObterTodosItens(id).ToList();
            Dictionary<int, Produto> produtos = new Dictionary<int, Produto>();

            foreach (Item_Pedido item in itens)
            {
                Produto? produto = _produtoRepository.ObterProduto(item.ID_Produto);
                if (produto != null)
                    produtos[item.ID_Produto] = produto;
            }

            ViewBag.Itens = itens;
            ViewBag.Produtos = produtos;
            return View(pedido);
        }


        // AUXILIARES
      

        // Relê cada prato do carrinho no banco: prato apagado ou indisponível sai do carrinho
        // e o preço é sempre o do banco (o cookie só guarda código e quantidade).
        private (List<Item_Pedido> Itens, Dictionary<int, Produto> Produtos, bool Alterou) CarregarCarrinho()
        {
            List<Item_Pedido> salvos = _cookieCarrinhoCompra.Consultar();
            List<Item_Pedido> validos = new List<Item_Pedido>();
            Dictionary<int, Produto> produtos = new Dictionary<int, Produto>();

            foreach (Item_Pedido item in salvos)
            {
                Produto? produto = _produtoRepository.ObterProduto(item.ID_Produto);

                if (produto == null || !produto.Disponibilidade)
                    continue;

                item.Preco = produto.Preco;
                item.Quantidade = Math.Clamp(item.Quantidade, 1, LimitePorItem);
                validos.Add(item);
                produtos[produto.ID_Produto] = produto;
            }

            bool alterou = validos.Count != salvos.Count;
            if (alterou)
            {
                _cookieCarrinhoCompra.Salvar(validos);
                TempData["MSG_E"] = "Alguns pratos ficaram indisponíveis e foram retirados do carrinho.";
            }

            return (validos, produtos, alterou);
        }

        private void PrepararFinalizacao(Dictionary<int, Produto> produtos)
        {
            Cliente cliente = _loginCliente.GetCliente();

            ViewBag.Produtos = produtos;
            ViewBag.Enderecos = _enderecoRepository.ObterEnderecoDoUsuario(cliente.ID_Usuario).ToList();
            ViewBag.TaxaEntrega = TaxaEntregaPadrao;
        }

        // O pedido só tem o CEP (tbPedido.CEP). Número, complemento e observação vão em DetalhesPedido (150 caracteres).
        private static string? MontarDetalhes(Endereco? endereco, string? observacao)
        {
            List<string> partes = new List<string>();

            if (endereco != null)
            {
                string local = "Nº " + endereco.Numero;
                if (!string.IsNullOrWhiteSpace(endereco.Complemento))
                    local += ", " + endereco.Complemento.Trim();
                partes.Add(local);
            }

            if (!string.IsNullOrWhiteSpace(observacao))
                partes.Add("Observação: " + observacao.Trim());

            if (partes.Count == 0)
                return null;

            string detalhes = string.Join(" | ", partes);
            return detalhes.Length > TamanhoDetalhes ? detalhes.Substring(0, TamanhoDetalhes) : detalhes;
        }
    }
}