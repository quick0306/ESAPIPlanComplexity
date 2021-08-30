using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace Complexity.ApertureMetric
{
    public class MLC
    {
        private int _Ncp = 0;
        private IEnumerable<Aperture> _apertures;
        private double[] _metersets;
        private Beam _beam;

        public MLC(Beam beam, IEnumerable<Aperture> apertures, double[] metersets) 
        {
            _Ncp = apertures.Count();
            _apertures = apertures;
            _metersets = metersets;
            _beam = beam;
        }

        public Dictionary<string, double[]> GetDeltaMUTime()
        {
            Dictionary<string, double[]> deltaMuTime = new Dictionary<string, double[]>();

            deltaMuTime.Add("cumulative_mu", _metersets);
            double[] deltaMUs = Algebra.Diff(_metersets);
            deltaMuTime.Add("delta_mu", deltaMUs);
            double[] deltaTimes = GetControlPointTime(deltaMUs);
            deltaMuTime.Add("delta_time", deltaTimes);

            return deltaMuTime;
        }

        // Calculation time between control points in seconds
        private double[] GetControlPointTime(double[] deltaMUs)
        {
            double[] deltaTimes = new double[deltaMUs.Length];
            deltaTimes[0] = double.NaN; // 0 position NaN, like Python Pandas

            double gantryRotationAngle = GetBeamGantryRotationAngle();
            
            for (int i = 1; i < deltaMUs.Length; i++)
            {
                if (_beam.TreatmentUnit.ToString() == "TRILOGY-SN5602")
                {
                    double delta = ((gantryRotationAngle / _Ncp) / 4.8) * (_beam.DoseRate / 60.0);
                    if (deltaMUs[i] <= delta)
                    {
                        deltaTimes[i] = (gantryRotationAngle / _Ncp) / 4.8;
                    }
                    else
                    {
                        deltaTimes[i] = deltaMUs[i] / (_beam.DoseRate / 60.0);
                    }
                }
                else
                {
                    double delta = ((gantryRotationAngle / _Ncp) / 6.0) * (_beam.DoseRate / 60.0);
                    if (deltaMUs[i] <= delta)
                    {
                        deltaTimes[i] = (gantryRotationAngle / _Ncp) / 6.0;
                    }
                    else
                    {
                        deltaTimes[i] = deltaMUs[i] / (_beam.DoseRate / 60.0);
                    }
                }
            }

            return deltaTimes;
        }

        // Calculation the Beam gantry rotation angle
        // Need More Test???
        private double GetBeamGantryRotationAngle()
        {
            double gantryRotationAngle = 0.0;
            if (_beam.GantryDirection.ToString() == "CW")
            {
                gantryRotationAngle = ((_beam.ControlPoints.AsQueryable().Last().GantryAngle - 
                    _beam.ControlPoints[0].GantryAngle) + 360.0) % 360.0;
            }
            else if (_beam.GantryDirection.ToString() == "CC")
            {
                gantryRotationAngle = (360.0 - (_beam.ControlPoints.AsQueryable().Last().GantryAngle -
                    _beam.ControlPoints[0].GantryAngle)) % 360.0;
            }
            return gantryRotationAngle;
        }

        public double[,] GetMLCPositions()
        {
            int m = _apertures.Count();
            int n = _apertures.FirstOrDefault().LeafPairs.Length;

            double[,] pos = new double[m, 2 * n];
            for (int i = 0; i < m; i++)
            {
                Aperture aperture = _apertures.ElementAt(i);

                LeafPair[] lps = aperture.LeafPairs;

                for (int j = 0; j < lps.Length; j++)
                {
                    pos[i, j] = lps[j].Left;
                    pos[i, j + n] = lps[j].Right;
                }
            }

            return pos;
        }

        public double[,] GetMLCSpeed(double[,] pos, double[] deltaTime)
        {
            int columnNumber = pos.GetLength(0);
            int rowNumber = pos.GetLength(1);
            double[,] mlcSpeeds = new double[columnNumber, rowNumber];

            for (int i = 0; i < rowNumber; i++)
            {
                double[] leafPairPos = ApertureMetric.MatrixArray<double>.GetColumn(pos, i);
                double[] leafPairPosDiff = ApertureMetric.Algebra.Diff(leafPairPos);
                for (int j = 0; j < columnNumber; j++)
                {
                    mlcSpeeds[i, j] = leafPairPosDiff[j] / deltaTime[j];
                }
            }

            return mlcSpeeds;
        }

        public double[] GetMLCSpeedStd(double[,] mlcSpeeds)
        {
            int columnNumber = mlcSpeeds.GetLength(0);
            int rowNumber = mlcSpeeds.GetLength(1);

            double[] mlcSpeedStds = new double[rowNumber];

            for (int i = 0; i < rowNumber; i++)
            {
                double[] leafPairMLCSpeed = ApertureMetric.MatrixArray<double>.GetColumn(mlcSpeeds, i);
                double mlcSpeedStd = ApertureMetric.Algebra.Std(leafPairMLCSpeed);
                mlcSpeedStds[i] = mlcSpeedStd;
            }

            return mlcSpeedStds;
        }
    }
}
