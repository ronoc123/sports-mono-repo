using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Organizations.Entities
{
    public class Theme : Entity<ThemeId>
    {
        public string ColorPrimary { get; set; } = string.Empty;
        public string ColorSecondary { get; set; } = string.Empty;
        public string ColorTertiary { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        internal Theme() { }

        public Theme(string name, string colorPrimary, string colorSecondary, string colorTertiary, string logo)
        {
            Id = ThemeId.Of(Guid.NewGuid());
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ColorPrimary = colorPrimary ?? string.Empty;
            ColorSecondary = colorSecondary ?? string.Empty;
            ColorTertiary = colorTertiary ?? string.Empty;
            Logo = logo ?? string.Empty;
        }
    }
}
