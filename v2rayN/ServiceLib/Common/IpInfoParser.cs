using System.Text.Json;

namespace ServiceLib.Common;

/// <summary>
/// 把各家 IP 接口的 JSON 归一化成 <see cref="IpInfoResult"/>。
/// 单独拆出来便于离线校验各家返回格式。
/// </summary>
public static class IpInfoParser
{
    /// <summary>
    /// 解析 IP 接口返回的 JSON，失败或接口报错时返回 null
    /// </summary>
    public static IpInfoResult? Parse(string? json)
    {
        var info = JsonUtils.Deserialize<IPAPIInfo>(json);
        if (info is null || string.Equals(info.status, "fail", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        return Build(info);
    }

    private static IpInfoResult Build(IPAPIInfo ipInfo)
    {
        var ip = ipInfo.ip ?? ipInfo.clientIp ?? ipInfo.ip_addr ?? ipInfo.query;
        var country = ipInfo.country_code
                      ?? ipInfo.countryCode
                      ?? ReadText(ipInfo.location, "country_code", "countryCode")
                      ?? ipInfo.country
                      ?? "unknown";
        var region = ipInfo.region
                     ?? ipInfo.regionName
                     ?? ReadText(ipInfo.location, "state", "region");
        var city = ipInfo.city ?? ReadText(ipInfo.location, "city");
        var asn = ipInfo.@as
                  ?? ReadText(ipInfo.asn, "org", "asn")
                  ?? ipInfo.isp
                  ?? ipInfo.org
                  ?? ReadText(ipInfo.company, "name");
        var (purity, ipType, flags) = CalcPurity(ipInfo);

        return new IpInfoResult(country, ip)
        {
            Region = region.IsNullOrEmpty() ? null : region,
            City = city.IsNullOrEmpty() ? null : city,
            Purity = purity,
            IpType = ipType,
            Flags = flags,
            Asn = asn.IsNullOrEmpty() ? null : asn,
        };
    }

    /// <summary>
    /// 从 JsonElement 里取文本，兼容「字段是字符串」和「字段是对象」两种返回形式
    /// </summary>
    private static string? ReadText(JsonElement? element, params string[] props)
    {
        if (element is null)
        {
            return null;
        }

        var e = element.Value;
        if (e.ValueKind == JsonValueKind.String)
        {
            return e.GetString();
        }
        if (e.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var prop in props)
        {
            if (!e.TryGetProperty(prop, out var v))
            {
                continue;
            }
            switch (v.ValueKind)
            {
                case JsonValueKind.String:
                    return v.GetString();

                case JsonValueKind.Number:
                    return v.ToString();
            }
        }
        return null;
    }

    /// <summary>
    /// 把 IP 属性折算成 0-100 的纯净度：住宅/原生 IP 接近满分，
    /// 机房 IP、被标记为代理/VPN/Tor、有滥用记录会逐项扣分。
    /// 优先使用 ip-api.com 的 hosting/proxy 标记，其次是 ipapi.is 的 is_* 标记。
    /// </summary>
    private static (int? Purity, string? IpType, string? Flags) CalcPurity(IPAPIInfo info)
    {
        var isHosting = info.hosting ?? info.is_datacenter;
        var isProxy = info.proxy ?? info.is_proxy;
        var isVpn = info.is_vpn;
        var isTor = info.is_tor;
        var isAbuser = info.is_abuser;
        var isCrawler = info.is_crawler;
        var isBogon = info.is_bogon;

        // 只有拿到 hosting/proxy/vpn/tor 这类判定字段才算检测过，
        // 单独一个 is_bogon 不足以给出评分，否则会误报成"原生 100"
        if (isHosting is null
            && isProxy is null
            && isVpn is null
            && isTor is null
            && isAbuser is null
            && isCrawler is null)
        {
            return (null, null, null);
        }

        var score = 100;
        List<string> flags = [];
        var ipType = "住宅";

        if (isTor == true) { score -= 45; flags.Add("Tor"); ipType = "代理"; }
        if (isProxy == true) { score -= 35; flags.Add("代理"); ipType = "代理"; }
        if (isVpn == true) { score -= 30; flags.Add("VPN"); ipType = "代理"; }
        if (isHosting == true)
        {
            score -= 45;
            flags.Add("IDC");
            if (ipType != "代理")
            {
                ipType = "机房";
            }
        }
        if (isAbuser == true) { score -= 25; flags.Add("滥用"); }
        if (isCrawler == true) { score -= 10; flags.Add("爬虫"); }
        if (isBogon == true) { score -= 50; flags.Add("保留地址"); }

        return (Math.Clamp(score, 0, 100), ipType, flags.Count > 0 ? string.Join("/", flags) : null);
    }
}
