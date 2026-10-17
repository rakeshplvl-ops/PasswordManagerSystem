using PasswordManager.Application.Features.Auth.Dtos;
using PasswordManager.Application.Features.Auth.Validators;
using Xunit;

namespace PasswordManager.Application.Tests;

public class ValidationTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();

    [Theory]
    [InlineData("valid@example.com", "Str0ng#Passw0rd", true)]
    [InlineData("", "Str0ng#Passw0rd", false)]
    [InlineData("invalid-email", "Str0ng#Passw0rd", false)]
    [InlineData("valid@example.com", "short1!", false)] // less than 10 chars
    [InlineData("valid@example.com", "lowercaseonly123!", false)] // missing uppercase
    [InlineData("valid@example.com", "UPPERCASEONLY123!", false)] // missing lowercase
    [InlineData("valid@example.com", "NoDigitsHere!", false)] // missing digit
    [InlineData("valid@example.com", "NoSpecialChar123", false)] // missing special char
    public void RegisterValidator_ShouldValidatePasswordAndEmail(string email, string password, bool expectedValid)
    {
        // Arrange
        var request = new RegisterRequest(email, password);

        // Act
        var result = _registerValidator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData("user@example.com", "any_password", true)]
    [InlineData("", "any_password", false)]
    [InlineData("user@example.com", "", false)]
    [InlineData("invalid-email", "any_password", false)]
    public void LoginValidator_ShouldValidateEmailAndPassword(string email, string password, bool expectedValid)
    {
        // Arrange
        var request = new LoginRequest(email, password);

        // Act
        var result = _loginValidator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }
}
