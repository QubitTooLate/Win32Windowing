using System;

namespace Qtl.Win32Windowing;

[Flags]
public enum WindowStyles : uint
{
	Caption = 0x00C00000,
	SysMenu = 0x00080000,
	ThickFrame = 0x00040000,
	MinimizeBox = 0x00020000,
	MaximizeBox = 0x00010000,
	OverlappedWindow = Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox,
	Popup = 0x80000000,
}
