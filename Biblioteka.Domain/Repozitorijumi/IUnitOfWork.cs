using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Domain.Repozitorijumi
{
    public interface IUnitOfWork : IDisposable
    {
        IKnjigaRepository Knjige { get; }
        IClanRepository Clanovi { get; }
        IRepository<Autor> Autori { get; }
        IRepository<Izdavanje> Izdavanja { get; }
        IRepository<Grad> Gradovi { get; }

        IRepository<Bibliotekar> Bibliotekari { get; }
        IRepository<Pisanje> Pisanja { get; }

        IRepository<StavkaIzdavanja> StavkeIzdavanja { get; }

        

        int SaveChanges();
    }
}
