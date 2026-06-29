using Biblioteka.API.DTOs.Clanovi;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Clanovi
{
    public record CreateClanCommand(string Jmbg, string Ime, string Prezime, string BrojTelefona,
        string KorisnickoIme, string Lozinka, string GradNaziv) : IRequest<ClanDto?>;

    public class CreateClanCommandHandler : IRequestHandler<CreateClanCommand, ClanDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateClanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<ClanDto?> Handle(CreateClanCommand request, CancellationToken ct)
        {
            var grad = _unitOfWork.Gradovi.Find(g => g.Naziv == request.GradNaziv).FirstOrDefault();
            if (grad == null) return Task.FromResult<ClanDto?>(null);

            if (_unitOfWork.Clanovi.GetById(request.Jmbg) != null)
                return Task.FromResult<ClanDto?>(null);

            var clan = new Clan
            {
                Jmbg = request.Jmbg,
                Ime = request.Ime,
                Prezime = request.Prezime,
                BrojTelefona = request.BrojTelefona,
                KorisnickoIme = request.KorisnickoIme,
                Lozinka = BCrypt.Net.BCrypt.HashPassword(request.Lozinka),
                GradId = grad.GradId
            };
            _unitOfWork.Clanovi.Add(clan);
            _unitOfWork.SaveChanges();

            return Task.FromResult<ClanDto?>(new ClanDto
            {
                Jmbg = clan.Jmbg,
                Ime = clan.Ime,
                Prezime = clan.Prezime,
                BrojTelefona = clan.BrojTelefona,
                KorisnickoIme = clan.KorisnickoIme,
                GradNaziv = grad.Naziv
            });
        }
    }

    public record UpdateClanCommand(string Jmbg, string? Ime, string? Prezime,
        string? BrojTelefona, string? Lozinka, string? GradNaziv) : IRequest<ClanDto?>;

    public class UpdateClanCommandHandler : IRequestHandler<UpdateClanCommand, ClanDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateClanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<ClanDto?> Handle(UpdateClanCommand request, CancellationToken ct)
        {
            var clan = _unitOfWork.Clanovi.GetById(request.Jmbg);
            if (clan == null) return Task.FromResult<ClanDto?>(null);

            if (!string.IsNullOrWhiteSpace(request.Ime)) clan.Ime = request.Ime;
            if (!string.IsNullOrWhiteSpace(request.Prezime)) clan.Prezime = request.Prezime;
            if (!string.IsNullOrWhiteSpace(request.BrojTelefona)) clan.BrojTelefona = request.BrojTelefona;
            if (!string.IsNullOrWhiteSpace(request.Lozinka))
                clan.Lozinka = BCrypt.Net.BCrypt.HashPassword(request.Lozinka);
            if (!string.IsNullOrWhiteSpace(request.GradNaziv))
            {
                var grad = _unitOfWork.Gradovi.Find(g => g.Naziv == request.GradNaziv).FirstOrDefault();
                if (grad == null) return Task.FromResult<ClanDto?>(null);
                clan.GradId = grad.GradId;
            }
            _unitOfWork.SaveChanges();

            var azGrad = _unitOfWork.Gradovi.GetById(clan.GradId);
            return Task.FromResult<ClanDto?>(new ClanDto
            {
                Jmbg = clan.Jmbg,
                Ime = clan.Ime,
                Prezime = clan.Prezime,
                BrojTelefona = clan.BrojTelefona,
                KorisnickoIme = clan.KorisnickoIme,
                GradNaziv = azGrad?.Naziv ?? "Nepoznato"
            });
        }
    }

    public record DeleteClanCommand(string Jmbg) : IRequest<bool>;

    public class DeleteClanCommandHandler : IRequestHandler<DeleteClanCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteClanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<bool> Handle(DeleteClanCommand request, CancellationToken ct)
        {
            var clan = _unitOfWork.Clanovi.GetById(request.Jmbg);
            if (clan == null) return Task.FromResult(false);

            var imaAktivna = _unitOfWork.Izdavanja
                .Find(i => i.ClanJmbg == request.Jmbg && (i.Status == "IZDATO" || i.Status == "REZERVISANO"))
                .Any();
            if (imaAktivna) return Task.FromResult(false);

            _unitOfWork.Clanovi.Remove(clan);
            _unitOfWork.SaveChanges();
            return Task.FromResult(true);
        }
    }
}
