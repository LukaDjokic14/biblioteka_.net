using Biblioteka.API.DTOs.Autori;
using Biblioteka.API.DTOs.Knjige;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Knjige
{
    public record GetAllKnjigeQuery() : IRequest<IEnumerable<KnjigaDto>>;

    public class GetAllKnjigeQueryHandler : IRequestHandler<GetAllKnjigeQuery, IEnumerable<KnjigaDto>>
    {
        private readonly IKnjigaRepository _knjigaRepository;
        public GetAllKnjigeQueryHandler(IKnjigaRepository knjigaRepository)
            => _knjigaRepository = knjigaRepository;

        public Task<IEnumerable<KnjigaDto>> Handle(GetAllKnjigeQuery request, CancellationToken ct)
        {
            var result = _knjigaRepository.GetAll().Select(k => new KnjigaDto
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
            return Task.FromResult(result);
        }
    }

    public record GetKnjigaByIdQuery(int Id) : IRequest<KnjigaDto?>;

    public class GetKnjigaByIdQueryHandler : IRequestHandler<GetKnjigaByIdQuery, KnjigaDto?>
    {
        private readonly IKnjigaRepository _knjigaRepository;
        public GetKnjigaByIdQueryHandler(IKnjigaRepository knjigaRepository)
            => _knjigaRepository = knjigaRepository;

        public Task<KnjigaDto?> Handle(GetKnjigaByIdQuery request, CancellationToken ct)
        {
            var k = _knjigaRepository.GetByIdWithAuthors(request.Id);
            if (k == null) return Task.FromResult<KnjigaDto?>(null);
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

    public record SearchKnjigeQuery(string Naslov) : IRequest<IEnumerable<KnjigaDto>>;

    public class SearchKnjigeQueryHandler : IRequestHandler<SearchKnjigeQuery, IEnumerable<KnjigaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public SearchKnjigeQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IEnumerable<KnjigaDto>> Handle(SearchKnjigeQuery request, CancellationToken ct)
        {
            var result = _unitOfWork.Knjige.Find(k => k.Naslov.Contains(request.Naslov)).Select(k => new KnjigaDto
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
            return Task.FromResult(result);
        }
    }

    public record GetAutoriKnjigeQuery(int KnjigaId) : IRequest<IEnumerable<AutorDto>>;

    public class GetAutoriKnjigeQueryHandler : IRequestHandler<GetAutoriKnjigeQuery, IEnumerable<AutorDto>>
    {
        private readonly IKnjigaRepository _knjigaRepository;
        public GetAutoriKnjigeQueryHandler(IKnjigaRepository knjigaRepository)
            => _knjigaRepository = knjigaRepository;

        public Task<IEnumerable<AutorDto>> Handle(GetAutoriKnjigeQuery request, CancellationToken ct)
        {
            var knjiga = _knjigaRepository.GetByIdWithAuthors(request.KnjigaId);
            if (knjiga == null) return Task.FromResult(Enumerable.Empty<AutorDto>());

            var result = (knjiga.Pisanja ?? new List<Pisanje>())
                .Where(p => p.Autor != null)
                .Select(p => new AutorDto
                {
                    AutorId = p.Autor!.AutorId,
                    Ime = p.Autor.Ime,
                    Prezime = p.Autor.Prezime,
                    Biografija = p.Autor.Biografija
                });
            return Task.FromResult(result);
        }
    }
}