using Biblioteka.API.DTOs.Gradovi;
using Biblioteka.API.Features.Gradovi;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GradoviController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateGradRequest> _createValidator;
        private readonly IValidator<UpdateGradRequest> _updateValidator;

        public GradoviController(IMediator mediator,
            IValidator<CreateGradRequest> createValidator,
            IValidator<UpdateGradRequest> updateValidator)
        { _mediator = mediator; _createValidator = createValidator; _updateValidator = updateValidator; }

        [HttpPost]
        public async Task<ActionResult<GradDto>> Create([FromBody] CreateGradRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new CreateGradCommand(request.Naziv));
            if (result == null) return BadRequest($"Grad sa nazivom '{request.Naziv}' već postoji.");
            return CreatedAtAction(nameof(GetById), new { id = result.GradId }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<GradDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetGradByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<GradDto>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllGradoviQuery());
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Bibliotekar")]
        public async Task<ActionResult<GradDto>> Update(int id, [FromBody] UpdateGradRequest request)
        {
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var result = await _mediator.Send(new UpdateGradCommand(id, request.Naziv));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Bibliotekar")]
        public async Task<IActionResult> Delete(int id)
        {
            var uspjeh = await _mediator.Send(new DeleteGradCommand(id));
            if (!uspjeh)
                return BadRequest("Nije moguće obrisati grad jer postoje članovi na ovoj lokaciji.");
            return Ok("Grad uspesno obrisan!");
        }
    }
}