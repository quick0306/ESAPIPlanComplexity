using System.Collections.Generic;
using System.Linq;
using Complexity.ApertureMetric;

namespace Complexity.ComplexityMetric
{
    // Subclass of ComplexityMetric that represents the edge metric
    // 计算边缘指标（Edge Metric, EM）参考Ref 1；也称为CA，即circumference / area，参考Ref 2
    // Ref 1: Younge KC, et al. Penalization of aperture complexity in inversely planned volumetric modulated 
    // Arc therapy: Penalization of aperture complexity in inversely planned VMAT.Med Phys 2012;39: 7160–70.
    // DOI: https://doi.org/10.1118/1.4762566
    // Ref 2: GÖTSTEDT, Julia, et al. Development and evaluation of aperture‐based complexity metrics using film
    // and EPID measurements of static MLC openings.Medical physics, 2015, 42.7: 3911-3921.
    // DOI: https://doi.org/10.1118/1.4921733
    public class EdgeMetric : ComplexityMetric
    {
        // Returns the unweighted edge metrics of a list of apertures
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures
                    select CalculateApertureEdgeMetric(aperture)).ToArray();
        }

        private double CalculateApertureEdgeMetric(Aperture aperture)
        {
            // C1 and C2 is scaling factor, represent vertical and horizonal direction, default C1=0.0 and C2=1.0
            double C1 = 0.0; double C2 = 1.0;
            double perimeter = C1 * aperture.SidePerimeterVertical() + C2 * aperture.SidePerimeterHorizontal();

            return ApertureMetric.Utilities.DivisionOrDefault(perimeter, aperture.Area());
        }

    }
}