using Biblioteka.API.DTOs.Bibliotekari;
using Biblioteka.API.Features.Bibliotekari;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class BibliotekariController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateBibliotekarRequest> _createValidator;
        private readonly IValidator<UpdateBibliotekarRequest> _updateValidator;

        public BibliotekariController(IMediator mediator,
            IValidator<CreateBibliotekarRequest> createValidator,
            IValidator<UpdateBibliotekarRequest> updateValidator)
        { _mediator = mediator; _createValidator = createValidator; _updateValidator = updateValidator; }

        [HttpPost]
        public async Task<ActionResult<BibliotekarDto>> Create([FromBody] CreateBibliotekarRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new CreateBibliotekarCommand(
                request.Jmbg, request.Ime, request.Prezime, request.KorisnickoIme, request.Lozinka));

            if (result == null) return BadRequest("Bibliotekar sa ovim JMBG-om već postoji.");
            return CreatedAtAction(nameof(GetById), new { jmbg = result.Jmbg }, result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BibliotekarDto>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllBibliotekariQuery());
            return Ok(result);
        }

        [HttpGet("{jmbg}")]
        [AllowAnonymous]
        public async Task<ActionResult<BibliotekarDto>> GetById(string jmbg)
        {
            var result = await _mediator.Send(new GetBibliotekarByIdQuery(jmbg));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{jmbg}")]
        public async Task<ActionResult<BibliotekarDto>> Update(string jmbg, [FromBody] UpdateBibliotekarRequest request)
        {
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new UpdateBibliotekarCommand(
                jmbg, request.Prezime, request.KorisnickoIme, request.NovaLozinka));

            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{jmbg}")]
        public async Task<IActionResult> Delete(string jmbg)
        {
            var uspjeh = await _mediator.Send(new DeleteBibliotekarCommand(jmbg));
            if (!uspjeh) return NotFound();
            return Ok("Uspesno obrisan bibliotekar!");
        }
    }
}