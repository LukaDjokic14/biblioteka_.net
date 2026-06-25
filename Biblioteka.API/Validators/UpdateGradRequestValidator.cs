using Biblioteka.API.DTOs.Gradovi;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class UpdateGradRequestValidator : AbstractValidator<UpdateGradRequest>
    {
        public UpdateGradRequestValidator()
        {
            RuleFor(x => x.Naziv)
                .NotEmpty().WithMessage("Naziv grada ne sme biti prazan.");
        }
    }
}
