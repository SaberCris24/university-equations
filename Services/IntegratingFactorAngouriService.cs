using AngouriMath;
using AngouriMath.Extensions;
using System;

namespace UniversityEquations.Services
{
    public class IntegratingFactorAngouriService
    {
        public static (bool Found, string Factor, string Type, string NewEquation, bool IsExactAfterMultiplying, string NewM, string NewN) 
            FindIntegratingFactor(string M, string N)
        {
            try
            {
                var mExpr = M.ToEntity();
                var nExpr = N.ToEntity();

                // Intentar μ(x)
                var muX = FindMuX(mExpr, nExpr);
                if (muX != null)
                {
                    return ConstructResult(muX, "μ(x)", mExpr, nExpr);
                }

                // Intentar μ(y)
                var muY = FindMuY(mExpr, nExpr);
                if (muY != null)
                {
                    return ConstructResult(muY, "μ(y)", mExpr, nExpr);
                }

                return (false, "No integrating factor found", "", "", false, "", "");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding integrating factor: {ex.Message}");
                return (false, "Error calculating integrating factor", "", "", false, "", "");
            }
        }

        private static Entity FindMuX(Entity M, Entity N)
        {
            try
            {
                // Calcular (∂N/∂x - ∂M/∂y) / M
                var dNdx = N.Differentiate("x");
                var dMdy = M.Differentiate("y");
                var diff = ((dNdx - dMdy) / M).Simplify();
                
                // Verificar si la expresión depende solo de x
                if (!diff.ToString().Contains("y"))
                {
                    // Integrar con respecto a x y calcular e^∫
                    return $"exp({diff.Integrate("x")})".ToEntity();
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static Entity FindMuY(Entity M, Entity N)
        {
            try
            {
                // Calcular (∂M/∂y - ∂N/∂x) / N
                var dMdy = M.Differentiate("y");
                var dNdx = N.Differentiate("x");
                var diff = ((dMdy - dNdx) / N).Simplify();
                
                // Verificar si la expresión depende solo de y
                if (!diff.ToString().Contains("x"))
                {
                    // Integrar con respecto a y y calcular e^∫
                    return $"exp({diff.Integrate("y")})".ToEntity();
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static (bool, string, string, string, bool, string, string) ConstructResult(
            Entity mu, string type, Entity M, Entity N)
        {
            var newM = (mu * M).Simplify();
            var newN = (mu * N).Simplify();
            var newEquation = $"{newM}dx + {newN}dy = 0";
            
            // Verificar si la nueva ecuación es exacta
            var dMdy = newM.Differentiate("y");
            var dNdx = newN.Differentiate("x");
            var isExact = ((dMdy - dNdx).Simplify()).EvaluableNumerical && 
                         ((dMdy - dNdx).Simplify()).EvalNumerical() == 0;

            return (true, mu.ToString(), type, newEquation, isExact, 
                   newM.ToString(), newN.ToString());
        }
    }
}