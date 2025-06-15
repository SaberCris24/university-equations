using System;
using Microsoft.UI.Xaml;
using UniversityEquations.Helpers;
using Windows.Storage;

namespace UniversityEquations
{
    /// <summary>
    /// Main application class that handles window management, themes, and backdrops
    /// </summary>
    public partial class App : Application
    {
        // Main application window reference
        public static Window MainWindow { get; private set; } = null!;
        
        // Tracks the current application theme
        public static ElementTheme CurrentTheme { get; private set; } = ElementTheme.Default;
        
        // Tracks the current backdrop effect
        public static BackdropHelper.BackdropType CurrentBackdrop { get; private set; } = BackdropHelper.BackdropType.Mica;

        /// <summary>
        /// Initializes the application components
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Called when the application is launched
        /// Sets up the main window, backdrop, and theme
        /// </summary>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                // Create and configure the main window
                MainWindow = new MainWindow();
                MainWindow.ExtendsContentIntoTitleBar = true;

                // Set initial backdrop
                CurrentBackdrop = BackdropHelper.BackdropType.Mica;
                BackdropHelper.SetBackdrop(MainWindow, CurrentBackdrop);

                // Load and apply saved theme
                if (MainWindow.Content is FrameworkElement rootElement)
                {
                    ElementTheme savedTheme = LoadSavedTheme();
                    rootElement.RequestedTheme = savedTheme;
                    CurrentTheme = savedTheme;
                }

                MainWindow.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnLaunched: {ex.Message}");
                // Set defaults if initialization fails
                CurrentTheme = ElementTheme.Default;
            }
        }

        /// <summary>
        /// Sets the application theme and persists the selection
        /// </summary>
        public static void SetTheme(ElementTheme theme)
        {
            try
            {
                if (MainWindow?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = theme;
                    CurrentTheme = theme;
                    ThemeHelper.UpdateCaptionButtonColors(MainWindow, theme);
                    SaveTheme(theme);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the window backdrop effect and persists the selection
        /// </summary>
        public static void SetBackdrop(BackdropHelper.BackdropType type)
        {
            try
            {
                if (BackdropHelper.SetBackdrop(MainWindow, type))
                {
                    CurrentBackdrop = type;
                    SaveBackdrop(type);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting backdrop: {ex.Message}");
            }
        }

        /// <summary>
        /// Persists the theme selection to application settings
        /// </summary>
        private static void SaveTheme(ElementTheme theme)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["AppTheme"] = theme.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the previously saved theme from application settings
        /// </summary>
        private static ElementTheme LoadSavedTheme()
        {
            try
            {
                var savedTheme = ApplicationData.Current?.LocalSettings?.Values["AppTheme"] as string;
                return savedTheme switch
                {
                    nameof(ElementTheme.Dark) => ElementTheme.Dark,
                    nameof(ElementTheme.Light) => ElementTheme.Light,
                    _ => ElementTheme.Default
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme: {ex.Message}");
                return ElementTheme.Default;
            }
        }

        /// <summary>
        /// Persists the backdrop selection to application settings
        /// </summary>
        private static void SaveBackdrop(BackdropHelper.BackdropType type)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["AppBackdrop"] = type.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving backdrop: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the previously saved backdrop from application settings
        /// </summary>
        private static BackdropHelper.BackdropType LoadSavedBackdrop()
        {
            try
            {
                var savedBackdrop = ApplicationData.Current?.LocalSettings?.Values["AppBackdrop"] as string;
                return savedBackdrop switch
                {
                    nameof(BackdropHelper.BackdropType.Mica) => BackdropHelper.BackdropType.Mica,
                    nameof(BackdropHelper.BackdropType.Acrylic) => BackdropHelper.BackdropType.Acrylic,
                    _ => BackdropHelper.BackdropType.Mica
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading backdrop: {ex.Message}");
                return BackdropHelper.BackdropType.Mica;
            }
        }
    }
}