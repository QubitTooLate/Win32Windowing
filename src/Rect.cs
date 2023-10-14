namespace Qtl.Win32Windowing;

public record struct Rect(
	int X,
	int Y,
	int Width,
	int Height
)
{
	public static Rect Default => new(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
}
