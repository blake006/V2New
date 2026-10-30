namespace ServiceLib.Common;

/// <summary>
/// IP 纯净度的展示与排序辅助
/// </summary>
public static class IpQualityHelper
{
    /// <summary>未知纯净度的占位符</summary>
    public const string Unknown = "--";

    /// <summary>
    /// 生成 "65" 这类纯净度分数文本
    /// </summary>
    public static string FormatPurity(int? purity)
    {
        return purity is null ? string.Empty : $"{purity}";
    }

    /// <summary>
    /// 生成类型标签：住宅且高分显示"原生"，其余显示 住宅 / 机房 / 代理
    /// </summary>
    public static string FormatType(string? ipType, int? purity)
    {
        if (purity is null)
        {
            return string.Empty;
        }
        if (ipType.IsNullOrEmpty())
        {
            return Unknown;
        }
        return ipType == "住宅" && purity >= 90 ? "原生" : ipType;
    }

    /// <summary>
    /// 把纯净度换算成 0-3 的等级，用于着色：3 原生、2 一般、1 较差、0 风险
    /// </summary>
    public static int Level(int? purity)
    {
        return purity switch
        {
            null => -1,
            >= 85 => 3,
            >= 60 => 2,
            >= 30 => 1,
            _ => 0,
        };
    }
}
