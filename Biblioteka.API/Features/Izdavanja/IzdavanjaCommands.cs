using Biblioteka.API.DTOs.Izdavanja;
using Biblioteka.API.DTOs.Knjige;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Izdavanja
{
    public record RezervisiCommand(string ClanJmbg, List<string> NasloviKnjiga, string Napomena)
        : IRequest<IzdavanjeDto?>;

    public class RezervisiCommandHandler : IRequestHandler<RezervisiCommand, IzdavanjeDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public RezervisiCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IzdavanjeDto?> Handle(RezervisiCommand request, CancellationToken ct)
        {
            var clan = _unitOfWork.Clanovi.GetById(request.ClanJmbg);
            if (clan == null) return Task.FromResult<IzdavanjeDto?>(null);

            var izdavanje = new Izdavanje
            {
                DatumIzdavanja = DateTime.Now,
                Status = "REZERVISANO",
                Napomena = request.Napomena ?? string.Empty,
                ClanJmbg = request.ClanJmbg,
                BibliotekarJmbg = null
            };
            _unitOfWork.Izdavanja.Add(izdavanje);

            var listaKnjiga = new List<KnjigaDto>();
            foreach (var naslov in request.NasloviKnjiga)
            {
                var knjiga = _unitOfWork.Knjige.Find(k => k.Naslov == naslov).FirstOrDefault();
                if (knjiga == null) return Task.FromResult<IzdavanjeDto?>(null);
                _unitOfWork.StavkeIzdavanja.Add(new StavkaIzdavanja { Izdavanje = izdavanje, Knjiga = knjiga });
                listaKnjiga.Add(new KnjigaDto { KnjigaId = knjiga.KnjigaId, Naslov = knjiga.Naslov, Slika = knjiga.Slika });
            }
            _unitOfWork.SaveChanges();

            return Task.FromResult<IzdavanjeDto?>(new IzdavanjeDto
            {
                IzdavanjeId = izdavanje.IzdavanjeId,
                DatumIzdavanja = izdavanje.DatumIzdavanja,
                Status = izdavanje.Status,
                Napomena = izdavanje.Napomena,
                BibliotekarImePrezime = "Ceka odobrenje bibliotekara",
                ClanJmbg = clan.Jmbg,
                ClanIme = clan.Ime,
                ClanPrezime = clan.Prezime,
                Knjige = listaKnjiga
            });
        }
    }

    public record OdobriIzdavanjeCommand(int IzdavanjeId, string BibliotekarJmbg)
        : IRequest<IzdavanjeDto?>;

    public class OdobriIzdavanjeCommandHandler : IRequestHandler<OdobriIzdavanjeCommand, IzdavanjeDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public OdobriIzdavanjeCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IzdavanjeDto?> Handle(OdobriIzdavanjeCommand request, CancellationToken ct)
        {
            var izdavanje = _unitOfWork.Izdavanja.GetById(request.IzdavanjeId);
            if (izdavanje == null || izdavanje.Status != "REZERVISANO")
                return Task.FromResult<IzdavanjeDto?>(null);

            var bibliotekar = _unitOfWork.Bibliotekari.GetById(request.BibliotekarJmbg);
            if (bibliotekar == null) return Task.FromResult<IzdavanjeDto?>(null);

            izdavanje.Status = "IZDATO";
            izdavanje.BibliotekarJmbg = request.BibliotekarJmbg;
            izdavanje.DatumIzdavanja = DateTime.Now;
            _unitOfWork.SaveChanges();

            var clan = _unitOfWork.Clanovi.GetById(izdavanje.ClanJmbg);
            var knjige = _unitOfWork.Knjige
                .Find(k => k.StavkeIzdavanja.Any(s => s.Izdavanje.IzdavanjeId == request.IzdavanjeId))
                .Select(k => new KnjigaDto { KnjigaId = k.KnjigaId, Naslov = k.Naslov, Slika = k.Slika })
                .ToList();

            return Task.FromResult<IzdavanjeDto?>(new IzdavanjeDto
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
    }

    // UlogovaniJmbg dolazi iz JWT tokena — kontroler ga prosliedi jer handler nema pristup HttpContext-u
    public record VratiKnjiguCommand(int IzdavanjeId, string UlogovaniJmbg) : IRequest<VratiResult>;

    public class VratiResult
    {
        public bool IsSuccess { get; set; }
        public bool IsNotFound { get; set; }
        public bool IsForbidden { get; set; }
        public bool IsBadRequest { get; set; }
        public string? Error { get; set; }
        public IzdavanjeDto? Dto { get; set; }
    }

    public class VratiKnjiguCommandHandler : IRequestHandler<VratiKnjiguCommand, VratiResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public VratiKnjiguCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<VratiResult> Handle(VratiKnjiguCommand request, CancellationToken ct)
        {
            var izdavanje = _unitOfWork.Izdavanja.GetById(request.IzdavanjeId);
            if (izdavanje == null)
                return Task.FromResult(new VratiResult { IsNotFound = true, Error = "Izdavanje ne postoji." });

            if (request.UlogovaniJmbg != izdavanje.ClanJmbg)
                return Task.FromResult(new VratiResult { IsForbidden = true, Error = "Zabranjeno! Ne mozete vratiti knjigu drugog clana." });

            if (izdavanje.Status != "IZDATO")
                return Task.FromResult(new VratiResult { IsBadRequest = true, Error = $"Nije moguce vratiti. Trenutni status: {izdavanje.Status}" });

            izdavanje.Status = "VRACENO";
            izdavanje.DatumVracanja = DateTime.Now;
            _unitOfWork.SaveChanges();

            var bibliotekar = !string.IsNullOrEmpty(izdavanje.BibliotekarJmbg)
                ? _unitOfWork.Bibliotekari.GetById(izdavanje.BibliotekarJmbg) : null;
            var clan = _unitOfWork.Clanovi.GetById(izdavanje.ClanJmbg);
            var knjige = _unitOfWork.Knjige
                .Find(k => k.StavkeIzdavanja.Any(s => s.Izdavanje.IzdavanjeId == request.IzdavanjeId))
                .Select(k => new KnjigaDto { KnjigaId = k.KnjigaId, Naslov = k.Naslov, Slika = k.Slika })
                .ToList();

            return Task.FromResult(new VratiResult
            {
                IsSuccess = true,
                Dto = new IzdavanjeDto
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
                }
            });
        }
    }
}