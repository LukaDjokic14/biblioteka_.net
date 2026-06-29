using Biblioteka.API.DTOs.Autori;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Autori
{
    public record GetAutorByIdQuery(int Id) : IRequest<AutorDto?>;

    public class GetAutorByIdQueryHandler
        : IRequestHandler<GetAutorByIdQuery, AutorDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAutorByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<AutorDto?> Handle(
            GetAutorByIdQuery request, CancellationToken cancellationToken)
        {
            var autor = _unitOfWork.Autori.GetById(request.Id);
            if (autor == null) return Task.FromResult<AutorDto?>(null);

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
