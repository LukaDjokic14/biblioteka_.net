namespace Biblioteka.API.DTOs.Knjige
{
    public class KnjigaDto
    {
        public int KnjigaId { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public int GodinaIzdanja { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public string Slika { get; set; } = string.Empty;
        public int BrojStrana { get; set; }
        public string Opis { get; set; } = string.Empty;
        public string Zanr { get; set; } = string.Empty;
    }
}
