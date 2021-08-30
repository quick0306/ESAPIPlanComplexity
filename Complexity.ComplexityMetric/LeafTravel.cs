using Complexity.ApertureMetric;
using System;
using System.Linq;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace Complexity.ComplexityMetric
{
    // 叶片运动范围平均值
    // Ref: MASI, Laura, et al.Impact of plan parameters on the dosimetric accuracy of volumetric modulated arc therapy. 
    // Medical physics, 2013, 40.7: 071718. DOI: https://doi.org/10.1118/1.4810969
    public class LeafTravel : ComplexityMetric
    {
        public override double CalculateForBeam(Patient patient, PlanSetup plan, Beam beam)
        {
            IEnumerable<Aperture> apertures = CreateApertures(patient, plan, beam);
            double[] leafTravel = CalculatePerAperture(apertures);

            return leafTravel.Average();
        }

        protected override double[] CalculatePerAperture(IEnumerable<Aperture> apertures)
        {
            Dictionary<int, List<double>> leftLeafTravelTrack = new Dictionary<int, List<double>>();
            Dictionary<int, List<double>> rightLeafTravelTrack = new Dictionary<int, List<double>>();

            foreach (Aperture aperture in apertures)
            {
                LeafPair[] lps = aperture.LeafPairs;
                for (int i = 0; i < lps.Length; i++)
                {
                    if (!lps[i].IsOutsideJaw())
                    {
                        if (leftLeafTravelTrack.ContainsKey(i))
                        {
                            leftLeafTravelTrack[i].Add(lps[i].Left);
                        }
                        else
                        {
                            leftLeafTravelTrack.Add(i, new List<double>() { lps[i].Left });
                        }

                        if (rightLeafTravelTrack.ContainsKey(i))
                        {
                            rightLeafTravelTrack[i].Add(lps[i].Right);
                        }
                        else
                        {
                            rightLeafTravelTrack.Add(i, new List<double>() { lps[i].Right });
                        }
                    }
                }
            }

            List<double> leafTravel = new List<double>();
            foreach (KeyValuePair<int, List<double>> tmp in leftLeafTravelTrack)
            {
                double[] travelTrack = tmp.Value.ToArray();
                double trackDistance = Algebra.DiffSum(travelTrack);
                leafTravel.Add(trackDistance);
            }
            foreach (KeyValuePair<int, List<double>> tmp in rightLeafTravelTrack)
            {
                double[] travelTrack = tmp.Value.ToArray();
                double trackDistance = Algebra.DiffSum(travelTrack);
                leafTravel.Add(trackDistance);
            }

            return leafTravel.ToArray();
        }
    }
}
