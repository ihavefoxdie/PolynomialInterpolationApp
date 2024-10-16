using Interpolation.Basics;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace Interpolation.Methods
{
    /*public class ChebyshevInterpolation : IInterpolationMethod
    {
        public string Name => "Chebyshev nodes Lagrange";

        private static double[] ChebyshevNodes(double a, double b, int numPoints)
        {
            double[] chebyshevNodes = new double[numPoints];
            for (int i = 1; i <= numPoints; i++)
            {
                double theta = Math.PI * (2 * i - 1) / (numPoints * 2.0);
                chebyshevNodes[i - 1] = (a + b + (b - a) * Math.Cos(theta)) * 0.5;
            }

            return chebyshevNodes;
        }

        public double Interpolate(double[][] points, double x)
        {
            double[] chebyshevNodes = ChebyshevNodes(points[0][0], points[points.GetLength(0) - 1][0], points.GetLength(0));
            chebyshevNodes = chebyshevNodes.Reverse().ToArray();
            double[] nodes = new double[chebyshevNodes.Length];
            for (int i = 0; i < chebyshevNodes.Length; i++)
            {
                nodes[i] = CalculateFunction(chebyshevNodes[i]);
            }
            int size = chebyshevNodes.Length;
            double result = 0.0;

            for (int i = 0; i < size; i++)
            {
                double term = 1.0;
                for (int j = 0; j < size; j++)
                {
                    if (j != i)
                    {
                        term *= (x - chebyshevNodes[j]) / (chebyshevNodes[i] - chebyshevNodes[j]);
                    }
                }
                result += term * nodes[i];
            }

            return result;
        }
    }*/
}