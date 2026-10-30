using System.Text.Json;

namespace ServiceLib.Models.Dto;

/// <summary>
/// 兼容多家 IP 信息接口的返回结构：
/// ip-api.com / ipapi.is / ip.sb / ipinfo.io
/// 注意：同一字段在不同接口里可能是字符串也可能是对象（如 asn / company），
/// 一律用 JsonElement 接收，避免反序列化整体失败
/// </summary>
internal class IPAPIInfo
{
    public string? status { get; set; }
    public string? message { get; set; }

    public string? ip { get; set; }
    public string? clientIp { get; set; }
    public string? ip_addr { get; set; }
    public string? query { get; set; }

    public string? country { get; set; }
    public string? country_name { get; set; }
    public string? country_code { get; set; }
    public string? countryCode { get; set; }
    public string? region { get; set; }
    public string? regionName { get; set; }
    public string? city { get; set; }

    /// <summary>ip-api.com：是否为机房/托管 IP</summary>
    public bool? hosting { get; set; }

    /// <summary>ip-api.com：是否为代理/VPN</summary>
    public bool? proxy { get; set; }

    /// <summary>ip-api.com：是否为移动网络</summary>
    public bool? mobile { get; set; }

    public bool? is_bogon { get; set; }
    public bool? is_datacenter { get; set; }
    public bool? is_vpn { get; set; }
    public bool? is_proxy { get; set; }
    public bool? is_tor { get; set; }
    public bool? is_abuser { get; set; }
    public bool? is_crawler { get; set; }

    public string? isp { get; set; }
    public string? org { get; set; }
    public string? @as { get; set; }

    public JsonElement? asn { get; set; }
    public JsonElement? company { get; set; }
    public JsonElement? location { get; set; }
}

/// <summary>
/// 单个节点的出口 IP 画像：地区 + 类型 + 纯净度
/// </summary>
public readonly record struct IpInfoResult(string Country, string? Ip)
{
    /// <summary>国家/地区代码，如 US；接口只给全名时保留全名</summary>
    public string? Region { get; init; }

    /// <summary>城市，可能为空</summary>
    public string? City { get; init; }

    /// <summary>0-100 的纯净度评分，null 表示未检测到</summary>
    public int? Purity { get; init; }

    /// <summary>IP 类型：住宅 / 机房 / 代理</summary>
    public string? IpType { get; init; }

    /// <summary>扣分原因，如 "IDC/VPN"</summary>
    public string? Flags { get; init; }

    /// <summary>ASN 组织名</summary>
    public string? Asn { get; init; }

    public override string ToString()
    {
        var emoji = Utils.IsWindows() ? null : Country.CountryToEmoji();
        return $"{emoji}({Country}) {Ip}";
    }
}
