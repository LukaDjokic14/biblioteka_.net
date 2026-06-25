using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Infrastructure.Repozitorijumi
{
    public class IzdavanjeRepository : Repository<Izdavanje>, IIzdavanjeRepository
    {

        private readonly BibliotekaContext _context;
        
        public IzdavanjeRepository(BibliotekaContext context) : base(context)
        {
            _context = context;
        }
        public IEnumerable<Izdavanje> GetAllWithDetails()
        {
            return _context.Izdavanja
            .Include(i => i.StavkeIzdavanja)      // Učitaj stavke
            .ThenInclude(s => s.Knjiga)       // Učitaj knjigu povezanu sa stavkom
        .ToList();
        }
    }
}
