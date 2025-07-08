using MudBlazor;

namespace LalaHealthCare.App.Themes;

public static class CustomTheme
{
    public static readonly MudTheme DefaultTheme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#3BAFBF",           // Tu color azul
            PrimaryContrastText = "#ffffff",
            PrimaryDarken = "#1e7bb8",     // Versión más oscura
            PrimaryLighten = "#5bb3f7",    // Versión más clara

            Secondary = "#ff6b35",         // Color secundario opcional
            SecondaryContrastText = "#ffffff",

            Tertiary = "#28a745",          // Color terciario opcional
            TertiaryContrastText = "#ffffff",

            Background = "#ffffff",
            BackgroundGray = "#f5f5f5",
            Surface = "#ffffff",

            AppbarBackground = "#2d9bf5",  // Mismo color para AppBar
            AppbarText = "#ffffff",

            DrawerBackground = "#ffffff",
            DrawerText = "rgba(0,0,0, 0.87)",

            TextPrimary = "rgba(0,0,0, 0.87)",
            TextSecondary = "rgba(0,0,0, 0.6)",

            ActionDefault = "#adadb1",
            ActionDisabled = "rgba(0,0,0, 0.26)",
            ActionDisabledBackground = "rgba(0,0,0, 0.12)",

            Divider = "rgba(0,0,0, 0.12)",
            DividerLight = "rgba(0,0,0, 0.06)",

            TableLines = "rgba(0,0,0, 0.12)",
            LinesDefault = "rgba(0,0,0, 0.12)",
            LinesInputs = "rgba(0,0,0, 0.42)"
        },

        PaletteDark = new PaletteDark()
        {
            Primary = "#3BAFBF",
            PrimaryContrastText = "#ffffff",
            PrimaryDarken = "#1a5c87",
            PrimaryLighten = "#6bc2ff",

            Secondary = "#ff6b35",
            SecondaryContrastText = "#ffffff",

            Background = "#1a1a1a",
            BackgroundGray = "#151515",
            Surface = "#1e1e1e",

            AppbarBackground = "#1a1a1a",
            AppbarText = "#ffffff",

            DrawerBackground = "#1a1a1a",
            DrawerText = "#ffffff",

            TextPrimary = "#ffffff",
            TextSecondary = "rgba(255,255,255, 0.7)",

            ActionDefault = "#74788d",
            ActionDisabled = "rgba(255,255,255, 0.26)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",

            Divider = "rgba(255,255,255, 0.12)",
            DividerLight = "rgba(255,255,255, 0.06)",

            TableLines = "rgba(255,255,255, 0.12)",
            LinesDefault = "rgba(255,255,255, 0.12)",
            LinesInputs = "rgba(255,255,255, 0.3)"
        }
    };
}
