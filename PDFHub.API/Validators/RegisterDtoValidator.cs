using FluentValidation;
using PDFHub.API.Models.DTOs;

namespace PDFHub.API.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterDtoValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Valid email address is required.");

        // Username validation
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters.");

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
