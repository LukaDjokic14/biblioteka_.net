using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using Biblioteka.Infrastructure.Repozitorijumi;


namespace Biblioteka.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BibliotekaContext _context;

        private IKnjigaRepository? _knjige;
        private IClanRepository? _clanovi;
        private IRepository<Autor>? _autori;
        private IRepository<Izdavanje>? _izdavanja;
        private IRepository<Grad>? _gradovi;
        private IRepository<Bibliotekar>? _bibliotekari;
        private IRepository<Pisanje>? _pisanja;
        private IRepository<StavkaIzdavanja> _stavkeIzdavanja;
        private IRepository<Student> _studenti;

        public UnitOfWork(BibliotekaContext context)
        {
            _context = context;
        }

        public IKnjigaRepository Knjige =>
            _knjige ??= new KnjigaRepository(_context);

        public IClanRepository Clanovi =>
            _clanovi ??= new ClanRepository(_context);

        public IRepository<Autor> Autori =>
            _autori ??= new Repository<Autor>(_context);

        public IRepository<Izdavanje> Izdavanja =>
            _izdavanja ??= new Repository<Izdavanje>(_context);

        public IRepository<Grad> Gradovi =>
            _gradovi ??= new Repository<Grad>(_context);

        public IRepository<Bibliotekar> Bibliotekari =>
        _bibliotekari ??= new Repository<Bibliotekar>(_context);

        public IRepository<Pisanje> Pisanja =>
        _pisanja ??= new Repository<Pisanje>(_context);

        public IRepository<StavkaIzdavanja> StavkeIzdavanja =>
            _stavkeIzdavanja = new Repository<StavkaIzdavanja>(_context);

        public IRepository<Student> Studenti => _studenti ??= new Repository<Student>(_context);

        public int SaveChanges() => _context.SaveChanges();

        public void Dispose() => _context.Dispose();
    }
}
