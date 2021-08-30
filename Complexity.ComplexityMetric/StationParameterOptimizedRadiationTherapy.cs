using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    // VMAT站参数优化
    // Ref: LI, Ruijiang; XING, Lei.An adaptive planning strategy for station parameter optimized radiation therapy (SPORT) :
    // Segmentally boosted VMAT.Medical physics, 2013, 40.5: 050701. DOI: https://doi.org/10.1118/1.4802748
    public class StationParameterOptimizedRadiationTherapy : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            throw new NotImplementedException();
        }

        public override double CalculateForBeam(Patient patient, PlanSetup plan, Beam beam)
        {
            IEnumerable<Aperture> apertures = CreateApertures(patient, plan, beam);
            double[] weights = GetWeights(beam);
            double[] metersets = new MetersetsFromMetersetWeightsCreator().GetCumulativeMetersets(beam);
            double[] metrics = CalculatePerAperture(apertures, metersets);

            return WeightedSum(weights, metrics);
        }

        protected double[] CalculatePerAperture(IEnumerable<Aperture> apertures, double[] metersets)
        {
            int k = 10;
            IEnumerable<int> enumerableK = Enumerable.Range(-k, 2 * k + 1).Where(x => x != 0);
            double[,] mi = new double[apertures.Count(), enumerableK.Count()];

            for (int i = 0; i < apertures.Count(); i++)
            {
                for (int j = 0; j < enumerableK.Count(); j++)
                {
                    if ((i + enumerableK.ElementAt(j) >= 0) && (i + enumerableK.ElementAt(j) < apertures.Count()))
                    {
                        double factor = Math.Abs((metersets[i] - metersets[i + enumerableK.ElementAt(j)]) / 
                            (apertures.ElementAt(i).GantryAngle - apertures.ElementAt(i + enumerableK.ElementAt(j)).GantryAngle));

                        double mlcSum = 0;
                        LeafPair[] lps = apertures.ElementAt(i).LeafPairs;
                        LeafPair[] lpsK = apertures.ElementAt(i + enumerableK.ElementAt(j)).LeafPairs;
                        for (int m = 0; m < lps.Length; m++)
                        {
                            mlcSum += Math.Abs(lps[m].Left - lpsK[m].Left) + Math.Abs(lps[m].Right - lpsK[m].Right);
                        }

                        mi[i, j] = mlcSum * factor;
                    }
                    else
                    {
                        mi[i, j] = 0;
                    }
                }
            }

            return (from i in Algebra.Sequence(mi.GetLength(0)) 
                    select MatrixArray<double>.GetRow(mi, i).Sum()).ToArray();
        }
    }
}
