using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complexity.ApertureMetric;

namespace Complexity.ComplexityMetric
{
    // Modulation Complexity Score, MCS
    // Ref: McNiven AL, et al. A new metric for assessing IMRT modulation complexity 
    // and plan deliverability.Med Phys 2010;37:505–15. DOI: http://dx.doi.org/10.1118/1.3276775.
    public class ModulationComplexityScore : ComplexityMetric
    {
        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            Dictionary<double, double[]> posMax = new Dictionary<double, double[]>();

            foreach (Aperture aperture in apertures)
            {
                if (aperture.isOpen())
                {
                    int nPairs = aperture.LeafPairs.Length;
                    for (int i = 0; i < nPairs; i++)
                    {
                        LeafPair lp = aperture.LeafPairs[i];
                        if (!lp.IsOutsideJaw())
                        {
                            if (posMax.ContainsKey(i))
                            {
                                posMax[i][0] = lp.Left < posMax[i][0] ? lp.Left : posMax[i][0];
                                posMax[i][1] = lp.Right > posMax[i][1] ? lp.Right : posMax[i][1];
                            }
                            else
                            {
                                posMax.Add(i, new double[] { lp.Left, lp.Right });
                            }
                        }
                    }
                }               
            }

            double aavNorm = 0.0;
            foreach (KeyValuePair<double, double[]> lpNum in posMax)
            {
                aavNorm += Math.Abs(lpNum.Value[1] - lpNum.Value[0]);
            }

            return (from aperture in apertures where aperture.isOpen()
                    select GetApertureMCS(aperture, aavNorm)).ToArray();
        }

        private double GetApertureMCS(Aperture aperture, double aavNorm)
        {
            List<double> lpLefts = new List<double>();
            List<double> lpRigths = new List<double>();
            double areaSum = 0.0;

            foreach (LeafPair lp in aperture.LeafPairs)
            {
                if (!lp.IsOutsideJaw())
                {
                    lpLefts.Add(lp.Left);
                    lpRigths.Add(lp.Right);
                    areaSum += lp.FieldSize();
                }
            }

            double posMaxLeft = lpLefts.Max() - lpLefts.Min();
            double posMaxRight = lpRigths.Max() - lpRigths.Min();

            int N = lpRigths.Count();
            double sumLeft = 0.0;
            double sumRight = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                sumLeft += posMaxLeft + lpLefts[i + 1] - lpLefts[i];
                sumRight += posMaxRight + lpRigths[i + 1] - lpRigths[i];
            }

            double lsv = (sumLeft / (N * posMaxLeft)) * (sumRight / (N * posMaxRight));
            double aav = Utilities.DivisionOrDefault(areaSum, aavNorm);
            double mcs = lsv * aav;

            return mcs > 0 ? mcs : 0;
        }
    }
}
