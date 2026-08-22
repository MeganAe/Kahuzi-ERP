using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace GestionCommerciale.Services;

public class ThemeService
{
    private readonly PaletteHelper _paletteHelper = new();

    // Teal sobre en mode clair, gris anthracite en mode sombre.
    private static readonly Color SidebarLight = (Color)ColorConverter.ConvertFromString("#0F766E");
    private static readonly Color SidebarDark = (Color)ColorConverter.ConvertFromString("#1F2937");

    public bool IsDarkTheme { get; private set; }

    public void ApplyTheme(bool isDark)
    {
        IsDarkTheme = isDark;

        Theme theme = _paletteHelper.GetTheme();
        theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);

        if (Application.Current != null)
        {
            Application.Current.Resources["SidebarBackgroundBrush"] =
                new SolidColorBrush(isDark ? SidebarDark : SidebarLight);

            foreach (Window window in Application.Current.Windows)
            {
                ApplyTitleBarTheme(window, isDark);
            }
        }
    }

    public void Toggle() => ApplyTheme(!IsDarkTheme);

    public static void ApplyTitleBarTheme(Window window, bool isDark)
    {
        if (window is null) return;

        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            window.SourceInitialized += (s, e) =>
            {
                var h = new WindowInteropHelper(window).Handle;
                if (h != IntPtr.Zero) SetTitleBarDarkMode(h, isDark);
            };
            return;
        }

        SetTitleBarDarkMode(handle, isDark);
    }

    private static void SetTitleBarDarkMode(IntPtr handle, bool isDark)
    {
        int enabled = isDark ? 1 : 0;

        // DWMWA_USE_IMMERSIVE_DARK_MODE (20 pour Windows 10 2004+ et Win 11, 19 pour les versions précédentes)
        if (DwmSetWindowAttribute(handle, 20, ref enabled, sizeof(int)) != 0)
        {
            DwmSetWindowAttribute(handle, 19, ref enabled, sizeof(int));
        }

        // DWMWA_CAPTION_COLOR (35) et DWMWA_TEXT_COLOR (36) pour Windows 11 / Windows 10 récent
        int captionColor = isDark ? 0x002A1F1F : 0x00FFFFFF; // BGR format
        int textColor = isDark ? 0x00FFFFFF : 0x00000000;
        DwmSetWindowAttribute(handle, 35, ref captionColor, sizeof(int));
        DwmSetWindowAttribute(handle, 36, ref textColor, sizeof(int));

        // Forcer le Desktop Window Manager (DWM) à redessiner immédiatement la barre de titre
        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOACTIVATE);
    }

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
}
