using Biblioteka.API.DTOs.Bibliotekari;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class BibliotekariController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateBibliotekarRequest> _createValidator;
        private readonly IValidator<UpdateBibliotekarRequest> _updateValidator;

        public BibliotekariController(
            IUnitOfWork unitOfWork,
            IValidator<CreateBibliotekarRequest> createValidator,
            IValidator<UpdateBibliotekarRequest> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpPost]
        public ActionResult<BibliotekarDto> Create([FromBody] CreateBibliotekarRequest request)
        {
            // 1. Fluent Validation
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            // 2. Provera biznis logike
            if (_unitOfWork.Bibliotekari.GetById(request.Jmbg) != null)
                return BadRequest("Bibliotekar sa ovim JMBG-om već postoji.");

            var bibliotekar = new Bibliotekar
            {
                Jmbg = request.Jmbg,
                Ime = request.Ime,
                Prezime = request.Prezime,
                KorisnickoIme = request.KorisnickoIme,
                Lozinka = BCrypt.Net.BCrypt.HashPassword(request.Lozinka)
            };

            _unitOfWork.Bibliotekari.Add(bibliotekar);
            _unitOfWork.SaveChanges();

            var dto = new BibliotekarDto
            {
                Jmbg = bibliotekar.Jmbg,
                Ime = bibliotekar.Ime,
                Prezime = bibliotekar.Prezime,
                KorisnickoIme = bibliotekar.KorisnickoIme
            };

            return CreatedAtAction(nameof(GetById), new { jmbg = dto.Jmbg }, dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<BibliotekarDto>> GetAll()
        {
            var bibliotekari = _unitOfWork.Bibliotekari.GetAll();
            var results = bibliotekari.Select(b => new BibliotekarDto
            {
                Jmbg = b.Jmbg,
                Ime = b.Ime,
                Prezime = b.Prezime,
                KorisnickoIme = b.KorisnickoIme
            }).ToList();

            return Ok(results);
        }

        [HttpGet("{jmbg}")]
        [AllowAnonymous]
        public ActionResult<BibliotekarDto> GetById(string jmbg)
        {
            var bibliotekar = _unitOfWork.Bibliotekari.GetById(jmbg);
            if (bibliotekar == null) return NotFound();

            var dto = new BibliotekarDto
            {
                Jmbg = bibliotekar.Jmbg,
                Ime = bibliotekar.Ime,
                Prezime = bibliotekar.Prezime,
                KorisnickoIme = bibliotekar.KorisnickoIme
            };

            return Ok(dto);
        }

        [HttpPut("{jmbg}")]
        public ActionResult<BibliotekarDto> Update(string jmbg, [FromBody] UpdateBibliotekarRequest request)
        {
            // 1. Fluent Validation
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var bibliotekar = _unitOfWork.Bibliotekari.GetById(jmbg);
            if (bibliotekar == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Prezime))
                bibliotekar.Prezime = request.Prezime;

            if (!string.IsNullOrWhiteSpace(request.KorisnickoIme))
                bibliotekar.KorisnickoIme = request.KorisnickoIme;

            if (!string.IsNullOrWhiteSpace(request.NovaLozinka))
                bibliotekar.Lozinka = BCrypt.Net.BCrypt.HashPassword(request.NovaLozinka);

            _unitOfWork.SaveChanges();

            return Ok(new BibliotekarDto
            {
                Jmbg = bibliotekar.Jmbg,
                Ime = bibliotekar.Ime,
                Prezime = bibliotekar.Prezime,
                KorisnickoIme = bibliotekar.KorisnickoIme
            });
        }

        [HttpDelete("{jmbg}")]
        public IActionResult Delete(string jmbg)
        {
            var bibliotekar = _unitOfWork.Bibliotekari.GetById(jmbg);
            if (bibliotekar == null) return NotFound();

            _unitOfWork.Bibliotekari.Remove(bibliotekar);
            _unitOfWork.SaveChanges();

            return Ok("Uspesno obrisan bibliotekar!");
        }
    }
}