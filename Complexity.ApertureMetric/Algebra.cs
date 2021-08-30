using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace Complexity.ApertureMetric
{
    public static class Algebra
    {
        public static double Sum(int start, int end, Func<int, double> func)
        {
            return Enumerable.Range(start, end - start + 1).Sum(func);
        }

        public static double Mean(int start, int end, Func<int, double> func)
        {
            return Sum(start, end, func) / (end - start + 1);
        }

        public static double Distance(double x, double y)
        {
            return Math.Abs(x - y);
        }

        public static IEnumerable<int> Sequence(int n)
        {
            return Enumerable.Range(0, n);
        }

        // Diff function 0 position is NaN, like Python pandas diff function, using Math.abs()
        public static double[] Diff(double[] val)
        {
            int n = val.Length;
            double[] valDiff = new double[n];

            valDiff[0] = double.NaN;
            for (int i = 1; i < n; i++)
            {
                valDiff[i] = Math.Abs(val[i] - val[i - 1]);
            }

            return valDiff;
        }

        // 计算标准差前需排除所有NaN元素
        public static double Std(double[] val)
        {
            double[] valNotNaN = (from i in Algebra.Sequence(val.Length) 
                                  where !double.IsNaN(val[i]) 
                                  select val[i]).ToArray();

            double average = valNotNaN.Average();
            double sumOfSquaresOfDifferences = valNotNaN.Select(t => (t - average) * (t - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / valNotNaN.Length);

            return sd;
        }

        // 矩阵差分求和
        public static double DiffSum(double[] val)
        {
            double sum = 0;
            
            for (int i = 1; i < val.Length; i++)
            {
                sum += Math.Abs(val[i] - val[i - 1]);
            }

            return sum;
        }
    }
}