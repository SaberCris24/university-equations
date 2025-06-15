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

                // Update partial derivatives display
                DMdyText.Text = dMdy;
                DNdxText.Text = dNdx;

                if (isExact)
                {
                    ExactnessResultText.Text = "Yes, this is an exact differential equation.";
                    IntegratingFactorPanel.Visibility = Visibility.Collapsed;
                    SolutionText.Text = $"The equation {M}dx + {N}dy = 0 is exact.\n" +
                                      "The partial derivatives are equal.";
                }
                else
                {
                    ExactnessResultText.Text = "No, this is not an exact differential equation.";
                    IntegratingFactorPanel.Visibility = Visibility.Visible;

                    // Find integrating factor
                    var integratingFactorResult = IntegratingFactorService.FindIntegratingFactor(M, N);

                    if (integratingFactorResult.Found)
                    {
                        IntegratingFactorText.Text = $"{integratingFactorResult.Type} = {integratingFactorResult.Factor}";
                        NewEquationText.Text = integratingFactorResult.NewEquation;

                        if (integratingFactorResult.IsExactAfterMultiplying)
                        {
                            VerificationText.Text = "The equation becomes exact after multiplying by the integrating factor.";
                            
                            // Show the new partial derivatives
                            var (newDMdy, newDNdx) = ExactDifferentialService.GetPartialDerivatives(
                                integratingFactorResult.NewM, 
                                integratingFactorResult.NewN);
                            
                            VerificationText.Text += $"\n\nNew partial derivatives:\n" +
                                                   $"∂(μM)/∂y = {newDMdy}\n" +
                                                   $"∂(μN)/∂x = {newDNdx}";
                        }
                        else
                        {
                            VerificationText.Text = "Warning: The equation is still not exact after multiplying by this integrating factor.";
                        }
                    }
                    else
                    {
                        IntegratingFactorText.Text = integratingFactorResult.Factor;
                        NewEquationText.Text = "N/A";
                        VerificationText.Text = "N/A";
                    }

                    SolutionText.Text = $"The equation {M}dx + {N}dy = 0 is not exact.\n" +
                                      "The partial derivatives are not equal.";
                }
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