namespace Calculation
{
    public interface IExpressionCalculator
    {
        public void SetMathExpression(string mathExpression);
        public double[,] CalculateMathExpression(uint numberOfPoints, double x0, double StepSize);
    }

    public interface IErrorCalculator
    {
        public static double CalculateError(double[,] interpMathExp, double[,] originalMathExp)
        {
            double error = 0;
            if (originalMathExp.GetLength(0) != interpMathExp.GetLength(0) ||
                originalMathExp.GetLength(1) != interpMathExp.GetLength(1))
                throw new ArgumentException("Given arrays are not of the same length!");

            for (int i = 0; i < interpMathExp.GetLength(0); i++)
            {
                decimal o = (decimal)originalMathExp[i, 1] - (decimal)interpMathExp[i, 1];
                decimal n = Math.Abs((decimal)originalMathExp[i, 1] - (decimal)interpMathExp[i, 1]);
                if (error < (double)n)
                {
                    error = (double)n;
                }
            }

            return error;
        }
    }
}