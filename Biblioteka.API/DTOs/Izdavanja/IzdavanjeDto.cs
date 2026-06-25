using Biblioteka.API.DTOs.Knjige;

namespace Biblioteka.API.DTOs.Izdavanja
{
        public class IzdavanjeDto
        {
            public int IzdavanjeId { get; set; }
            public DateTime DatumIzdavanja { get; set; }
            public DateTime? DatumVracanja { get; set; }
            public string Status { get; set; }
            public string BibliotekarImePrezime { get; set; }
            
            public string Napomena { get; set; }

            public string ClanJmbg { get; set; }
            public string ClanIme { get; set; }
            public string ClanPrezime { get; set; }
            public List<KnjigaDto> Knjige { get; set; } = new List<KnjigaDto>();
        }
    
}
