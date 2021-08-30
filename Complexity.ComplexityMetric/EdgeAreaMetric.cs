using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complexity.ComplexityMetric
{
    // Edge Area Metric，该方法R1计算方法有误差？？？
    // Ref: GÖTSTEDT, et al. Development and evaluation of aperture‐based complexity metrics using film and EPID 
    // measurements of static MLC openings.Medical physics, 2015, 42.7: 3911-3921. DOI: https://doi.org/10.1118/1.4921733
    public class EdgeAreaMetric : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures select CalculateEdgeAreaMetric(aperture)).ToArray();
        }

        private double CalculateEdgeAreaMetric(Aperture aperture)
        {
            double perimeter = aperture.SidePerimeterVertical();
            double r1 = perimeter * 10;
            double r2 = aperture.Area() - r1 / 2;

            return (r1 / (r1 + r2));
        }
    }
}
