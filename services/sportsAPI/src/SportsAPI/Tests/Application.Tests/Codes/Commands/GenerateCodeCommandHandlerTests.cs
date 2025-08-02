using Application.Codes.Commands.GenerateCode;
using Application.Common.Interfaces;
using Domain.Organizations;
using Domain.Repositories;
using Domain.SharedKernal;
using Domain.ValueObjects.ConcreteTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Application.Tests.Codes.Commands;

public class GenerateCodeCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IOrganizationRepository> _mockOrgRepository;
    private readonly Mock<DbSet<Code>> _mockCodeDbSet;
    private readonly GenerateCodeCommandHandler _handler;

    public GenerateCodeCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockOrgRepository = new Mock<IOrganizationRepository>();
        _mockCodeDbSet = new Mock<DbSet<Code>>();
        
        _mockContext.Setup(x => x.Codes).Returns(_mockCodeDbSet.Object);
        
        _handler = new GenerateCodeCommandHandler(_mockContext.Object, _mockOrgRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidOrganizationId_ShouldGenerateCode()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = CreateTestOrganization(organizationId);
        var command = new GenerateCodeCommand(organizationId);

        _mockOrgRepository
            .Setup(x => x.GetOrganizationByIdAsync(organizationId))
            .ReturnsAsync(organization);

        _mockContext
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        
        _mockCodeDbSet.Verify(x => x.Add(It.IsAny<Code>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidOrganizationId_ShouldReturnFailure()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var command = new GenerateCodeCommand(organizationId);

        _mockOrgRepository
            .Setup(x => x.GetOrganizationByIdAsync(organizationId))
            .ReturnsAsync((Organization?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Organization not found");
        
        _mockCodeDbSet.Verify(x => x.Add(It.IsAny<Code>()), Times.Never);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Organization CreateTestOrganization(Guid organizationId)
    {
        // This is a simplified test organization creation
        // In a real scenario, you'd use the proper factory methods
        var venue = new Domain.ValueObjects.Venue("Test Stadium", "Test City", 50000);
        var mediaAssets = new Domain.ValueObjects.MediaAssets("badge.png", "logo.png", "fan1.png", "fan2.png", "fan3.png");
        var socialLinks = new Domain.ValueObjects.SocialLinks("website.com", "facebook.com", "twitter.com", "instagram.com");
        var teamColors = new Domain.ValueObjects.TeamColors("Red", "Blue", "White");

        return new Organization(
            OrganizationId.Of(organizationId),
            LeagueId.Of(Guid.NewGuid()),
            "Test Organization",
            "TEST",
            "Test Team",
            "TT",
            2000,
            "Football",
            venue,
            mediaAssets,
            socialLinks,
            teamColors,
            "Test Description"
        );
    }
}
