namespace ServiceLib.Common;

/// <summary>
/// Extension methods for country code utilities
/// </summary>
public static class CountryExtension
{
    /// <summary>
    /// Country code to emoji flag mapping for common countries
    /// </summary>
    private static readonly Dictionary<string, string> CountryEmojiMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Asia
        { "CN", "🇨🇳" }, // China
        { "HK", "🇭🇰" }, // Hong Kong
        { "TW", "🇹🇼" }, // Taiwan
        { "JP", "🇯🇵" }, // Japan
        { "SG", "🇸🇬" }, // Singapore
        { "KR", "🇰🇷" }, // South Korea
        { "TH", "🇹🇭" }, // Thailand
        { "VN", "🇻🇳" }, // Vietnam
        { "ID", "🇮🇩" }, // Indonesia
        { "PH", "🇵🇭" }, // Philippines
        { "MY", "🇲🇾" }, // Malaysia
        { "IN", "🇮🇳" }, // India
        { "PK", "🇵🇰" }, // Pakistan
        { "BD", "🇧🇩" }, // Bangladesh
        { "LK", "🇱🇰" }, // Sri Lanka
        { "KH", "🇰🇭" }, // Cambodia
        { "LA", "🇱🇦" }, // Laos
        { "MM", "🇲🇲" }, // Myanmar

        // Americas
        { "US", "🇺🇸" }, // United States
        { "CA", "🇨🇦" }, // Canada
        { "MX", "🇲🇽" }, // Mexico
        { "BR", "🇧🇷" }, // Brazil
        { "AR", "🇦🇷" }, // Argentina
        { "CL", "🇨🇱" }, // Chile
        { "CO", "🇨🇴" }, // Colombia

        // Europe
        { "GB", "🇬🇧" }, // United Kingdom
        { "DE", "🇩🇪" }, // Germany
        { "FR", "🇫🇷" }, // France
        { "IT", "🇮🇹" }, // Italy
        { "ES", "🇪🇸" }, // Spain
        { "RU", "🇷🇺" }, // Russia
        { "NL", "🇳🇱" }, // Netherlands
        { "CH", "🇨🇭" }, // Switzerland
        { "SE", "🇸🇪" }, // Sweden
        { "NO", "🇳🇴" }, // Norway
        { "DK", "🇩🇰" }, // Denmark
        { "FI", "🇫🇮" }, // Finland
        { "PL", "🇵🇱" }, // Poland
        { "CZ", "🇨🇿" }, // Czech Republic
        { "AT", "🇦🇹" }, // Austria
        { "GR", "🇬🇷" }, // Greece
        { "PT", "🇵🇹" }, // Portugal
        { "TR", "🇹🇷" }, // Turkey
        { "UA", "🇺🇦" }, // Ukraine
        { "RO", "🇷🇴" }, // Romania

        // Middle East & Central Asia
        { "AE", "🇦🇪" }, // United Arab Emirates
        { "SA", "🇸🇦" }, // Saudi Arabia
        { "IL", "🇮🇱" }, // Israel
        { "KZ", "🇰🇿" }, // Kazakhstan

        // Oceania
        { "AU", "🇦🇺" }, // Australia
        { "NZ", "🇳🇿" }, // New Zealand

        // Africa
        { "ZA", "🇿🇦" }, // South Africa
        { "EG", "🇪🇬" }, // Egypt
    };

    /// <summary>
    /// Converts country code to flag emoji using predefined mapping
    /// Example: "US" -> "🇺🇸", "CN" -> "🇨🇳"
    /// </summary>
    public static string? CountryToEmoji(this string? countryCode)
    {
        if (countryCode.IsNullOrEmpty())
        {
            return null;
        }

        return CountryEmojiMap.GetValueOrDefault(countryCode);
    }

    /// <summary>
    /// 常用节点的国家/地区中文名，未收录的回落为原始代码
    /// </summary>
    private static readonly Dictionary<string, string> CountryNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "CN", "中国" }, { "HK", "中国香港" }, { "MO", "中国澳门" }, { "TW", "中国台湾" },
        { "JP", "日本" }, { "KR", "韩国" }, { "SG", "新加坡" }, { "MY", "马来西亚" },
        { "TH", "泰国" }, { "VN", "越南" }, { "PH", "菲律宾" }, { "ID", "印度尼西亚" },
        { "IN", "印度" }, { "PK", "巴基斯坦" }, { "BD", "孟加拉国" }, { "LK", "斯里兰卡" },
        { "KH", "柬埔寨" }, { "LA", "老挝" }, { "MM", "缅甸" }, { "NP", "尼泊尔" },
        { "MN", "蒙古" }, { "KZ", "哈萨克斯坦" }, { "UZ", "乌兹别克斯坦" },
        { "US", "美国" }, { "CA", "加拿大" }, { "MX", "墨西哥" }, { "BR", "巴西" },
        { "AR", "阿根廷" }, { "CL", "智利" }, { "CO", "哥伦比亚" }, { "PE", "秘鲁" },
        { "GB", "英国" }, { "DE", "德国" }, { "FR", "法国" }, { "NL", "荷兰" },
        { "BE", "比利时" }, { "CH", "瑞士" }, { "AT", "奥地利" }, { "IT", "意大利" },
        { "ES", "西班牙" }, { "PT", "葡萄牙" }, { "IE", "爱尔兰" }, { "SE", "瑞典" },
        { "NO", "挪威" }, { "DK", "丹麦" }, { "FI", "芬兰" }, { "IS", "冰岛" },
        { "PL", "波兰" }, { "CZ", "捷克" }, { "SK", "斯洛伐克" }, { "HU", "匈牙利" },
        { "RO", "罗马尼亚" }, { "BG", "保加利亚" }, { "GR", "希腊" }, { "UA", "乌克兰" },
        { "RU", "俄罗斯" }, { "TR", "土耳其" }, { "AE", "阿联酋" }, { "SA", "沙特" },
        { "IL", "以色列" }, { "IR", "伊朗" }, { "EG", "埃及" }, { "ZA", "南非" },
        { "NG", "尼日利亚" }, { "AU", "澳大利亚" }, { "NZ", "新西兰" },
        { "SC", "塞舌尔" }, { "MU", "毛里求斯" }, { "CY", "塞浦路斯" }, { "MT", "马耳他" },
        { "LU", "卢森堡" }, { "EE", "爱沙尼亚" }, { "LV", "拉脱维亚" }, { "LT", "立陶宛" },
    };

    /// <summary>
    /// 国家/地区代码转中文名，例如 "US" -> "美国"；未收录时保留原值
    /// </summary>
    public static string CountryToName(this string? countryCode)
    {
        if (countryCode.IsNullOrEmpty())
        {
            return string.Empty;
        }

        if (CountryNameMap.TryGetValue(countryCode, out var name))
        {
            return name;
        }
        // 接口只返回国家全名时直接使用，短代码则统一大写
        return countryCode.Length <= 3 ? countryCode.ToUpperInvariant() : countryCode;
    }

    /// <summary>
    /// 拼出 "美国 · 洛杉矶" 这类地区显示文本
    /// </summary>
    public static string ToRegionDisplay(this string? countryCode, string? city, string? region = null)
    {
        var name = countryCode.CountryToName();
        if (name.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var sub = city.IsNotEmpty() ? city : region;
        return sub.IsNullOrEmpty() ? name : $"{name} · {sub}";
    }
}
