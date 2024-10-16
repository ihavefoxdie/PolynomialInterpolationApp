
namespace Interpolation.Basics
{
    public interface IInterpolationMethod
    {
        public string Name { get; }
        public double InterpolatePoint(double[,] points, double x);
    }

    public interface IInterpolationCalculator
    {
        public IInterpolationMethod ActiveInterpolationMethod { get; }
        public double[,] Interpolate(double[,] originalArray, uint numOfInterpPts, double x0);
        public void AddMethod(IInterpolationMethod method);
        public void SwitchMethod();
    }
}
