using Biblioteka.API.DTOs.Autori;
using Biblioteka.API.Features.Autori;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class AutoriController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateAutorRequest> _createValidator;
        private readonly IValidator<UpdateAutorRequest> _updateValidator;

        public AutoriController(
            IMediator mediator,
            IValidator<CreateAutorRequest> createValidator,
            IValidator<UpdateAutorRequest> updateValidator)
        {
            _mediator = mediator;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // 1. CREATE
        [HttpPost]
        public async Task<ActionResult<AutorDto>> Create([FromBody] CreateAutorRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var command = new CreateAutorCommand(request.Ime, request.Prezime, request.Biografija);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.AutorId }, result);
        }

        // 2. READ ALL
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AutorDto>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllAutoriQuery());
            return Ok(result);
        }

        // 3. READ BY ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AutorDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetAutorByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        // 4. UPDATE
        [HttpPut("{id}")]
        public async Task<ActionResult<AutorDto>> Update(int id, [FromBody] UpdateAutorRequest request)
        {
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var command = new UpdateAutorCommand(id, request.Ime, request.Prezime, request.Biografija);
            var result = await _mediator.Send(command);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // 5. DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uspjeh = await _mediator.Send(new DeleteAutorCommand(id));
            if (!uspjeh)
                return BadRequest("Nije moguće obrisati autora jer već ima povezane knjige u bazi. Prvo morate obrisati njegove knjige.");
            return NoContent();
        }
    }
}