using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using UniversityEquations.Services;

namespace UniversityEquations.Pages.Calculator
{
    public sealed partial class CalculatorPage : Page
    {
        private string currentM;
        private string currentN;
        private bool isCurrentExact;
        private (bool Found, string Factor, string Type, string NewEquation, bool IsExactAfterMultiplying, string NewM, string NewN, string Steps) currentIntegratingFactor;

        public CalculatorPage()
        {
            this.InitializeComponent();
        }

        private bool ValidateInput()
        {
            string M = MFunctionTextBox.Text.Trim();
            string N = NFunctionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(M) || string.IsNullOrWhiteSpace(N))
            {
                ShowError("Please enter both M(x,y) and N(x,y) functions.");
                return false;
            }

            currentM = M;
            currentN = N;
            ResultsCard.Visibility = Visibility.Visible;
            return true;
        }

        private void CheckExactButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                var (dMdy, dNdx) = ExactDifferentialAngouriService.GetPartialDerivatives(currentM, currentN);
                isCurrentExact = ExactDifferentialAngouriService.IsExact(currentM, currentN);

                // Show partial derivatives
                DMdyText.Text = dMdy;
                DNdxText.Text = dNdx;

                ExactnessResultText.Text = isCurrentExact
                    ? "Yes, this is an exact differential equation."
                    : "No, this is not an exact differential equation.";

                ExactnessPanel.Visibility = Visibility.Visible;
                IntegratingFactorPanel.Visibility = Visibility.Collapsed;
                NewEquationPanel.Visibility = Visibility.Collapsed;
                SolutionPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }

        private void FindIntegratingFactorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                if (isCurrentExact)
                {
                    ShowError("The equation is already exact. No integrating factor needed.");
                    return;
                }

                currentIntegratingFactor = IntegratingFactorAngouriService.FindIntegratingFactor(currentM, currentN);

                if (currentIntegratingFactor.Found)
                {
                    IntegratingFactorText.Text = $"{currentIntegratingFactor.Type} = {currentIntegratingFactor.Factor}\n\n" +
                                               $"Steps to find the integrating factor:\n{currentIntegratingFactor.Steps}";
                }
                else
                {
                    IntegratingFactorText.Text = "No integrating factor found.";
                }

                ExactnessPanel.Visibility = Visibility.Collapsed;
                IntegratingFactorPanel.Visibility = Visibility.Visible;
                NewEquationPanel.Visibility = Visibility.Collapsed;
                SolutionPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }

        private void CheckNewEquationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                if (currentIntegratingFactor.Factor == null)
                {
                    ShowError("Please find the integrating factor first.");
                    return;
                }

                NewEquationText.Text = currentIntegratingFactor.NewEquation;

                if (currentIntegratingFactor.IsExactAfterMultiplying)
                {
                    var (newDMdy, newDNdx) = ExactDifferentialAngouriService.GetPartialDerivatives(
                        currentIntegratingFactor.NewM, currentIntegratingFactor.NewN);

                    VerificationText.Text = "The equation becomes exact after multiplying by the integrating factor.\n\n" +
                                        $"New partial derivatives:\n" +
                                        $"∂(μM)/∂y = {newDMdy}\n" +
                                        $"∂(μN)/∂x = {newDNdx}";
                }
                else
                {
                    VerificationText.Text = "Warning: The equation is still not exact after multiplying by this integrating factor.";
                }

                ExactnessPanel.Visibility = Visibility.Collapsed;
                IntegratingFactorPanel.Visibility = Visibility.Collapsed;
                NewEquationPanel.Visibility = Visibility.Visible;
                SolutionPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }

        private void ShowSolutionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                if (isCurrentExact)
                {
                    var (solution, steps) = ExactDifferentialAngouriService.SolveExactEquation(currentM, currentN);
                    SolutionText.Text = $"The equation {currentM}dx + {currentN}dy = 0 is exact.\n" +
                                    "The partial derivatives are equal.\n\n" +
                                    $"Solution: {solution}\n\n" +
                                    $"Solution steps:\n{steps}";
                }
                else if (currentIntegratingFactor.IsExactAfterMultiplying)
                {
                    var (solution, steps) = ExactDifferentialAngouriService.SolveExactEquation(
                        currentIntegratingFactor.NewM, currentIntegratingFactor.NewN);

                    SolutionText.Text = $"Solution after applying the integrating factor:\n" +
                                    $"{solution}\n\n" +
                                    $"Solution steps:\n{steps}";
                }
                else
                {
                    ShowError("Cannot solve the equation. It's not exact and no valid integrating factor was found.");
                    return;
                }

                ExactnessPanel.Visibility = Visibility.Collapsed;
                IntegratingFactorPanel.Visibility = Visibility.Collapsed;
                NewEquationPanel.Visibility = Visibility.Collapsed;
                SolutionPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                // Check if exact
                CheckExactButton_Click(sender, e);

                if (!isCurrentExact)
                {
                    // Find integrating factor
                    FindIntegratingFactorButton_Click(sender, e);

                    if (currentIntegratingFactor.Found)
                    {
                        // Check new equation
                        CheckNewEquationButton_Click(sender, e);
                    }
                }

                // Show solution
                ShowSolutionButton_Click(sender, e);

                // Show all panels
                ExactnessPanel.Visibility = Visibility.Visible;
                IntegratingFactorPanel.Visibility = Visibility.Visible;
                NewEquationPanel.Visibility = Visibility.Visible;
                SolutionPanel.Visibility = Visibility.Visible;
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