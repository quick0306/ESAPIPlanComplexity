using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Complexity.ComplexityMetric;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Complexity.Script
{
    class IMRT
    {
        public static void Execute(Patient patient, PlanSetup ps, StreamWriter patient_file, String[] patient_infos)
        {
            EdgeMetric edgeMetricObj = new EdgeMetric();
            double planEdgeMetric = edgeMetricObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"EDGE Metric = {planEdgeMetric}");

            LeafArea planLeafAreaObj = new LeafArea();
            double planLeafArea = planLeafAreaObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Leaf Gap = {planLeafArea}");

            PlanIrregularity planIrregularityObj = new PlanIrregularity();
            double planIrregularity = planIrregularityObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Plan Irregularity = {planIrregularity}");

            PlanModulation planModulationObj = new PlanModulation();
            double planModulation = planModulationObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Plan Modulation = {planModulation}");

            ModulationComplexityScore planMCSObj = new ModulationComplexityScore();
            double planMCS = planMCSObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Modulation Complexity Score = {planMCS}");

            SmallApertureScore planSmallApertureScoreObj = new SmallApertureScore();
            double planSmallApertureScore5mm = planSmallApertureScoreObj.CalculateForPlan(patient, ps, 5.0);
            double planSmallApertureScore10mm = planSmallApertureScoreObj.CalculateForPlan(patient, ps, 10.0);
            double planSmallApertureScore20mm = planSmallApertureScoreObj.CalculateForPlan(patient, ps, 20.0);
            Console.WriteLine(
                $"Small Aperture Score 5mm = {planSmallApertureScore5mm}\n" +
                $"Small Aperture Score 10mm = {planSmallApertureScore10mm}\n" +
                $"Small Aperture Score 20mm = {planSmallApertureScore20mm}");

            MeanFieldArea planMFDObj = new MeanFieldArea();
            double planMFD = planMFDObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Mean Field Area = {planMFD}");

            MeanAsymmetryDistance planMSDObj = new MeanAsymmetryDistance();
            double planMSD = planMSDObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Mean Asymmetry Distance = {planMSD}");

            ApertureAreaRationJawArea planApertureAreaRatioJawAreaObj = new ApertureAreaRationJawArea();
            double planApertureAreaRatioJawArea = planApertureAreaRatioJawAreaObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Aperture Area Ration Jaw Area = {planApertureAreaRatioJawArea}");

            ApertureSubRegions planApertureSubRegionsObj = new ApertureSubRegions();
            double planApertureSubRegions = planApertureSubRegionsObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Aperture Sub Regions = {planApertureSubRegions}");

            ApertureXJawDistance planApertureXJawDistanceObj = new ApertureXJawDistance();
            double planApertureXJawDistance = planApertureXJawDistanceObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Aperture X Jaw Distance = {planApertureXJawDistance}");

            ApertureYJawDistance planApertureYJawDistanceObj = new ApertureYJawDistance();
            double planApertureYJawDistance = planApertureYJawDistanceObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Aperture Y Jaw Distance = {planApertureYJawDistance}");

            LeafGap planLeafGapObj = new LeafGap();
            Dictionary<string, double> planLeafGap = planLeafGapObj.CalculateForPlanDictionary(patient, ps);
            foreach (KeyValuePair<string, double> tmp in planLeafGap)
            {
                Console.WriteLine($"Leaf Gap Proportion for {tmp.Key} = {tmp.Value}");
            }

            LeafTravel planLeafTravelObj = new LeafTravel();
            double planLeafTravel = planLeafTravelObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Leaf Travel = {planLeafTravel}");

            ConvertedApertureMetric camObj = new ConvertedApertureMetric();
            double planCam = camObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Converted Aperture Metric = {planCam}");

            EdgeAreaMetric eamObj = new EdgeAreaMetric();
            double planEam = eamObj.CalculateForPlan(patient, ps);
            Console.WriteLine($"Edge Area Metric = {planEam}");

            String patient_line = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, " +
            "{19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}",
            patient_infos[0], patient_infos[1], patient_infos[2], patient_infos[3], patient_infos[4], ps.Id, PlanUtilities.getTreatmentUnit(ps), ps.PhotonCalculationModel,
            PlanUtilities.getPrescribedDosePerFraction(ps), PlanUtilities.getTotalMU(ps), planEdgeMetric, planLeafArea, planIrregularity, planModulation, planMCS,
            planSmallApertureScore5mm, planSmallApertureScore10mm, planSmallApertureScore20mm,
            planMFD, planMSD, planApertureAreaRatioJawArea, planApertureSubRegions, planApertureXJawDistance, planApertureYJawDistance, planLeafGap["Average"],
            planLeafGap["Std"], planLeafTravel, planCam, planEam);

            patient_file.WriteLine(patient_line);
        }
    }
}
