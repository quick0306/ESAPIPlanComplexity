using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Integration;
using Complexity.ApertureMetric;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    public class ModulationIndexTotal
    {
        private readonly MLCGantryDoseStatistics MLCGantryDose;

        public ModulationIndexTotal(Beam beam, IEnumerable<Aperture> apertures, double[] metersets)
        {
            MLCGantryDose = new MLCGantryDoseStatistics(beam, apertures, metersets);
        }

        public Dictionary<string, double> CalculateIntegrate(double k = 1.0, double beta = 2.0, double gamma = 2.0)
        {
            Dictionary<string, double> mi = new Dictionary<string, double>();

            double[,] mlcSpeed = Utilities.Convert2DArrayNaNToNumber(MLCGantryDose.MLCSpeed);
            double[] mlcSpeedStd = Utilities.Convert1DArrayNaNToNumber(MLCGantryDose.MLCSpeedStd);
            double mis = CalculateMISpeed(mlcSpeed, mlcSpeedStd, k);
            mi.Add("mis", mis);

            double[,] mlcAccelerate = Utilities.Convert2DArrayNaNToNumber(MLCGantryDose.MLCAccelerate);
            double[] mlcAccelerateStd = Utilities.Convert1DArrayNaNToNumber(MLCGantryDose.MLCAccelerateStd);
            double alphaAcc = 1.0 / (Utilities.Get1DArrayAverage(MLCGantryDose.DeltaTime));
            double mia = CalculateMIAccelerate(mlcSpeed, mlcSpeedStd, mlcAccelerate, mlcAccelerateStd, k, alphaAcc);
            mi.Add("mia", mia);

            double[] gantryAccelerate = Utilities.Convert1DArrayNaNToNumber(MLCGantryDose.GantryAccelerate);
            double[] wga = GetExpArray(gantryAccelerate, beta, gamma);
            double[] deltaDoseRate = Utilities.Convert1DArrayNaNToNumber(MLCGantryDose.DeltaDoseRate);
            double[] wmu = GetExpArray(deltaDoseRate, beta, gamma);
            double mit = CalculateMITotal(mlcSpeed, mlcSpeedStd, mlcAccelerate, mlcAccelerateStd, k, alphaAcc, wga, wmu);
            mi.Add("mit", mit);

            return mi;
        }


        private double CalculateMISpeed(double[,] mlcSpeed, double[] mlcSpeedStd, double k)
        {
            int columnNumber = mlcSpeed.GetLength(1);         // leaf count 
            double mis = 0.0;

            for (int i = 0; i < columnNumber; i++)
            {
                double[] columnArray = MatrixArray<double>.GetColumn(mlcSpeed, i);
               
                Func<double, double> calc_z = f => 1.0 / (MLCGantryDose.Ncp - 1.0) * 
                                                    (from j in Algebra.Sequence(columnArray.Length) 
                                                    where (columnArray[j] > f * mlcSpeedStd[i]) 
                                                    select columnArray[j]).Count();

                mis += NewtonCotesTrapeziumRule.IntegrateAdaptive(calc_z, 0.0, k, 1e-5);
            }

            return mis;
        }

        private double CalculateMIAccelerate(double[,] mlcSpeed, double[] mlcSpeedStd, 
            double[,] mlcAccelerate, double[] mlcAccelerateStd, double k, double alphaAcc)
        {
            int columnNumber = mlcSpeed.GetLength(1);         // leaf count 
            double mia = 0.0;

            for (int i = 0; i < columnNumber; i++)
            {
                double[] mlcSpeedArray = MatrixArray<double>.GetColumn(mlcSpeed, i);
                double[] mlcAccelerateArray = MatrixArray<double>.GetColumn(mlcAccelerate, i);

                Func<double, double> calc_z = f => 1.0 / (MLCGantryDose.Ncp - 2.0) *
                                                    (from j in Algebra.Sequence(mlcSpeedArray.Length)
                                                     where (mlcSpeedArray[j] > f * mlcSpeedStd[i] || 
                                                     mlcAccelerateArray[j] > alphaAcc * f * mlcAccelerateStd[i])
                                                     select mlcSpeedArray[j]).Count();

                mia += NewtonCotesTrapeziumRule.IntegrateAdaptive(calc_z, 0.0, k, 1e-5);
            }

            return mia;
        }


        private double CalculateMITotal(double[,] mlcSpeed, double[] mlcSpeedStd, double[,] mlcAccelerate,
            double[] mlcAccelerateStd, double k, double alphaAcc, double[] wga, double[] wmu)
        {
            int columnNumber = mlcSpeed.GetLength(1);         // leaf count 
            double mit = 0.0;

            for (int i = 0; i < columnNumber; i++)
            {
                double[] mlcSpeedArray = MatrixArray<double>.GetColumn(mlcSpeed, i);
                double[] mlcAccelerateArray = MatrixArray<double>.GetColumn(mlcAccelerate, i);

                Func<double, double> calc_z = f => 1.0 / (MLCGantryDose.Ncp - 2.0) *
                                                    (from j in Algebra.Sequence(mlcSpeedArray.Length)
                                                     where (mlcSpeedArray[j] > f * mlcSpeedStd[i] ||
                                                     mlcAccelerateArray[j] > alphaAcc * f * mlcAccelerateStd[i])
                                                     select wga[j] * wmu[j]).Sum();

                mit += NewtonCotesTrapeziumRule.IntegrateAdaptive(calc_z, 0.0, k, 1e-5);
            }

            return mit;
        }

        private double[] GetExpArray(double[] val, double beta = 2.0, double gamma = 2.0)
        {
            double[] wga = new double[val.Length];

            for (int i = 0; i < val.Length; i++)
            {
                wga[i] = beta / (1 + (beta - 1) * Math.Exp(-val[i] / gamma));
            }

            return wga;
        }
    }
}
