using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NAT
{
    public static class CalamityUtility
    {
        public static List<Hediff_Calamity> activeHediffs = new List<Hediff_Calamity>();

		public static bool AffectedByCalamity(this Pawn pawn, out float power)
        {
			power = 0;
            if (activeHediffs.NullOrEmpty())
            {
                return false;
            }
			Hediff_Calamity hediff = activeHediffs.FirstOrDefault((x)=>x.pawn == pawn);
			if(hediff == null)
            {
				return false;
			}
            power = hediff.Severity;
			return true;
        }
    }
}
