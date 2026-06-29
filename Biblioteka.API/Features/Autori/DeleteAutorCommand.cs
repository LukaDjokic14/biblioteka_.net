using Biblioteka.Domain.Repozitorijumi;
using MediatR;

namespace Biblioteka.API.Features.Autori
{
    public record DeleteAutorCommand(int Id) : IRequest<bool>;

    public class DeleteAutorCommandHandler
        : IRequestHandler<DeleteAutorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAutorCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<bool> Handle(
            DeleteAutorCommand request, CancellationToken cancellationToken)
        {
            var autor = _unitOfWork.Autori.GetById(request.Id);
            if (autor == null) return Task.FromResult(false);

            
            var imaKnjige = _unitOfWork.Pisanja
                .Find(p => p.Autor.AutorId == request.Id)
                .Any();

            if (imaKnjige) return Task.FromResult(false);

            _unitOfWork.Autori.Remove(autor);
            _unitOfWork.SaveChanges();
            return Task.FromResult(true);
        }
    }
}
