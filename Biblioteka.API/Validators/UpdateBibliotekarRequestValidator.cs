using Biblioteka.API.DTOs.Bibliotekari;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class UpdateBibliotekarRequestValidator : AbstractValidator<UpdateBibliotekarRequest>
    {
        public UpdateBibliotekarRequestValidator()
        {
            // Validiramo samo ako je poslata nova lozinka (ako nije prazna)
            RuleFor(x => x.NovaLozinka)
                .MinimumLength(4).WithMessage("Nova lozinka mora imati barem 4 karaktera.")
                .When(x => !string.IsNullOrEmpty(x.NovaLozinka));
        }
    }
}
