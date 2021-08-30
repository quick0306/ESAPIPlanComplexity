using Complexity.ApertureMetric;
using System.Collections.Generic;
using System.Linq;

namespace Complexity.ComplexityMetric
{
    // 平均射野面积，子野面积与MU加权平均，Ref 2中PA，BA
    // Ref 1: Crowe SB, et al.Examination of the properties of IMRT and VMAT beams and 
    // evaluation against pre-treatment quality assurance results.Phys Med Biol 2015; 60(6):2587-2601.
    // DOI: http://doi.org/10.1088/0031-9155/60/6/2587.
    // Ref 2: Du W, et al. Quantification of beam complexity in intensity-modulated radiation therapy 
    // treatment plans.Med Phys 2014;41:21716. DOI: http://dx.doi.org/10.1118/1.4861821.
    public class MeanFieldArea : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures select aperture.Area()).ToArray();
        }
    }
}
