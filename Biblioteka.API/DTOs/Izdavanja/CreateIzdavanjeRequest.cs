namespace Biblioteka.API.DTOs.Izdavanja
{
    public class CreateIzdavanjeRequest
    {
        public string Napomena { get; set; } = string.Empty;
        public string BibliotekarJmbg { get; set; } = string.Empty;
        public string ClanJmbg { get; set; } = string.Empty;

        public List<string> NasloviKnjiga { get; set; } = new List<string>();
    }
}
