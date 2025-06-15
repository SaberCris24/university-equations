using org.mariuszgromada.math.mxparser;
using System;

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
                // Try to find μ(x)
                var muX = FindMuX(M, N);
                if (!string.IsNullOrEmpty(muX))
                {
                    return new IntegratingFactorResult
                    {
                        Found = true,
                        Factor = muX,
                        Type = "μ(x)",
                        NewEquation = $"({muX})({M})dx + ({muX})({N})dy = 0",
                        NewM = $"({muX})({M})",
                        NewN = $"({muX})({N})",
                        IsExactAfterMultiplying = true // This should be verified
                    };
                }

                // Try to find μ(y)
                var muY = FindMuY(M, N);
                if (!string.IsNullOrEmpty(muY))
                {
                    return new IntegratingFactorResult
                    {
                        Found = true,
                        Factor = muY,
                        Type = "μ(y)",
                        NewEquation = $"({muY})({M})dx + ({muY})({N})dy = 0",
                        NewM = $"({muY})({M})",
                        NewN = $"({muY})({N})",
                        IsExactAfterMultiplying = true // This should be verified
                    };
                }

                return new IntegratingFactorResult
                {
                    Found = false,
                    Factor = "Could not find a simple integrating factor of the form μ(x) or μ(y)",
                    IsExactAfterMultiplying = false
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding integrating factor: {ex.Message}");
                return new IntegratingFactorResult
                {
                    Found = false,
                    Factor = "Error calculating integrating factor",
                    IsExactAfterMultiplying = false
                };
            }
        }

        private static string FindMuX(string M, string N)
        {
            try
            {
                var (dMdy, dNdx) = ExactDifferentialService.GetPartialDerivatives(M, N);
                
                // Calculate (∂N/∂x - ∂M/∂y) / M
                Expression numerator = new Expression($"({dNdx}) - ({dMdy})");
                Expression denominator = new Expression(M);
                
                // TODO: Implement the logic to:
                // 1. Check if the expression depends only on x
                // 2. Integrate the expression with respect to x
                // 3. Return exp of the integral

                return null; // Return null if no μ(x) is found
            }
            catch
            {
                return null;
            }
        }

        private static string FindMuY(string M, string N)
        {
            try
            {
                var (dMdy, dNdx) = ExactDifferentialService.GetPartialDerivatives(M, N);
                
                // Calculate (∂M/∂y - ∂N/∂x) / N
                Expression numerator = new Expression($"({dMdy}) - ({dNdx})");
                Expression denominator = new Expression(N);
                
                // TODO: Implement the logic to:
                // 1. Check if the expression depends only on y
                // 2. Integrate the expression with respect to y
                // 3. Return exp of the integral

                return null; // Return null if no μ(y) is found
            }
            catch
            {
                return null;
            }
        }
    }
}