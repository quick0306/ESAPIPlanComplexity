using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    // 小野评分，计算小于给定阈值距离（x）的开放叶片对的比例
    // Ref: Crowe SB, et al. Examination of the properties of IMRT and VMAT beams and evaluation against pre-treatment 
    // quality assurance results. Phys Med Biol 2015; 60(6):2587-2601. DOI: https://doi.org/10.1088/0031-9155/60/6/2587
    public class SmallApertureScoreMetric : ComplexityMetric
    {
        public double CalculateForPlan(Patient patient, PlanSetup plan, double x =5.0)
        {
            return WeightedSum(GetWeights(plan), GetMetrics(patient, plan, x));
        }

        private double[] GetMetrics(Patient patient, PlanSetup plan, double x = 5.0)
        {
            return CalculateForPlanPerBeam(patient, plan, x);
        }

        private double[] CalculateForPlanPerBeam(Patient patient, PlanSetup plan, double x = 5.0)
        {
            return (from beam in plan.Beams
                    where !beam.IsSetupField
                    select CalculateForBeam(patient, plan, beam, x)).ToArray();
        }

        public virtual double CalculateForBeam(Patient patient, PlanSetup plan, Beam beam, double x = 5.0)
        {
            return WeightedSum(GetWeights(beam), GetMetrics(patient, plan, beam, x));
        }

        protected virtual double[] GetMetrics(Patient patient, PlanSetup plan, Beam beam, double x)
        {
            return CalculateForBeamPerAperture(patient, plan, beam, x);
        }

        private double[] CalculateForBeamPerAperture(Patient patient, PlanSetup plan, Beam beam, double x = 5.0)
        {
            return CalculatePerAperture(CreateApertures(patient, plan, beam), x);
        }

        private double[] CalculatePerAperture(IEnumerable<Aperture> apertures, double x)
        {
            return (from aperture in apertures
                    select SmallApertureScoreCalculate(aperture, x)).ToArray();
        }

        private double SmallApertureScoreCalculate(Aperture aperture, double x)
        {
            int lpJ = 0;    // inside field LeafPairs number
            int lpX = 0;    // field size < X leafPairs number

            foreach (LeafPair lp in aperture.LeafPairs)
            {
                if (!lp.IsOutsideJaw())
                {
                    lpJ += 1;
                    if (lp.FieldSize() < x)
                        lpX += 1;
                }
            }

            return Utilities.DivisionOrDefault(lpX, lpJ);
        }

        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            throw new NotImplementedException();
        }
    }
}
