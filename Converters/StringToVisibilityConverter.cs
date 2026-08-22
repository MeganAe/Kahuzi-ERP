using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GestionCommerciale.Converters;

/// <summary>Visible si la chaîne n'est ni nulle ni vide, Collapsed sinon.</summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
