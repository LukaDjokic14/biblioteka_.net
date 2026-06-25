using Biblioteka.API.DTOs.Clanovi;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class UpdateClanRequestValidator : AbstractValidator<UpdateClanRequest>
    {
        public UpdateClanRequestValidator()
        {
            
            RuleFor(x => x.Ime)
                .MinimumLength(2).WithMessage("Ime mora imati bar 2 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Ime));

            RuleFor(x => x.Lozinka)
                .MinimumLength(4).WithMessage("Lozinka mora imati barem 4 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.Lozinka));
        }
    }
}
