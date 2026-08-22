using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GestionCommerciale.Converters;

/// <summary>Retourne un fond rouge pâle si vrai (alerte), transparent sinon.</summary>
public class BoolToAlertBrushConverter : IValueConverter
{
    private static readonly Brush AlertBrush = new SolidColorBrush(Color.FromArgb(40, 244, 67, 54));
    private static readonly Brush NormalBrush = Brushes.Transparent;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? AlertBrush : NormalBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
