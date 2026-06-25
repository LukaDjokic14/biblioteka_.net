using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Biblioteka.API.DTOs.Autori;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using System.Linq;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class AutoriController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateAutorRequest> _createValidator;
        private readonly IValidator<UpdateAutorRequest> _updateValidator;

        public AutoriController(
            IUnitOfWork unitOfWork,
            IValidator<CreateAutorRequest> createValidator,
            IValidator<UpdateAutorRequest> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // 1. CREATE
        [HttpPost]
        public ActionResult<AutorDto> Create([FromBody] CreateAutorRequest request)
        {
            // Fluent Validacija
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var autor = new Autor
            {
                Ime = request.Ime,
                Prezime = request.Prezime,
                Biografija = request.Biografija
            };

            _unitOfWork.Autori.Add(autor);
            _unitOfWork.SaveChanges();

            var dto = new AutorDto
            {
                AutorId = autor.AutorId,
                Ime = autor.Ime,
                Prezime = autor.Prezime,
                Biografija = autor.Biografija
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.AutorId }, dto);
        }

        // 2. READ ALL
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<AutorDto>> GetAll()
        {
            var autori = _unitOfWork.Autori.GetAll();
            var results = autori.Select(a => new AutorDto
            {
                AutorId = a.AutorId,
                Ime = a.Ime,
                Prezime = a.Prezime,
                Biografija = a.Biografija
            }).ToList();

            return Ok(results);
        }

        // 3. READ BY ID 
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<AutorDto> GetById(int id)
        {
            var autor = _unitOfWork.Autori.GetById(id);
            if (autor == null) return NotFound();

            var dto = new AutorDto
            {
                AutorId = autor.AutorId,
                Ime = autor.Ime,
                Prezime = autor.Prezime,
                Biografija = autor.Biografija
            };

            return Ok(dto);
        }

        // 4. UPDATE
        [HttpPut("{id}")]
        public ActionResult<AutorDto> Update(int id, [FromBody] UpdateAutorRequest request)
        {
            // Fluent Validacija
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var autor = _unitOfWork.Autori.GetById(id);
            if (autor == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Ime))
                autor.Ime = request.Ime;

            if (!string.IsNullOrWhiteSpace(request.Prezime))
                autor.Prezime = request.Prezime;

            if (!string.IsNullOrWhiteSpace(request.Biografija))
                autor.Biografija = request.Biografija;

            _unitOfWork.SaveChanges();

            var izmenjeniDto = new AutorDto
            {
                AutorId = autor.AutorId,
                Ime = autor.Ime,
                Prezime = autor.Prezime,
                Biografija = autor.Biografija
            };

            return Ok(izmenjeniDto);
        }

        // 5. DELETE 
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var autor = _unitOfWork.Autori.GetById(id);
            if (autor == null) return NotFound();

            // Biznis logika ostaje netaknuta
            var imaKnjige = _unitOfWork.Pisanja.Find(p => p.Autor.AutorId == id).Any();

            if (imaKnjige)
            {
                return BadRequest("Nije moguće obrisati autora jer već ima povezane knjige u bazi. Prvo morate obrisati njegove knjige.");
            }

            _unitOfWork.Autori.Remove(autor);
            _unitOfWork.SaveChanges();

            return NoContent();
        }
    }
}

