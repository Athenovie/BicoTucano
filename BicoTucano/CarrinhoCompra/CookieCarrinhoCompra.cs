using BicoTucano.Models;
using Newtonsoft.Json;

namespace BicoTucano.CarrinhoCompra
{
    public class CookieCarrinhoCompra
    {
        private string Key = "Carrinho.Compras";
        private const int LimitePorItem = 99;
        private Cookie.Cookie _cookie;

        public CookieCarrinhoCompra(Cookie.Cookie cookie)
        {
            _cookie = cookie;
        }

        public void Salvar(List<Item_Pedido> Lista)
        {
            string Valor = JsonConvert.SerializeObject(Lista);
            _cookie.Cadastrar(Key, Valor);
        }

        public List<Item_Pedido> Consultar()
        {
            if (!_cookie.Existe(Key))
                return new List<Item_Pedido>();

            try
            {
                string valor = _cookie.Consultar(Key);
                List<Item_Pedido>? lista = JsonConvert.DeserializeObject<List<Item_Pedido>>(valor);
                return lista ?? new List<Item_Pedido>();
            }
            catch (JsonException)
            {
                return new List<Item_Pedido>();   // cookie corrompido ou alterado
            }
        }

        public void Cadastrar(Item_Pedido item)
        {
            List<Item_Pedido> Lista = Consultar();
            Item_Pedido? ItemLocalizado = Lista.FirstOrDefault(a => a.ID_Produto == item.ID_Produto);

            if (ItemLocalizado == null)
                Lista.Add(item);
            else
                ItemLocalizado.Quantidade = Math.Min(ItemLocalizado.Quantidade + item.Quantidade, LimitePorItem);

            Salvar(Lista);
        }

        public void Remover(Item_Pedido itens)
        {
            var Lista = Consultar();
            var ItemLocalizado = Lista.SingleOrDefault(a => a.ID_Produto == itens.ID_Produto);

            if (ItemLocalizado != null)
            {
                Lista.Remove(ItemLocalizado);
                Salvar(Lista);
            }
        }
        //Atualiza
        public void Atualizar(Item_Pedido item)
        {
            List<Item_Pedido> Lista = Consultar();
            Item_Pedido? ItemLocalizado = Lista.FirstOrDefault(a => a.ID_Produto == item.ID_Produto);

            if (ItemLocalizado != null)
            {
                ItemLocalizado.Quantidade = Math.Min(item.Quantidade, LimitePorItem);
                Salvar(Lista);
            }
        }

        public void RemoverTodos()
        {
            _cookie.Remover(Key);
        }
        public int TotalItens()
        {
            return Consultar().Sum(i => i.Quantidade);
        }

    }
}
