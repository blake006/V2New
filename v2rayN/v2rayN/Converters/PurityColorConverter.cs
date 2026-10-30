using System.Windows.Media;

namespace v2rayN.Converters;

/// <summary>
/// IP 纯净度着色：原生绿、一般琥珀、较差橙、风险红
/// </summary>
public class PurityColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Native = new(Color.FromRgb(0x2E, 0x9E, 0x4F));
    private static readonly SolidColorBrush Fair = new(Color.FromRgb(0xC9, 0x9A, 0x00));
    private static readonly SolidColorBrush Poor = new(Color.FromRgb(0xE0, 0x7B, 0x00));
    private static readonly SolidColorBrush Risky = new(Color.FromRgb(0xD3, 0x2F, 0x2F));
    private static readonly SolidColorBrush Unknown = new(Color.FromRgb(0x9E, 0x9E, 0x9E));

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var purity = value?.ToString().ToInt() ?? -1;

        return IpQualityHelper.Level(purity) switch
        {
            3 => Native,
            2 => Fair,
            1 => Poor,
            0 => Risky,
            _ => Unknown,
        };
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return null;
    }
}
