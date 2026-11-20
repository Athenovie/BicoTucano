using BicoTucano.Models;

namespace BicoTucano.Repository.Contract
{
    
        public interface ICategoriaRepository
        {
            IEnumerable<Categoria>ObterTodasCategorias();
        }
    
}
