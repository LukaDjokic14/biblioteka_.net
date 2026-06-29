using Biblioteka.API.DTOs.Bibliotekari;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Bibliotekari
{
    public record GetAllBibliotekariQuery() : IRequest<IEnumerable<BibliotekarDto>>;

    public class GetAllBibliotekariQueryHandler
        : IRequestHandler<GetAllBibliotekariQuery, IEnumerable<BibliotekarDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllBibliotekariQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IEnumerable<BibliotekarDto>> Handle(GetAllBibliotekariQuery request, CancellationToken ct)
        {
            var result = _unitOfWork.Bibliotekari.GetAll().Select(b => new BibliotekarDto
            {
                Jmbg = b.Jmbg,
                Ime = b.Ime,
                Prezime = b.Prezime,
                KorisnickoIme = b.KorisnickoIme
            });
            return Task.FromResult(result);
        }
    }

    public record GetBibliotekarByIdQuery(string Jmbg) : IRequest<BibliotekarDto?>;

    public class GetBibliotekarByIdQueryHandler
        : IRequestHandler<GetBibliotekarByIdQuery, BibliotekarDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetBibliotekarByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<BibliotekarDto?> Handle(GetBibliotekarByIdQuery request, CancellationToken ct)
        {
            var b = _unitOfWork.Bibliotekari.GetById(request.Jmbg);
            if (b == null) return Task.FromResult<BibliotekarDto?>(null);
            return Task.FromResult<BibliotekarDto?>(new BibliotekarDto
            {
                Jmbg = b.Jmbg,
                Ime = b.Ime,
                Prezime = b.Prezime,
                KorisnickoIme = b.KorisnickoIme
            });
        }
    }
}
