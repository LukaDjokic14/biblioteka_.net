using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Domain
{
    public class StavkaIzdavanja
    {
        public int KnjigaId { get; set; }
        public Knjiga? Knjiga { get; set; }

        public int IzdavanjeId { get; set; }
        public Izdavanje? Izdavanje { get; set; }
    }
}
