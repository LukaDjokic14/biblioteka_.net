using Biblioteka.API.DTOs.Autori;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Autori
{
    public record UpdateAutorCommand(
        int Id,
        string? Ime,
        string? Prezime,
        string? Biografija) : IRequest<AutorDto?>;

    public class UpdateAutorCommandHandler
        : IRequestHandler<UpdateAutorCommand, AutorDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAutorCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<AutorDto?> Handle(
            UpdateAutorCommand request, CancellationToken cancellationToken)
        {
            var autor = _unitOfWork.Autori.GetById(request.Id);
            if (autor == null) return Task.FromResult<AutorDto?>(null);

            if (!string.IsNullOrWhiteSpace(request.Ime))
                autor.Ime = request.Ime;

            if (!string.IsNullOrWhiteSpace(request.Prezime))
                autor.Prezime = request.Prezime;

            if (!string.IsNullOrWhiteSpace(request.Biografija))
                autor.Biografija = request.Biografija;

            _unitOfWork.SaveChanges();

            return Task.FromResult<AutorDto?>(new AutorDto
            {
                AutorId = autor.AutorId,
                Ime = autor.Ime,
                Prezime = autor.Prezime,
                Biografija = autor.Biografija
            });
        }
    }
}