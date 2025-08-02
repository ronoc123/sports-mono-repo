using Application.Organizations.Commands.CreateOrganization;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Organizations.Commands;

public class CreateOrganizationCommandValidatorTests
{
    private readonly CreateOrganizationCommandValidator _validator;

    public CreateOrganizationCommandValidatorTests()
    {
        _validator = new CreateOrganizationCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateOrganizationCommand(
            "Test Organization",
            Guid.NewGuid(),
            "TEST",
            "Test Team",
            "TT",
            2000,
            "Football",
            "Test Stadium",
            "Test City",
            50000,
            "website.com",
            "facebook.com",
            "twitter.com",
            "instagram.com",
            "Test Description",
            "Red",
            "Blue",
            "White",
            "badge.png",
            "logo.png",
            "fan1.png",
            "fan2.png",
            "fan3.png"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        // Arrange
        var command = new CreateOrganizationCommand(
            "",
            Guid.NewGuid(),
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Name");
    }

    [Fact]
    public void Validate_EmptyLeagueId_ShouldFail()
    {
        // Arrange
        var command = new CreateOrganizationCommand(
            "Test Organization",
            Guid.Empty,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "LeagueId");
    }

    [Fact]
    public void Validate_InvalidFormedYear_ShouldFail()
    {
        // Arrange
        var command = new CreateOrganizationCommand(
            "Test Organization",
            Guid.NewGuid(),
            null, null, null, 1700, // Invalid year
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "FormedYear");
    }
}
