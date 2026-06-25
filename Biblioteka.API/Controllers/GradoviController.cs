using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Biblioteka.API.DTOs.Gradovi;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GradoviController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateGradRequest> _createValidator;
        private readonly IValidator<UpdateGradRequest> _updateValidator;

        public GradoviController(
            IUnitOfWork unitOfWork,
            IValidator<CreateGradRequest> createValidator,
            IValidator<UpdateGradRequest> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpPost]
        public ActionResult<GradDto> Create([FromBody] CreateGradRequest request)
        {
            // 1. Fluent Validacija
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }


            var postojeciGrad = _unitOfWork.Gradovi.Find(g => g.Naziv == request.Naziv).FirstOrDefault();
            if (postojeciGrad != null)
                return BadRequest($"Grad sa nazivom '{request.Naziv}' već postoji u sistemu.");

            var grad = new Grad { Naziv = request.Naziv };
            _unitOfWork.Gradovi.Add(grad);
            _unitOfWork.SaveChanges();

            var dto = new GradDto { GradId = grad.GradId, Naziv = grad.Naziv };
            return CreatedAtAction(nameof(GetById), new { id = dto.GradId }, dto);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<GradDto> GetById(int id)
        {
            var grad = _unitOfWork.Gradovi.GetById(id);
            if (grad == null) return NotFound();
            return Ok(new GradDto { GradId = grad.GradId, Naziv = grad.Naziv });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<GradDto>> GetAll()
        {
            var gradovi = _unitOfWork.Gradovi.GetAll();
            var results = gradovi.Select(g => new GradDto { GradId = g.GradId, Naziv = g.Naziv }).ToList();
            return Ok(results);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Bibliotekar")]
        public ActionResult<GradDto> Update(int id, [FromBody] UpdateGradRequest request)
        {
            // 1. Fluent Validacija
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var grad = _unitOfWork.Gradovi.GetById(id);
            if (grad == null) return NotFound();

            grad.Naziv = request.Naziv;
            _unitOfWork.SaveChanges();

            return Ok(new GradDto { GradId = grad.GradId, Naziv = grad.Naziv });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Bibliotekar")]
        public IActionResult Delete(int id)
        {
            var grad = _unitOfWork.Gradovi.GetById(id);
            if (grad == null) return NotFound();

            // PROVERA INTEGRITETA: Da li postoje članovi iz ovog grada?
            var imaClanova = _unitOfWork.Clanovi.Find(c => c.GradId == id).Any();
            if (imaClanova)
            {
                return BadRequest("Nije moguće obrisati grad jer u sistemu postoje članovi koji su prijavljeni na ovoj lokaciji.");
            }

            _unitOfWork.Gradovi.Remove(grad);
            _unitOfWork.SaveChanges();

            return Ok("Grad uspesno obrisan!");
        }
    }
}