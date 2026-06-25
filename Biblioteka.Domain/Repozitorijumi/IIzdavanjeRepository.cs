using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Domain.Repozitorijumi
{
    public interface IIzdavanjeRepository:IRepository<Izdavanje>
    {
        IEnumerable<Izdavanje> GetAllWithDetails();
    }
}
