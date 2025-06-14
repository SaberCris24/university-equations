using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.UI;

namespace UniversityEquations.Helpers
{
    public static class TitleBarHelper
    {
        public static void SetCaptionButtonColors(Window window, Color color)
        {
            try
            {
                var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Win32Interop.GetWindowIdFromWindow(handle);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                if (appWindow.TitleBar.ExtendsContentIntoTitleBar)
                {
                    appWindow.TitleBar.ButtonForegroundColor = color;
                    appWindow.TitleBar.ButtonHoverForegroundColor = color;
                    appWindow.TitleBar.ButtonPressedForegroundColor = color;
                    appWindow.TitleBar.ButtonInactiveForegroundColor = color;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set caption button colors: {ex.Message}");
            }
        }
    }
}