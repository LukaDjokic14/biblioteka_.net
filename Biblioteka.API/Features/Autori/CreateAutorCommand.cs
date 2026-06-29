// CreateAutorCommand.cs
using Biblioteka.API.DTOs.Autori;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Autori
{
    // Command = zahtev koji MIJENJA stanje (dodaje, mijenja, briše)
    public record CreateAutorCommand(
        string Ime,
        string Prezime,
        string Biografija) : IRequest<AutorDto>;

    public class CreateAutorCommandHandler
        : IRequestHandler<CreateAutorCommand, AutorDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateAutorCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<AutorDto> Handle(
            CreateAutorCommand request, CancellationToken cancellationToken)
        {
            var autor = new Autor
            {
                Ime = request.Ime,
                Prezime = request.Prezime,
                Biografija = request.Biografija
            };

            _unitOfWork.Autori.Add(autor);
            _unitOfWork.SaveChanges();

            return Task.FromResult(new AutorDto
            {
                AutorId = autor.AutorId,
                Ime = autor.Ime,
                Prezime = autor.Prezime,
                Biografija = autor.Biografija
            });
        }
    }
}
