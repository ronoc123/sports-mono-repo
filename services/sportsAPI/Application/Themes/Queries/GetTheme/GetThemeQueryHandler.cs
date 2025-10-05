using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Themes.Queries.GetTheme;

public class GetThemeQueryHandler : IRequestHandler<GetThemeQuery, Result<ThemeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetThemeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ThemeDto>> Handle(GetThemeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Result<ThemeDto>.Failure("Theme name is required");
            }

            var theme = await _context.Themes
                .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (theme == null)
            {
                return Result<ThemeDto>.Failure("Theme not found");
            }

            var dto = new ThemeDto
            {
                Name = theme.Name,
                ColorPrimary = theme.ColorPrimary,
                ColorSecondary = theme.ColorSecondary,
                ColorTertiary = theme.ColorTertiary,
                Logo = theme.Logo
            };

            return Result<ThemeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ThemeDto>.Failure($"Failed to get theme: {ex.Message}");
        }
    }
}
