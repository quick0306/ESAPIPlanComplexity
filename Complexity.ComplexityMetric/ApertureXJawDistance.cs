using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complexity.ComplexityMetric
{
    // 计算X方向Jaw距离
    // Ref: LI, Jiaqi, et al.Machine Learning for Patient-Specific Quality Assurance of VMAT: Prediction and 
    // Classification Accuracy. International Journal of Radiation Oncology* Biology* Physics, 2019, 105.4: 893-902.
    // DOI: https://doi.org/10.1016/j.ijrobp.2019.07.049
    public class ApertureXJawDistance : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures select (Math.Abs(aperture.Jaw.Right - aperture.Jaw.Left))).ToArray();
        }
    }
}
