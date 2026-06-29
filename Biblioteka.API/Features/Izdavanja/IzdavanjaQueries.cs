using Biblioteka.API.DTOs.Izdavanja;
using Biblioteka.API.DTOs.Knjige;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Izdavanja
{
    public record GetAllIzdavanjaQuery() : IRequest<IEnumerable<IzdavanjeDto>>;

    public class GetAllIzdavanjaQueryHandler
        : IRequestHandler<GetAllIzdavanjaQuery, IEnumerable<IzdavanjeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllIzdavanjaQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IEnumerable<IzdavanjeDto>> Handle(GetAllIzdavanjaQuery request, CancellationToken ct)
        {
            var sva = _unitOfWork.Izdavanja.GetAll().ToList();
            var result = sva.Select(i =>
            {
                var clan = _unitOfWork.Clanovi.GetById(i.ClanJmbg);
                var bibliotekar = !string.IsNullOrEmpty(i.BibliotekarJmbg)
                    ? _unitOfWork.Bibliotekari.GetById(i.BibliotekarJmbg) : null;
                var knjige = _unitOfWork.Knjige
                    .Find(k => k.StavkeIzdavanja.Any(s => s.Izdavanje.IzdavanjeId == i.IzdavanjeId))
                    .Select(k => new KnjigaDto { KnjigaId = k.KnjigaId, Naslov = k.Naslov, Slika = k.Slika })
                    .ToList();
                return new IzdavanjeDto
                {
                    IzdavanjeId = i.IzdavanjeId,
                    DatumIzdavanja = i.DatumIzdavanja,
                    DatumVracanja = i.DatumVracanja,
                    Status = i.Status,
                    Napomena = i.Napomena ?? "",
                    BibliotekarImePrezime = bibliotekar != null ? $"{bibliotekar.Ime} {bibliotekar.Prezime}" : "Nije odobreno",
                    ClanJmbg = i.ClanJmbg,
                    ClanIme = clan?.Ime ?? "Nepoznat",
                    ClanPrezime = clan?.Prezime ?? "",
                    Knjige = knjige
                };
            });
            return Task.FromResult(result);
        }
    }
}