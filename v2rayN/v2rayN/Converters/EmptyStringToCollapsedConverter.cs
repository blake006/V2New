namespace v2rayN.Converters;

/// <summary>
/// 字符串为空时折叠元素，用于隐藏没有内容的类型徽章
/// </summary>
public class EmptyStringToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value?.ToString().IsNullOrEmpty() == false ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return null;
    }
}
