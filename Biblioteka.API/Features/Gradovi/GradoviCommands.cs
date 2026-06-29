using Biblioteka.API.DTOs.Gradovi;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Gradovi
{
    public record CreateGradCommand(string Naziv) : IRequest<GradDto?>;

    public class CreateGradCommandHandler : IRequestHandler<CreateGradCommand, GradDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateGradCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<GradDto?> Handle(CreateGradCommand request, CancellationToken ct)
        {
            if (_unitOfWork.Gradovi.Find(g => g.Naziv == request.Naziv).Any())
                return Task.FromResult<GradDto?>(null);

            var grad = new Grad { Naziv = request.Naziv };
            _unitOfWork.Gradovi.Add(grad);
            _unitOfWork.SaveChanges();
            return Task.FromResult<GradDto?>(new GradDto { GradId = grad.GradId, Naziv = grad.Naziv });
        }
    }

    public record UpdateGradCommand(int Id, string Naziv) : IRequest<GradDto?>;

    public class UpdateGradCommandHandler : IRequestHandler<UpdateGradCommand, GradDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateGradCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<GradDto?> Handle(UpdateGradCommand request, CancellationToken ct)
        {
            var grad = _unitOfWork.Gradovi.GetById(request.Id);
            if (grad == null) return Task.FromResult<GradDto?>(null);
            grad.Naziv = request.Naziv;
            _unitOfWork.SaveChanges();
            return Task.FromResult<GradDto?>(new GradDto { GradId = grad.GradId, Naziv = grad.Naziv });
        }
    }

    public record DeleteGradCommand(int Id) : IRequest<bool>;

    public class DeleteGradCommandHandler : IRequestHandler<DeleteGradCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteGradCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<bool> Handle(DeleteGradCommand request, CancellationToken ct)
        {
            var grad = _unitOfWork.Gradovi.GetById(request.Id);
            if (grad == null) return Task.FromResult(false);
            if (_unitOfWork.Clanovi.Find(c => c.GradId == request.Id).Any())
                return Task.FromResult(false);
            _unitOfWork.Gradovi.Remove(grad);
            _unitOfWork.SaveChanges();
            return Task.FromResult(true);
        }
    }
}