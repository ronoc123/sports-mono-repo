

namespace Domain.ValueObjects
{
    public record MediaAssets
    {
        public string? BadgeUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string? Fanart1Url { get; set; }
        public string? Fanart2Url { get; set; }
        public string? Fanart3Url { get; set; }
        public string? Fanart4Url { get; set; }
        public string? BannerUrl { get; set; }
        public string? EquipmentUrl { get; set; }

        private MediaAssets(string? badgeUrl, string? logoUrl, string? fanart1Url, string? fanart2Url, string? fanart3Url, string? fanart4Url, string? bannerUrl, string? equipmentUrl)
        {
            BadgeUrl = badgeUrl;
            LogoUrl = logoUrl;
            Fanart1Url = fanart1Url;
            Fanart2Url = fanart2Url;
            Fanart3Url = fanart3Url;
            Fanart4Url = fanart4Url;
            BannerUrl = bannerUrl;
            EquipmentUrl = equipmentUrl;
        }

        public MediaAssets(string? badgeUrl, string? logoUrl, string? fanart1Url, string? fanart2Url, string? fanart3Url)
        {
            BadgeUrl = badgeUrl;
            LogoUrl = logoUrl;
            Fanart1Url = fanart1Url;
            Fanart2Url = fanart2Url;
            Fanart3Url = fanart3Url;
        }

        public static MediaAssets Of(string? badgeUrl, string? logoUrl, string? fanart1Url, string? fanart2Url, string? fanart3Url, string? fanart4Url, string? bannerUrl, string? equipmentUrl)
        {
            return new MediaAssets(badgeUrl, logoUrl, fanart1Url, fanart2Url, fanart3Url, fanart4Url, bannerUrl, equipmentUrl);
        }
    }
}
