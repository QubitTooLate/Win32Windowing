using System;

namespace Qtl.Win32Windowing;

[Flags]
public enum ExtendedWindowStyles : uint
{
	Topmost = 0x00000008,
	ToolWindow = 0x00000080,
	NoActivate = 0x08000000,
	Layered = 0x00080000,
}
