using System;
using Qtl.Win32Windowing.Dwm;

namespace Qtl.Win32Windowing;

public record Win32WindowProperties(
	string Title,
	string ClassName,
	WindowStyles Styles, // replace style enums with something else?
	ExtendedWindowStyles ExtendedStyles,
	Rect Rect,
	IntPtr Parent,
	int Visibility, // make enum
	LayeredProperties Layered,
	DwmProperties Dwm,
	bool IsComposed
)
{

}
