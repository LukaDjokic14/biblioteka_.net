using Biblioteka.API.DTOs.Autori;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class UpdateAutorRequestValidator : AbstractValidator<UpdateAutorRequest>
    {
        public UpdateAutorRequestValidator()
        {
            RuleFor(x => x.Ime)
                .MaximumLength(50).WithMessage("Ime ne može biti duže od 50 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Ime));

            RuleFor(x => x.Prezime)
                .MaximumLength(50).WithMessage("Prezime ne može biti duže od 50 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Prezime));
        }
    }
}
