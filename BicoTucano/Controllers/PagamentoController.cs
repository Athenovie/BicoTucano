using BicoTucano.CarrinhoCompra;
using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BicoTucano.Controllers
{
    [ClienteAutorizacao]
    public class PagamentoController : Controller
    {
        // Chave Pix do restaurante (é o que fica gravado em tbPagamento.ChavePix)
        private const string ChavePixLoja = "pix@bicotucano.com.br";

        // o mesmo texto fixo para todos os pedidos (não é um Pix de verdade)
        private const string CodigoPixFixo = "BICOTUCANO-PIX-SIMULADO-123456789";

        // cartão terminado em 0002 é "recusado pela operadora" (serve para testar o erro)
        private const string FinalRecusado = "0002";

        // nomes usados para guardar o pedido na sessão (preenchidos em PedidoController.Finalizar)
        private const string ChavePedido = "PedidoPendente";
        private const string ChaveItens = "ItensPendentes";

        private readonly IPedidoRepository _pedidoRepository;
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly CookieCarrinhoCompra _cookieCarrinhoCompra;
        private readonly LoginCliente _loginCliente;

        public PagamentoController(IPedidoRepository pedidoRepository,
                                   IPagamentoRepository pagamentoRepository,
                                   ICartaoRepository cartaoRepository,
                                   CookieCarrinhoCompra cookieCarrinhoCompra,
                                   LoginCliente loginCliente)
        {
            _pedidoRepository = pedidoRepository;
            _pagamentoRepository = pagamentoRepository;
            _cartaoRepository = cartaoRepository;
            _cookieCarrinhoCompra = cookieCarrinhoCompra;
            _loginCliente = loginCliente;
        }

        // Tela onde o cliente escolhe como pagar. O pedido ainda NÃO está no banco.
        public IActionResult Index()
        {
            Pedido? pedido = ObterPendente(out _);
            if (pedido == null)
                return RedirectToAction("Carrinho", "Pedido");

            PrepararIndex();
            return View(pedido);
        }

        // PIX

        [HttpPost]
        public IActionResult PagarPix()
        {
            if (ObterPendente(out _) == null)
                return RedirectToAction("Carrinho", "Pedido");

            return RedirectToAction(nameof(Pix));
        }

        public IActionResult Pix()
        {
            Pedido? pedido = ObterPendente(out _);
            if (pedido == null)
                return RedirectToAction("Carrinho", "Pedido");

            ViewBag.CodigoPix = CodigoPixFixo;
            return View(pedido);
        }

        // Botão "Confirmar pagamento" (simulação do banco avisando que o Pix caiu).
        // É AQUI que o pedido é gravado de verdade.
        [HttpPost]
        public IActionResult ConfirmarPix()
        {
            Pedido? pedido = ObterPendente(out List<Item_Pedido> itens);
            if (pedido == null)
                return RedirectToAction("Carrinho", "Pedido");

            int nf = ConcluirPedido(pedido, itens, FormaPagamento.Pix, ChavePixLoja, null);
            return RedirectToAction(nameof(Confirmacao), new { id = nf });
        }

        // CARTÃO (crédito ou débito)

        [HttpPost]
        public IActionResult PagarCartao(FormaPagamento forma, int? idCartao, string? cvv)
        {
            Pedido? pedido = ObterPendente(out List<Item_Pedido> itens);
            if (pedido == null)
                return RedirectToAction("Carrinho", "Pedido");

            // esta tela só aceita crédito ou débito
            if (forma != FormaPagamento.CartaoCredito && forma != FormaPagamento.CartaoDebito)
                forma = FormaPagamento.CartaoCredito;

            Cliente cliente = _loginCliente.GetCliente();

            if (!idCartao.HasValue)
                return ErroCartao(pedido, forma, null, "Selecione um cartão.");

            Cartao? cartao = _cartaoRepository.ObterCartao(idCartao.Value);

            // o cartão precisa existir e ser do cliente logado
            if (cartao == null || cartao.ID_Usuario != cliente.ID_Usuario)
                return ErroCartao(pedido, forma, null, "Cartão inválido.");

            if (cartao.DataValidade.Date < DateTime.Today)
                return ErroCartao(pedido, forma, cartao.ID_Cartao, "Este cartão está vencido.");

            // o CVV nunca é salvo: só confere o formato (3 ou 4 dígitos)
            string cvvLimpo = new string((cvv ?? "").Where(char.IsDigit).ToArray());
            if (cvvLimpo.Length < 3 || cvvLimpo.Length > 4)
                return ErroCartao(pedido, forma, cartao.ID_Cartao, "Informe o código de segurança (CVV) com 3 ou 4 dígitos.");

            // cartão terminado em 0002 simula recusa da operadora (nada é gravado)
            if (cartao.NumeroCartao.EndsWith(FinalRecusado))
                return ErroCartao(pedido, forma, cartao.ID_Cartao, "Pagamento recusado pela operadora. Tente outro cartão.");

            int nf = ConcluirPedido(pedido, itens, forma, null, cartao.ID_Cartao);
            return RedirectToAction(nameof(Confirmacao), new { id = nf });
        }

        // Volta para a tela de pagamento mantendo o que o cliente escolheu
        private IActionResult ErroCartao(Pedido pedido, FormaPagamento forma, int? idCartao, string mensagem)
        {
            PrepararIndex();
            ViewBag.Erro = mensagem;
            ViewBag.FormaSelecionada = forma;
            ViewBag.CartaoSelecionado = idCartao ?? 0;
            return View("Index", pedido);
        }

        // CONFIRMAÇÃO (só existe depois que o pedido foi gravado e pago)

        public IActionResult Confirmacao(int id)
        {
            Pedido? pedido = ObterPedidoDoCliente(id);
            if (pedido == null)
                return RedirectToAction("Index", "Home");

            Pagamento? pagamento = _pagamentoRepository.ObterPagamentoDoPedido(id);
            if (pagamento == null || pagamento.StatusPagamento != StatusPagamento.Pago)
                return RedirectToAction("MeusPedidos", "Pedido");

            if (pagamento.ID_Cartao.HasValue)
                ViewBag.Cartao = _cartaoRepository.ObterCartao(pagamento.ID_Cartao.Value);

            ViewBag.Pagamento = pagamento;
            return View(pedido);
        }

        // AUXILIARES

        // Só devolve o pedido se ele existir e for do cliente logado
        private Pedido? ObterPedidoDoCliente(int nf)
        {
            Cliente cliente = _loginCliente.GetCliente();
            Pedido? pedido = _pedidoRepository.ObterPedidos(nf);

            if (pedido == null || pedido.ID_Usuario != cliente.ID_Usuario)
                return null;

            return pedido;
        }

        // Pedido e itens guardados na sessão (ainda não gravados), só se forem do cliente logado
        private Pedido? ObterPendente(out List<Item_Pedido> itens)
        {
            itens = new List<Item_Pedido>();

            string? jsonPedido = HttpContext.Session.GetString(ChavePedido);
            string? jsonItens = HttpContext.Session.GetString(ChaveItens);

            if (string.IsNullOrEmpty(jsonPedido) || string.IsNullOrEmpty(jsonItens))
                return null;

            Pedido? pedido = JsonSerializer.Deserialize<Pedido>(jsonPedido);
            List<Item_Pedido>? lista = JsonSerializer.Deserialize<List<Item_Pedido>>(jsonItens);

            Cliente cliente = _loginCliente.GetCliente();

            if (pedido == null || lista == null || lista.Count == 0 || pedido.ID_Usuario != cliente.ID_Usuario)
                return null;

            itens = lista;
            return pedido;
        }

        // Pagamento aprovado: grava pedido, itens e pagamento; depois limpa a sessão e o carrinho
        private int ConcluirPedido(Pedido pedido, List<Item_Pedido> itens, FormaPagamento forma, string? chavePix, int? idCartao)
        {
            pedido.DataPedido = DateTime.Now;
            pedido.StatusPedido = StatusPedido.EmPreparo;

            int nf = _pedidoRepository.Cadastrar(pedido, itens);

            _pagamentoRepository.Cadastrar(new Pagamento
            {
                NF = nf,
                FormaPagamento = forma,
                ChavePix = chavePix,
                ID_Cartao = idCartao,
                StatusPagamento = StatusPagamento.Pago,
                ValorPago = pedido.ValorTotal,
                DataPagamento = DateTime.Now
            });

            HttpContext.Session.Remove(ChavePedido);
            HttpContext.Session.Remove(ChaveItens);
            _cookieCarrinhoCompra.RemoverTodos();

            return nf;
        }

        private void PrepararIndex()
        {
            Cliente cliente = _loginCliente.GetCliente();
            ViewBag.Cartoes = _cartaoRepository.ObterCartoesDoUsuario(cliente.ID_Usuario).ToList();
        }
    }
}