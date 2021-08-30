using System.Collections.Generic;
using System.Linq;
using Complexity.ApertureMetric;

namespace Complexity.ComplexityMetric
{
    // 计算平均叶片面积，叶片间距 * 叶片宽度，Ref计算Leaf gap，考虑到相同叶片间距，不同叶片宽度对剂量影响不同，
    // 认为叶片面积指标更为合理
    // Ref: Nauta M, et al. Fractal analysis for assessing the level of modulation of IMRT fields. Med Phys 2011;38:5385–93. 
    // doi: https://doi.org/10.1118/1.3633912
    public class LeafArea: ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures 
                    select CalculateApertureLeafGapArea(aperture)).ToArray();

        }
        private double CalculateApertureLeafGapArea(Aperture aperture)
        {
            //Some leaf pairs open behind Jaws, see example 1901088c B2VMATa, in this situation aperature area should be 0 ???
            return aperture.LeafPairArea().Count() > 0 ? aperture.LeafPairArea().Average() : 0;
        }
    }
}
