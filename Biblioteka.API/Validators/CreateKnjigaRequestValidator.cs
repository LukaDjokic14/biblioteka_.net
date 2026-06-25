using Biblioteka.API.DTOs.Knjige;
using FluentValidation;

namespace Biblioteka.API.Validators
{
    public class CreateKnjigaRequestValidator : AbstractValidator<CreateKnjigaRequest>
    {
        public CreateKnjigaRequestValidator()
        {
            RuleFor(x => x.Naslov).NotEmpty().WithMessage("Naslov je obavezan.");
            RuleFor(x => x.Isbn).NotEmpty().WithMessage("ISBN je obavezan.");
            RuleFor(x => x.BrojStrana).GreaterThan(0).WithMessage("Broj strana mora biti veći od 0.");
            RuleFor(x => x.Zanr).NotEmpty().WithMessage("Zanr je obavezan.");
        }
    }
}  

