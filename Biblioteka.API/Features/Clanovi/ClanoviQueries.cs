using Biblioteka.API.DTOs.Clanovi;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Clanovi
{
    public record GetAllClanoviQuery() : IRequest<IEnumerable<ClanDto>>;

    public class GetAllClanoviQueryHandler : IRequestHandler<GetAllClanoviQuery, IEnumerable<ClanDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllClanoviQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IEnumerable<ClanDto>> Handle(GetAllClanoviQuery request, CancellationToken ct)
        {
            var result = _unitOfWork.Clanovi.GetAll().Select(c =>
            {
                var grad = _unitOfWork.Gradovi.GetById(c.GradId);
                return new ClanDto
                {
                    Jmbg = c.Jmbg,
                    Ime = c.Ime,
                    Prezime = c.Prezime,
                    BrojTelefona = c.BrojTelefona,
                    KorisnickoIme = c.KorisnickoIme,
                    GradNaziv = grad?.Naziv ?? "Nepoznato"
                };
            });
            return Task.FromResult(result);
        }
    }

    public record GetClanByIdQuery(string Jmbg) : IRequest<ClanDto?>;

    public class GetClanByIdQueryHandler : IRequestHandler<GetClanByIdQuery, ClanDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetClanByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<ClanDto?> Handle(GetClanByIdQuery request, CancellationToken ct)
        {
            var c = _unitOfWork.Clanovi.GetById(request.Jmbg);
            if (c == null) return Task.FromResult<ClanDto?>(null);
            var grad = _unitOfWork.Gradovi.GetById(c.GradId);
            return Task.FromResult<ClanDto?>(new ClanDto
            {
                Jmbg = c.Jmbg,
                Ime = c.Ime,
                Prezime = c.Prezime,
                BrojTelefona = c.BrojTelefona,
                KorisnickoIme = c.KorisnickoIme,
                GradNaziv = grad?.Naziv ?? "Nepoznato"
            });
        }
    }

    public record GetClanByGradQuery(int GradId) : IRequest<IEnumerable<ClanDto>>;

    public class GetClanByGradQueryHandler : IRequestHandler<GetClanByGradQuery, IEnumerable<ClanDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetClanByGradQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IEnumerable<ClanDto>> Handle(GetClanByGradQuery request, CancellationToken ct)
        {
            var grad = _unitOfWork.Gradovi.GetById(request.GradId);
            if (grad == null) return Task.FromResult(Enumerable.Empty<ClanDto>());

            var result = _unitOfWork.Clanovi.GetByGrad(request.GradId).Select(c => new ClanDto
            {
                Jmbg = c.Jmbg,
                Ime = c.Ime,
                Prezime = c.Prezime,
                BrojTelefona = c.BrojTelefona,
                KorisnickoIme = c.KorisnickoIme,
                GradNaziv = grad.Naziv
            });
            return Task.FromResult(result);
        }
    }
}