using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Domain
{
    public class Pisanje
    {
        public int KnjigaId { get; set; }
        public Knjiga? Knjiga { get; set; }

        public int AutorId { get; set; }
        public Autor? Autor { get; set; }
    }
}
