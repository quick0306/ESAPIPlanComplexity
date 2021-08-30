using Complexity.ApertureMetric;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Complexity.ComplexityMetric
{
    // Ratio of the average area of an aperture over the area defined by jaws
    // Ref: LAM, et al.Predicting gamma passing rates for portal dosimetry‐based IMRT QA using machine learning.
    // Medical physics, 2019, 46.10: 4666-4675. DOI: https://doi.org/10.1002/mp.13752.
    public class ApertureAreaRationJawArea : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            return (from aperture in apertures select CalculateApertureAreaRatioJawArea(aperture)).ToArray();
        }

        private double CalculateApertureAreaRatioJawArea(Aperture aperture)
        {
            double apertureArea = aperture.Area();
            Jaw jaws = aperture.Jaw;
            double jawArea = Math.Abs(jaws.Right - jaws.Left) * Math.Abs(jaws.Top - jaws.Bottom);

            return Utilities.DivisionOrDefault(apertureArea, jawArea);
        }
    }
}
