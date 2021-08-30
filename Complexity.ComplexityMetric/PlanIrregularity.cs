using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complexity.ApertureMetric;

namespace Complexity.ComplexityMetric
{
    // 计划不规则性指标
    // Du W, et al. Quantification of beam complexity in intensity-modulated radiation therapy treatment plans.
    // Med Phys 2014;41:21716. DOI: http://dx.doi.org/10.1118/1.4861821.
    public class PlanIrregularity: ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures 
                    select CalculateApertureIrregularity(aperture)).ToArray();
        }

        private double CalculateApertureIrregularity(Aperture aperture)
        {
            double area = aperture.Area();
            double perimeter = aperture.SidePerimeterHorizontal() + aperture.SidePerimeterVertical();
            return ApertureMetric.Utilities.DivisionOrDefault(Math.Pow(perimeter, 2), 4 * Math.PI * area);
        }
    }
}
