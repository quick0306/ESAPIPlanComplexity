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
    public abstract class ProportionMLC : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            throw new NotImplementedException();
        }

        #region Multiple return parameters
        public Dictionary<string, double> CalculateForPlanReturnDictionary(Patient patient, PlanSetup plan)
        {
            Dictionary<string, List<double>> miPlan = new Dictionary<string, List<double>>();

            foreach (Beam beam in plan.Beams.Where(t => !t.IsSetupField))
            {
                Dictionary<string, double> miBeam = CalculateForBeamReturnDictionary(patient, plan, beam);

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
                double weightValues = WeightedSum(weights, values);

                miWeight.Add(tmp.Key, weightValues);
            }

            return miWeight;
        }

        public abstract Dictionary<string, double> CalculateForBeamReturnDictionary(Patient patient, PlanSetup plan, Beam beam);

        #endregion // Multiple return parameters
    }
}
