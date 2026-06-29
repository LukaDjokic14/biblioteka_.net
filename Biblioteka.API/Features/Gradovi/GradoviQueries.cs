using Biblioteka.API.DTOs.Gradovi;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Gradovi
{
    public record GetAllGradoviQuery() : IRequest<IEnumerable<GradDto>>;

    public class GetAllGradoviQueryHandler : IRequestHandler<GetAllGradoviQuery, IEnumerable<GradDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllGradoviQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<IEnumerable<GradDto>> Handle(GetAllGradoviQuery request, CancellationToken ct)
        {
            var result = _unitOfWork.Gradovi.GetAll()
                .Select(g => new GradDto { GradId = g.GradId, Naziv = g.Naziv });
            return Task.FromResult(result);
        }
    }

    public record GetGradByIdQuery(int Id) : IRequest<GradDto?>;

    public class GetGradByIdQueryHandler : IRequestHandler<GetGradByIdQuery, GradDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetGradByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<GradDto?> Handle(GetGradByIdQuery request, CancellationToken ct)
        {
            var g = _unitOfWork.Gradovi.GetById(request.Id);
            if (g == null) return Task.FromResult<GradDto?>(null);
            return Task.FromResult<GradDto?>(new GradDto { GradId = g.GradId, Naziv = g.Naziv });
        }
    }
}