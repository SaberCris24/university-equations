using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using UniversityEquations.Services;

namespace UniversityEquations.Pages.Calculator
{
    public sealed partial class CalculatorPage : Page
    {
        public CalculatorPage()
        {
            this.InitializeComponent();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string M = MFunctionTextBox.Text.Trim();
                string N = NFunctionTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(M) || string.IsNullOrWhiteSpace(N))
                {
                    ShowError("Please enter both M(x,y) and N(x,y) functions.");
                    return;
                }

                var (dMdy, dNdx) = ExactDifferentialService.GetPartialDerivatives(M, N);
                bool isExact = ExactDifferentialService.IsExact(M, N);
                ResultsCard.Visibility = Visibility.Visible;

                if (isExact)
                {
                    ExactnessResultText.Text = $"Yes, this is an exact differential equation.\n\n" +
                                             $"∂M/∂y = ∂N/∂x\n" +
                                             $"∂M/∂y = {dMdy}\n" +
                                             $"∂N/∂x = {dNdx}\n\n" +
                                             $"The equation {M}dx + {N}dy = 0 is exact.";
                }
                else
                {
                    ExactnessResultText.Text = $"No, this is not an exact differential equation.\n\n" +
                                             $"∂M/∂y ≠ ∂N/∂x\n" +
                                             $"∂M/∂y = {dMdy}\n" +
                                             $"∂N/∂x = {dNdx}\n\n" +
                                             $"The equation {M}dx + {N}dy = 0 is not exact.\n" +
                                             "You may need to find an integrating factor.";
                }

                // Por ahora ocultamos la solución ya que eso se implementará después
                SolutionText.Text = "";
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }

        private async void ShowError(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}