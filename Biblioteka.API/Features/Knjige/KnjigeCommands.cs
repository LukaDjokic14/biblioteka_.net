using Biblioteka.API.DTOs.Knjige;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Knjige
{
    public record CreateKnjigaCommand(string Naslov, int GodinaIzdanja, string Isbn, string Slika,
        int BrojStrana, string Opis, string Zanr,
        string AutorIme, string AutorPrezime, string AutorBiografija) : IRequest<KnjigaDto?>;

    public class CreateKnjigaCommandHandler : IRequestHandler<CreateKnjigaCommand, KnjigaDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateKnjigaCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<KnjigaDto?> Handle(CreateKnjigaCommand request, CancellationToken ct)
        {
            if (!Enum.TryParse<Zanr>(request.Zanr, true, out var zanrEnum))
                return Task.FromResult<KnjigaDto?>(null);

            var knjiga = new Knjiga
            {
                Naslov = request.Naslov,
                GodinaIzdanja = request.GodinaIzdanja,
                Isbn = request.Isbn,
                Slika = request.Slika,
                BrojStrana = request.BrojStrana,
                Opis = request.Opis,
                Zanr = zanrEnum
            };
            _unitOfWork.Knjige.Add(knjiga);

            var autor = _unitOfWork.Autori
                .Find(a => a.Ime == request.AutorIme && a.Prezime == request.AutorPrezime)
                .FirstOrDefault();
            if (autor == null)
            {
                autor = new Autor { Ime = request.AutorIme, Prezime = request.AutorPrezime, Biografija = request.AutorBiografija };
                _unitOfWork.Autori.Add(autor);
            }
            _unitOfWork.Pisanja.Add(new Pisanje { Knjiga = knjiga, Autor = autor });
            _unitOfWork.SaveChanges();

            return Task.FromResult<KnjigaDto?>(new KnjigaDto
            {
                KnjigaId = knjiga.KnjigaId,
                Naslov = knjiga.Naslov,
                GodinaIzdanja = knjiga.GodinaIzdanja,
                Isbn = knjiga.Isbn,
                Slika = knjiga.Slika,
                BrojStrana = knjiga.BrojStrana,
                Opis = knjiga.Opis,
                Zanr = knjiga.Zanr.ToString()
            });
        }
    }

    public record UpdateKnjigaCommand(int Id, string? Naslov, int? GodinaIzdanja, string? Isbn,
        string? Slika, int? BrojStrana, string? Opis, string? Zanr) : IRequest<KnjigaDto?>;

    public class UpdateKnjigaCommandHandler : IRequestHandler<UpdateKnjigaCommand, KnjigaDto?>
    {
        private readonly IKnjigaRepository _knjigaRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateKnjigaCommandHandler(IKnjigaRepository knjigaRepository, IUnitOfWork unitOfWork)
        { _knjigaRepository = knjigaRepository; _unitOfWork = unitOfWork; }

        public Task<KnjigaDto?> Handle(UpdateKnjigaCommand request, CancellationToken ct)
        {
            var k = _knjigaRepository.GetById(request.Id);
            if (k == null) return Task.FromResult<KnjigaDto?>(null);

            if (!string.IsNullOrWhiteSpace(request.Naslov)) k.Naslov = request.Naslov;
            if (request.GodinaIzdanja.HasValue) k.GodinaIzdanja = request.GodinaIzdanja.Value;
            if (!string.IsNullOrWhiteSpace(request.Isbn)) k.Isbn = request.Isbn;
            if (!string.IsNullOrWhiteSpace(request.Slika)) k.Slika = request.Slika;
            if (request.BrojStrana.HasValue) k.BrojStrana = request.BrojStrana.Value;
            if (!string.IsNullOrWhiteSpace(request.Opis)) k.Opis = request.Opis;
            if (!string.IsNullOrWhiteSpace(request.Zanr) && Enum.TryParse<Zanr>(request.Zanr, true, out var z))
                k.Zanr = z;

            _unitOfWork.SaveChanges();

            return Task.FromResult<KnjigaDto?>(new KnjigaDto
            {
                KnjigaId = k.KnjigaId,
                Naslov = k.Naslov,
                GodinaIzdanja = k.GodinaIzdanja,
                Isbn = k.Isbn,
                Slika = k.Slika,
                BrojStrana = k.BrojStrana,
                Opis = k.Opis,
                Zanr = k.Zanr.ToString()
            });
        }
    }

    public record DeleteKnjigaCommand(int Id) : IRequest<bool>;

    public class DeleteKnjigaCommandHandler : IRequestHandler<DeleteKnjigaCommand, bool>
    {
        private readonly IKnjigaRepository _knjigaRepository;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteKnjigaCommandHandler(IKnjigaRepository knjigaRepository, IUnitOfWork unitOfWork)
        { _knjigaRepository = knjigaRepository; _unitOfWork = unitOfWork; }

        public Task<bool> Handle(DeleteKnjigaCommand request, CancellationToken ct)
        {
            var k = _knjigaRepository.GetById(request.Id);
            if (k == null) return Task.FromResult(false);

            var zauzeta = _unitOfWork.StavkeIzdavanja
                .Find(s => s.Knjiga.KnjigaId == request.Id &&
                    (s.Izdavanje.Status == "IZDATO" || s.Izdavanje.Status == "REZERVISANO"))
                .Any();
            if (zauzeta) return Task.FromResult(false);

            _unitOfWork.Knjige.Remove(k);
            _unitOfWork.SaveChanges();
            return Task.FromResult(true);
        }
    }
}