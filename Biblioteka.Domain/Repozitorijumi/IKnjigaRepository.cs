using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Domain.Repozitorijumi
{
    public interface IKnjigaRepository: IRepository<Knjiga>
    {
        IEnumerable<Knjiga> GetByZanr(Zanr zanr);
        Knjiga? GetByIdWithAuthors(int id);
    }
}
