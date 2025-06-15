using org.mariuszgromada.math.mxparser;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace UniversityEquations.Services
{
    public class IntegratingFactorService
    {
        public class IntegratingFactorResult
        {
            public bool Found { get; set; }
            public string Factor { get; set; }
            public string NewEquation { get; set; }
            public string Type { get; set; } // "μ(x)" or "μ(y)"
            public bool IsExactAfterMultiplying { get; set; }
            public string NewM { get; set; }
            public string NewN { get; set; }
        }

        public static IntegratingFactorResult FindIntegratingFactor(string M, string N)
        {
            try
            {
                // Normalizar las expresiones primero
                var normalizedM = NormalizeExpression(M);
                var normalizedN = NormalizeExpression(N);

                // Intentar encontrar μ(x)
                var muXResult = FindMuX(normalizedM, normalizedN);
                if (muXResult.Found)
                {
                    // Verificar si realmente hace exacta la ecuación
                    var newM = $"({muXResult.Factor})*({normalizedM})";
                    var newN = $"({muXResult.Factor})*({normalizedN})";
                    
                    bool isExact = ExactDifferentialService.IsExact(newM, newN);
                    
                    return new IntegratingFactorResult
                    {
                        Found = true,
                        Factor = muXResult.Factor,
                        Type = "μ(x)",
                        NewEquation = $"({muXResult.Factor})({M})dx + ({muXResult.Factor})({N})dy = 0",
                        NewM = newM,
                        NewN = newN,
                        IsExactAfterMultiplying = isExact
                    };
                }

                // Intentar encontrar μ(y)
                var muYResult = FindMuY(normalizedM, normalizedN);
                if (muYResult.Found)
                {
                    // Verificar si realmente hace exacta la ecuación
                    var newM = $"({muYResult.Factor})*({normalizedM})";
                    var newN = $"({muYResult.Factor})*({normalizedN})";
                    
                    bool isExact = ExactDifferentialService.IsExact(newM, newN);
                    
                    return new IntegratingFactorResult
                    {
                        Found = true,
                        Factor = muYResult.Factor,
                        Type = "μ(y)",
                        NewEquation = $"({muYResult.Factor})({M})dx + ({muYResult.Factor})({N})dy = 0",
                        NewM = newM,
                        NewN = newN,
                        IsExactAfterMultiplying = isExact
                    };
                }

                // Intentar algunos factores integrantes comunes
                var commonResult = TryCommonIntegratingFactors(normalizedM, normalizedN, M, N);
                if (commonResult.Found)
                {
                    return commonResult;
                }

                return new IntegratingFactorResult
                {
                    Found = false,
                    Factor = "No se pudo encontrar un factor integrante simple de la forma μ(x) o μ(y)",
                    IsExactAfterMultiplying = false
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding integrating factor: {ex.Message}");
                return new IntegratingFactorResult
                {
                    Found = false,
                    Factor = "Error al calcular el factor integrante",
                    IsExactAfterMultiplying = false
                };
            }
        }

        private static (bool Found, string Factor) FindMuX(string M, string N)
        {
            try
            {
                // Calcular las derivadas parciales
                var (dMdy, dNdx) = ExactDifferentialService.GetPartialDerivatives(M, N);
                
                // Para μ(x): (∂M/∂y - ∂N/∂x) / N debe depender solo de x
                var testPoints = new[] { (1.0, 0.5), (2.0, 0.5), (0.5, 0.5), (1.5, 0.5), (3.0, 0.5) };
                var ratioValues = new double[testPoints.Length];
                bool validRatio = true;

                for (int i = 0; i < testPoints.Length; i++)
                {
                    var (x, y) = testPoints[i];
                    
                    try
                    {
                        // Evaluar (∂M/∂y - ∂N/∂x) / N en el punto
                        Expression numeratorExpr = new Expression($"({dMdy}) - ({dNdx})");
                        Expression denominatorExpr = new Expression(N);
                        
                        numeratorExpr.addArguments(new Argument("x", x), new Argument("y", y));
                        denominatorExpr.addArguments(new Argument("x", x), new Argument("y", y));
                        
                        double numerator = numeratorExpr.calculate();
                        double denominator = denominatorExpr.calculate();
                        
                        if (Math.Abs(denominator) < 1e-10 || double.IsNaN(numerator) || double.IsNaN(denominator))
                        {
                            validRatio = false;
                            break;
                        }
                        
                        ratioValues[i] = numerator / denominator;
                    }
                    catch
                    {
                        validRatio = false;
                        break;
                    }
                }

                if (!validRatio)
                    return (false, null);

                // Verificar si todos los valores son aproximadamente iguales (solo depende de x)
                double tolerance = 1e-6;
                double baseValue = ratioValues[0];
                
                for (int i = 1; i < ratioValues.Length; i++)
                {
                    if (Math.Abs(ratioValues[i] - baseValue) > tolerance)
                    {
                        return (false, null);
                    }
                }

                // Si el ratio es constante, el factor integrante es exp(integral del ratio)
                // Para casos simples, intentamos algunos patrones comunes
                return TrySimpleIntegratingFactorX(baseValue, M, N);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindMuX: {ex.Message}");
                return (false, null);
            }
        }

        private static (bool Found, string Factor) FindMuY(string M, string N)
        {
            try
            {
                // Calcular las derivadas parciales
                var (dMdy, dNdx) = ExactDifferentialService.GetPartialDerivatives(M, N);
                
                // Para μ(y): (∂N/∂x - ∂M/∂y) / M debe depender solo de y
                var testPoints = new[] { (0.5, 1.0), (0.5, 2.0), (0.5, 0.5), (0.5, 1.5), (0.5, 3.0) };
                var ratioValues = new double[testPoints.Length];
                bool validRatio = true;

                for (int i = 0; i < testPoints.Length; i++)
                {
                    var (x, y) = testPoints[i];
                    
                    try
                    {
                        // Evaluar (∂N/∂x - ∂M/∂y) / M en el punto
                        Expression numeratorExpr = new Expression($"({dNdx}) - ({dMdy})");
                        Expression denominatorExpr = new Expression(M);
                        
                        numeratorExpr.addArguments(new Argument("x", x), new Argument("y", y));
                        denominatorExpr.addArguments(new Argument("x", x), new Argument("y", y));
                        
                        double numerator = numeratorExpr.calculate();
                        double denominator = denominatorExpr.calculate();
                        
                        if (Math.Abs(denominator) < 1e-10 || double.IsNaN(numerator) || double.IsNaN(denominator))
                        {
                            validRatio = false;
                            break;
                        }
                        
                        ratioValues[i] = numerator / denominator;
                    }
                    catch
                    {
                        validRatio = false;
                        break;
                    }
                }

                if (!validRatio)
                    return (false, null);

                // Verificar si todos los valores son aproximadamente iguales (solo depende de y)
                double tolerance = 1e-6;
                double baseValue = ratioValues[0];
                
                for (int i = 1; i < ratioValues.Length; i++)
                {
                    if (Math.Abs(ratioValues[i] - baseValue) > tolerance)
                    {
                        return (false, null);
                    }
                }

                // Si el ratio es constante, el factor integrante es exp(integral del ratio)
                return TrySimpleIntegratingFactorY(baseValue, M, N);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindMuY: {ex.Message}");
                return (false, null);
            }
        }

        private static (bool Found, string Factor) TrySimpleIntegratingFactorX(double ratio, string M, string N)
        {
            try
            {
                // Para ratios constantes simples
                if (Math.Abs(ratio) < 1e-10)
                {
                    return (true, "1"); // Factor integrante = 1
                }
                
                // Si el ratio es constante, μ(x) = exp(∫ratio dx) = exp(ratio * x)
                if (Math.Abs(ratio - Math.Round(ratio)) < 1e-6)
                {
                    int intRatio = (int)Math.Round(ratio);
                    if (intRatio == 1)
                        return (true, "exp(x)");
                    else if (intRatio == -1)
                        return (true, "exp(-x)");
                    else if (intRatio > 0)
                        return (true, $"exp({intRatio}*x)");
                    else
                        return (true, $"exp({intRatio}*x)");
                }
                
                // Para otros casos
                return (true, $"exp({ratio:F3}*x)");
            }
            catch
            {
                return (false, null);
            }
        }

        private static (bool Found, string Factor) TrySimpleIntegratingFactorY(double ratio, string M, string N)
        {
            try
            {
                // Para ratios constantes simples
                if (Math.Abs(ratio) < 1e-10)
                {
                    return (true, "1"); // Factor integrante = 1
                }
                
                // Si el ratio es constante, μ(y) = exp(∫ratio dy) = exp(ratio * y)
                if (Math.Abs(ratio - Math.Round(ratio)) < 1e-6)
                {
                    int intRatio = (int)Math.Round(ratio);
                    if (intRatio == 1)
                        return (true, "exp(y)");
                    else if (intRatio == -1)
                        return (true, "exp(-y)");
                    else if (intRatio > 0)
                        return (true, $"exp({intRatio}*y)");
                    else
                        return (true, $"exp({intRatio}*y)");
                }
                
                // Para otros casos
                return (true, $"exp({ratio:F3}*y)");
            }
            catch
            {
                return (false, null);
            }
        }

        private static IntegratingFactorResult TryCommonIntegratingFactors(string M, string N, string originalM, string originalN)
        {
            // Lista de factores integrantes comunes para probar
            var commonFactors = new[]
            {
                ("1/x", "1/x"),
                ("1/y", "1/y"),
                ("x", "x"),
                ("y", "y"),
                ("1/(x*y)", "1/(x*y)"),
                ("x*y", "x*y"),
                ("exp(x)", "exp(x)"),
                ("exp(-x)", "exp(-x)"),
                ("exp(y)", "exp(y)"),
                ("exp(-y)", "exp(-y)"),
                ("1/(x^2)", "1/pow(x,2)"),
                ("1/(y^2)", "1/pow(y,2)"),
                ("x^2", "pow(x,2)"),
                ("y^2", "pow(y,2)")
            };

            foreach (var (displayFactor, computeFactor) in commonFactors)
            {
                try
                {
                    var newM = $"({computeFactor})*({M})";
                    var newN = $"({computeFactor})*({N})";
                    
                    if (ExactDifferentialService.IsExact(newM, newN))
                    {
                        return new IntegratingFactorResult
                        {
                            Found = true,
                            Factor = displayFactor,
                            Type = "μ(x,y)",
                            NewEquation = $"{displayFactor}({originalM})dx + {displayFactor}({originalN})dy = 0",
                            NewM = newM,
                            NewN = newN,
                            IsExactAfterMultiplying = true
                        };
                    }
                }
                catch
                {
                    // Continuar con el siguiente factor
                    continue;
                }
            }

            return new IntegratingFactorResult { Found = false };
        }

        private static string NormalizeExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return string.Empty;

            expression = expression.Replace(" ", "")
                                 .Replace("²", "^2")
                                 .Replace("³", "^3");

            // Convertir x^n a pow(x,n)
            expression = Regex.Replace(expression, @"(\w+)\^(\d+)", "pow($1,$2)");

            // Asegurar multiplicaciones explícitas
            expression = Regex.Replace(expression, @"(\d+)([a-zA-Z])", "$1*$2");
            expression = Regex.Replace(expression, @"([a-zA-Z])(\d+)", "$1*$2");
            expression = Regex.Replace(expression, @"([a-zA-Z])([a-zA-Z])", "$1*$2");

            return expression;
        }
    }
}