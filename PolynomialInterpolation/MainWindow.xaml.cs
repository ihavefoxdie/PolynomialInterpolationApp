using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using xFunc.Maths;
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Parameters;
using xFunc.Maths.Results;
using static xFunc.Maths.Results.Result;

namespace PolynomialInterpolation
{
    public partial class MainWindow : Window
    {
        public PlotModel MyModel { get; private set; }
        public List<DataPoint> Points { get; private set; }
        private static readonly Regex _regexAny = new("[^0-9.-]+");
        private static readonly Regex _regexUnsigned = new("[^0-9.]");
        public string MathExpression { get; private set; } = "x^(-1/2)";
        public uint NumberOfPoints { get; private set; } = 20;
        public double X0 { get; private set; } = 0.1;
        public double StepSize { get; private set; } = 0.1;
        public uint NumberOfInterpolatedPoints { get; private set; } = 100;


        public MainWindow()
        {
            InitializeComponent();

            MyModel = new PlotModel();
            //MyModel.InvalidatePlot(true);
        }

        private static bool IsTextAllowed(string text)
        {
            return _regexAny.IsMatch(text);
        }
        private static bool IsTextAllowedUnsigned(string text)
        {
            return _regexUnsigned.IsMatch(text);
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
            List<DataPoint> points = new List<DataPoint>();
            List<String> data = new();
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(path);
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    data.Add(line);
                    //write the line to console window
                    Console.WriteLine(line);
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
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

            for (int i = 0; i < data.Count; i++)
            {
                String[] arr = data[i].Split(" ");
                points.Add(new DataPoint(double.Parse(arr[0], NumberStyles.Float, CultureInfo.InvariantCulture), double.Parse(arr[1], CultureInfo.InvariantCulture)));
            }

            return points;
        }

        private void NumberVerifier(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }
        private void NumberVerifierUnsigned(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowedUnsigned(e.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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

        private void Proceed(object sender, RoutedEventArgs e)
        {
            MyModel.InvalidatePlot(true);
            MyModel.Series.Clear();
            Processor proc = new();
            var exp = proc.Parse(MathExpression);
            
            Func<double, double> powFunction = (x) =>
            {
                var parameters = new ExpressionParameters
                {
                    new Parameter("x", x)
                };
                NumberValue a = (NumberValue)exp.Execute(parameters);
                
                //var result = (Result)a;
                return a.Number;
            };

            MyModel.Series.Add(new FunctionSeries(powFunction, X0, NumberOfPoints, StepSize, MathExpression) { StrokeThickness = 10f, Color = OxyColor.FromRgb(255, 200, 200) });
            double[][] arr = new double[NumberOfPoints][];

            double x = X0;
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new double[2];
                var parameters = new ExpressionParameters
                {
                    new Parameter("x", x)
                };
                NumberValue a = (NumberValue)exp.Execute(parameters);
                arr[i][0] = x;
                arr[i][1] = a.Number;
                x += StepSize;
            }
            WritePoints(arr);
            LineSeries interpolatedFunction = new();
            interpolatedFunction.Title = "interpolated";
            interpolatedFunction.Color = OxyColor.FromRgb(200, 10, 10);

            Process process = new();
            process.StartInfo.FileName = "lagrange_interpolating_polynomial.exe";
            process.StartInfo.Arguments = NumberOfPoints.ToString() + " " + X0.ToString() + " " + StepSize.ToString() + " " + NumberOfInterpolatedPoints.ToString();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            Points = ParsePoints("points.txt");
            File.Delete("points.txt");
            Points.ForEach((x => { interpolatedFunction.Points.Add(x); }));
            MyModel.Series.Add(interpolatedFunction);
            MyModel.InvalidatePlot(true);
        }
    }
}