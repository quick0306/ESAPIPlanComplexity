using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    // 计算叶片Leaf Gap平均值与标准差
    // Ref: Nauta M, et al.Fractal analysis for assessing the level ofmodulation of IMRT fields.
    // Med Phys 2011; 38: 5385–93. doi: https://doi.org/10.1118/1.3633912.
    public class LeafGap : ComplexityMetricDictionary
    {
        public override Dictionary<string, double> CalculateForBeamDictionary(Patient patient, PlanSetup plan, Beam beam)
        {
            IEnumerable<Aperture> apertures = CreateApertures(patient, plan, beam);
            double[] leafFieldSizes = CalculatePerAperture(apertures);

            double average = leafFieldSizes.Average();
            double sumOfSquaresOfDifferences = leafFieldSizes.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / leafFieldSizes.Length);

            Dictionary<string, double> leafGapStatistics = new Dictionary<string, double>
            {
                { "Average", average },
                { "Std", sd }
            };

            return leafGapStatistics;
        }

        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            List<double> leafFiedSizes = new List<double>();

            foreach (Aperture aperture in apertures)
            {
                foreach (LeafPair lp in aperture.LeafPairs)
                {
                    if (!lp.IsOutsideJaw())
                        leafFiedSizes.Add(lp.FieldSize());
                }
            }

            return leafFiedSizes.ToArray();
        }
    }
}
