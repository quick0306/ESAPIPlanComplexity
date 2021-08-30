using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    /// <summary>
    /// 计算VMAT计划MLC速度、加速度、机架加速度和剂量率变化调制指数（Modulation Index for Speed and Acceleration of MLC, 
    /// Gantry Acceleration and Dose Rate Variation, MIs, MIa, MIt）
    /// Ref: Park JM, Park S-Y, Kim H, et al.Modulation indices for volumetric modulated Arc therapy.
    /// Phys Med Biol 2014; 59: 7315–40. doi: https://doi.org/10.1088/0031-9155/59/23/7315
    /// </summary>
    public class ModulationIndexScoreMetric : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, double> CalculateForPlan(Patient patient, PlanSetup plan, double k = 0.02)
        {
            Dictionary<string, List<double>> miPlan = new Dictionary<string, List<double>>();

            foreach (Beam beam in plan.Beams.Where(t => !t.IsSetupField))
            {
                Dictionary<string, double> miBeam = CalculateForBeam(patient, plan, beam, k);

                foreach (KeyValuePair<string, double> tmp in miBeam)
                {
                    if (miPlan.ContainsKey(tmp.Key))
                    {
                        miPlan[tmp.Key].Add(tmp.Value);
                    }
                    else
                    {
                        miPlan.Add(tmp.Key, new List<double>() { tmp.Value });
                    } 
                }
            }

            double[] weights = GetWeights(plan);

            // Beam MI weight MU
            Dictionary<string, double> miWeight = new Dictionary<string, double>();
            foreach (KeyValuePair<string, List<double>> tmp in miPlan)
            {
                double[] values = tmp.Value.ToArray();
                double weightValues = WeightedSum(weights, values);

                miWeight.Add(tmp.Key, weightValues);
            }

            return miWeight;
        }

        public Dictionary<string, double> CalculateForBeam(Patient patient, PlanSetup plan, Beam beam, double k = 0.02)
        {
            IEnumerable<Aperture> apertures = CreateApertures(patient, plan, beam);
            double[] metersets = new MetersetsFromMetersetWeightsCreator().GetCumulativeMetersets(beam);

            ModulationIndexTotal mid = new ModulationIndexTotal(beam, apertures, metersets);
            Dictionary<string, double> mi = mid.CalculateIntegrate(k);

            return mi;
        }
    }
}
