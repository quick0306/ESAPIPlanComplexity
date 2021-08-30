using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace Complexity.ApertureMetric
{
    public class MLCGantryDoseStatistics
    {
        private IEnumerable<Aperture> _apertures;
        private double[] _metersets;
        private Beam _beam;

        public int Ncp { get; }
        public double[] DeltaMU { get; }
        public double[] DeltaTime { get; }
        public double[,] MLCPosition { get; }
        public double[,] MLCSpeed { get; }
        public double[] MLCSpeedStd { get; }
        public double[,] MLCAccelerate { get; }
        public double[] MLCAccelerateStd { get; }
        public double[] GantryAngle { get; }
        public double[] DeltaGantryAngle { get; }
        public double[] GantrySpeed { get; }
        public double[] DeltaGantrySpeed { get; }
        public double[] GantryAccelerate { get; }
        public double[] DoseRate { get; }
        public double[] DeltaDoseRate { get; }


        public MLCGantryDoseStatistics(Beam beam, IEnumerable<Aperture> apertures, double[] metersets)
        {
            Ncp = apertures.Count();
            _apertures = apertures;
            _metersets = metersets;
            _beam = beam;

            DeltaMU = Algebra.Diff(_metersets);
            DeltaTime = GetControlPointTime();

            MLCPosition = GetMLCPositions();
            MLCSpeed = GetMLCSpeed();
            MLCSpeedStd = GetMLCSpeedStd();
            MLCAccelerate = GetMLCAccelerate();
            MLCAccelerateStd = GetMLCAccelerateStd();

            GantryAngle = (from aperture in apertures
                            select aperture.GantryAngle).ToArray();
            DeltaGantryAngle = GetDeltaGantry();
            GantrySpeed = GetGantrySpeed();
            DeltaGantrySpeed = Algebra.Diff(GantrySpeed);
            GantryAccelerate = GetGantryAccelerate();

            DoseRate = GetDoseRate();
            DeltaDoseRate = Algebra.Diff(DoseRate);
        }

        // Calculation time between control points in seconds
        // 静态DMLC控制点时间怎么确定？？？
        private double[] GetControlPointTime()
        {
            double[] deltaTimes = new double[DeltaMU.Length];
            deltaTimes[0] = double.NaN; // 0 position NaN, like Python Pandas

            double gantryRotationAngle = GetBeamGantryRotationAngle();
            
            for (int i = 1; i < DeltaMU.Length; i++)
            {
                if (_beam.TreatmentUnit.ToString() == "TRILOGY-SN5602")
                {
                    double delta = ((gantryRotationAngle / Ncp) / 4.8) * (_beam.DoseRate / 60.0);
                    if (DeltaMU[i] <= delta)
                    {
                        deltaTimes[i] = (gantryRotationAngle / Ncp) / 4.8;
                    }
                    else
                    {
                        deltaTimes[i] = DeltaMU[i] / (_beam.DoseRate / 60.0);
                    }
                }
                else
                {
                    double delta = ((gantryRotationAngle / Ncp) / 6.0) * (_beam.DoseRate / 60.0);
                    if (DeltaMU[i] <= delta)
                    {
                        deltaTimes[i] = (gantryRotationAngle / Ncp) / 6.0;
                    }
                    else
                    {
                        deltaTimes[i] = DeltaMU[i] / (_beam.DoseRate / 60.0);
                    }
                }

                // Console.WriteLine($"deltaTimes {i} = {deltaTimes[i]}");
            }

            return deltaTimes;
        }

        // Calculation the Beam gantry rotation angle
        // Need More Test???
        private double GetBeamGantryRotationAngle()
        {
            double gantryRotationAngle = 0.0;
            if (_beam.GantryDirection.ToString() == "Clockwise")
            {
                gantryRotationAngle = ((_beam.ControlPoints.AsQueryable().Last().GantryAngle -
                    _beam.ControlPoints[0].GantryAngle) + 360.0) % 360.0;
            }
            else if (_beam.GantryDirection.ToString() == "CounterClockwise")
            {
                gantryRotationAngle = (360.0 - (_beam.ControlPoints.AsQueryable().Last().GantryAngle -
                    _beam.ControlPoints[0].GantryAngle)) % 360.0;
            }

            return gantryRotationAngle;
        }

        #region MLC property, include position, speed, accelerate
        // MLC位置矩阵，行(row)：aperture; 列(column)：Leaf 
        private double[,] GetMLCPositions()
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

        private double[,] GetMLCSpeed()
        {
            return Utilities.Get2DArrayColumnDivision(MLCPosition, DeltaTime);
        }

        private double[] GetMLCSpeedStd()
        {
            return Utilities.Get2DArrayColumnStd(MLCSpeed);
        }

        private double[,] GetMLCAccelerate()
        {
            return Utilities.Get2DArrayColumnDivision(MLCSpeed, DeltaTime);
        }

        private double[] GetMLCAccelerateStd()
        {
            return Utilities.Get2DArrayColumnStd(MLCAccelerate);
        }
        #endregion // MLC property, include position, speed, accelerate

        #region Gantry property, include angle, speed, accelerate
        private double[] GetDeltaGantry()
        {
            double[] deltaGantryAngles = new double[GantryAngle.Length];
            deltaGantryAngles[0] = double.NaN;
            for (int i = 1; i < GantryAngle.Length; i++)
            {
                double phi = Math.Abs(GantryAngle[i] - GantryAngle[i - 1]) % 360;
                deltaGantryAngles[i] = phi > 180 ? 360 - phi : phi;
            }
            return deltaGantryAngles;
        }

        private double[] GetGantrySpeed()
        {
            return (from i in Algebra.Sequence(DeltaTime.Length)
                    select (DeltaGantryAngle[i] / DeltaTime[i])).ToArray();
        }

        private double[] GetGantryAccelerate()
        {
            return (from i in Algebra.Sequence(DeltaTime.Length)
                    select (DeltaGantrySpeed[i] / DeltaTime[i])).ToArray();
        }
        #endregion // Gantry property, include angle, speed, accelerate

        private double[] GetDoseRate()
        {
            return (from i in Algebra.Sequence(DeltaTime.Length)
                    select (DeltaMU[i] / DeltaTime[i])).ToArray();
        }

        #region MLC all leaf speed and accelerate statistics result
        public double GetAllMLCSpeedAverage()
        {
            return Math.Round(Utilities.Get2DArrayAverage(MLCSpeed), 2);
        }

        public double GetAllMLCSpeedStdAverage()
        {
            return Math.Round(Utilities.Get1DArrayAverage(MLCSpeedStd), 2);
        }

        public double GetAllMLCAccelerateAverage()
        {
            return Math.Round(Utilities.Get2DArrayAverage(MLCAccelerate), 2);
        }

        public double GetAllMLCAccelerateStdAverage()
        {
            return Math.Round(Utilities.Get1DArrayAverage(MLCAccelerateStd), 2);
        }
        #endregion // MLC all leaf speed and accelerate statistics result
    }
}
