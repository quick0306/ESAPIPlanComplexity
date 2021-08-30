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
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                using (Application app = Application.CreateApplication("hujy", "hjy2818"))
                {
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
        static void Execute(Application app)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            String outputdir = @"d:\temp\PlanComplexity";
            //System.IO.Directory.CreateDirectory(outputdir);

            String vmat_filename = string.Format(@"{0}\patient_vmat_analysis.csv", outputdir);
            StreamWriter vmat_patient_file = new StreamWriter(vmat_filename, true, Encoding.GetEncoding("gb2312"));
            vmat_patient_file.WriteLine("ID, Name, Sites, Doctor, Physicist, PlanID, MachineID, Calculation_Model, Prescribed_Dose, MU, " +
                "EDGE_Metric, Leaf_Area, Plan_Irregularity, Plan_Modulation, Modulation_Complexity_Score, " +
                "Small_Aperture_Score_5mm, Small_Aperture_Score_10mm, Small_Aperture_Score_20mm, Mean_Field_Area, " +
                "Mean_Asymmetry_Distance, Aperture_Area_Ration_Jaw_Area, Aperture_Sub_Regions, Aperture_X_Jaw_Distance, " +
                "Aperture_Y_Jaw_Distance, Leaf_Gap_Average, Leaf_Gap_Std, Leaf_Travel, Converted_Aperture_Metric, Edge_Area_Metric, " +
                "Speed_0_4, Speed_4_8, Speed_8_12, Speed_12_16, Speed_16_20, Speed_20_25, Speed_Average, Speed_Std, " +
                "Acc_0_10, Acc_10_20, Acc_20_40, Acc_40_60, Acc_Average, Acc_std, SPORT");

            String imrt_filename = string.Format(@"{0}\patient_imrt_analysis.csv", outputdir);
            StreamWriter imrt_patient_file = new StreamWriter(imrt_filename, true, Encoding.GetEncoding("gb2312"));
            imrt_patient_file.WriteLine("ID, Name, Sites, Doctor, Physicist, PlanID, MachineID, Calculation_Model, Prescribed_Dose, MU, " +
                "EDGE_Metric, Leaf_Area, Plan_Irregularity, Plan_Modulation, Modulation_Complexity_Score, " +
                "Small_Aperture_Score_5mm, Small_Aperture_Score_10mm, Small_Aperture_Score_20mm, Mean_Field_Area, " +
                "Mean_Asymmetry_Distance, Aperture_Area_Ration_Jaw_Area, Aperture_Sub_Regions, Aperture_X_Jaw_Distance, " +
                "Aperture_Y_Jaw_Distance, Leaf_Gap_Average, Leaf_Gap_Std, Leaf_Travel, Converted_Aperture_Metric, Edge_Area_Metric");

            using (var reader = new StreamReader(@"D:\temp\PlanComplexity\patient_list.csv", Encoding.GetEncoding("gb2312"), true))
            {
                while (!reader.EndOfStream)
                {
                    string patient_info = reader.ReadLine();
                    String[] patient_infos = patient_info.Split(',');
                    Patient patient = app.OpenPatientById(patient_infos[0]);
                    if (patient == null)
                    {
                        patient = app.OpenPatientById(patient_infos[0].ToString().ToLower());
                    }

                    if (patient != null)
                    {
                        Console.WriteLine($"Open Patient by Id = {patient.Id}");
                        
                        // Only choose DMLC/IMRT and VMAT plan to analysis
                        var approvedPlans = from Course c in patient.Courses
                                            where (c.Id.ToString().ToUpper().Equals("C1") ||
                                            c.Id.ToString().ToUpper().Equals("C2") ||
                                            c.Id.ToString().ToUpper().Equals("C3"))
                                            from PlanSetup ps in c.PlanSetups
                                            where (ps.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved &&
                                            (ps.Id.ToString().ToUpper().Contains("IMRTA") ||
                                             ps.Id.ToString().ToUpper().Contains("VMATA") ||
                                             ps.Id.ToString().ToUpper().Contains("SBRTA") ||
                                             ps.Id.ToString().ToUpper().Contains("DMLCA")))
                                            select new
                                            {
                                                Patient = patient,
                                                Cource = c,
                                                Plan = ps
                                            };

                        if (approvedPlans.Any())
                        {
                            foreach (var p in approvedPlans)
                            {                             
                                PlanSetup ps = p.Plan;
                                Console.WriteLine($"Open Course by Id = {p.Cource.Id}, Open Plan by ID = {ps.Id}");

                                if (PlanUtilities.getTreatmentTechnique(ps).ToUpper().Equals("STATIC"))
                                {
                                    IMRT.Execute(p.Patient, ps, imrt_patient_file, patient_infos);
                                }
                                else if(PlanUtilities.getTreatmentTechnique(ps).ToUpper().Equals("ARC"))
                                {
                                    VMAT.Execute(p.Patient, ps, vmat_patient_file, patient_infos);
                                }
                            }
                        }
                    }
                    app.ClosePatient();
                }
            }
            imrt_patient_file.Close();
            vmat_patient_file.Close();
        }
    }
}
