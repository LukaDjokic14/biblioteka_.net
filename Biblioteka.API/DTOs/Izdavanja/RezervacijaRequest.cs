namespace Biblioteka.API.DTOs.Izdavanja
{
    public class RezervacijaRequest
    {
        public string ClanJmbg { get; set; } = string.Empty;
        public List<string> NasloviKnjiga { get; set; } = new();
        public string Napomena { get; set; } = string.Empty;
    }
}
