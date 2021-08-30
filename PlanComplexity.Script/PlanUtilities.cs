using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace Complexity.Script
{
    public class PlanUtilities
    {
        //Plan total MU
        static public double getTotalMU(PlanSetup plan)
        {
            return (from beam in plan.Beams
                    where (!beam.IsSetupField && beam.MLC != null)
                    select beam.Meterset.Value).ToArray().Sum();
        }

        //Prescribed Dose Per Fraction
        static public double getPrescribedDosePerFraction(PlanSetup plan)
        {
            double fractionDose = 0.0;
            try
            {
                fractionDose = plan.UniqueFractionation.PrescribedDosePerFraction.Dose;
            }
            catch(Exception)
            {
                ;
            }

            return fractionDose;
        }

        //Beam treatment Unit
        static public string getTreatmentUnit(PlanSetup plan)
        {
            return (from beam in plan.Beams
                    where (!beam.IsSetupField && beam.MLC != null)
                    select beam.TreatmentUnit.Id).FirstOrDefault();
        }
        
        static public string getTreatmentTechnique(PlanSetup plan)
        {
            return (from beam in plan.Beams
                    where (!beam.IsSetupField && beam.MLC != null)
                    select beam.Technique).FirstOrDefault().ToString();
        }       
    }
}
