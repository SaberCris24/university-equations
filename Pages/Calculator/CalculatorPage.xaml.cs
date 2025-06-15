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

                // Check if the equation is exact
                bool isExact = ExactDifferentialService.IsExact(M, N);
                var (dMdy, dNdx) = ExactDifferentialService.GetPartialDerivatives(M, N);
                
                // Show results card
                ResultsCard.Visibility = Visibility.Visible;

                // Update partial derivatives
                DMdyText.Text = CleanDerivativeExpression(dMdy);
                DNdxText.Text = CleanDerivativeExpression(dNdx);

                if (isExact)
                {
                    // The equation is exact
                    ExactnessResultText.Text = "✅ Yes, this is an exact differential equation.";
                    
                    IntegratingFactorPanel.Visibility = Visibility.Collapsed;
                    
                    SolutionText.Text = $"The equation {M}dx + {N}dy = 0 is exact.\n\n" +
                                      "The partial derivatives are equal:\n" +
                                      $"∂M/∂y = ∂N/∂x\n\n" +
                                      "This means there exists a function F(x,y) such that:\n" +
                                      "∂F/∂x = M(x,y) and ∂F/∂y = N(x,y)\n\n" +
                                      "The general solution is F(x,y) = C, where C is a constant.";
                }
                else
                {
                    // The equation is not exact, search for integrating factor
                    ExactnessResultText.Text = "❌ No, this is not an exact differential equation.";
                    
                    IntegratingFactorPanel.Visibility = Visibility.Visible;

                    // Find integrating factor
                    var integratingFactorResult = IntegratingFactorService.FindIntegratingFactor(M, N);

                    if (integratingFactorResult.Found)
                    {
                        IntegratingFactorText.Text = $"✅ Integrating factor found:\n{integratingFactorResult.Type} = {integratingFactorResult.Factor}";
                        
                        NewEquationText.Text = $"New equation:\n{integratingFactorResult.NewEquation}";

                        if (integratingFactorResult.IsExactAfterMultiplying)
                        {
                            VerificationText.Text = "✅ Verification successful: The equation becomes exact after multiplying by the integrating factor.";
                            
                            // Show new partial derivatives
                            var (newDMdy, newDNdx) = ExactDifferentialService.GetPartialDerivatives(
                                integratingFactorResult.NewM, 
                                integratingFactorResult.NewN);
                            
                            VerificationText.Text += $"\n\nNew partial derivatives:\n" +
                                                   $"∂(μM)/∂y = {CleanDerivativeExpression(newDMdy)}\n" +
                                                   $"∂(μN)/∂x = {CleanDerivativeExpression(newDNdx)}\n\n" +
                                                   "Now: ∂(μM)/∂y = ∂(μN)/∂x ✅";

                            SolutionText.Text = 
                                              $"1. The original equation {M}dx + {N}dy = 0 is not exact.\n\n" +
                                              $"2. Found integrating factor {integratingFactorResult.Type} = {integratingFactorResult.Factor}\n\n" +
                                              $"3. Multiplying by the integrating factor:\n   {integratingFactorResult.NewEquation}\n\n" +
                                              $"4. The new equation is exact and can be solved using the standard method for exact equations.\n\n" +
                                              $"5. The general solution is of the form F(x,y) = C, where F is the potential function of the resulting exact equation.";
                        }
                        else
                        {
                            VerificationText.Text = "⚠️ Warning: The equation is still not exact after multiplying by this integrating factor. A different method may be needed.";
                            
                            SolutionText.Text = $"The equation {M}dx + {N}dy = 0 is not exact.\n\n" +
                                              $"Attempted to use integrating factor {integratingFactorResult.Type} = {integratingFactorResult.Factor}, " +
                                              $"but the resulting equation is still not exact.\n\n" +
                                              "A more complex integrating factor or a different solution method may be needed.";
                        }
                    }
                    else
                    {
                        IntegratingFactorText.Text = "❌ " + integratingFactorResult.Factor;
                        
                        NewEquationText.Text = "Not available";
                        VerificationText.Text = "Could not verify as no simple integrating factor was found.";
                        
                        SolutionText.Text = $"The equation {M}dx + {N}dy = 0 is not exact.\n\n" +
                                          "The partial derivatives are not equal:\n" +
                                          $"∂M/∂y ≠ ∂N/∂x\n\n" +
                                          "Could not find a simple integrating factor of the form μ(x) or μ(y).\n\n" +
                                          "Alternative approaches:\n" +
                                          "• Look for an integrating factor of the form μ(x,y)\n" +
                                          "• Check if it's a separable equation\n" +
                                          "• Try an appropriate substitution\n" +
                                          "• Use numerical methods";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error processing equation: {ex.Message}\n\nVerify that the functions are written correctly using standard mathematical syntax.");
            }
        }

        private string CleanDerivativeExpression(string derivative)
        {
            if (string.IsNullOrEmpty(derivative) || derivative == "Error")
                return "Error in calculation";

            string cleaned = derivative;

            if (cleaned.StartsWith("der(") && cleaned.EndsWith(")"))
            {
                cleaned = cleaned.Substring(4, cleaned.Length - 5);
            }

            // Reemplazar operadores por símbolos más legibles
            cleaned = cleaned.Replace("pow(", "")
                            .Replace(")", "")
                            .Replace("*", "·");

            return cleaned;
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