using Qtl.Win32Windowing;
using Qtl.Win32Windowing.Dwm;

using var window = new MainWindow();
window.Run();

return;

class MainWindow : Win32Window
{
	public void Run()
	{
		var properties = new Win32WindowProperties(
			"Hello, world!",
			"Qtl.Win32WindowingWindowClass",
			WindowStyles.OverlappedWindow,
			ExtendedWindowStyles.Layered,
			Rect.Default,
			default,
			1,
			LayeredProperties.Default,
			DwmProperties.Default,
			true
		);

		if (TryCreate(properties))
		{
			PumpMessages();
		}
	}

	protected override bool OnMessage(uint message, nuint w, nint l)
	{
		return base.OnMessage(message, w, l);
	}
}
