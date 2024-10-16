using Interpolation.Basics;

namespace Calculation
{
    public class InterpolationCalculator : IInterpolationCalculator
    {
        public IInterpolationMethod ActiveInterpolationMethod { get; private set; }
        private List<IInterpolationMethod> InterpolationMethods { get; set; }


        public InterpolationCalculator(IInterpolationMethod interpolationMethod)
        {
            InterpolationMethods = [interpolationMethod];
            ActiveInterpolationMethod = interpolationMethod;
        }

        /*private static double[] ChebyshevNodes(double a, double b, int numPoints)
        {
            double[] chebyshevNodes = new double[numPoints];
            for (int i = 1; i <= numPoints; i++)
            {
                double theta = Math.PI * (2 * i - 1) / (numPoints * 2.0);
                chebyshevNodes[i - 1] = (a + b + (b - a) * Math.Cos(theta)) * 0.5;
            }

            return chebyshevNodes;
        }

        private double ChebyshevInterpolation(double[][] points, double x)
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
        }*/

        /*/// <summary>
        /// Calculates Y for given X parameter.
        /// </summary>
        /// <param name="x">Function parameter.</param>
        /// <returns>Function value for given parameter.</returns>
        private double CalculateFunction(double x)
        {
            Processor proc = new();
            IExpression? exp = null;
            try
            {
                exp = proc.Parse(MathExp);
            }
            catch (Exception ex)
            {
                MathExp = "x";
                Console.WriteLine(ex.Message);
                exp = proc.Parse(MathExp);
            }

            var parameters = new ExpressionParameters
                {
                    new Parameter("x", x)
                };

            NumberValue a = (NumberValue)exp.Execute(parameters);

            return a.Number;
        }

        /// <summary>
        /// Interpolation error calculation.
        /// </summary>
        private void CalculateError()
        {
            double error = 0;
            for (int i = 0; i < InterpMathExp.Points.Count; i++)
            {
                double n = Math.Abs(CalculateFunction(InterpMathExp.Points[i].X) - InterpMathExp.Points[i].Y);
                if (error < n)
                {
                    error = n;
                }
            }

            Error = error;
            //ErrorLabel.Content = "ε = " + Error;
        }*/

        public void AddMethod(IInterpolationMethod interpolationMethod)
        {
            InterpolationMethods.Add(interpolationMethod);
        }

        public void SwitchMethod()
        {
            int index = InterpolationMethods.FindIndex(x => x == ActiveInterpolationMethod);
            IInterpolationMethod? element = InterpolationMethods.ElementAtOrDefault(index + 1);
            ActiveInterpolationMethod = element ?? InterpolationMethods[0];
        }

        public void SwitchMethod(string methodName)
        {
            int index = InterpolationMethods.FindIndex(x => x.Name == methodName);
            IInterpolationMethod? element = InterpolationMethods.ElementAtOrDefault(index);
            ActiveInterpolationMethod = element ?? InterpolationMethods[0];
        }

        /// <summary>
        /// Calculate a fixed number interpolated points from given array of points.
        /// </summary>
        /// <param name="originalArray">Array of points of some function</param>
        /// <param name="numOfInterpPts">Number of points to interpolate</param>
        /// <param name="x0">X0</param>
        public double[,] Interpolate(double[,] originalArray, uint numOfInterpPts, double x0)
        {
            double[,] interArr = new double[numOfInterpPts, 2];
            double interStep = (originalArray[originalArray.GetLength(0) - 1,0] - x0) / numOfInterpPts;

            Task[] tasks = new Task[numOfInterpPts];
            for (int i = 0; i < numOfInterpPts; i++)
            {
                int n = i;
                tasks[i] = new Task(() =>
                {
                    interArr[n, 0] = n * interStep + x0;
                    interArr[n, 1] = ActiveInterpolationMethod.InterpolatePoint(originalArray, interArr[n, 0]);
                });
                tasks[i].Start();
            }
            Task.WaitAll(tasks);

            return interArr;
        }
    }
}