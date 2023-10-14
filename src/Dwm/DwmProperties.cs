namespace Qtl.Win32Windowing.Dwm;

public record struct DwmProperties(
	BackdropType BackdropType,
	bool UseDarkTheme,
	CornerPreference CornerPreference
)
{
	public static DwmProperties Default => new(BackdropType.Mica, true, CornerPreference.Default);
}
