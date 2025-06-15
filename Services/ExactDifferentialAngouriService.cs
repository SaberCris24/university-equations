using System;
using AngouriMath.Extensions;

namespace UniversityEquations.Services
{
    public class ExactDifferentialAngouriService
    {
        public static bool IsExact(string M, string N)
        {
            try
            {
                var mExpr = M.ToEntity();
                var nExpr = N.ToEntity();
                
                // Calcular ∂M/∂y y ∂N/∂x
                var dMdy = mExpr.Differentiate("y");
                var dNdx = nExpr.Differentiate("x");
                
                // Simplificar la diferencia
                var diff = (dMdy - dNdx).Simplify();
                
                // Verificar si son iguales (la diferencia es 0)
                return diff.EvaluableNumerical && diff.EvalNumerical() == 0;
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
                var mExpr = M.ToEntity();
                var nExpr = N.ToEntity();

                var dMdy = mExpr.Differentiate("y").Simplify();
                var dNdx = nExpr.Differentiate("x").Simplify();

                return (dMdy.ToString(), dNdx.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting derivatives: {ex.Message}");
                return ("Error", "Error");
            }
        }

        public static (string solution, string steps) SolveExactEquation(string M, string N)
        {
            try
            {
                var mExpr = M.ToEntity();
                var nExpr = N.ToEntity();
                
                // Integrar M con respecto a x
                var phi = mExpr.Integrate("x").Simplify();
                
                // Derivar phi con respecto a y
                var dPhiDy = phi.Differentiate("y").Simplify();
                
                // Calcular g(y) resolviendo N - dPhi/dy
                var gPrime = (nExpr - dPhiDy).Simplify();
                var g = gPrime.Integrate("y").Simplify();
                
                // La solución es phi + g = C
                var solution = $"{phi} + {g} = C";
                
                var steps = $@"Steps to solve:
1. ∫ M dx = {phi}
2. ∂/∂y({phi}) = {dPhiDy}
3. N - ∂φ/∂y = {gPrime}
4. ∫({gPrime})dy = {g}
5. Therefore, φ(x,y) = {phi} + {g} = C";

                return (solution, steps);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error solving exact equation: {ex.Message}");
                return ("Error solving equation", "Error in solution steps");
            }
        }
    }
}