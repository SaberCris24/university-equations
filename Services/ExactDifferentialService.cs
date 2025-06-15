using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UniversityEquations.Services
{
    public class ExactDifferentialService
    {
        public static bool IsExact(string M, string N)
        {
            try
            {
                // Normalize expressions
                var (normalizedM, normalizedN) = NormalizeExpressions(M, N);
                if (string.IsNullOrEmpty(normalizedM) || string.IsNullOrEmpty(normalizedN))
                {
                    return false;
                }

                Expression dMdy = new Expression($"der({normalizedM}, y)");
                Expression dNdx = new Expression($"der({normalizedN}, x)");

                var testPoints = GenerateAppropriateTestPoints(normalizedM, normalizedN);
                int validComparisons = 0;
                const int MINIMUM_VALID_COMPARISONS = 5;

                foreach (var point in testPoints)
                {
                    var (dMdyValue, dNdxValue) = EvaluateDerivativesAtPoint(dMdy, dNdx, point);

                    if (IsValidComparison(dMdyValue, dNdxValue))
                    {
                        validComparisons++;
                        if (!AreDerivativesEqual(dMdyValue, dNdxValue))
                        {
                            return false;
                        }
                    }
                }

                return validComparisons >= MINIMUM_VALID_COMPARISONS;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in IsExact: {ex.Message}");
                return false;
            }
        }

        public static (string dMdy, string dNdx) GetPartialDerivatives(string M, string N)
        {
            try
            {
                var (normalizedM, normalizedN) = NormalizeExpressions(M, N);
                if (string.IsNullOrEmpty(normalizedM) || string.IsNullOrEmpty(normalizedN))
                {
                    return ("Error", "Error");
                }

                Expression dMdy = new Expression($"der({normalizedM}, y)");
                Expression dNdx = new Expression($"der({normalizedN}, x)");

                return (dMdy.getExpressionString(), dNdx.getExpressionString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting derivatives: {ex.Message}");
                return ("Error", "Error");
            }
        }

        public static string FormatPartialDerivativesOutput(string M, string N)
        {
            var (dMdy, dNdx) = GetPartialDerivatives(M, N);
            bool isExact = IsExact(M, N);
            
            return $@"Partial Derivatives:
∂M/∂y = {dMdy}
∂N/∂x = {dNdx}

{(isExact ? "The equation is exact." : "The equation is not exact.")}
{(isExact ? "The partial derivatives are equal." : "The partial derivatives are not equal.")}";
        }

        private static (string M, string N) NormalizeExpressions(string M, string N)
        {
            try
            {
                M = NormalizeExpression(M);
                N = NormalizeExpression(N);
                return (M, N);
            }
            catch (Exception)
            {
                return (string.Empty, string.Empty);
            }
        }

        private static string NormalizeExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return string.Empty;

            expression = expression.Replace(" ", "")
                                 .Replace("²", "^2")
                                 .Replace("³", "^3");

            // Critical phase: Handle exponents in exp()
            expression = Regex.Replace(expression, @"exp\(([^)]+)\)", match =>
            {
                string innerExp = match.Groups[1].Value;
                // Convert x^2*y to pow(x,2)*y
                innerExp = Regex.Replace(innerExp, @"(\w+)\^(\d+)", "pow($1,$2)");
                // Ensure multiplications
                innerExp = Regex.Replace(innerExp, @"([a-zA-Z])([a-zA-Z\d])", "$1*$2");
                return $"exp({innerExp})";
            });

            // Convert e^x to exp(x)
            expression = Regex.Replace(expression, @"e\^(\w+)", "exp($1)");

            // Rest of normalizations
            expression = NormalizeMultiplication(expression);
            expression = NormalizeTrigonometricFunctions(expression);

            return expression;
        }

        private static string NormalizeMultiplication(string expression)
        {
            // Exclude already processed areas (exp, pow)
            return Regex.Replace(expression, @"(?<!exp\(|pow\([^,]+,[^\)]+\))(\d+)([a-zA-Z])", "$1*$2");
        }

        private static string NormalizeTrigonometricFunctions(string expression)
        {
            var replacements = new Dictionary<string, string>
            {
                { @"(?<!arc)sin", "sin" },
                { @"(?<!arc)cos", "cos" },
                { @"(?<!arc)tan", "tan" },
                { @"arcsin", "asin" },
                { @"arccos", "acos" },
                { @"arctan", "atan" }
            };

            foreach (var (pattern, replacement) in replacements)
            {
                expression = Regex.Replace(expression, pattern, replacement);
            }

            return expression;
        }

        private static IEnumerable<(double x, double y)> GenerateAppropriateTestPoints(string M, string N)
        {
            var points = new HashSet<(double x, double y)>();

            // Base safe points (avoid 0)
            for (int i = 1; i <= 5; i++)
            {
                double val = i * 0.2;  // 0.2, 0.4, 0.6, 0.8, 1.0
                points.Add((val, val));
            }

            // Special handling for exponentials
            if (M.Contains("exp") || N.Contains("exp"))
            {
                // Avoid values that might cause overflow
                points.Add((0.5, 0.5));
                points.Add((0.3, 0.3));
                points.Add((1.0, 0.5));
                points.Add((0.5, 1.0));
                points.Add((0.1, 0.5));
                points.Add((0.5, 0.1));
            }

            // Special points for trigonometric functions
            if (M.Contains("sin") || M.Contains("cos") || N.Contains("sin") || N.Contains("cos"))
            {
                points.Add((Math.PI / 6, 0.5));
                points.Add((Math.PI / 4, 0.5));
                points.Add((Math.PI / 3, 0.5));
            }

            // Special points for logarithms
            if (M.Contains("ln") || N.Contains("ln"))
            {
                points.Add((1.0, 1.0));
                points.Add((2.0, 2.0));
                points.Add((Math.E, Math.E));
            }

            return points;
        }

        private static (double dMdy, double dNdx) EvaluateDerivativesAtPoint(
            Expression dMdy, Expression dNdx, (double x, double y) point)
        {
            try
            {
                dMdy.removeAllArguments();
                dNdx.removeAllArguments();

                var xArg = new Argument("x", point.x);
                var yArg = new Argument("y", point.y);

                dMdy.addArguments(xArg, yArg);
                dNdx.addArguments(xArg, yArg);

                return (dMdy.calculate(), dNdx.calculate());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error evaluating at point ({point.x}, {point.y}): {ex.Message}");
                return (double.NaN, double.NaN);
            }
        }

        private static bool IsValidComparison(double dMdy, double dNdx)
        {
            return !double.IsNaN(dMdy) && !double.IsNaN(dNdx) &&
                   !double.IsInfinity(dMdy) && !double.IsInfinity(dNdx);
        }

        private static bool AreDerivativesEqual(double dMdy, double dNdx)
        {
            const double ABS_TOL = 1e-8;
            const double REL_TOL = 1e-6;

            // Absolute tolerance for values close to zero
            if (Math.Abs(dMdy - dNdx) < ABS_TOL)
                return true;

            // Relative tolerance for large values
            double magnitude = Math.Max(Math.Abs(dMdy), Math.Abs(dNdx));
            return Math.Abs(dMdy - dNdx) < REL_TOL * magnitude;
        }
    }
}