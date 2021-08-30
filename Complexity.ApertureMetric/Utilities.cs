using System.Linq;
using Complexity.ApertureMetric;

namespace Complexity.ApertureMetric
{
    public class Utilities
    {
        public static double DivisionOrDefault(double a, double b)
        {
            return (b != 0.0) ? (a / b) : 0.0;
        }

        // 2D array column (MLC Leafs) division, deltaTime length equal 2D array column vector length
        public static double[,] Get2DArrayColumnDivision(double[,] pos, double[] deltaTime)
        {
            int rowNumber = pos.GetLength(0);        // aperture count
            int columnNumber = pos.GetLength(1);     // leaf count
            double[,] val = new double[rowNumber, columnNumber];

            for (int i = 0; i < columnNumber; i++)
            {
                double[] leafPairPos = MatrixArray<double>.GetColumn(pos, i);
                double[] leafPairPosDiff = Algebra.Diff(leafPairPos);
                for (int j = 0; j < rowNumber; j++)
                {
                    val[j, i] = leafPairPosDiff[j] / deltaTime[j];
                }
            }

            return val;
        }

        // 2D array column（MLC Leafs） std
        public static double[] Get2DArrayColumnStd(double[,] pos)
        {
            int columnNumber = pos.GetLength(1);         // leaf count

            double[] val = new double[columnNumber];

            for (int i = 0; i < columnNumber; i++)
            {
                double[] columnArray = MatrixArray<double>.GetColumn(pos, i);
                double columnArrayStd = Algebra.Std(columnArray);

                val[i] = columnArrayStd;
            }

            return val;
        }

        // Get 2D array average, except 0
        public static double Get2DArrayAverage(double[,] val)
        {
            double sum = 0.0;
            int N = 0;

            foreach (double tmp in val)
            {
                if (tmp > 0)
                {
                    sum += tmp;
                    N += 1;
                }
            }

            return DivisionOrDefault(sum, N);
        }

        // Get 1D array average, except 0
        public static double Get1DArrayAverage(double[] val)
        {
            double sum = 0.0;
            int N = 0;

            foreach (double tmp in val)
            {
                if (tmp > 0)
                {
                    sum += tmp;
                    N += 1;
                }
            }

            return DivisionOrDefault(sum, N);
        }

        // 2D array NaN convert to 0.0
        public static double[,] Convert2DArrayNaNToNumber(double[,] val)
        {
            int m = val.GetLength(0);
            int n = val.GetLength(1);
            double[,] valNoNaN = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    valNoNaN[i, j] = double.IsNaN(val[i, j]) ? 0.0 : val[i, j];
                }
            }

            return valNoNaN;
        }

        // 1D array NaN convert to 0.0
        public static double[] Convert1DArrayNaNToNumber(double[] val)
        {
            int m = val.Length;
            double[] valNoNaN = new double[m];
            for (int i = 0; i < m; i++)
            {
                valNoNaN[i] = double.IsNaN(val[i]) ? 0.0 : val[i];
            }

            return valNoNaN;
        }
    }
}
