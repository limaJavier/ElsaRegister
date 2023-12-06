using ElsaRegister.Models;
using FluentValidation;

namespace ElsaRegister.Services;

public class UserValidator : AbstractValidator<UserDTO>
{
    public UserValidator()
    {
        RuleFor(dto => dto.Name).NotEmpty();
        // RuleFor(dto => dto.Email).EmailAddress();
    }
}