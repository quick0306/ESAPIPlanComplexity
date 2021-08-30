using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complexity.ComplexityMetric
{
    // Maximum number of regions in the aperture
    // Ref: LAM, et al.Predicting gamma passing rates for portal dosimetry‐based IMRT QA using machine learning.
    // Medical physics, 2019, 46.10: 4666-4675. DOI: https://doi.org/10.1002/mp.13752.
    public class ApertureSubRegions : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures select aperture.ApertureSubRegions()).ToArray();
        }
    }
}
