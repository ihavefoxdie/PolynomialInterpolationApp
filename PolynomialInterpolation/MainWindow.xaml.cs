using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using xFunc.Maths;
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Parameters;

namespace PolynomialInterpolation
{
    public partial class MainWindow : Window
    {
        #region Properties
        public PlotModel MyModel { get; private set; }
        public List<DataPoint> Points { get; private set; }
        private static readonly Regex _regexAny = new("[^0-9.,-]+");
        private static readonly Regex _regexUnsigned = new("[^0-9.,]");
        public string MathExp { get; private set; } = "x^(-1/2)";
        private LineSeries MathExpPts { get; set; }
        private LineSeries InterpMathExp { get; set; }
        public uint NumOfPts { get; private set; } = 20;
        public double X0 { get; private set; } = 0.1;
        public double StepSize { get; private set; } = 0.1;
        public double Error { get; private set; } = 0;
        public uint NumOfInterpPts { get; private set; } = 100;
        private Func<double[][], double, double> InterpolationMethod { get; set; }
        #endregion





        public MainWindow()
        {
            InitializeComponent();
            MyModel = new PlotModel();
            Points = new();
            InterpolationMethod = NewtonPolynomial;
            IntMethod.Content = "Newton Polynomial";
            MathExpPts = new()
            {
                Title = "original",
                StrokeThickness = 10f,
                Color = OxyColor.FromRgb(255, 200, 200)
            };

            InterpMathExp = new()
            {
                Title = "interpolated",
                Color = OxyColor.FromRgb(10, 200, 10)
            };

            MyModel.Series.Add(MathExpPts);
            MyModel.Series.Add(InterpMathExp);
        }





        #region TextBox typing restricting
        private static bool IsTextAllowed(string text)
        {
            return _regexAny.IsMatch(text);
        }

        private static bool IsTextAllowedUnsigned(string text)
        {
            return _regexUnsigned.IsMatch(text);
        }

        private void NumberVerifier(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }

        private void NumberVerifierUnsigned(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowedUnsigned(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        #endregion




        #region Buttons
        private void MathExpressoinInput(object sender, RoutedEventArgs e)
        {
            try
            {
                var exp = new Processor().Parse(MathExpressionText.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to set entered value:\n" + ex.Message);
                MathExpressionText.Text = MathExp;
            }
            MathExp = MathExpressionText.Text;
        }

        private void NumOfPointsInput(object sender, RoutedEventArgs e)
        {
            try
            {
                NumOfPts = Convert.ToUInt32(PointsNumberText.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to set entered value:\n" + ex.Message);
            }
        }

        private void X0Input(object sender, RoutedEventArgs e)
        {
            try
            {
                X0 = Convert.ToDouble(X0Text.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to set entered value:\n" + ex.Message);
            }
        }

        private void StepSizeInput(object sender, RoutedEventArgs e)
        {
            try
            {
                double temp = Convert.ToDouble(StepSizeText.Text);
                if (temp > 0)
                {
                    StepSize = temp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to set entered value:\n" + ex.Message);
            }
        }





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
        }







        private void InterpolatedPointsNumberInput(object sender, RoutedEventArgs e)
        {
            try
            {
                NumOfInterpPts = Convert.ToUInt32(InterpolatedPointsNumberText.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to set entered value:\n" + ex.Message);
            }
        }
        #endregion

        private static void DividedDifferences(double[][] points, double[,] differences, int numberOfPoints)
        {
            for (int i = 1; i < numberOfPoints; i++)
            {
                for (int j = 0; j < numberOfPoints - i; j++)
                {
                    differences[j, i] = (differences[j, i - 1] - differences[j + 1, i - 1]) /
                        (points[j][0] - points[i + j][0]);
                }
            }
        }

        private static double BasisPolynomialCalc(int currentPoint, double x, double[][] points)
        {
            double result = 1;
            for (int i = 0; i < currentPoint; i++)
            {
                result *= (x - points[i][0]);
            }

            return result;
        }

        /// <summary>
        /// Newton interpolation polynomial method (divided differences).
        /// </summary>
        /// <param name="points">Array of points of some function.</param>
        /// <param name="x">The value of X coordinate to find the Y coordinate with.</param>
        /// <returns>Interpolated function value for given parameter X.</returns>
        private static double NewtonPolynomial(double[][] points, double x)
        {
            int size = points.GetLength(0);
            double[,] differences = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                differences[i, 0] = points[i][1];
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



        /// <summary>
        /// Lagrange interpolation polynomial method.
        /// </summary>
        /// <param name="points">Array of points of some function.</param>
        /// <param name="x">The value of X coordinate to find the Y coordinate with.</param>
        /// <returns>Interpolated function value for given parameter X.</returns>
        private static double Lagrange(double[][] points, double x)
        {
            int size = points.GetLength(0);
            double interpolatedY = 0.0;

            for (int i = 0; i < size; i++)
            {
                double u = 1.0;

                for (int j = 0; j < size; j++)
                {
                    if (i != j)
                    {
                        u = (x - points[j][0]) / (points[i][0] - points[j][0]) * u;
                    }
                }
                interpolatedY += (u * points[i][1]);
            }

            return interpolatedY;
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
            ErrorLabel.Content = "ε = " + Error;
        }





        /// <summary>
        /// Calculate a fixed number points of the math expression.
        /// </summary>
        /// <returns>Array of calculated points.</returns>
        private double[][] MathExpPtsCalc()
        {
            double[][] originalArray = new double[NumOfPts][];
            Task[] tasks = new Task[NumOfPts];
            double x = X0;

            for (int i = 0; i < NumOfPts; i++)
            {
                int n = i;
                double tempX = x;
                tasks[i] = new Task(() =>
                {
                    originalArray[n] = new double[2];
                    originalArray[n][0] = tempX;
                    originalArray[n][1] = CalculateFunction(tempX);

                });
                tasks[i].Start();
                x += StepSize;
            }
            Task.WaitAll(tasks);

            for (int i = 0; i < NumOfPts; i++)
            {
                MathExpPts.Points.Add(new DataPoint(originalArray[i][0], originalArray[i][1]));
            }

            return originalArray;
        }





        /// <summary>
        /// Calculate a fixed number interpolated points from given array of points.
        /// </summary>
        /// <param name="originalArray">Array of points of some function</param>
        private void InterpMathExpPtsCalc(double[][] originalArray)
        {
            double[][] interArr = new double[NumOfInterpPts][];
            double interStep = (originalArray[originalArray.GetLength(0) - 1][0] - X0) / NumOfInterpPts;

            Points = new();
            Task[] tasks = new Task[NumOfInterpPts];
            for (int i = 0; i < NumOfInterpPts; i++)
            {
                int n = i;
                tasks[i] = new Task(() =>
                {
                    interArr[n] = new double[2];
                    interArr[n][0] = n * interStep + X0;
                    interArr[n][1] = InterpolationMethod(originalArray, interArr[n][0]);
                });
                tasks[i].Start();
            }
            Task.WaitAll(tasks);

            for (int i = 0; i < NumOfInterpPts; i++)
            {
                InterpMathExp.Points.Add(new DataPoint(interArr[i][0], interArr[i][1]));
            }
        }





        private void Proceed(object sender, RoutedEventArgs e)
        {
            MathExpPts.Points.Clear();
            InterpMathExp.Points.Clear();

            InterpMathExpPtsCalc(MathExpPtsCalc());
            CalculateError();

            MyModel.InvalidatePlot(true);
        }

        private void Change(object sender, RoutedEventArgs e)
        {
            if (InterpolationMethod == Lagrange)
            {
                InterpolationMethod = NewtonPolynomial;
                IntMethod.Content = "Newton Polynomial";
            }
            else if (InterpolationMethod == NewtonPolynomial)
            {
                InterpolationMethod = ChebyshevInterpolation;
                IntMethod.Content = "Chebyshev nodes Lagrange";
            }
            else
            {
                InterpolationMethod = Lagrange;
                IntMethod.Content = "Lagrange Polynomial";

            }
        }
    }
}