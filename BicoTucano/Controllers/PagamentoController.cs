using BicoTucano.Libraries.Filtro;
using BicoTucano.Libraries.Login;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using Microsoft.AspNetCore.Mvc;

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

        private readonly IPedidoRepository _pedidoRepository;
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly LoginCliente _loginCliente;

        public PagamentoController(IPedidoRepository pedidoRepository,
                                   IPagamentoRepository pagamentoRepository,
                                   ICartaoRepository cartaoRepository,
                                   LoginCliente loginCliente)
        {
            _pedidoRepository = pedidoRepository;
            _pagamentoRepository = pagamentoRepository;
            _cartaoRepository = cartaoRepository;
            _loginCliente = loginCliente;
        }

        // id = NF do pedido. Tela onde o cliente escolhe como pagar.
        public IActionResult Index(int id)
        {
            Pedido? pedido = ObterPedidoDoCliente(id);
            if (pedido == null)
                return RedirectToAction("Index", "Home");

            if (JaFoiPago(id))
                return RedirectToAction(nameof(Confirmacao), new { id });

            PrepararIndex();
            return View(pedido);
        }

        
        // PIX
        

        [HttpPost]
        public IActionResult PagarPix(int id)
        {
            Pedido? pedido = ObterPedidoDoCliente(id);
            if (pedido == null)
                return RedirectToAction("Index", "Home");

            if (JaFoiPago(id))
                return RedirectToAction(nameof(Confirmacao), new { id });

            
            GravarPagamento(pedido, FormaPagamento.Pix, ChavePixLoja, null, StatusPagamento.NaoPago);
            return RedirectToAction(nameof(Pix), new { id });
        }

        public IActionResult Pix(int id)
        {
            Pedido? pedido = ObterPedidoDoCliente(id);
            if (pedido == null)
                return RedirectToAction("Index", "Home");

            if (JaFoiPago(id))
                return RedirectToAction(nameof(Confirmacao), new { id });

            Pagamento? pagamento = _pagamentoRepository.ObterPagamentoDoPedido(id);
            if (pagamento == null || pagamento.FormaPagamento != FormaPagamento.Pix)
                return RedirectToAction(nameof(Index), new { id });

            ViewBag.CodigoPix = CodigoPixFixo;
            return View(pedido);
        }

        // Botão "Já paguei" (simulação do banco avisando que o Pix caiu)
        [HttpPost]
        public IActionResult ConfirmarPix(int id)
        {
            Pedido? pedido = ObterPedidoDoCliente(id);
            if (pedido == null)
                return RedirectToAction("Index", "Home");

            if (JaFoiPago(id))
                return RedirectToAction(nameof(Confirmacao), new { id });

            Pagamento? pagamento = _pagamentoRepository.ObterPagamentoDoPedido(id);
            if (pagamento == null || pagamento.FormaPagamento != FormaPagamento.Pix)
                return RedirectToAction(nameof(Index), new { id });

            GravarPagamento(pedido, FormaPagamento.Pix, ChavePixLoja, null, StatusPagamento.Pago);
            return RedirectToAction(nameof(Confirmacao), new { id });
        }


        // CARTÃO (crédito ou débito)

        [HttpPost]
        public IActionResult PagarCartao(int id, FormaPagamento forma, int? idCartao, string? cvv)
        {
            Pedido? pedido = ObterPedidoDoCliente(id);
            if (pedido == null)
                return RedirectToAction("Index", "Home");

            if (JaFoiPago(id))
                return RedirectToAction(nameof(Confirmacao), new { id });

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

            // cartão terminado em 0002 simula recusa da operadora
            if (cartao.NumeroCartao.EndsWith(FinalRecusado))
                return ErroCartao(pedido, forma, cartao.ID_Cartao, "Pagamento recusado pela operadora. Tente outro cartão.");

            GravarPagamento(pedido, forma, null, cartao.ID_Cartao, StatusPagamento.Pago);
            return RedirectToAction(nameof(Confirmacao), new { id = pedido.NF });
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



        // CONFIRMAÇÃO


        public IActionResult Confirmacao(int id)
        {
            Pedido? pedido = ObterPedidoDoCliente(id);
            if (pedido == null)
                return RedirectToAction("Index", "Home");

            Pagamento? pagamento = _pagamentoRepository.ObterPagamentoDoPedido(id);
            if (pagamento == null || pagamento.StatusPagamento != StatusPagamento.Pago)
                return RedirectToAction(nameof(Index), new { id });

            if (pagamento.ID_Cartao.HasValue)
                ViewBag.Cartao = _cartaoRepository.ObterCartao(pagamento.ID_Cartao.Value);

            ViewBag.Pagamento = pagamento;
            return View(pedido);
        }

        // ---------------------------------------------------------------
        // AUXILIARES
        // ---------------------------------------------------------------

        // Só devolve o pedido se ele existir e for do cliente logado
        private Pedido? ObterPedidoDoCliente(int nf)
        {
            Cliente cliente = _loginCliente.GetCliente();
            Pedido? pedido = _pedidoRepository.ObterPedidos(nf);

            if (pedido == null || pedido.ID_Usuario != cliente.ID_Usuario)
                return null;

            return pedido;
        }

        private bool JaFoiPago(int nf)
        {
            Pagamento? pagamento = _pagamentoRepository.ObterPagamentoDoPedido(nf);
            return pagamento != null && pagamento.StatusPagamento == StatusPagamento.Pago;
        }

        private void PrepararIndex()
        {
            Cliente cliente = _loginCliente.GetCliente();
            ViewBag.Cartoes = _cartaoRepository.ObterCartoesDoUsuario(cliente.ID_Usuario).ToList();
        }

        // Cria o pagamento do pedido ou atualiza o que já existe (um pagamento por pedido).
       
        private void GravarPagamento(Pedido pedido, FormaPagamento forma, string? chavePix, int? idCartao, StatusPagamento status)
        {
            Pagamento? pagamento = _pagamentoRepository.ObterPagamentoDoPedido(pedido.NF);
            bool novo = pagamento == null;

            if (pagamento == null)
                pagamento = new Pagamento { NF = pedido.NF };

            bool pago = status == StatusPagamento.Pago;

            pagamento.FormaPagamento = forma;
            pagamento.ChavePix = chavePix;
            pagamento.ID_Cartao = idCartao;
            pagamento.StatusPagamento = status;
            pagamento.ValorPago = pago ? pedido.ValorTotal : 0m;
            pagamento.DataPagamento = pago ? DateTime.Now : null;

            if (novo)
                _pagamentoRepository.Cadastrar(pagamento);
            else
                _pagamentoRepository.Atualizar(pagamento);
        }
    }
}