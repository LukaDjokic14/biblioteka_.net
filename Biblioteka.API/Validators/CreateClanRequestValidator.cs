using Biblioteka.API.DTOs.Clanovi;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class CreateClanRequestValidator : AbstractValidator<CreateClanRequest>
    {
        public CreateClanRequestValidator()
        {
            RuleFor(x => x.Jmbg)
                .NotEmpty().WithMessage("JMBG je obavezan.")
                .Matches(@"^\d{13}$").WithMessage("JMBG mora sadržati tačno 13 cifara.");

            RuleFor(x => x.Ime)
                .NotEmpty().WithMessage("Ime je obavezno.")
                .MaximumLength(50).WithMessage("Ime je predugačko.");

            RuleFor(x => x.Prezime)
                .NotEmpty().WithMessage("Prezime je obavezno.");

            RuleFor(x => x.KorisnickoIme)
                .NotEmpty().WithMessage("Korisničko ime je obavezno.");

            RuleFor(x => x.Lozinka)
                .NotEmpty().WithMessage("Lozinka je obavezna.")
                .MinimumLength(4).WithMessage("Lozinka mora imati barem 4 karaktera.");

            RuleFor(x => x.GradNaziv)
                .NotEmpty().WithMessage("Naziv grada je obavezan.");
        }
    }
}
