using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complexity.ComplexityMetric
{
    // Converted Aperture Metric，该方法有部分未实现：垂直于叶片运动方向Distance？？？
    // Ref: GÖTSTEDT, et al. Development and evaluation of aperture‐based complexity metrics using film and EPID 
    // measurements of static MLC openings.Medical physics, 2015, 42.7: 3911-3921. DOI: https://doi.org/10.1118/1.4921733
    public class ConvertedApertureMetric : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures select CalculateConvertedApertureMetric(aperture)).ToArray();
        }

        private double CalculateConvertedApertureMetric(Aperture aperture)
        {
            double[] lpDistanceCam = (from lp in aperture.LeafPairs 
                                      where !lp.IsOutsideJaw() 
                                      select (1 - Math.Exp(-(lp.FieldSize() / 10)))).ToArray();
            double areaCam = 1 - Math.Exp(-(Math.Sqrt(aperture.Area()) / 10));

            double lpDistanceCamAverage = lpDistanceCam.Count() > 0 ? lpDistanceCam.Average() : 0;
            return (1 - lpDistanceCamAverage * areaCam);
        }
    }
}
