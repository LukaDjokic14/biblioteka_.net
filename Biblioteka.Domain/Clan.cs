using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Biblioteka.Domain
{
    public class Clan
    {
        public string Jmbg { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string BrojTelefona { get; set; } = string.Empty;
        public string KorisnickoIme { get; set; } = string.Empty;
        public string Lozinka { get; set; } = string.Empty;

        public int GradId { get; set; }
        public Grad? Grad { get; set; }

        [JsonIgnore]
        public List<Izdavanje> Izdavanja { get; set; } = new();
    }
}
