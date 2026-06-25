namespace Biblioteka.API.DTOs.Knjige
{
    public class UpdateKnjigaRequest
    {
        public string? Naslov { get; set; }
        public int? GodinaIzdanja { get; set; }
        public string? Isbn { get; set; }
        public string? Slika { get; set; }
        public int? BrojStrana { get; set; }
        public string? Opis { get; set; }
        public string? Zanr { get; set; }
    }
}
