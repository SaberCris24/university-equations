using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UniversityEquations.Helpers;
using UniversityEquations.Pages.Calculator;
using UniversityEquations.Pages.About;
using UniversityEquations.Pages.Settings;
using System;

namespace UniversityEquations
{
    public sealed partial class MainWindow : Window
    {
        #region Properties
        public NavigationView NavigationViewControl => NavView;
        private NavigationHelper _navigationHelper;
        private Frame ContentFrame => contentFrame;
        #endregion

        public MainWindow()
        {
            try
            {
                this.InitializeComponent();

                // Initialize navigation helper
                _navigationHelper = new NavigationHelper(NavView);

                // Set up window components
                WindowSetupHelper.SetupWindowSize(this, 800, 600);
                WindowSetupHelper.SetupWindowIcon(this);

                WindowSetupHelper.SetupTitleBar(this, AppTitleBar);


                // Setup theme
                SetupTheme();

                // Navigate to Calculator page as default
                contentFrame.Navigate(typeof(CalculatorPage));

                // Initialize NavigationView
                _navigationHelper.LoadNavigationViewPosition();

                // Subscribe to events
                NavView.DisplayModeChanged += NavView_DisplayModeChanged;
                if (Content is FrameworkElement rootElement)
                {
                    rootElement.ActualThemeChanged += MainWindow_ActualThemeChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing MainWindow: {ex.Message}");
                throw; // Re-throw to ensure window creation fails if initialization fails
            }
        }

        private void SetupTheme()
        {
            if (Content is FrameworkElement rootElement)
            {
                ElementTheme savedTheme = ThemeHelper.LoadSavedTheme();
                rootElement.RequestedTheme = savedTheme;
                ThemeHelper.UpdateCaptionButtonColors(this, rootElement.ActualTheme);
            }
        }

        public void SaveNavigationViewPosition(bool isLeftMode)
        {
            _navigationHelper.SaveNavigationViewPosition(isLeftMode);
        }

        public void UpdateNavigationViewMode(bool isLeftMode)
        {
            _navigationHelper.UpdateNavigationViewMode(isLeftMode);
        }

        private void MainWindow_ActualThemeChanged(FrameworkElement sender, object args)
        {
            try
            {
                if (Content is FrameworkElement rootElement)
                {
                    ThemeHelper.UpdateCaptionButtonColors(this, rootElement.ActualTheme);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling theme change: {ex.Message}");
            }
        }

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            _navigationHelper.SaveNavigationViewPosition(sender.PaneDisplayMode != NavigationViewPaneDisplayMode.Top);
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (args.IsSettingsSelected)
                {
                    contentFrame.Navigate(typeof(SettingsPage));
                }
                else if (args.SelectedItem is NavigationViewItem selectedItem)
                {
                    switch (selectedItem.Tag.ToString())
                    {
                        case "calculator":
                            contentFrame.Navigate(typeof(CalculatorPage));
                            break;
                        case "about":
                            contentFrame.Navigate(typeof(AboutPage));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling navigation: {ex.Message}");
            }
        }
    }
}