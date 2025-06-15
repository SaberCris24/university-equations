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

                var (dMdy, dNdx) = ExactDifferentialAngouriService.GetPartialDerivatives(M, N);
                bool isExact = ExactDifferentialAngouriService.IsExact(M, N);
                ResultsCard.Visibility = Visibility.Visible;

                // Show partial derivatives
                DMdyText.Text = dMdy;
                DNdxText.Text = dNdx;

                if (isExact)
                {
                    ExactnessResultText.Text = "Yes, this is an exact differential equation.";
                    IntegratingFactorPanel.Visibility = Visibility.Collapsed;

                    var (solution, steps) = ExactDifferentialAngouriService.SolveExactEquation(M, N);
                    SolutionText.Text = $"The equation {M}dx + {N}dy = 0 is exact.\n" +
                                    "The partial derivatives are equal.\n\n" +
                                    $"Solution: {solution}\n\n" +
                                    $"Solution steps:\n{steps}";
                }
                else
                {
                    ExactnessResultText.Text = "No, this is not an exact differential equation.";
                    IntegratingFactorPanel.Visibility = Visibility.Visible;

                    var result = IntegratingFactorAngouriService.FindIntegratingFactor(M, N);

                    if (result.Found)
                    {
                        IntegratingFactorText.Text = $"{result.Type} = {result.Factor}";
                        NewEquationText.Text = result.NewEquation;

                        if (result.IsExactAfterMultiplying)
                        {
                            VerificationText.Text = "The equation becomes exact after multiplying by the integrating factor.\n\n" +
                                                result.Steps;

                            var (newDMdy, newDNdx) = ExactDifferentialAngouriService.GetPartialDerivatives(
                                result.NewM, result.NewN);

                            VerificationText.Text += $"\n\nNew partial derivatives:\n" +
                                                $"∂(μM)/∂y = {newDMdy}\n" +
                                                $"∂(μN)/∂x = {newDNdx}";

                            var (solution, steps) = ExactDifferentialAngouriService.SolveExactEquation(
                                result.NewM, result.NewN);

                            SolutionText.Text = $"Solution after applying the integrating factor:\n" +
                                            $"{solution}\n\n" +
                                            $"Solution steps:\n{steps}";
                        }
                        else
                        {
                            VerificationText.Text = "Warning: The equation is still not exact after multiplying by this integrating factor.";
                            SolutionText.Text = "Cannot solve the equation.";
                        }
                    }
                    else
                    {
                        IntegratingFactorText.Text = result.Factor;
                        NewEquationText.Text = "N/A";
                        VerificationText.Text = result.Steps;
                        SolutionText.Text = "No integrating factor found.";
                    }
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