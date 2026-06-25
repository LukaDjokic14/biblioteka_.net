using Biblioteka.API.DTOs.Autori;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class CreateAutorRequestValidator : AbstractValidator<CreateAutorRequest>
    {
        public CreateAutorRequestValidator()
        {
            RuleFor(x => x.Ime)
                .NotEmpty().WithMessage("Ime autora je obavezno.")
                .MaximumLength(50).WithMessage("Ime ne može biti duže od 50 karaktera.");

            RuleFor(x => x.Prezime)
                .NotEmpty().WithMessage("Prezime autora je obavezno.")
                .MaximumLength(50).WithMessage("Prezime ne može biti duže od 50 karaktera.");
        }
    }
}
