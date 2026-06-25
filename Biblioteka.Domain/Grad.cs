using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Biblioteka.Domain
{
    public class Grad
    {
        public int GradId { get; set; }
        public string Naziv { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Clan> Clanovi { get; set; } = new();
    }
}
