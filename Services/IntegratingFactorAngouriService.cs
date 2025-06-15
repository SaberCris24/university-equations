using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UniversityEquations.Services
{
    public class IntegratingFactorAngouriService
    {
        public static (bool Found, string Factor, string Type, string NewEquation, bool IsExactAfterMultiplying, string NewM, string NewN, string Steps) 
            FindIntegratingFactor(string M, string N)
        {
            try
            {
                var mExpr = M.ToEntity();
                var nExpr = N.ToEntity();

                // List to store integrating factor attempts
                var attempts = new List<(Entity? mu, string type, string steps)>();

                // 1. Try μ(x)
                attempts.Add(FindMuX(mExpr, nExpr));

                // 2. Try μ(y)
                attempts.Add(FindMuY(mExpr, nExpr));

                // 3. Try μ(xy)
                attempts.Add(FindMuXY(mExpr, nExpr));

                // 4. Try μ = x^m * y^n
                attempts.Add(FindMuPower(mExpr, nExpr));

                // 5. Try μ = e^(ax + by)
                attempts.Add(FindMuExponential(mExpr, nExpr));

                // Filter successful attempts
                var successfulAttempts = attempts
                    .Where(a => a.mu != null)
                    .Select(a => (a.mu!, a.type, a.steps))
                    .ToList();

                if (successfulAttempts.Any())
                {
                    foreach (var (mu, type, steps) in successfulAttempts)
                    {
                        var result = ConstructResult(mu, type, mExpr, nExpr);
                        if (result.Item5) // If the equation becomes exact
                        {
                            return (true, result.Item2, type, result.Item3, true, 
                                   result.Item6, result.Item7, steps);
                        }
                    }

                    // If none makes the equation exact, return the first one
                    var firstAttempt = successfulAttempts.First();
                    var firstResult = ConstructResult(firstAttempt.Item1, firstAttempt.type, mExpr, nExpr);
                    return (true, firstResult.Item2, firstAttempt.type, firstResult.Item3, false, 
                           firstResult.Item6, firstResult.Item7, firstAttempt.steps);
                }

                return (false, "No integrating factor found", "", "", false, "", "", 
                       "The following methods were attempted:\n" +
                       "1. μ(x)\n" +
                       "2. μ(y)\n" +
                       "3. μ(xy)\n" +
                       "4. μ = x^m * y^n\n" +
                       "5. μ = e^(ax + by)\n" +
                       "None produced a valid integrating factor.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", "", "", false, "", "", 
                       "Error calculating integrating factor.");
            }
        }

        private static (Entity? mu, string type, string steps) FindMuX(Entity M, Entity N)
        {
            try
            {
                var dNdx = N.Differentiate("x");
                var dMdy = M.Differentiate("y");
                var diff = ((dNdx - dMdy) / M).Simplify();
                
                if (!diff.ToString().Contains("y"))
                {
                    var integral = diff.Integrate("x").Simplify();
                    var mu = $"exp({integral})".ToEntity();
                    
                    var steps = $@"Finding μ(x):
1. Calculate (∂N/∂x - ∂M/∂y) / M = {diff}
2. Verify it depends only on x ✓
3. Integrate with respect to x: ∫({diff})dx = {integral}
4. Integrating factor μ(x) = e^({integral}) = {mu}";
                    
                    return (mu, "μ(x)", steps);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindMuX: {ex.Message}");
            }
            
            return (null, "μ(x)", "");
        }

        private static (Entity? mu, string type, string steps) FindMuY(Entity M, Entity N)
        {
            try
            {
                var dMdy = M.Differentiate("y");
                var dNdx = N.Differentiate("x");
                var diff = ((dMdy - dNdx) / N).Simplify();
                
                if (!diff.ToString().Contains("x"))
                {
                    var integral = diff.Integrate("y").Simplify();
                    var mu = $"exp({integral})".ToEntity();
                    
                    var steps = $@"Finding μ(y):
1. Calculate (∂M/∂y - ∂N/∂x) / N = {diff}
2. Verify it depends only on y ✓
3. Integrate with respect to y: ∫({diff})dy = {integral}
4. Integrating factor μ(y) = e^({integral}) = {mu}";
                    
                    return (mu, "μ(y)", steps);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindMuY: {ex.Message}");
            }
            
            return (null, "μ(y)", "");
        }

        private static (Entity? mu, string type, string steps) FindMuXY(Entity M, Entity N)
        {
            try
            {
                // Check if the equation is homogeneous
                var mDegree = GetHomogeneousDegree(M);
                var nDegree = GetHomogeneousDegree(N);

                if (mDegree == nDegree && mDegree != -1)
                {
                    var mu = $"1/(x*y)^{mDegree}".ToEntity();
                    
                    var steps = $@"Finding μ(xy):
1. Check if equation is homogeneous
2. Degree of M = {mDegree}, Degree of N = {nDegree}
3. Integrating factor μ = 1/(xy)^{mDegree}";
                    
                    return (mu, "μ(xy)", steps);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindMuXY: {ex.Message}");
            }
            
            return (null, "μ(xy)", "");
        }

        private static (Entity? mu, string type, string steps) FindMuPower(Entity M, Entity N)
        {
            try
            {
                // Try simple values for m and n
                for (int m = -3; m <= 3; m++)
                {
                    for (int n = -3; n <= 3; n++)
                    {
                        var mu = $"x^{m} * y^{n}".ToEntity();
                        var newM = (mu * M).Simplify();
                        var newN = (mu * N).Simplify();
                        
                        if (ExactDifferentialAngouriService.IsExact(newM.ToString(), newN.ToString()))
                        {
                            var steps = $@"Finding μ = x^m * y^n:
1. Try different values for m and n
2. Found: m = {m}, n = {n}
3. Integrating factor μ = x^{m} * y^{n}";
                            
                            return (mu, "μ = x^m * y^n", steps);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindMuPower: {ex.Message}");
            }
            
            return (null, "μ = x^m * y^n", "");
        }

        private static (Entity? mu, string type, string steps) FindMuExponential(Entity M, Entity N)
        {
            try
            {
                // Try simple values for a and b
                for (int a = -3; a <= 3; a++)
                {
                    for (int b = -3; b <= 3; b++)
                    {
                        if (a == 0 && b == 0) continue;
                        
                        var mu = $"exp({a}*x + {b}*y)".ToEntity();
                        var newM = (mu * M).Simplify();
                        var newN = (mu * N).Simplify();
                        
                        if (ExactDifferentialAngouriService.IsExact(newM.ToString(), newN.ToString()))
                        {
                            var steps = $@"Finding μ = e^(ax + by):
1. Try different values for a and b
2. Found: a = {a}, b = {b}
3. Integrating factor μ = e^({a}x + {b}y)";
                            
                            return (mu, "μ = e^(ax + by)", steps);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindMuExponential: {ex.Message}");
            }
            
            return (null, "μ = e^(ax + by)", "");
        }

        private static int GetHomogeneousDegree(Entity expr)
        {
            try
            {
                // Replace x with tx and y with ty
                var transformed = expr.Substitute("x", "t*x").Substitute("y", "t*y");
                // Expand and collect terms in t
                var expanded = transformed.Expand();
                // The degree is the exponent of t
                var terms = expanded.ToString().Split('+');
                var degrees = new HashSet<int>();

                foreach (var term in terms)
                {
                    var degree = 0;
                    if (term.Contains("t"))
                    {
                        var parts = term.Split('^');
                        if (parts.Length > 1)
                        {
                            int.TryParse(parts[1].Trim(), out degree);
                        }
                        else
                        {
                            degree = 1;
                        }
                    }
                    degrees.Add(degree);
                }

                return degrees.Count == 1 ? degrees.First() : -1;
            }
            catch
            {
                return -1;
            }
        }

        private static (bool, string, string, string, bool, string, string) ConstructResult(
            Entity mu, string type, Entity M, Entity N)
        {
            var newM = (mu * M).Simplify();
            var newN = (mu * N).Simplify();
            var newEquation = $"{newM}dx + {newN}dy = 0";
            
            // Check if the new equation is exact
            var isExact = ExactDifferentialAngouriService.IsExact(newM.ToString(), newN.ToString());

            return (true, mu.ToString(), type, newEquation, isExact, 
                   newM.ToString(), newN.ToString());
        }
    }
}