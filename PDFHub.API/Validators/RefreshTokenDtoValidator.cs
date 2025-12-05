using FluentValidation;
using PDFHub.API.Models.DTOs;

namespace PDFHub.API.Validators;

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
