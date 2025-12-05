using FluentValidation;
using PDFHub.API.Models.DTOs;

namespace PDFHub.API.Validators;

public class LoginDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Email or username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
