using Biblioteka.API.DTOs.Clanovi;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class ClanoviController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClanRepository _clanRepository;
        private readonly IValidator<CreateClanRequest> _createValidator;
        private readonly IValidator<UpdateClanRequest> _updateValidator;

        public ClanoviController(
            IUnitOfWork unitOfWork,
            IClanRepository clanRepository,
            IValidator<CreateClanRequest> createValidator,
            IValidator<UpdateClanRequest> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _clanRepository = clanRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // 1. CREATE
        [HttpPost]
        [AllowAnonymous]
        public ActionResult<ClanDto> Create([FromBody] CreateClanRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var grad = _unitOfWork.Gradovi.Find(g => g.Naziv == request.GradNaziv).FirstOrDefault();
            if (grad == null) return BadRequest($"Grad sa nazivom '{request.GradNaziv}' nije pronađen.");

            var postojeciClan = _unitOfWork.Clanovi.GetById(request.Jmbg);
            if (postojeciClan != null) return BadRequest("Član sa ovim JMBG-om već postoji u sistemu.");

            var clan = new Clan
            {
                Jmbg = request.Jmbg,
                Ime = request.Ime,
                Prezime = request.Prezime,
                BrojTelefona = request.BrojTelefona,
                KorisnickoIme = request.KorisnickoIme,
                Lozinka = BCrypt.Net.BCrypt.HashPassword(request.Lozinka),
                GradId = grad.GradId
            };

            _unitOfWork.Clanovi.Add(clan);
            _unitOfWork.SaveChanges();

            var dto = new ClanDto { Jmbg = clan.Jmbg, Ime = clan.Ime, Prezime = clan.Prezime, BrojTelefona = clan.BrojTelefona, KorisnickoIme = clan.KorisnickoIme, GradNaziv = grad.Naziv };
            return CreatedAtAction(nameof(GetById), new { jmbg = dto.Jmbg }, dto);
        }

        // 2. READ
        [HttpGet("{jmbg}")]
        public ActionResult<ClanDto> GetById(string jmbg)
        {
            var clan = _unitOfWork.Clanovi.GetById(jmbg);
            if (clan == null) return NotFound("Traženi član ne postoji.");

            var grad = _unitOfWork.Gradovi.GetById(clan.GradId);
            return Ok(new ClanDto { Jmbg = clan.Jmbg, Ime = clan.Ime, Prezime = clan.Prezime, BrojTelefona = clan.BrojTelefona, KorisnickoIme = clan.KorisnickoIme, GradNaziv = grad?.Naziv ?? "Nepoznato" });
        }

        // 3. READ (Get All)
        [HttpGet]
        public ActionResult<IEnumerable<ClanDto>> GetAll()
        {
            var clanovi = _unitOfWork.Clanovi.GetAll();
            var dtos = clanovi.Select(clan =>
            {
                var grad = _unitOfWork.Gradovi.GetById(clan.GradId);
                return new ClanDto { Jmbg = clan.Jmbg, Ime = clan.Ime, Prezime = clan.Prezime, BrojTelefona = clan.BrojTelefona, KorisnickoIme = clan.KorisnickoIme, GradNaziv = grad?.Naziv ?? "Nepoznato" };
            }).ToList();

            return Ok(dtos);
        }

        // 4. READ (Get By Grad)
        [HttpGet("grad/{gradId}")]
        public ActionResult<IEnumerable<ClanDto>> GetByGrad(int gradId)
        {
            var clanovi = _clanRepository.GetByGrad(gradId);
            var grad = _unitOfWork.Gradovi.GetById(gradId);

            if (grad == null) return NotFound("Traženi grad ne postoji.");

            var dtos = clanovi.Select(clan => new ClanDto
            {
                Jmbg = clan.Jmbg,
                Ime = clan.Ime,
                Prezime = clan.Prezime,
                BrojTelefona = clan.BrojTelefona,
                KorisnickoIme = clan.KorisnickoIme,
                GradNaziv = grad.Naziv
            }).ToList();

            return Ok(dtos);
        }

        // 5. UPDATE
        [HttpPut("{jmbg}")]
        public ActionResult<ClanDto> Update(string jmbg, [FromBody] UpdateClanRequest request)
        {

            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var clan = _unitOfWork.Clanovi.GetById(jmbg);
            if (clan == null) return NotFound("Traženi član ne postoji.");

            if (!string.IsNullOrWhiteSpace(request.Ime)) clan.Ime = request.Ime;
            if (!string.IsNullOrWhiteSpace(request.Prezime)) clan.Prezime = request.Prezime;
            if (!string.IsNullOrWhiteSpace(request.BrojTelefona)) clan.BrojTelefona = request.BrojTelefona;

            if (!string.IsNullOrWhiteSpace(request.Lozinka))
                clan.Lozinka = BCrypt.Net.BCrypt.HashPassword(request.Lozinka);

            if (!string.IsNullOrWhiteSpace(request.GradNaziv))
            {
                var grad = _unitOfWork.Gradovi.Find(g => g.Naziv == request.GradNaziv).FirstOrDefault();
                if (grad == null) return BadRequest($"Grad sa nazivom '{request.GradNaziv}' nije pronađen.");
                clan.GradId = grad.GradId;
            }

            _unitOfWork.SaveChanges();

            var azuriraniGrad = _unitOfWork.Gradovi.GetById(clan.GradId);
            return Ok(new ClanDto { Jmbg = clan.Jmbg, Ime = clan.Ime, Prezime = clan.Prezime, BrojTelefona = clan.BrojTelefona, KorisnickoIme = clan.KorisnickoIme, GradNaziv = azuriraniGrad?.Naziv ?? "Nepoznato" });
        }

        // 6. DELETE
        [HttpDelete("{jmbg}")]
        public IActionResult Delete(string jmbg)
        {
            var clan = _unitOfWork.Clanovi.GetById(jmbg);
            if (clan == null) return NotFound("Traženi član ne postoji.");

            var imaAktivnaIzdavanja = _unitOfWork.Izdavanja
                .Find(i => i.ClanJmbg == jmbg && (i.Status == "IZDATO" || i.Status == "REZERVISANO"))
                .Any();

            if (imaAktivnaIzdavanja)
            {
                return BadRequest("Nije moguće obrisati člana jer trenutno ima nekompletirana izdavanja.");
            }

            _unitOfWork.Clanovi.Remove(clan);
            _unitOfWork.SaveChanges();

            return NoContent();
        }
    }
}