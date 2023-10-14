using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Qtl.Win32Windowing.Dwm;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Qtl.Win32Windowing;

public unsafe class Win32Window : IDisposable
{
	[UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
	private static LRESULT WindowProcedure(HWND windowHandle, uint message, WPARAM w, LPARAM l)
	{
		var handle = Native.GetWindowLongPtr(windowHandle, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA);
		if (handle == IntPtr.Zero) { return Native.DefWindowProc(windowHandle, message, w, l); }
		var win32Window = GCHandle.FromIntPtr(handle).Target as Win32Window ?? throw new UnreachableException();
		if (win32Window.OnMessage(message, w, l)) { return default; }
		return Native.DefWindowProc(windowHandle, message, w, l);
	}

	private static bool TryCreateWindow(Win32WindowProperties properties, out HWND windowHandle)
	{
		var extendedStyles = (WINDOW_EX_STYLE)properties.ExtendedStyles |
			(properties.IsComposed ? WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP : 0) |
			WINDOW_EX_STYLE.WS_EX_LAYERED;

		windowHandle = Native.CreateWindowEx(
			extendedStyles,
			properties.ClassName,
			properties.Title,
			(WINDOW_STYLE)properties.Styles,
			properties.Rect.X,
			properties.Rect.Y,
			properties.Rect.Width,
			properties.Rect.Height,
			(HWND)properties.Parent,
			HMENU.Null,
			HINSTANCE.Null,
			(void*)null
		);

		return windowHandle != HWND.Null;
	}

	private static bool TryCreateWindowClass(string className)
	{
		fixed (char* classNamePtr = className)
		{
			var windowClass = new WNDCLASSEXW
			{
				cbSize = (uint)sizeof(WNDCLASSEXW),
				hCursor = Native.LoadCursor(HINSTANCE.Null, Native.IDC_ARROW),
				lpfnWndProc = &WindowProcedure,
				style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_OWNDC,
				lpszClassName = classNamePtr
			};

			return Native.RegisterClassEx(&windowClass) != 0;
		}
	}

	private static bool TrySetLayeredProperties(HWND windowHandle, LayeredProperties properties)
	{
		if (!Native.SetLayeredWindowAttributes(
			windowHandle,
			(COLORREF)properties.CutoutColor,
			properties.AlphaValue,
			properties.IsAlphaNotCutout ?
			LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA :
			LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_COLORKEY
		))
		{
			return false;
		}

		return true;
	}

	private static bool TrySetDwmProperties(HWND windowHandle, DwmProperties properties)
	{
		if (properties.BackdropType != 0)
		{
			var backdropType = properties.BackdropType;
			if (Native.DwmSetWindowAttribute(
				windowHandle,
				DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
				&backdropType,
				sizeof(int)
			) != HRESULT.S_OK)
			{
				return false;
			}
		}

		var useDarkTheme = properties.UseDarkTheme ? 1 : 0;
		if (Native.DwmSetWindowAttribute(
			windowHandle,
			DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
			&useDarkTheme,
			sizeof(int)
		) != HRESULT.S_OK)
		{
			return false;
		}

		var cornerPreferences = properties.CornerPreference;
		if (Native.DwmSetWindowAttribute(
			windowHandle,
			DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
			&cornerPreferences,
			sizeof(int)
		) != HRESULT.S_OK)
		{
			return false;
		}

		return true;
	}

	protected static void PumpMessages()
	{
		MSG msg;
		while (Native.GetMessage(&msg, HWND.Null, 0, 0))
		{
			_ = Native.DispatchMessage(&msg);
		}
	}

	private GCHandle _unmanagedHandle;
	private IntPtr _windowHandle;
	private bool _isDisposed;

	protected virtual bool OnMessage(uint message, nuint w, nint l)
	{
		switch (message)
		{
			case Native.WM_CLOSE:
				_ = Native.DestroyWindow((HWND)_windowHandle);
				break;
			case Native.WM_DESTROY:
				Native.PostQuitMessage(0);
				break;
			default:
				return false;
		}

		return true;
	}

	protected bool TryCreate(Win32WindowProperties properties)
	{
		if (
			!TryCreateWindowClass(properties.ClassName) ||
			!TryCreateWindow(properties, out var windowHandle) ||
			!TrySetLayeredProperties(windowHandle, properties.Layered) ||
			!TrySetDwmProperties(windowHandle, properties.Dwm))
		{
			return false;
		}

		_windowHandle = windowHandle;
		_unmanagedHandle = GCHandle.Alloc(this, GCHandleType.Normal);
		_ = Native.SetWindowLongPtr(windowHandle, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA, GCHandle.ToIntPtr(_unmanagedHandle));

		_ = Native.ShowWindow(windowHandle, (SHOW_WINDOW_CMD)properties.Visibility);

		return true;
	}

	protected void Dispose(bool disposing)
	{
		_ = disposing;

		if (_isDisposed) { return; }
		_isDisposed = true;

		if (_unmanagedHandle.IsAllocated)
		{
			_unmanagedHandle.Free();
		}
	}

	public void Dispose()
	{
		Dispose(true);
	}

	~Win32Window()
	{
		Dispose(false);
		GC.SuppressFinalize(this);
	}
}
