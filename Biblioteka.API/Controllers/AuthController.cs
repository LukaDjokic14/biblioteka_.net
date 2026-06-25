using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Biblioteka.API.DTOs.Auth;
using Biblioteka.API.Service;
using Biblioteka.Domain.Repozitorijumi;

namespace Biblioteka.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _tokenService;
        private readonly IValidator<LoginRequest> _loginValidator;

        public AuthController(
            IUnitOfWork unitOfWork,
            JwtTokenService tokenService,
            IValidator<LoginRequest> loginValidator)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _loginValidator = loginValidator;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"Pokusaj logina: {request.KorisnickoIme}, Lozinka: {request.Lozinka}");
            var validationResult = _loginValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var bibliotekar = _unitOfWork.Bibliotekari.Find(b => b.KorisnickoIme == request.KorisnickoIme).FirstOrDefault();
            if (bibliotekar != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Lozinka, bibliotekar.Lozinka))
                    return Unauthorized("Pogrešna lozinka.");
                var token = _tokenService.CreateToken(bibliotekar.KorisnickoIme, "Bibliotekar", bibliotekar.Jmbg);
                return Ok(new { token, uloga = "Bibliotekar", jmbg = bibliotekar.Jmbg, ime = bibliotekar.Ime, prezime = bibliotekar.Prezime, korisnickoIme = bibliotekar.KorisnickoIme });
            }

            var clan = _unitOfWork.Clanovi.Find(c => c.KorisnickoIme == request.KorisnickoIme).FirstOrDefault();
            if (clan != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Lozinka, clan.Lozinka))
                    return Unauthorized("Pogrešna lozinka.");
                var token = _tokenService.CreateToken(clan.KorisnickoIme, "Clan", clan.Jmbg);
                var grad = _unitOfWork.Gradovi.GetById(clan.GradId);
                return Ok(new { token, uloga = "Clan", jmbg = clan.Jmbg, ime = clan.Ime, prezime = clan.Prezime, korisnickoIme = clan.KorisnickoIme, brojTelefona = clan.BrojTelefona, gradNaziv = grad?.Naziv ?? "" });
            }

            return Unauthorized("Korisnik nije pronađen.");
        }
    }
}