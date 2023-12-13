using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private static readonly Regex _regexAny = new("[^0-9.-]+");
        private static readonly Regex _regexUnsigned = new("[^0-9.]");
        public string MathExpression { get; private set; } = "x^(-1/2)";
        public uint NumberOfPoints { get; private set; } = 20;
        public double X0 { get; private set; } = 0.1;
        public double StepSize { get; private set; } = 0.1;
        public double Error { get; private set; } = 0;
        public uint NumberOfInterpolatedPoints { get; private set; } = 100;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            MyModel = new PlotModel();
            Points = new();
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


        private static void WritePoints(double[][] arr)
        {
            string path = "initPoints.txt";
            File.Delete(path);

            for (int i = 0; i < arr.Length; i++)
            {
                File.AppendAllText(path, arr[i][0] + " " + arr[i][1] + "\n");
            }
        }

        private static List<DataPoint> ParsePoints(string path)
        {
            List<DataPoint> points = new();
            String? line;
            try
            {
                StreamReader sr = new(path);
                line = sr.ReadLine();

                while (line != null)
                {
                    String[] arr = line.Split(" ");
                    points.Add(new DataPoint(double.Parse(arr[0], NumberStyles.Float, CultureInfo.InvariantCulture), double.Parse(arr[1], CultureInfo.InvariantCulture)));
                    line = sr.ReadLine();
                }
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            return points;
        }


        #region Buttons
        private void MathExpressoinInput(object sender, RoutedEventArgs e)
        {
            MathExpression = MathExpressionText.Text;
        }

        private void NumOfPointsInput(object sender, RoutedEventArgs e)
        {
            NumberOfPoints = Convert.ToUInt32(PointsNumberText.Text);
        }

        private void X0Input(object sender, RoutedEventArgs e)
        {
            X0 = Convert.ToDouble(X0Text.Text);
        }

        private void StepSizeInput(object sender, RoutedEventArgs e)
        {
            double temp = Convert.ToDouble(StepSizeText.Text);
            if (temp > 0)
            {
                StepSize = temp;
            }
        }

        private void InterpolatedPointsNumberInput(object sender, RoutedEventArgs e)
        {
            NumberOfInterpolatedPoints = Convert.ToUInt32(InterpolatedPointsNumberText.Text);
        }
        #endregion


        private static double Lagrange(double[] pointsX, double[] pointsY, double x)
        {
            int size = pointsX.Length;
            double interpolatedY = 0.0;

            for (int i = 0; i < size; i++)
            {
                double u = 1.0;

                for (int j = 0; j < size; j++)
                {
                    if (i != j)
                    {
                        u = (x - pointsX[j]) / (pointsX[i] - pointsX[j]) * u;
                    }
                }
                interpolatedY += (u * pointsY[i]);
            }

            return interpolatedY;
        }

        private double CalculateFunction(double x)
        {
            Processor proc = new();
            var exp = proc.Parse(MathExpression);

            var parameters = new ExpressionParameters
                {
                    new Parameter("x", x)
                };

            NumberValue a = (NumberValue)exp.Execute(parameters);

            return a.Number;
        }

        private void MakeCalculations()
        {
            using (Process process = new())
            {
                process.StartInfo.FileName = "lagrange_interpolating_polynomial.exe";
                process.StartInfo.Arguments = NumberOfPoints.ToString() + " " + X0.ToString() + " " + StepSize.ToString() + " " + NumberOfInterpolatedPoints.ToString();
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit(5000);
                process.Close();
                process.Dispose();
            }
        }

        private void CalculateError()
        {
            double error = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                double n = Math.Abs(CalculateFunction(Points[i].X) - Points[i].Y);
                if (error < n)
                {
                    error = n;
                }
            }

            Error = error;
            ErrorLabel.Content = "ε = " + Error.ToString("e3", CultureInfo.InvariantCulture);
        }

        private void Proceed(object sender, RoutedEventArgs e)
        {
            MyModel.InvalidatePlot(true);
            MyModel.Series.Clear();

            LineSeries originalFunction = new()
            {
                Title = "interpolated",
                StrokeThickness = 10f,
                Color = OxyColor.FromRgb(255, 200, 200)
            };

            double[][] arr = new double[NumberOfPoints][];
            Task[] tasks = new Task[NumberOfPoints];

            double x = X0;
            for (int i = 0; i < NumberOfPoints; i++)
            {
                int n = i;
                double tempX = x;
                tasks[i] = new Task(() =>
                {
                    arr[n] = new double[2];
                    arr[n][0] = tempX;
                    arr[n][1] = CalculateFunction(tempX);
                });
                tasks[i].Start();
                x += StepSize;
            }
            Task.WaitAll(tasks);
            WritePoints(arr);

            for (int i = 0; i < NumberOfPoints; i++)
            {
                originalFunction.Points.Add(new DataPoint(arr[i][0], arr[i][1]));
            }

            //Points.ForEach((x => { originalFunction.Points.Add(x); }));

            MyModel.Series.Add(originalFunction);





            LineSeries interpolatedFunction = new()
            {
                Title = "interpolated",
                Color = OxyColor.FromRgb(10, 200, 10)
            };

            /*double[] pointsY = new double[NumberOfPoints];
            double[] pointsX = new double[NumberOfPoints];

            for (int i = 0; i < NumberOfPoints; i++)
            {
                pointsX[i] = i * StepSize + X0;
            }

            double intStep = Math.Round((pointsX[pointsX.Length - 1] - pointsX[0]) / NumberOfInterpolatedPoints, 5);

            for (int i = 0; i < NumberOfPoints; i++)
            {
                pointsY[i] = Math.Pow(pointsX[i], -(1.0 / 2.0));
            }

            Points = new();

            for (int i = 0; i < NumberOfInterpolatedPoints; i++)
            {
                Points.Add(new DataPoint(i * intStep + X0, Lagrange(pointsX, pointsY, i * intStep + X0)));
            }*/

            try
            {
                MakeCalculations();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Points = ParsePoints("points.txt");
            Points.ForEach((x => { interpolatedFunction.Points.Add(x); }));
            MyModel.Series.Add(interpolatedFunction);

            CalculateError();

            MyModel.InvalidatePlot(true);
        }
    }
}