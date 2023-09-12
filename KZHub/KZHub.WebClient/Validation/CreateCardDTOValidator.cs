using FluentValidation;
using KZHub.WebClient.DTOs.Card;

namespace KZHub.WebClient.Validation
{
    public class CreateCardDTOValidator : AbstractValidator<CreateCardDTO>
    {
        public CreateCardDTOValidator()
        {
            RuleFor(x => x.Zastep).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.Points).NotEmpty();
            RuleForEach(x => x.Points).SetValidator(new CreatePointDTOValidator()).WithMessage("Nie każdy punkt zbiórki jest uzupełniony poprawnie!");
        }
    }
}
