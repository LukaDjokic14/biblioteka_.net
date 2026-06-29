using Biblioteka.API.DTOs.Autori;
using Biblioteka.API.DTOs.Knjige;
using Biblioteka.API.Features.Knjige;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class KnjigeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateKnjigaRequest> _createValidator;
        private readonly IValidator<UpdateKnjigaRequest> _updateValidator;

        public KnjigeController(IMediator mediator,
            IValidator<CreateKnjigaRequest> createValidator,
            IValidator<UpdateKnjigaRequest> updateValidator)
        { _mediator = mediator; _createValidator = createValidator; _updateValidator = updateValidator; }

        [HttpPost]
        public async Task<ActionResult<KnjigaDto>> Create([FromBody] CreateKnjigaRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new CreateKnjigaCommand(
                request.Naslov, request.GodinaIzdanja, request.Isbn, request.Slika,
                request.BrojStrana, request.Opis, request.Zanr,
                request.AutorIme, request.AutorPrezime, request.AutorBiografija));

            if (result == null) return BadRequest("Nevalidan žanr.");
            return CreatedAtAction(nameof(GetById), new { id = result.KnjigaId }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<KnjigaDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetKnjigaByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<KnjigaDto>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllKnjigeQuery());
            return Ok(result);
        }

        [HttpGet("search/{naslov}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<KnjigaDto>>> Search(string naslov)
        {
            var result = await _mediator.Send(new SearchKnjigeQuery(naslov));
            return Ok(result);
        }

        [HttpGet("{id}/autori")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AutorDto>>> GetAutori(int id)
        {
            var result = await _mediator.Send(new GetAutoriKnjigeQuery(id));
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<KnjigaDto>> Update(int id, [FromBody] UpdateKnjigaRequest request)
        {
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new UpdateKnjigaCommand(
                id, request.Naslov, request.GodinaIzdanja, request.Isbn,
                request.Slika, request.BrojStrana, request.Opis, request.Zanr));

            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uspjeh = await _mediator.Send(new DeleteKnjigaCommand(id));
            if (!uspjeh)
                return BadRequest("Knjiga ne postoji ili je trenutno izdata/rezervisana.");
            return Ok("Uspesno obrisana knjiga!");
        }
    }
}