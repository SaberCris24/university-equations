using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;

namespace UniversityEquations.Helpers
{
    public static class BackdropHelper
    {
        public enum BackdropType
        {
            Mica,
            MicaAlt,
            Acrylic
        }

        public static bool SetBackdrop(Window window, BackdropType type)
        {
            if (window == null)
                return false;

            try
            {
                window.SystemBackdrop = type switch
                {
                    BackdropType.Mica when MicaController.IsSupported() => 
                        new MicaBackdrop { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base },
                    BackdropType.MicaAlt when MicaController.IsSupported() => 
                        new MicaBackdrop { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt },
                    BackdropType.Acrylic when DesktopAcrylicController.IsSupported() => 
                        new DesktopAcrylicBackdrop(),
                    _ => new MicaBackdrop() // Default to regular Mica
                };

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set backdrop: {ex.Message}");
                return false;
            }
        }

        public static BackdropType GetBestAvailableBackdrop()
        {
            if (MicaController.IsSupported())
                return BackdropType.Mica;
            if (DesktopAcrylicController.IsSupported())
                return BackdropType.Acrylic;
            return BackdropType.Mica;
        }
    }
}