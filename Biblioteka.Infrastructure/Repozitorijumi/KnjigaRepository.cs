using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Infrastructure.Repozitorijumi
{
    public class KnjigaRepository : Repository<Knjiga>, IKnjigaRepository
    {
        public KnjigaRepository(BibliotekaContext context) : base(context) { }

        public IEnumerable<Knjiga> GetByZanr(Zanr zanr)
        {
            return DbSet.Where(k => k.Zanr == zanr).ToList();
        }

        public Knjiga? GetByIdWithAuthors(int id)
        {
            return DbSet
                .Include(k => k.Pisanja)
                    .ThenInclude(p => p.Autor)
                .FirstOrDefault(k => k.KnjigaId == id);
        }
    }
}
