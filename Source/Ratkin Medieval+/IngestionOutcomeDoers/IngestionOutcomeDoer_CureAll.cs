using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class IngestionOutcomeDoer_CureAll : IngestionOutcomeDoer
    {
        public List<HediffDef> excludedHediffs;       
        public List<string> excludedCategories;       
        public bool cureAddictions = true;           
        public bool cureDiseases = true;              
        public bool cureToxins = true;               
        public bool cureInjuries = false;             

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            List<Hediff> hediffsToRemove = new List<Hediff>();

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (excludedHediffs?.Contains(hediff.def) == true)
                    continue;

                if (excludedCategories?.Contains(hediff.def.hediffClass.Name) == true)
                    continue;

                if (ShouldRemoveHediff(hediff))
                {
                    hediffsToRemove.Add(hediff);
                }
            }

            foreach (var hediff in hediffsToRemove)
            {
                pawn.health.RemoveHediff(hediff);
            }

            // 显示治愈效果
            //if (pawn.Spawned)
            //{
            //    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_HealingCross, 2.0f);
            //}
        }

        private bool ShouldRemoveHediff(Hediff hediff)
        {
            if (cureToxins && (hediff.def == HediffDefOf.ToxicBuildup ||
                              hediff.def == HediffDefOf.FoodPoisoning ||
                              hediff.def.defName.Contains("Poison")))
                return true;

            if (cureDiseases && (hediff.def.tendable || hediff.def.chronic || 
                                hediff.def.defName.Contains("Disease") ||
                                hediff.def.isInfection ||
                                hediff.def.defaultLabelColor == UnityEngine.Color.red))
                return true;

            if (cureAddictions && hediff.def.IsAddiction)
                return true;

            if (cureInjuries && hediff is Hediff_Injury)
                return true;

            return false;
        }
    }
}
