using FluentValidation;

namespace PaginationKit.AspNetCore.Validation;

public class CursorPaginationRequestValidator : AbstractValidator<CursorPaginationRequestValidationModel>
{
    public CursorPaginationRequestValidator()
    {
        RuleFor(x => x.Limit)
            .NotNull()
            .WithMessage("`limit` must be both provided and an integer")
            .GreaterThanOrEqualTo(0)
            .WithMessage("`limit` must be greater or equal to 0. If 0 default system value will be used.");
    }
}
