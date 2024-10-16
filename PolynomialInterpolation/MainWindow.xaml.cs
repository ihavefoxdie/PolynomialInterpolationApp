using OxyPlot;
using OxyPlot.Series;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using Interpolation.Methods;
using Interpolation.Basics;
using Calculation;

namespace PolynomialInterpolation
{
    public partial class MainWindow : Window
    {
        #region Properties
        public PlotModel MyModel { get; private set; }
        private LineSeries MathExpPts { get; set; }
        private LineSeries InterpMathExp { get; set; }

        private static readonly Regex _regexAny = new("[^0-9.,-]+");
        private static readonly Regex _regexUnsigned = new("[^0-9.,]");

        public string MathExp { get; private set; } = "x^(-1/2)";

        public double Error { get; private set; } = 0;

        private IInterpolationCalculator FunctionCalculation { get; set; }
        private IExpressionCalculator ExpressionCalculator { get; set; }

        public uint NumOfInterpPts { get; private set; } = 100;
        public double StepSize { get; private set; } = 0.1;
        public double X0 { get; private set; } = 0.1;
        public uint NumOfPts { get; private set; } = 20;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            ExpressionCalculator = new ExpressionCalculator(MathExp);
            FunctionCalculation = new InterpolationCalculator(new NewtonInterpolation());
            MyModel = new PlotModel();
            FunctionCalculation.AddMethod(new LagrangeInterpolation());

            IntMethod.Content = FunctionCalculation.ActiveInterpolationMethod.Name;
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
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
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
                ExpressionCalculator.SetMathExpression(MathExpressionText.Text);
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

        private void Proceed(object sender, RoutedEventArgs e)
        {
            MathExpPts.Points.Clear();
            InterpMathExp.Points.Clear();
            double[,] originalArray = ExpressionCalculator.CalculateMathExpression(NumOfPts, X0, StepSize);
            double[,] interpolatedArray = FunctionCalculation.Interpolate(originalArray, NumOfInterpPts, X0);


            for (int i = 0; i < NumOfPts; i++)
            {
                MathExpPts.Points.Add(new DataPoint(originalArray[i,0], originalArray[i, 1]));
            }

            for (int i = 0; i < NumOfInterpPts; i++)
            {
                InterpMathExp.Points.Add(new DataPoint(interpolatedArray[i, 0], interpolatedArray[i, 1]));
            }

            double stepSize = (originalArray[originalArray.GetLength(0) - 1,0] - X0) / NumOfInterpPts;

            originalArray = ExpressionCalculator.CalculateMathExpression(NumOfInterpPts, X0, stepSize);

            Error = IErrorCalculator.CalculateError(interpolatedArray, originalArray);
            ErrorLabel.Content = "ε = " + Error;

            MyModel.InvalidatePlot(true);
        }

        private void Change(object sender, RoutedEventArgs e)
        {
            FunctionCalculation.SwitchMethod();
            IntMethod.Content = FunctionCalculation.ActiveInterpolationMethod.Name;
        }
        #endregion
    }
}