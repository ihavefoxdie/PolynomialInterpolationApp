using xFunc.Maths;
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Parameters;

namespace Calculation
{
    public class ExpressionCalculator : IExpressionCalculator
    {
        private string MathExpression { get; set; }

        public ExpressionCalculator(string mathExpression)
        {
            MathExpression = mathExpression;
        }

        public void SetMathExpression(string mathExpression)
        {
            var lel = new Processor().Parse(mathExpression);
            MathExpression = mathExpression;
        }

        /// <summary>
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
                exp = proc.Parse(MathExpression);
            }
            catch (Exception ex)
            {
                MathExpression = "x";
                Console.WriteLine(ex.Message);
                exp = proc.Parse(MathExpression);
            }

            var parameters = new ExpressionParameters
                {
                    new Parameter("x", x)
                };

            NumberValue a = (NumberValue)exp.Execute(parameters);

            return a.Number;
        }

        /// <summary>
        /// Calculate a fixed number points of the math expression.
        /// </summary>
        /// <returns>Array of calculated points.</returns>
        public double[,] CalculateMathExpression(uint numberOfPoints, double x0, double StepSize)
        {
            double[,] originalArray = new double[numberOfPoints,2];
            Task[] tasks = new Task[numberOfPoints];
            double x = x0;

            for (int i = 0; i < numberOfPoints; i++)
            {
                int n = i;
                double tempX = x;
                tasks[i] = new Task(() =>
                {
                    originalArray[n, 0] = tempX;
                    originalArray[n, 1] = CalculateFunction(tempX);

                });
                tasks[i].Start();
                x += StepSize;
            }
            Task.WaitAll(tasks);

            return originalArray;
        }
    }
}