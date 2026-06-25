using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteka.Domain.Repozitorijumi
{
    public interface IClanRepository : IRepository<Clan>
    {
        IEnumerable<Clan> GetByGrad(int gradId);
    }
}
