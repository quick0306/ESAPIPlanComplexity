using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complexity.ApertureMetric;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    // Plan averaged beam modulation (PM)
    // Ref: Du W, et al. Quantification of beam complexity in intensity-modulated radiation therapy 
    // treatment plans. Med Phys 2014;41:21716. DOI: http://dx.doi.org/10.1118/1.4861821.
    public class PlanModulation : ComplexityMetric
    {
        public override double CalculateForBeam(Patient patient, PlanSetup plan, Beam beam)
        {
            double[] weights = GetWeights(beam);
            double[] metrics = GetMetrics(patient, plan, beam);
            double uaa = CalculateBeamUnionArea(patient, plan, beam);

            return WeightedModulationSum(weights, metrics, uaa);
        }

        // Calculater union area of all aperture of beam
        public double CalculateBeamUnionArea(Patient patient, PlanSetup plan, Beam beam)
        {
            IEnumerable<Aperture> apertures = new AperturesFromBeamCreator().Create(patient, plan, beam);

            Dictionary<int, double> areaMax = new Dictionary<int, double>();

            foreach (Aperture aperture in apertures)
            {
                int nPairs = aperture.LeafPairs.Length;
                for (int i = 0; i < nPairs; i++)
                {
                    LeafPair lp = aperture.LeafPairs[i];
                    if (!lp.IsOutsideJaw())
                    {
                        if (areaMax.ContainsKey(i))
                        {
                            areaMax[i] = lp.FieldArea() > areaMax[i] ? lp.FieldArea() : areaMax[i];
                        }
                        else
                        {
                            areaMax.Add(i, lp.FieldArea());
                        }
                    }
                }
            }

            return areaMax.Values.Sum();
        }

        private double WeightedModulationSum(double[] weights, double[] metrics, double uaa)
        {
            double sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * metrics[i];
            }

            return 1.0 - sum / (weights.Sum() * uaa);
        }

        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures
                    select aperture.Area()).ToArray();
        }
    }
}
