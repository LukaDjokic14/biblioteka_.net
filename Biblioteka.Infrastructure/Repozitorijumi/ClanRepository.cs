using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;


namespace Biblioteka.Infrastructure.Repozitorijumi
{
    public class ClanRepository : Repository<Clan>, IClanRepository
    {
        public ClanRepository(BibliotekaContext context) : base(context) { }

        public IEnumerable<Clan> GetByGrad(int gradId)
        {
            return DbSet.Where(c => c.GradId == gradId).ToList();
        }
    }
}
