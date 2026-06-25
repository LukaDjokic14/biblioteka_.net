using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Biblioteka.API.DTOs.Knjige;
using Biblioteka.API.DTOs.Autori;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Bibliotekar")]
    public class KnjigeController : ControllerBase
    {
        private readonly IKnjigaRepository _knjigaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateKnjigaRequest> _createValidator;
        private readonly IValidator<UpdateKnjigaRequest> _updateValidator;

        public KnjigeController(
            IKnjigaRepository knjigaRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateKnjigaRequest> createValidator,
            IValidator<UpdateKnjigaRequest> updateValidator)
        {
            _knjigaRepository = knjigaRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // 1. CREATE
        [HttpPost]
        public ActionResult<KnjigaDto> Create([FromBody] CreateKnjigaRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            if (!Enum.TryParse<Zanr>(request.Zanr, true, out var zanrEnum))
                return BadRequest($"Nevalidan zanr: {request.Zanr}");

            var knjiga = new Knjiga
            {
                Naslov = request.Naslov,
                GodinaIzdanja = request.GodinaIzdanja,
                Isbn = request.Isbn,
                Slika = request.Slika,
                BrojStrana = request.BrojStrana,
                Opis = request.Opis,
                Zanr = zanrEnum
            };
            _unitOfWork.Knjige.Add(knjiga);

            var autor = _unitOfWork.Autori
                .Find(a => a.Ime == request.AutorIme && a.Prezime == request.AutorPrezime)
                .FirstOrDefault();
            if (autor == null)
            {
                autor = new Autor { Ime = request.AutorIme, Prezime = request.AutorPrezime, Biografija = request.AutorBiografija };
                _unitOfWork.Autori.Add(autor);
            }

            _unitOfWork.Pisanja.Add(new Pisanje { Knjiga = knjiga, Autor = autor });
            _unitOfWork.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = knjiga.KnjigaId }, new KnjigaDto
            {
                KnjigaId = knjiga.KnjigaId,
                Naslov = knjiga.Naslov,
                GodinaIzdanja = knjiga.GodinaIzdanja,
                Isbn = knjiga.Isbn,
                Slika = knjiga.Slika,
                BrojStrana = knjiga.BrojStrana,
                Opis = knjiga.Opis,
                Zanr = knjiga.Zanr.ToString()
            });
        }

        // 2. READ BY ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<KnjigaDto> GetById(int id)
        {
            var knjiga = _knjigaRepository.GetByIdWithAuthors(id);
            if (knjiga == null) return NotFound();

            return Ok(new KnjigaDto
            {
                KnjigaId = knjiga.KnjigaId,
                Naslov = knjiga.Naslov,
                GodinaIzdanja = knjiga.GodinaIzdanja,
                Isbn = knjiga.Isbn,
                Slika = knjiga.Slika,
                BrojStrana = knjiga.BrojStrana,
                Opis = knjiga.Opis,
                Zanr = knjiga.Zanr.ToString()
            });
        }

        // 3. READ ALL
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<KnjigaDto>> GetAll()
        {
            var knjige = _knjigaRepository.GetAll();
            return Ok(knjige.Select(k => new KnjigaDto
            {
                KnjigaId = k.KnjigaId,
                Naslov = k.Naslov,
                GodinaIzdanja = k.GodinaIzdanja,
                Isbn = k.Isbn,
                Slika = k.Slika,
                BrojStrana = k.BrojStrana,
                Opis = k.Opis,
                Zanr = k.Zanr.ToString()
            }).ToList());
        }

        // 4. PRETRAGA
        [HttpGet("search/{naslov}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<KnjigaDto>> Search(string naslov)
        {
            var knjige = _unitOfWork.Knjige.Find(k => k.Naslov.Contains(naslov));
            return Ok(knjige.Select(k => new KnjigaDto
            {
                KnjigaId = k.KnjigaId,
                Naslov = k.Naslov,
                GodinaIzdanja = k.GodinaIzdanja,
                Isbn = k.Isbn,
                Slika = k.Slika,
                BrojStrana = k.BrojStrana,
                Opis = k.Opis,
                Zanr = k.Zanr.ToString()
            }).ToList());
        }

        // 5. AUTORI KNJIGE
        [HttpGet("{id}/autori")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<AutorDto>> GetAutori(int id)
        {
            var knjiga = _knjigaRepository.GetByIdWithAuthors(id);
            if (knjiga == null) return NotFound();

            var autori = (knjiga.Pisanja ?? new List<Pisanje>())
                .Where(p => p.Autor != null)
                .Select(p => new AutorDto
                {
                    AutorId = p.Autor!.AutorId,
                    Ime = p.Autor.Ime,
                    Prezime = p.Autor.Prezime,
                    Biografija = p.Autor.Biografija
                })
                .ToList();

            return Ok(autori);
        }

        // 6. UPDATE
        [HttpPut("{id}")]
        public ActionResult<KnjigaDto> Update(int id, [FromBody] UpdateKnjigaRequest request)
        {
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var knjiga = _knjigaRepository.GetById(id);
            if (knjiga == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Naslov)) knjiga.Naslov = request.Naslov;
            if (request.GodinaIzdanja.HasValue) knjiga.GodinaIzdanja = request.GodinaIzdanja.Value;
            if (!string.IsNullOrWhiteSpace(request.Isbn)) knjiga.Isbn = request.Isbn;
            if (!string.IsNullOrWhiteSpace(request.Slika)) knjiga.Slika = request.Slika;
            if (request.BrojStrana.HasValue) knjiga.BrojStrana = request.BrojStrana.Value;
            if (!string.IsNullOrWhiteSpace(request.Opis)) knjiga.Opis = request.Opis;

            if (!string.IsNullOrWhiteSpace(request.Zanr))
            {
                if (Enum.TryParse<Zanr>(request.Zanr, true, out var noviZanr))
                    knjiga.Zanr = noviZanr;
            }

            _unitOfWork.SaveChanges();

            return Ok(new KnjigaDto
            {
                KnjigaId = knjiga.KnjigaId,
                Naslov = knjiga.Naslov,
                GodinaIzdanja = knjiga.GodinaIzdanja,
                Isbn = knjiga.Isbn,
                Slika = knjiga.Slika,
                BrojStrana = knjiga.BrojStrana,
                Opis = knjiga.Opis,
                Zanr = knjiga.Zanr.ToString()
            });
        }

        // 7. DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var knjiga = _knjigaRepository.GetById(id);
            if (knjiga == null) return NotFound();

            var knjigaJeZauzeta = _unitOfWork.StavkeIzdavanja
                .Find(s => s.Knjiga.KnjigaId == id && (s.Izdavanje.Status == "IZDATO" || s.Izdavanje.Status == "REZERVISANO"))
                .Any();

            if (knjigaJeZauzeta)
                return BadRequest("Nije moguce obrisati knjigu koja je trenutno izdata ili rezervisana.");

            _unitOfWork.Knjige.Remove(knjiga);
            _unitOfWork.SaveChanges();
            return Ok("Uspesno obrisana knjiga!");
        }
    }
}