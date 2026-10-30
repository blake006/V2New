namespace ServiceLib.Models.Entities;

[Serializable]
public class ProfileExItem
{
    [PrimaryKey]
    public string IndexId { get; set; }

    public int Delay { get; set; }
    public decimal Speed { get; set; }
    public int Sort { get; set; }
    public string? Message { get; set; }
    public string? IpInfo { get; set; }

    /// <summary>出口 IP 所属国家/地区代码，如 US</summary>
    public string? IpCountry { get; set; }

    /// <summary>出口 IP 所属州/省</summary>
    public string? IpRegion { get; set; }

    /// <summary>出口 IP 所属城市/州</summary>
    public string? IpCity { get; set; }

    /// <summary>IP 类型：住宅 / 机房 / 代理</summary>
    public string? IpType { get; set; }

    /// <summary>0-100 纯净度评分，null 表示未检测</summary>
    public int? IpPurity { get; set; }

    /// <summary>扣分原因，如 IDC/VPN</summary>
    public string? IpFlags { get; set; }
}
