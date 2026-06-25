using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Domain
{
    public class Izdavanje
    {
        public int IzdavanjeId { get; set; }
        public DateTime DatumIzdavanja { get; set; }
        public DateTime? DatumVracanja { get; set; } //može biti null (jer knjiga možda još nije vraćena)
        public string Status { get; set; } = string.Empty;
        public string Napomena { get; set; } = string.Empty;

        public string? BibliotekarJmbg { get; set; }
        public Bibliotekar? Bibliotekar { get; set; }

        public string ClanJmbg { get; set; } = string.Empty;
        public Clan? Clan { get; set; }

        public List<StavkaIzdavanja> StavkeIzdavanja { get; set; } = new();
    }
}
