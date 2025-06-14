using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UniversityEquations.Helpers;
using Windows.Storage;

namespace UniversityEquations
{
    public partial class App : Application
    {
        public static Window MainWindow { get; private set; } = null!;
        public static ElementTheme CurrentTheme { get; set; } = ElementTheme.Default;
        public static BackdropHelper.BackdropType CurrentBackdrop { get; set; } = BackdropHelper.BackdropType.Mica;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            
            // Cargar tema guardado
            CurrentTheme = ThemeHelper.LoadSavedTheme();
            if (MainWindow.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = CurrentTheme;
                ThemeHelper.UpdateCaptionButtonColors(MainWindow, CurrentTheme);
            }

            // Aplicar backdrop
            BackdropHelper.SetBackdrop(MainWindow, CurrentBackdrop);
            
            MainWindow.Activate();
        }

        public static void SetTheme(ElementTheme theme)
        {
            try
            {
                if (MainWindow?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = theme;
                    CurrentTheme = theme;
                    ThemeHelper.UpdateCaptionButtonColors(MainWindow, theme);

                    // Guardar preferencia
                    ApplicationData.Current.LocalSettings.Values["AppTheme"] = theme.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting theme: {ex.Message}");
            }
        }

        public static void SetBackdrop(BackdropHelper.BackdropType type)
        {
            try
            {
                if (BackdropHelper.SetBackdrop(MainWindow, type))
                {
                    CurrentBackdrop = type;
                    ApplicationData.Current.LocalSettings.Values["AppBackdrop"] = type.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting backdrop: {ex.Message}");
            }
        }
    }
}