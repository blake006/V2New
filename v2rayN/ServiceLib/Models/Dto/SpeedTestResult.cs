namespace ServiceLib.Models.Dto;

[Serializable]
public class SpeedTestResult
{
    public string? IndexId { get; set; }

    public string? Delay { get; set; }

    public string? Speed { get; set; }

    public string? IpInfo { get; set; }

    public string? IpCountry { get; set; }

    public string? IpRegion { get; set; }

    public string? IpCity { get; set; }

    public string? IpType { get; set; }

    public int? IpPurity { get; set; }

    public string? IpFlags { get; set; }
}
