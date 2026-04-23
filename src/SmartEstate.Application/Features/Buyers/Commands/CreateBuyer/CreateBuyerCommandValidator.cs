using FluentValidation;

namespace SmartEstate.Application.Features.Buyers.Commands.CreateBuyer;

public class CreateBuyerCommandValidator : AbstractValidator<CreateBuyerCommand>
{
    public CreateBuyerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.LifestyleDescription)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email is not null);

        RuleFor(x => x.Phone)
            .MaximumLength(50)
            .When(x => x.Phone is not null);

        RuleFor(x => x.BudgetMinEur)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BudgetMinEur is not null);

        RuleFor(x => x.BudgetMaxEur)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BudgetMaxEur is not null);

        RuleFor(x => x)
            .Must(x => x.BudgetMinEur <= x.BudgetMaxEur)
            .When(x => x.BudgetMinEur is not null && x.BudgetMaxEur is not null)
            .WithMessage("BudgetMinEur must be less than or equal to BudgetMaxEur.");
    }
}
