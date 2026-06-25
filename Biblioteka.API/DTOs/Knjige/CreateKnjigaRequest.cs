using Biblioteka.Domain;

namespace Biblioteka.API.DTOs.Knjige
{
    public class CreateKnjigaRequest
    {
        public string Naslov { get; set; } = string.Empty;
        public int GodinaIzdanja { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public string Slika { get; set; } = string.Empty;
        public int BrojStrana { get; set; }
        public string Opis { get; set; } = string.Empty;
        public string Zanr { get; set; } = string.Empty;  

        // Podaci za autora
        public string AutorIme { get; set; } = string.Empty;
        public string AutorPrezime { get; set; } = string.Empty;
        public string AutorBiografija { get; set; } = string.Empty;
    }
}
