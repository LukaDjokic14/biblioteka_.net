using Biblioteka.API.DTOs.Bibliotekari;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Bibliotekari
{
    public record CreateBibliotekarCommand(string Jmbg, string Ime, string Prezime,
        string KorisnickoIme, string Lozinka) : IRequest<BibliotekarDto?>;

    public class CreateBibliotekarCommandHandler
        : IRequestHandler<CreateBibliotekarCommand, BibliotekarDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateBibliotekarCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<BibliotekarDto?> Handle(CreateBibliotekarCommand request, CancellationToken ct)
        {
            if (_unitOfWork.Bibliotekari.GetById(request.Jmbg) != null)
                return Task.FromResult<BibliotekarDto?>(null); // Već postoji

            var b = new Bibliotekar
            {
                Jmbg = request.Jmbg,
                Ime = request.Ime,
                Prezime = request.Prezime,
                KorisnickoIme = request.KorisnickoIme,
                Lozinka = BCrypt.Net.BCrypt.HashPassword(request.Lozinka)
            };
            _unitOfWork.Bibliotekari.Add(b);
            _unitOfWork.SaveChanges();

            return Task.FromResult<BibliotekarDto?>(new BibliotekarDto
            {
                Jmbg = b.Jmbg,
                Ime = b.Ime,
                Prezime = b.Prezime,
                KorisnickoIme = b.KorisnickoIme
            });
        }
    }

    public record UpdateBibliotekarCommand(string Jmbg, string? Prezime,
        string? KorisnickoIme, string? NovaLozinka) : IRequest<BibliotekarDto?>;

    public class UpdateBibliotekarCommandHandler
        : IRequestHandler<UpdateBibliotekarCommand, BibliotekarDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateBibliotekarCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<BibliotekarDto?> Handle(UpdateBibliotekarCommand request, CancellationToken ct)
        {
            var b = _unitOfWork.Bibliotekari.GetById(request.Jmbg);
            if (b == null) return Task.FromResult<BibliotekarDto?>(null);

            if (!string.IsNullOrWhiteSpace(request.Prezime)) b.Prezime = request.Prezime;
            if (!string.IsNullOrWhiteSpace(request.KorisnickoIme)) b.KorisnickoIme = request.KorisnickoIme;
            if (!string.IsNullOrWhiteSpace(request.NovaLozinka))
                b.Lozinka = BCrypt.Net.BCrypt.HashPassword(request.NovaLozinka);

            _unitOfWork.SaveChanges();
            return Task.FromResult<BibliotekarDto?>(new BibliotekarDto
            {
                Jmbg = b.Jmbg,
                Ime = b.Ime,
                Prezime = b.Prezime,
                KorisnickoIme = b.KorisnickoIme
            });
        }
    }

    public record DeleteBibliotekarCommand(string Jmbg) : IRequest<bool>;

    public class DeleteBibliotekarCommandHandler
        : IRequestHandler<DeleteBibliotekarCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteBibliotekarCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<bool> Handle(DeleteBibliotekarCommand request, CancellationToken ct)
        {
            var b = _unitOfWork.Bibliotekari.GetById(request.Jmbg);
            if (b == null) return Task.FromResult(false);
            _unitOfWork.Bibliotekari.Remove(b);
            _unitOfWork.SaveChanges();
            return Task.FromResult(true);
        }
    }
}
