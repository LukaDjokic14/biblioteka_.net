using Biblioteka.API.DTOs.Izdavanja;
using Biblioteka.API.Features.Izdavanja;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IzdavanjaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<RezervacijaRequest> _rezervacijaValidator;
        private readonly IValidator<OdobriIzdavanjeRequest> _odobriValidator;

        public IzdavanjaController(IMediator mediator,
            IValidator<RezervacijaRequest> rezervacijaValidator,
            IValidator<OdobriIzdavanjeRequest> odobriValidator)
        { _mediator = mediator; _rezervacijaValidator = rezervacijaValidator; _odobriValidator = odobriValidator; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<IzdavanjeDto>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllIzdavanjaQuery());
            return Ok(result);
        }

        [HttpPost("rezervisi")]
        [Authorize(Roles = "Clan,Bibliotekar")]
        public async Task<ActionResult<IzdavanjeDto>> Rezervisi([FromBody] RezervacijaRequest request)
        {
            var validationResult = _rezervacijaValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new RezervisiCommand(
                request.ClanJmbg, request.NasloviKnjiga, request.Napomena ?? ""));

            if (result == null) return BadRequest("Nepostojeci clan ili knjiga nije pronađena.");
            return Ok(result);
        }

        [HttpPut("{id}/odobri")]
        [Authorize(Roles = "Bibliotekar")]
        public async Task<ActionResult<IzdavanjeDto>> Odobri(int id, [FromBody] OdobriIzdavanjeRequest request)
        {
            var validationResult = _odobriValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new OdobriIzdavanjeCommand(id, request.BibliotekarJmbg));
            if (result == null) return BadRequest("Izdavanje ne postoji, status nije REZERVISANO ili bibliotekar nije pronađen.");
            return Ok(result);
        }

        [HttpPut("{id}/vrati")]
        [Authorize(Roles = "Clan")]
        public async Task<ActionResult<IzdavanjeDto>> Vrati(int id)
        {
            // Handler nema pristup HttpContext-u pa JMBG proslijedimo iz kontrolera
            var ulogovaniJmbg = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _mediator.Send(new VratiKnjiguCommand(id, ulogovaniJmbg!));

            if (result.IsNotFound) return NotFound(result.Error);
            if (result.IsForbidden) return StatusCode(403, result.Error);
            if (result.IsBadRequest) return BadRequest(result.Error);
            return Ok(result.Dto);
        }
    }
}