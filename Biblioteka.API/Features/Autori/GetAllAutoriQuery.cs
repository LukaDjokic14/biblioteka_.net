// GetAllAutoriQuery.cs
using Biblioteka.API.DTOs.Autori;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Autori
{
    // Query = zahtev koji ČITA podatke, ne mijenja stanje
    public record GetAllAutoriQuery() : IRequest<IEnumerable<AutorDto>>;

    public class GetAllAutoriQueryHandler
        : IRequestHandler<GetAllAutoriQuery, IEnumerable<AutorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllAutoriQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<IEnumerable<AutorDto>> Handle(
            GetAllAutoriQuery request, CancellationToken cancellationToken)
        {
            var autori = _unitOfWork.Autori.GetAll();
            var result = autori.Select(a => new AutorDto
            {
                AutorId = a.AutorId,
                Ime = a.Ime,
                Prezime = a.Prezime,
                Biografija = a.Biografija
            });
            return Task.FromResult(result);
        }
    }
}
