using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Biblioteka.Domain
{
    public class Knjiga
    {
        public int KnjigaId { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public int GodinaIzdanja { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public string Slika { get; set; } = string.Empty;
        public int BrojStrana { get; set; }
        public string Opis { get; set; } = string.Empty;

        public Zanr Zanr { get; set; } 

        [JsonIgnore]
        public List<Pisanje> Pisanja { get; set; } = new();

        [JsonIgnore]
        public List<StavkaIzdavanja> StavkeIzdavanja { get; set; } = new();
    }
}
