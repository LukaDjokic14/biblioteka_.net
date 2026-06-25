using Biblioteka.API.DTOs.Knjige;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class UpdateKnjigaRequestValidator : AbstractValidator<UpdateKnjigaRequest>
    {
        public UpdateKnjigaRequestValidator()
        {
            RuleFor(x => x.BrojStrana)
                .GreaterThan(0).WithMessage("Broj strana mora biti veći od 0.")
                .When(x => x.BrojStrana.HasValue);
        }
    }
}
