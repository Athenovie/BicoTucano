
using System.Globalization;
using System.Text.RegularExpressions;
using BicoTucano.Models;
using BicoTucano.Repository.Contract;
using BicoTucano.Libraries.Login;
using Microsoft.AspNetCore.Mvc;

namespace BicoTucano.Controllers
{
    public class CartaoController : Controller
    {
        private readonly ICartaoRepository _cartaoRepository;
        private readonly LoginCliente _loginCliente;

        public CartaoController(
            ICartaoRepository cartaoRepository,
            LoginCliente loginCliente)
        {
            _cartaoRepository = cartaoRepository;
            _loginCliente = loginCliente;
        }

        // Obtém o ID do cliente logado
        private int ObterIdUsuarioLogado()
        {
            Cliente cliente = _loginCliente.GetCliente();

            if (cliente == null)
            {
                throw new Exception("Usuário não está logado.");
            }

            return cliente.ID_Usuario;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cartoes = _cartaoRepository.ObterCartoesDoUsuario(
                ObterIdUsuarioLogado()
            );

            return View(cartoes);
        }

        [HttpGet]
        public IActionResult Cadastrar(int? nf)
        {
            ViewBag.Nf = nf;
            return View(new Cartao());
        }

        [HttpPost]
        public IActionResult Cadastrar(Cartao cartao, string validade, int? nf)
        {
            // Remove validações de campos preenchidos pelo sistema
            ModelState.Remove("DataValidade");
            ModelState.Remove("Bandeira");
            ModelState.Remove("ID_Usuario");
            ModelState.Remove("NumeroCartao");

            string numero = new string(
                (cartao.NumeroCartao ?? "")
                .Where(char.IsDigit)
                .ToArray()
            );

            if (numero.Length < 13)
            {
                ModelState.AddModelError(
                    "NumeroCartao",
                    "Número de cartão inválido."
                );
            }

            DateTime primeiroDia = DateTime.MinValue;

            if (!DateTime.TryParseExact(
                "01/" + validade,
                "dd/MM/yy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out primeiroDia))
            {
                ModelState.AddModelError(
                    "validade",
                    "Validade inválida. Use MM/AA."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Nf = nf;
                ViewBag.Validade = validade;
                return View(cartao);
            }

            // Associa o cartão ao cliente logado
            cartao.ID_Usuario = ObterIdUsuarioLogado();

            cartao.Bandeira = DetectarBandeira(numero);

            // Armazena apenas os últimos 4 dígitos
            cartao.NumeroCartao = "**** **** **** " + numero[^4..];

            cartao.DataValidade = primeiroDia
                .AddMonths(1)
                .AddDays(-1);

            _cartaoRepository.Cadastrar(cartao);

            TempData["MSG_S"] = "Cartão cadastrado com sucesso!";

            return nf.HasValue
                ? RedirectToAction("Index", "Pagamento", new { id = nf })
                : RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Excluir(int id)
        {
            var cartao = _cartaoRepository.ObterCartao(id);

            // Verifica se o cartão pertence ao cliente logado
            if (cartao != null &&
                cartao.ID_Usuario == ObterIdUsuarioLogado())
            {
                _cartaoRepository.Excluir(id);

                TempData["MSG_S"] = "Cartão excluído.";
            }
            else
            {
                TempData["MSG_E"] = "Cartão não encontrado.";
            }

            return RedirectToAction("Index");
        }

        private static string? DetectarBandeira(string n)
        {
            if (Regex.IsMatch(n, @"^(401178|401179|431274|438935|451416|457393|457631|457632|504175|5066|5067|509|627780|636297|636368|650[0-9]|651[67]|655[0-9])"))
                return "Elo";

            if (Regex.IsMatch(n, @"^3[47]"))
                return "Amex";

            if (n.StartsWith("4"))
                return "Visa";

            if (Regex.IsMatch(n, @"^(5[1-5]|2(2[2-9]|[3-6]\d|7[01]|720))"))
                return "Mastercard";

            return null;
        }
    }
}
