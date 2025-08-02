using Application.Common.Models;
using MediatR;

namespace Application.Themes.Queries.GetTheme;

public record GetThemeQuery(string Name) : IRequest<Result<ThemeDto>>;
