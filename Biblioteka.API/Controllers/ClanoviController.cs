using Biblioteka.API.DTOs.Clanovi;
using Biblioteka.API.Features.Clanovi;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class ClanoviController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateClanRequest> _createValidator;
        private readonly IValidator<UpdateClanRequest> _updateValidator;

        public ClanoviController(IMediator mediator,
            IValidator<CreateClanRequest> createValidator,
            IValidator<UpdateClanRequest> updateValidator)
        { _mediator = mediator; _createValidator = createValidator; _updateValidator = updateValidator; }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ClanDto>> Create([FromBody] CreateClanRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new CreateClanCommand(
                request.Jmbg, request.Ime, request.Prezime, request.BrojTelefona,
                request.KorisnickoIme, request.Lozinka, request.GradNaziv));

            if (result == null) return BadRequest("Član već postoji ili grad nije pronađen.");
            return CreatedAtAction(nameof(GetById), new { jmbg = result.Jmbg }, result);
        }

        [HttpGet("{jmbg}")]
        public async Task<ActionResult<ClanDto>> GetById(string jmbg)
        {
            var result = await _mediator.Send(new GetClanByIdQuery(jmbg));
            if (result == null) return NotFound("Traženi član ne postoji.");
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClanDto>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllClanoviQuery());
            return Ok(result);
        }

        [HttpGet("grad/{gradId}")]
        public async Task<ActionResult<IEnumerable<ClanDto>>> GetByGrad(int gradId)
        {
            var result = await _mediator.Send(new GetClanByGradQuery(gradId));
            return Ok(result);
        }

        [HttpPut("{jmbg}")]
        public async Task<ActionResult<ClanDto>> Update(string jmbg, [FromBody] UpdateClanRequest request)
        {
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new UpdateClanCommand(
                jmbg, request.Ime, request.Prezime, request.BrojTelefona,
                request.Lozinka, request.GradNaziv));

            if (result == null) return NotFound("Traženi član ne postoji ili grad nije pronađen.");
            return Ok(result);
        }

        [HttpDelete("{jmbg}")]
        public async Task<IActionResult> Delete(string jmbg)
        {
            var uspjeh = await _mediator.Send(new DeleteClanCommand(jmbg));
            if (!uspjeh)
                return BadRequest("Nije moguće obrisati člana jer trenutno ima nekompletirana izdavanja.");
            return NoContent();
        }
    }
}