using FluentValidation;

namespace Application.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsQueryValidator : AbstractValidator<GetAllOrganizationsQuery>
{
    private readonly string[] _allowedSortFields = { "Name", "CreatedAt", "FormedYear", "Sport", "TeamName" };

    public GetAllOrganizationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100");

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrEmpty(sortBy) || _allowedSortFields.Contains(sortBy))
            .WithMessage($"SortBy must be one of: {string.Join(", ", _allowedSortFields)}");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}
