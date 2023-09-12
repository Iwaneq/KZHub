using FluentValidation;
using KZHub.WebClient.DTOs.Card;

namespace KZHub.WebClient.Validation
{
    public class CreatePointDTOValidator : AbstractValidator<CreatePointDTO>
    {
        public CreatePointDTOValidator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.ZastepMember).NotEmpty();
        }
    }
}
