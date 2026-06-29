using Biblioteka.API.DTOs.Auth;
using Biblioteka.API.Features.Auth;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteka.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public AuthController(IMediator mediator, IValidator<LoginRequest> loginValidator)
        {
            _mediator = mediator;
            _loginValidator = loginValidator;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validationResult = _loginValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new LoginCommand(request.KorisnickoIme, request.Lozinka));

            if (!result.IsSuccess)
                return Unauthorized(result.ErrorMessage);

            return Ok(new
            {
                token = result.Token,
                uloga = result.Uloga,
                jmbg = result.Jmbg,
                ime = result.Ime,
                prezime = result.Prezime,
                korisnickoIme = result.KorisnickoIme,
                brojTelefona = result.BrojTelefona,
                gradNaziv = result.GradNaziv
            });
        }
    }
}