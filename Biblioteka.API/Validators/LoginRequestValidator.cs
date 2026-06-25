using FluentValidation;
using Biblioteka.API.DTOs.Auth;

namespace Biblioteka.API.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.KorisnickoIme)
                .NotEmpty().WithMessage("Korisničko ime je obavezno.");

            RuleFor(x => x.Lozinka)
                .NotEmpty().WithMessage("Lozinka je obavezna.");
        }
    }
}
