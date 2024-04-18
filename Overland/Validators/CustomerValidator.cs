using FluentValidation;
using Overland.Models;

namespace Overland.Validators;

public class CustomerValidator : AbstractValidator<Customer> {
    public CustomerValidator() {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Enter a valid email.").EmailAddress().WithMessage("Enter a valid email.");
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Length(10, 18).WithMessage("Phone number is not a valid number.");
        RuleFor(x => x.PhoneType)
            .NotEmpty();
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.");
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.State)
            .NotNull().WithMessage("State is required.");
        RuleFor(x => x.Zip)
            .NotEmpty().WithMessage("Zip code is required.");
       // RuleFor(x => x.MonthlyIncome);
       // RuleFor(x => x.EstimatedDebt);
    }
}