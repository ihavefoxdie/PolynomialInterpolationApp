using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private LineSeries InterpMathExp { get; set; }
        public uint NumOfPts { get; private set; } = 20;
        public double X0 { get; private set; } = 0.1;
        public double Y0 { get; private set; } = 2;
        public double StepSize { get; private set; } = 0.1;
        public uint NumOfInterpPts { get; private set; } = 100;
        #endregion




        public MainWindow()
        {
            InitializeComponent();
            MyModel = new PlotModel();
            Points = new();

            InterpMathExp = new()
            {
                Title = "interpolated",
                Color = OxyColor.FromRgb(10, 200, 10)
            };

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

        private void Y0Input(object sender, RoutedEventArgs e)
        {
            try
            {
                Y0 = Convert.ToDouble(yOfZero.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to set entered value:\n" + ex.Message);
            }
        }
        #endregion




        #region Calculations
        private double[,] RungeKutta(double y0, double x0, double step, int n)
        {
            double[,] result = new double[n + 1, 2];
            result[0, 0] = x0; result[0, 1] = y0;

            for (int i = 0; i < n; i++)
            {
                double k1 = step * CalculateFunction(result[i, 0], result[i, 1]);
                double k2 = step * CalculateFunction(result[i, 0] + step / 2, result[i, 1] + k1 / 2);
                double k3 = step * CalculateFunction(result[i, 0] + step / 2, result[i, 1] + k2 / 2);
                double k4 = step * CalculateFunction(result[i, 0] + step, result[i, 1] + k3);

                result[i + 1, 0] = result[i, 0] + step;
                result[i + 1, 1] = result[i, 1] + (k1 + 2 * k2 + 2 * k3 + k4) / 6;
            }

            return result;
        }


        /// <summary>
        /// Calculates Y for given X parameter.
        /// </summary>
        /// <param name="x">Function parameter.</param>
        /// <returns>Function value for given parameter.</returns>
        private double CalculateFunction(double x, double y)
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
                    new Parameter("x", x),
                    new Parameter("y", y)
                };

            try
            {
                NumberValue a = (NumberValue)exp.Execute(parameters);
                return a.Number;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return 0.0;
        }


        /// <summary>
        /// Calculate a fixed number interpolated points from given array of points.
        /// </summary>
        /// <param name="originalArray">Array of points of some function</param>
        private void InterpMathExpPtsCalc()
        {
            double[,] interArr = RungeKutta(X0, Y0, StepSize, (int)NumOfPts);

            for (int i = 0; i < interArr.GetLength(0); i++)
            {
                InterpMathExp.Points.Add(new DataPoint(interArr[i,0], interArr[i,1]));
            }
        }
        #endregion




        private void Proceed(object sender, RoutedEventArgs e)
        {
            InterpMathExp.Points.Clear();

            InterpMathExpPtsCalc();

            MyModel.InvalidatePlot(true);
        }
    }
}