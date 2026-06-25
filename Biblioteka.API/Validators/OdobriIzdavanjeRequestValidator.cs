using Biblioteka.API.DTOs.Izdavanja;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class OdobriIzdavanjeRequestValidator : AbstractValidator<OdobriIzdavanjeRequest>
    {
        public OdobriIzdavanjeRequestValidator()
        {
            RuleFor(x => x.BibliotekarJmbg)
                .NotEmpty().WithMessage("Bibliotekar JMBG je obavezan.")
                .Matches(@"^\d{13}$").WithMessage("JMBG mora imati tačno 13 cifara.");
        }
    }
}
