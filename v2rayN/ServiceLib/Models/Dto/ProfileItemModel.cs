namespace ServiceLib.Models.Dto;

[Serializable]
public partial class ProfileItemModel : ReactiveObject
{
    public bool IsActive { get; set; }
    public string IndexId { get; set; }
    public EConfigType ConfigType { get; set; }
    public string Remarks { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public string Network { get; set; }
    public string StreamSecurity { get; set; }
    public string Subid { get; set; }
    public string SubRemarks { get; set; }
    public int Sort { get; set; }

    [Reactive]
    public partial int Delay { get; set; }

    public decimal Speed { get; set; }

    [Reactive]
    public partial string DelayVal { get; set; }

    [Reactive]
    public partial string SpeedVal { get; set; }

    [Reactive]
    public partial string IpInfo { get; set; }

    /// <summary>出口 IP 所属国家/地区代码</summary>
    public string IpCountry { get; set; }

    /// <summary>出口 IP 所属州/省</summary>
    public string IpRegion { get; set; }

    /// <summary>出口 IP 所属城市/州</summary>
    public string IpCity { get; set; }

    /// <summary>IP 类型：住宅 / 机房 / 代理</summary>
    public string IpType { get; set; }

    /// <summary>类型标签，如 "原生" / "机房"</summary>
    [Reactive]
    public partial string IpTypeVal { get; set; }

    /// <summary>地区显示文本，如 "美国 · 洛杉矶"</summary>
    [Reactive]
    public partial string IpRegionVal { get; set; }

    /// <summary>纯净度评分，-1 表示未检测</summary>
    [Reactive]
    public partial int IpPurity { get; set; }

    /// <summary>纯净度显示文本，如 "65 IDC"</summary>
    [Reactive]
    public partial string IpPurityVal { get; set; }

    [Reactive]
    public partial string TodayUp { get; set; }

    [Reactive]
    public partial string TodayDown { get; set; }

    [Reactive]
    public partial string TotalUp { get; set; }

    [Reactive]
    public partial string TotalDown { get; set; }

    public string GetSummary()
    {
        var summary = $"[{ConfigType}] {Remarks}";
        if (!ConfigType.IsComplexType())
        {
            summary += $"({Address}:{Port})";
        }

        return summary;
    }
}
