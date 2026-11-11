namespace BicoTucano.GerenciaArquivo
{
    public class GerenciadorArquivo
    {
        public static string CadastrarImagemProduto(IFormFile file)
        {
            var NomeArquivo = Path.GetFileName(file.FileName);
            var Caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Imagens", NomeArquivo);

            using (var Stream = new FileStream(Caminho, FileMode.Create))
            {
                file.CopyTo(Stream);
            }

            return Path.Combine("/Imagens", NomeArquivo).Replace("//", "/");
        }
    }
}
