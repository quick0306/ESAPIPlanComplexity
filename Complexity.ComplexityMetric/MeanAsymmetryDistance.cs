using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complexity.ComplexityMetric
{
    // 叶片对中点与中心轴之间平均距离
    // Ref: Crowe SB, et al.Examination of the properties of IMRT and VMAT beams and 
    // evaluation against pre-treatment quality assurance results.Phys Med Biol 2015; 60(6):2587-2601.
    // DOI: http://doi.org/10.1088/0031-9155/60/6/2587.
    public class MeanAsymmetryDistance : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures select CalculateMeanAsymmetryDistance(aperture)).ToArray();
        }

        private double CalculateMeanAsymmetryDistance(Aperture aperture)
        {
            List<double> asymmetryDistance = new List<double>();

            foreach (LeafPair lp in aperture.LeafPairs)
            {
                if (!lp.IsOutsideJaw())
                {
                    double midX = (lp.Left + lp.Right) / 2;
                    double midY = (lp.Top + lp.Bottom) / 2;

                    asymmetryDistance.Add(Math.Sqrt(midX * midX + midY * midY));
                }
            }

            return asymmetryDistance.Count() > 0 ? asymmetryDistance.Average() : 0;
            //return asymmetryDistance.Average();
        }
    }
}
