using System.Collections.Generic;
using System;
using VMS.TPS.Common.Model.API;
using Complexity.ApertureMetric;

namespace Complexity.ComplexityMetric
{
    // 计算MLC加速度在特定范围内控制点所占比例
    // 加速度范围（0，10），（10，20），（20，40）及（40，60）mm/s2
    // 注意：该复杂性指标需要控制点时间信息，当前只适应于Eclipse TPS（Varian设备？），而Elekta设备执行时剂量率
    // 和机架速度均在变化，无法得到控制点执行时间
    // Ref: Park JM, et al.The effect of MLC speed and acceleration on the plan delivery accuracy of VMAT.
    // Br J Radiol 2015; 88(1049):20140698. DOI: https://doi.org/10.1259/bjr.20140698
    public class ProportionMLCAccelerate : ComplexityMetricDictionary
    {
        public override Dictionary<string, double> CalculateForBeamDictionary(Patient patient, PlanSetup plan, Beam beam)
        {
            IEnumerable<Aperture> apertures = CreateApertures(patient, plan, beam);
            double[] metersets = GetWeights(beam);

            MLCGantryDoseStatistics MLCGantryDose = new MLCGantryDoseStatistics(beam, apertures, metersets);

            Dictionary<string, double> mlcAccelerateProportion = GetMLCAccelerateProportion(MLCGantryDose.MLCAccelerate);

            mlcAccelerateProportion.Add("Acc Average", MLCGantryDose.GetAllMLCAccelerateAverage());
            mlcAccelerateProportion.Add("Acc Std", MLCGantryDose.GetAllMLCAccelerateStdAverage());

            return mlcAccelerateProportion;
        }

        protected Dictionary<string, double> GetMLCAccelerateProportion(double[,] mlcAccelerate)
        {
            Dictionary<string, double> mlcAccelerateProportion = new Dictionary<string, double>();
            double A_0_10 = 0.0, A_10_20 = 0.0, A_20_40 = 0.0, A_40_60 = 0.0;

            foreach (double tmp in mlcAccelerate)
            {
                if (tmp >= 0 && tmp < 10)
                    A_0_10 += 1;
                if (tmp >= 10 && tmp < 20)
                    A_10_20 += 1;
                if (tmp >= 20 && tmp < 40)
                    A_20_40 += 1;
                if (tmp >= 40 && tmp < 60)
                    A_40_60 += 1;
            }

            mlcAccelerateProportion.Add("Acc (0, 10)", Math.Round(A_0_10 / mlcAccelerate.Length, 2));
            mlcAccelerateProportion.Add("Acc (10, 20)", Math.Round(A_10_20 / mlcAccelerate.Length, 2));
            mlcAccelerateProportion.Add("Acc (20, 40)", Math.Round(A_20_40 / mlcAccelerate.Length, 2));
            mlcAccelerateProportion.Add("Acc (40, 60)", Math.Round(A_40_60 / mlcAccelerate.Length, 2));

            return mlcAccelerateProportion;
        }
    }
}
