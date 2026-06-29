using Biblioteka.API.DTOs.Auth;
using Biblioteka.API.Service;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Auth
{
    public record LoginCommand(string KorisnickoIme, string Lozinka) : IRequest<LoginResultDto>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _tokenService;

        public LoginCommandHandler(IUnitOfWork unitOfWork, JwtTokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public Task<LoginResultDto> Handle(LoginCommand request, CancellationToken ct)
        {
            var bibliotekar = _unitOfWork.Bibliotekari
                .Find(b => b.KorisnickoIme == request.KorisnickoIme)
                .FirstOrDefault();

            if (bibliotekar != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Lozinka, bibliotekar.Lozinka))
                    return Task.FromResult(new LoginResultDto { IsSuccess = false, ErrorMessage = "Pogrešna lozinka." });

                var token = _tokenService.CreateToken(bibliotekar.KorisnickoIme, "Bibliotekar", bibliotekar.Jmbg);
                return Task.FromResult(new LoginResultDto
                {
                    IsSuccess = true,
                    Token = token,
                    Uloga = "Bibliotekar",
                    Jmbg = bibliotekar.Jmbg,
                    Ime = bibliotekar.Ime,
                    Prezime = bibliotekar.Prezime,
                    KorisnickoIme = bibliotekar.KorisnickoIme
                });
            }

            var clan = _unitOfWork.Clanovi
                .Find(c => c.KorisnickoIme == request.KorisnickoIme)
                .FirstOrDefault();

            if (clan != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Lozinka, clan.Lozinka))
                    return Task.FromResult(new LoginResultDto { IsSuccess = false, ErrorMessage = "Pogrešna lozinka." });

                var token = _tokenService.CreateToken(clan.KorisnickoIme, "Clan", clan.Jmbg);
                var grad = _unitOfWork.Gradovi.GetById(clan.GradId);
                return Task.FromResult(new LoginResultDto
                {
                    IsSuccess = true,
                    Token = token,
                    Uloga = "Clan",
                    Jmbg = clan.Jmbg,
                    Ime = clan.Ime,
                    Prezime = clan.Prezime,
                    KorisnickoIme = clan.KorisnickoIme,
                    BrojTelefona = clan.BrojTelefona,
                    GradNaziv = grad?.Naziv ?? ""
                });
            }

            return Task.FromResult(new LoginResultDto { IsSuccess = false, ErrorMessage = "Korisnik nije pronađen." });
        }
    }
}
