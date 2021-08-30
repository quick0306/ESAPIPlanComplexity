using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    public class ProportionMLCSpeed : ComplexityMetricDictionary
    {
        // 计算MLC速度在特定范围内控制点所占比例
        // 速度范围（0，4），（4，8），（4，12）, (12，16)，（16，20）及（20，25）mm/s
        // 注意：该复杂性指标需要控制点时间信息，当前只适应于Eclipse TPS（Varian设备？），而Elekta设备执行时剂量率
        // 和机架速度均在变化，无法得到控制点执行时间
        // Ref: Park JM, et al.The effect of MLC speed and acceleration on the plan delivery accuracy of VMAT.
        // Br J Radiol 2015; 88(1049):20140698. DOI: https://doi.org/10.1259/bjr.20140698
        public override Dictionary<string, double> CalculateForBeamDictionary(Patient patient, PlanSetup plan, Beam beam)
        {
            IEnumerable<Aperture> apertures = CreateApertures(patient, plan, beam);
            double[] metersets = GetWeights(beam);

            MLCGantryDoseStatistics MLCGantryDose = new MLCGantryDoseStatistics(beam, apertures, metersets);

            Dictionary<string, double> mlcSpeedProportion = GetMLCSpeedProportion(MLCGantryDose.MLCSpeed);

            mlcSpeedProportion.Add("Speed Average", MLCGantryDose.GetAllMLCSpeedAverage());
            mlcSpeedProportion.Add("Speed Std", MLCGantryDose.GetAllMLCSpeedStdAverage());

            return mlcSpeedProportion;
        }

        protected Dictionary<string, double> GetMLCSpeedProportion(double[,] mlcSpeed)
        {
            Dictionary<string, double> mlcSpeedProportion = new Dictionary<string, double>();
            double S_0_4 = 0.0, S_4_8 = 0.0, S_8_12 = 0.0, S_12_16 = 0.0, S_16_20 = 0.0, S_20_25 = 0.0;

            foreach (double tmp in mlcSpeed)
            {
                if (tmp >= 0 && tmp < 4)
                    S_0_4 += 1;
                if (tmp >= 4 && tmp < 8)
                    S_4_8 += 1;
                if (tmp >= 8 && tmp < 12)
                    S_8_12 += 1;
                if (tmp >= 12 && tmp < 16)
                    S_12_16 += 1;
                if (tmp >= 16 && tmp < 20)
                    S_16_20 += 1;
                if (tmp >= 20 && tmp < 25)
                    S_20_25 += 1;
                if (tmp >= 25)
                    Console.WriteLine($"Error Speed > 25 mm/s, Speed = {tmp}");
            }
         
            mlcSpeedProportion.Add("Speed (0, 4)", Math.Round(S_0_4 / mlcSpeed.Length, 2));
            mlcSpeedProportion.Add("Speed (4, 8)", Math.Round(S_4_8 / mlcSpeed.Length, 2));
            mlcSpeedProportion.Add("Speed (8, 12)", Math.Round(S_8_12 / mlcSpeed.Length, 2));
            mlcSpeedProportion.Add("Speed (12, 16)", Math.Round(S_12_16 / mlcSpeed.Length, 2));
            mlcSpeedProportion.Add("Speed (16, 20)", Math.Round(S_16_20 / mlcSpeed.Length, 2));
            mlcSpeedProportion.Add("Speed (20, 25)", Math.Round(S_20_25 / mlcSpeed.Length, 2));

            return mlcSpeedProportion;
        }
    }
}
