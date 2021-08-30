using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complexity.ApertureMetric;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    // 继承Complexity Metric类，计算结果返回类型为Dictionary<string, double>, string为计算metric名称，double存储metric计算结果
    public abstract class ComplexityMetricDictionary : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            throw new NotImplementedException();
        }

        #region Multiple return parameters
        public Dictionary<string, double> CalculateForPlanDictionary(Patient patient, PlanSetup plan)
        {
            Dictionary<string, List<double>> miPlan = new Dictionary<string, List<double>>();

            foreach (Beam beam in plan.Beams.Where(t => !t.IsSetupField && t.MLC != null))
            {
                Dictionary<string, double> miBeam = CalculateForBeamDictionary(patient, plan, beam);

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

            Dictionary<string, double> miWeight = new Dictionary<string, double>();
            foreach (KeyValuePair<string, List<double>> tmp in miPlan)
            {
                double[] values = tmp.Value.ToArray();
                double weightValues = Math.Round(WeightedSum(weights, values), 2);

                miWeight.Add(tmp.Key, weightValues);
            }

            return miWeight;
        }

        public abstract Dictionary<string, double> CalculateForBeamDictionary(Patient patient, PlanSetup plan, Beam beam);

        #endregion // Multiple return parameters
    }
}
