using Interpolation.Basics;

namespace Interpolation.Methods
{
    /// <summary>
    /// Newton interpolation polynomial method (divided differences).
    /// </summary>
    /// <param name="points">Array of points of some function.</param>
    /// <param name="x">The value of X coordinate to find the Y coordinate with.</param>
    /// <returns>Interpolated function value for given parameter X.</returns>
    public class NewtonInterpolation : IInterpolationMethod
    {
        public string Name => "Newton Polynomial";

        private static void DividedDifferences(double[,] points, double[,] differences, int numberOfPoints)
        {
            for (int i = 1; i < numberOfPoints; i++)
            {
                for (int j = 0; j < numberOfPoints - i; j++)
                {
                    differences[j, i] = (differences[j, i - 1] - differences[j + 1, i - 1]) /
                        (points[j,0] - points[i + j, 0]);
                }
            }
        }

        private static double BasisPolynomialCalc(int currentPoint, double x, double[,] points)
        {
            double result = 1;
            for (int i = 0; i < currentPoint; i++)
            {
                result *= (x - points[i, 0]);
            }

            return result;
        }

        public double InterpolatePoint(double[,] points, double x)
        {
            int size = points.GetLength(0);
            double[,] differences = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                differences[i, 0] = points[i, 1];
            }

            DividedDifferences(points, differences, size);

            double result = 0.0;
            for (int i = 0; i < size; i++)
            {
                result += BasisPolynomialCalc(i, x, points) * differences[0, i];
            }

            Console.WriteLine("\nValue at " + (x) + " is "
            + result);

            return result;
        }
    }
}
