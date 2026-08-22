using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GestionCommerciale.Converters;

/// <summary>
/// Retourne Visible si la valeur numérique est égale à 0, Collapsed sinon.
/// Utile pour afficher un message "aucun résultat" / "aucune alerte".
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var count = value switch
        {
            int i => i,
            long l => (int)l,
            _ => 0
        };
        return count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
