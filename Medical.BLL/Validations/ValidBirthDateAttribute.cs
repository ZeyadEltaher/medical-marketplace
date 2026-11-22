using System.ComponentModel.DataAnnotations;

public class ValidBirthDateAttribute : ValidationAttribute
{
    private readonly int _minAge;
    private readonly int _maxAge;

    public ValidBirthDateAttribute(int minAge = 18, int maxAge = 120)
    {
        _minAge = minAge;
        _maxAge = maxAge;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateOnly birthDate)
            return new ValidationResult("Invalid date format.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;

        if (age < _minAge)
        {
            return new ValidationResult(ErrorMessage ?? $"You must be at least {_minAge} years old.");
        }
        
        if (age > _maxAge)
        {
            return new ValidationResult(ErrorMessage ?? $"Invalid birth date.");
        }

        return ValidationResult.Success;
    }
}