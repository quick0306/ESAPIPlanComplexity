using System;
using System.Linq;

namespace Complexity.ApertureMetric
{
    public class Aperture
    {
        public LeafPair[] LeafPairs { get; set; }
        public Jaw Jaw { get; set; }
        public double GantryAngle { get; set; }

        // The first dimension of leafPositions corresponds to the bank,
        // and the second dimension corresponds to the leaf pair.
        // Leaf coordinates follow the IEC 61217 standard:
        //
        //                   Negative Y         x = isocenter (0, 0)
        //                       -
        //                       |
        //                       |
        //                       |
        // Negative X |----------x----------| Positive X
        //                       |
        //                       |
        //                       |
        //                       -
        //                   Positive Y

        // leafPositions and leafWidths must not be null,
        // and they must have the same number of leaves

        // jaw is the position of the jaw (cannot be null), given as
        // left, top, right, bottom; for a completely open jaw, use:
        //     new double[] { double.MinValue, double.MinValue,
        //                    double.MaxValue, double.MaxValue };

        public Aperture(double[,] leafPositions, double[] leafWidths, double[] jaw, double gantryAngle)
        {
            Jaw = CreateJaw(jaw);
            LeafPairs = CreateLeafPairs(leafPositions, leafWidths, Jaw);
            GantryAngle = gantryAngle;
        }

        //如果VMAT计划中有静态野，则会报错
        private LeafPair[] CreateLeafPairs(double[,] positions, double[] widths, Jaw jaw)
        {
            double[] leafTops = GetLeafTops(widths);
            return (from i in Algebra.Sequence(widths.Length)
                    select new LeafPair(positions[0, i], positions[1, i],
                                        widths[i], leafTops[i], jaw)).ToArray();
        }

        // Using the leaf widths, creates an array of the location
        // of all the leaf tops (relative to the isocenter)

        private double[] GetLeafTops(double[] widths)
        {
            double[] leafTops = new double[widths.Length];

            // Leaf index right below isocenter
            int middleIndex = widths.Length / 2;
            leafTops[middleIndex] = 0.0;

            // Do bottom half
            for (int i = middleIndex + 1; i < widths.Length; i++)
            {
                leafTops[i] = leafTops[i - 1] - widths[i - 1];
            }

            // Do top half
            for (int i = middleIndex - 1; i >= 0; i--)
            {
                leafTops[i] = leafTops[i + 1] + widths[i];
            }
          
            return leafTops;
        }

        private Jaw CreateJaw(double[] pos)
        {
            return new Jaw
            {
                Position = new Rect(pos[0], pos[1], pos[2], pos[3])
            };
        }

        public bool HasAnyOpenLeafOutsideJaws()
        {
            return LeafPairs.Any(leafPair => leafPair.IsOpenButBehindJaw());
        }  

        public bool isOpen()
        {
            return LeafPairs.Any(leafPair => leafPair.IsOpen());
        }

        public double Area()
        {
            return LeafPairs.Sum(s => s.FieldArea());
        }

        public double[] LeafPairArea()
        {
            return (from leafPair in LeafPairs where leafPair.FieldArea() > 0 select leafPair.FieldArea()).ToArray();
        }

        public double SidePerimeterHorizontal()
        {
            if (LeafPairs.Length == 0)
                return 0.0;

            // Top end of first leaf pair
            double perimeter = LeafPairs[0].FieldSize();

            for (int i = 1; i < LeafPairs.Length; i++)
                perimeter += SidePerimeter(LeafPairs[i - 1], LeafPairs[i]);

            // Bottom end of last leaf pair
            perimeter += LeafPairs[LeafPairs.Length - 1].FieldSize();

            return perimeter;
        }

        public double SidePerimeterVertical()
        {
            if (LeafPairs.Length == 0)
                return 0.0;

            double perimeter = 0.0;
            for (int i = 0; i < LeafPairs.Length; i++)
            {
                if (!LeafPairs[i].IsOutsideJaw())
                    perimeter += LeafPairs[i].OpenLeafWidth();

            }

            return perimeter;
        }

        private double SidePerimeter(LeafPair topLeafPair, LeafPair bottomLeafPair)
        {
            if (LeafPairsAreOutsideJaw(topLeafPair, bottomLeafPair))
            {
                return 0.0;
            }

            // _____         ________
            //      |       |
            // _____|___    |________
            //  +-------|------|---+
            // _|_______|      |___|_

            if (JawTopIsBelowTopLeafPair(topLeafPair))
            {
                return bottomLeafPair.FieldSize();
            }

            // _|___         ______|_
            //  +---|-------|------+
            // _____|___    |________
            //          |      |
            // _________|      |_____

            if (JawBottomIsAboveBottomLeafPair(bottomLeafPair))
            {
                return topLeafPair.FieldSize();
            }

            // At this point, the edge between the top and bottom leaf pairs
            // should be fully or partially exposed (depending on the jaw)
            // ___    _______________
            //  +-|--|-------+
            // _|_|__|_______|_______
            //  +-------|----+ |
            // _________|      |_____

            if (LeafPairsAreDisjoint(topLeafPair, bottomLeafPair))
            {
                return topLeafPair.FieldSize() + bottomLeafPair.FieldSize();
            }

            // ___         __________
            //  +-|-------|--+
            // _|_|___    |__|_______
            //  +-----|------+ |
            // _______|        |_____

            double topEdgeLeft     = Math.Max(Jaw.Left,  topLeafPair.Left);
            double bottomEdgeLeft  = Math.Max(Jaw.Left,  bottomLeafPair.Left);
            double topEdgeRight    = Math.Min(Jaw.Right, topLeafPair.Right);
            double bottomEdgeRight = Math.Min(Jaw.Right, bottomLeafPair.Right);

            return Math.Abs(topEdgeLeft  - bottomEdgeLeft) +
                   Math.Abs(topEdgeRight - bottomEdgeRight);
        }

        private bool JawBottomIsAboveBottomLeafPair(LeafPair bottomLeafPair)
        {
            return Jaw.Bottom >= bottomLeafPair.Top;
        }

        private bool JawTopIsBelowTopLeafPair(LeafPair topLeafPair)
        {
            return Jaw.Top <= topLeafPair.Bottom;
        }

        private bool LeafPairsAreOutsideJaw(LeafPair topLeafPair, LeafPair bottomLeafPair)
        {
            return topLeafPair.IsOutsideJaw() && bottomLeafPair.IsOutsideJaw();
        }

        private static bool LeafPairsAreDisjoint(LeafPair topLeafPair, LeafPair bottomLeafPair)
        {
            return (bottomLeafPair.Left > topLeafPair.Right) ||
                   (bottomLeafPair.Right < topLeafPair.Left);
        }

        // 计算Aperture小子野（sub regions）个数，MLC interplay
        public double ApertureSubRegions()
        {
            double numberRegs = 0;
            bool isNewReg = true;
            
            for (int i = 0; i < LeafPairs.Length; i++)
            {
                if (LeafPairs[i].FieldSize() > 0.5)
                {
                    if (isNewReg)
                    {
                        numberRegs += 1.0;
                        isNewReg = false;
                    }
                    else
                    {
                        if (LeafPairs[i].Left >= LeafPairs[i - 1].Right || LeafPairs[i].Right <= LeafPairs[i - 1].Left)
                            numberRegs += 1.0;
                    }
                }
                else
                {
                    if (!isNewReg)
                        isNewReg = true;
                }
            }

            return numberRegs;
        }
    }
}
