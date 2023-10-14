namespace Qtl.Win32Windowing;

public record struct LayeredProperties(
	bool IsClickTrough,
	bool IsAlphaNotCutout,
	uint CutoutColor,
	byte AlphaValue
)
{
	public static LayeredProperties Default => new(false, true, 0, 255);
}
