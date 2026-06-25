using Biblioteka.API.DTOs.Izdavanja;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class RezervacijaRequestValidator : AbstractValidator<RezervacijaRequest>
    {
        public RezervacijaRequestValidator()
        {
            RuleFor(x => x.ClanJmbg)
                .NotEmpty().WithMessage("JMBG člana je obavezan.")
                .Matches(@"^\d{13}$").WithMessage("JMBG mora imati tačno 13 cifara.");

            RuleFor(x => x.NasloviKnjiga)
                .NotEmpty().WithMessage("Morate uneti barem jednu knjigu za rezervaciju.");
        }
    }
}