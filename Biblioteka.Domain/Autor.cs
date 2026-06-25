using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Biblioteka.Domain
{
    public class Autor
    {
        public int AutorId { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Biografija { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Pisanje> Pisanja { get; set; } = new();
    }
}
