namespace Biblioteka.API.DTOs.Autori
{
    public class AutorDto
    {
        public int AutorId { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Biografija { get; set; } = string.Empty;
    }
}
