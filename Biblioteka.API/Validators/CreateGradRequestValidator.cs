using Biblioteka.API.DTOs.Gradovi;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class CreateGradRequestValidator : AbstractValidator<CreateGradRequest>
    {
        public CreateGradRequestValidator()
        {
            RuleFor(x => x.Naziv)
                .NotEmpty().WithMessage("Naziv grada je obavezan.")
                .MaximumLength(100).WithMessage("Naziv grada je predugačak.");
        }
    }
}
