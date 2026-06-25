namespace Biblioteka.API.DTOs.Bibliotekari
{
    public class UpdateBibliotekarRequest
    {
        public string? Prezime { get; set; } = string.Empty;
        public string? KorisnickoIme { get; set; } = string.Empty;
        public string? NovaLozinka { get; set; }
    }
}
