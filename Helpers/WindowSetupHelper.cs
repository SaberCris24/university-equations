using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace UniversityEquations.Helpers
{
    public static class WindowSetupHelper
    {
        public static void SetupTitleBar(Window window, UIElement customTitleBar)
        {
            // Hide default title bar
            var appWindow = GetAppWindow(window);
            if (appWindow is not null)
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            }

            // Set custom title bar
            window.ExtendsContentIntoTitleBar = true;
            window.SetTitleBar(customTitleBar);
        }

        public static void SetupWindowSize(Window window, int minWidth, int minHeight)
        {
            var appWindow = GetAppWindow(window);
            if (appWindow is not null)
            {
                var preferredSize = new Windows.Graphics.SizeInt32(minWidth, minHeight);
                appWindow.Resize(preferredSize);
            }
        }

        public static void SetupWindowIcon(Window window)
        {
            var appWindow = GetAppWindow(window);
            if (appWindow is not null)
            {
                appWindow.SetIcon("Assets/icon.ico");
            }
        }

        private static AppWindow GetAppWindow(Window window)
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            return AppWindow.GetFromWindowId(windowId);
        }
    }
}