using FluentValidation;

namespace PaginationKit.AspNetCore.Validation;

public class PaginationRequestValidator : AbstractValidator<PaginationRequestValidationModel>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .NotNull()
            .WithMessage("`page` must be both provided and an integer")
            .GreaterThanOrEqualTo(0)
            .WithMessage("`page` must be greater or equal to 0. If 0 default system value will be used.");

        RuleFor(x => x.Size)
            .NotNull()
            .WithMessage("`size` must be both provided and an integer greater than 0")
            .GreaterThanOrEqualTo(0)
            .WithMessage("`size` must be greater or equal to 0. If 0 default system value will be used.");
    }
}
