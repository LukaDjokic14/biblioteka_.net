using Biblioteka.API.DTOs.Izdavanja;
using Biblioteka.API.DTOs.Knjige;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IzdavanjaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<RezervacijaRequest> _rezervacijaValidator;
        private readonly IValidator<OdobriIzdavanjeRequest> _odobriValidator;

        public IzdavanjaController(
            IUnitOfWork unitOfWork,
            IValidator<RezervacijaRequest> rezervacijaValidator,
            IValidator<OdobriIzdavanjeRequest> odobriValidator)
        {
            _unitOfWork = unitOfWork;
            _rezervacijaValidator = rezervacijaValidator;
            _odobriValidator = odobriValidator;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<IzdavanjeDto>> GetAll()
        {
            var svaIzdavanja = _unitOfWork.Izdavanja.GetAll().ToList();
            var result = new List<IzdavanjeDto>();

            foreach (var i in svaIzdavanja)
            {
                var clan = _unitOfWork.Clanovi.GetById(i.ClanJmbg);
                var bibliotekar = !string.IsNullOrEmpty(i.BibliotekarJmbg)
                    ? _unitOfWork.Bibliotekari.GetById(i.BibliotekarJmbg)
                    : null;

                // FIX: Pitamo sa Knjiga strane - SELECT koristi samo direktne property-je
                // (KnjigaId, Naslov, Slika) - bez navigation property-ja - bez NullReferenceException.
                // Predikat k.StavkeIzdavanja.Any(...) prevodi EF Core u SQL (isti pattern kao Delete).
                var knjige = _unitOfWork.Knjige
                    .Find(k => k.StavkeIzdavanja.Any(s => s.Izdavanje.IzdavanjeId == i.IzdavanjeId))
                    .Select(k => new KnjigaDto
                    {
                        KnjigaId = k.KnjigaId,
                        Naslov = k.Naslov,
                        Slika = k.Slika
                    })
                    .ToList();

                result.Add(new IzdavanjeDto
                {
                    IzdavanjeId = i.IzdavanjeId,
                    DatumIzdavanja = i.DatumIzdavanja,
                    DatumVracanja = i.DatumVracanja,
                    Status = i.Status,
                    Napomena = i.Napomena ?? "",
                    BibliotekarImePrezime = bibliotekar != null
                        ? $"{bibliotekar.Ime} {bibliotekar.Prezime}"
                        : "Nije odobreno",
                    ClanJmbg = i.ClanJmbg,
                    ClanIme = clan?.Ime ?? "Nepoznat",
                    ClanPrezime = clan?.Prezime ?? "",
                    Knjige = knjige
                });
            }

            return Ok(result);
        }

        [HttpPost("rezervisi")]
        [Authorize(Roles = "Clan,Bibliotekar")]
        public ActionResult<IzdavanjeDto> Rezervisi([FromBody] RezervacijaRequest request)
        {
            var validationResult = _rezervacijaValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var clan = _unitOfWork.Clanovi.GetById(request.ClanJmbg);
            if (clan == null) return BadRequest("Nepostojeci clan.");

            var izdavanje = new Izdavanje
            {
                DatumIzdavanja = DateTime.Now,
                Status = "REZERVISANO",
                Napomena = request.Napomena ?? string.Empty,
                ClanJmbg = request.ClanJmbg,
                // FIX: null je sada validno jer smo napravili kolonu nullable (migracija)
                // Prazan string "" bi krsio FK constraint ka Bibliotekari tabeli
                BibliotekarJmbg = null
            };

            _unitOfWork.Izdavanja.Add(izdavanje);

            var listaKnjigaZaDto = new List<KnjigaDto>();

            foreach (var naslov in request.NasloviKnjiga)
            {
                var knjiga = _unitOfWork.Knjige.Find(k => k.Naslov == naslov).FirstOrDefault();
                if (knjiga == null) return BadRequest($"Knjiga '{naslov}' ne postoji.");

                _unitOfWork.StavkeIzdavanja.Add(new StavkaIzdavanja { Izdavanje = izdavanje, Knjiga = knjiga });
                listaKnjigaZaDto.Add(new KnjigaDto { KnjigaId = knjiga.KnjigaId, Naslov = knjiga.Naslov, Slika = knjiga.Slika });
            }

            _unitOfWork.SaveChanges();

            return Ok(new IzdavanjeDto
            {
                IzdavanjeId = izdavanje.IzdavanjeId,
                DatumIzdavanja = izdavanje.DatumIzdavanja,
                Status = izdavanje.Status,
                Napomena = izdavanje.Napomena,
                BibliotekarImePrezime = "Ceka odobrenje bibliotekara",
                ClanJmbg = clan.Jmbg,
                ClanIme = clan.Ime,
                ClanPrezime = clan.Prezime,
                Knjige = listaKnjigaZaDto
            });
        }

        [HttpPut("{id}/odobri")]
        [Authorize(Roles = "Bibliotekar")]
        public ActionResult<IzdavanjeDto> Odobri(int id, [FromBody] OdobriIzdavanjeRequest request)
        {
            var validationResult = _odobriValidator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var izdavanje = _unitOfWork.Izdavanja.GetById(id);
            if (izdavanje == null) return NotFound("Trazeno izdavanje ne postoji.");

            if (izdavanje.Status != "REZERVISANO")
                return BadRequest($"Nije moguce odobriti. Trenutni status: {izdavanje.Status}");

            var bibliotekar = _unitOfWork.Bibliotekari.GetById(request.BibliotekarJmbg);
            if (bibliotekar == null) return BadRequest("Nepostojeci bibliotekar.");

            izdavanje.Status = "IZDATO";
            izdavanje.BibliotekarJmbg = request.BibliotekarJmbg;
            izdavanje.DatumIzdavanja = DateTime.Now;
            _unitOfWork.SaveChanges();

            var clan = _unitOfWork.Clanovi.GetById(izdavanje.ClanJmbg);
            var knjige = _unitOfWork.Knjige
                .Find(k => k.StavkeIzdavanja.Any(s => s.Izdavanje.IzdavanjeId == id))
                .Select(k => new KnjigaDto { KnjigaId = k.KnjigaId, Naslov = k.Naslov, Slika = k.Slika })
                .ToList();

            return Ok(new IzdavanjeDto
            {
                IzdavanjeId = izdavanje.IzdavanjeId,
                DatumIzdavanja = izdavanje.DatumIzdavanja,
                Status = izdavanje.Status,
                Napomena = izdavanje.Napomena ?? "",
                BibliotekarImePrezime = $"{bibliotekar.Ime} {bibliotekar.Prezime}",
                ClanJmbg = izdavanje.ClanJmbg,
                ClanIme = clan?.Ime ?? "Nepoznat",
                ClanPrezime = clan?.Prezime ?? "",
                Knjige = knjige
            });
        }

        [HttpPut("{id}/vrati")]
        [Authorize(Roles = "Clan")]
        public ActionResult<IzdavanjeDto> Vrati(int id)
        {
            var izdavanje = _unitOfWork.Izdavanja.GetById(id);
            if (izdavanje == null) return NotFound("Izdavanje ne postoji.");

            var ulogovaniJmbg = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ulogovaniJmbg != izdavanje.ClanJmbg)
                return StatusCode(403, "Zabranjeno! Ne mozete vratiti knjigu drugog clana.");

            if (izdavanje.Status != "IZDATO")
                return BadRequest($"Nije moguce vratiti. Trenutni status: {izdavanje.Status}");

            izdavanje.Status = "VRACENO";
            izdavanje.DatumVracanja = DateTime.Now;
            _unitOfWork.SaveChanges();

            var bibliotekar = !string.IsNullOrEmpty(izdavanje.BibliotekarJmbg)
                ? _unitOfWork.Bibliotekari.GetById(izdavanje.BibliotekarJmbg)
                : null;
            var clan = _unitOfWork.Clanovi.GetById(izdavanje.ClanJmbg);
            var knjige = _unitOfWork.Knjige
                .Find(k => k.StavkeIzdavanja.Any(s => s.Izdavanje.IzdavanjeId == id))
                .Select(k => new KnjigaDto { KnjigaId = k.KnjigaId, Naslov = k.Naslov, Slika = k.Slika })
                .ToList();

            return Ok(new IzdavanjeDto
            {
                IzdavanjeId = izdavanje.IzdavanjeId,
                DatumIzdavanja = izdavanje.DatumIzdavanja,
                DatumVracanja = izdavanje.DatumVracanja,
                Status = izdavanje.Status,
                Napomena = izdavanje.Napomena ?? "",
                BibliotekarImePrezime = bibliotekar != null ? $"{bibliotekar.Ime} {bibliotekar.Prezime}" : "Nepoznat",
                ClanJmbg = izdavanje.ClanJmbg,
                ClanIme = clan?.Ime ?? "Nepoznat",
                ClanPrezime = clan?.Prezime ?? "",
                Knjige = knjige
            });
        }
    }
}