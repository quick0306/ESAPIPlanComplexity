using System;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using Patient = VMS.TPS.Common.Model.API.Patient;
using PlanSetup = VMS.TPS.Common.Model.API.PlanSetup;

namespace Complexity.ApertureMetric
{
    public class AperturesFromBeamCreator
    {
        public IEnumerable<Aperture> Create(Patient patient, PlanSetup plan, Beam beam)
        {
            List<Aperture> apertures = new List<Aperture>();

            double[] leafWidths = getLeafWidth(beam);

            foreach (ControlPoint controlPoint in beam.ControlPoints)
            {
                double gantryAngle = controlPoint.GantryAngle;
                double[,] leafPositions = GetLeafPositions(controlPoint);
                double[] jaw = CreateJaw(controlPoint);
                apertures.Add(new Aperture(leafPositions, leafWidths, jaw, gantryAngle));
            }

            return apertures;
        }

        private double[] CreateJaw(ControlPoint cp)
        {
            double left = cp.JawPositions.X1;
            double top = cp.JawPositions.Y2;
            double right = cp.JawPositions.X2;
            double bottom = cp.JawPositions.Y1;

            return new double[] { left, top, right, bottom };
        }

        public double[,] GetLeafPositions(ControlPoint controlPoint)
        {
            int m = controlPoint.LeafPositions.GetLength(0);
            int n = controlPoint.LeafPositions.GetLength(1);
            
            double[,] leafPositions = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // Leaf positions are given from bottom to top by ESAPI,
                    // but the Aperture class expects them from top to bottom
                    leafPositions[i, j] = controlPoint.LeafPositions[i, n - j - 1];
                }
            }
            return leafPositions;
        }

        //Return leaf width, HDMLC (EDGE="TrueBeamSN2716"), Millennium MLC (TrueBeam="TrueBeamSN1352" and Trilogy="TRILOGY-SN5602") 
        private double[] getLeafWidth(Beam beam)
        {
            if (beam.TreatmentUnit.ToString() == "TrueBeamSN2716")
            {
                return new double[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
                2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5,
                2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5, 2.5,
                5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
            } 
            else
            {
                return new double[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
                5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
                10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };
            }
        }
    }
}
