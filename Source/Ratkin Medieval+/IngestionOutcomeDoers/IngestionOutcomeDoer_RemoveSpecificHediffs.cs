using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class IngestionOutcomeDoer_RemoveSpecificHediffs : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffsToRemove;    
        public float chance = 1.0f;                
        public bool removeAll = false;             

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            foreach (var hediffDef in hediffsToRemove)
            {
                if (Rand.Chance(chance))
                {
                    if (removeAll)
                    {
                        pawn.health.hediffSet.hediffs.RemoveAll(h => h.def == hediffDef);
                    }
                    else
                    {
                        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                        if (hediff != null)
                        {
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                }
            }
        }
    }
}
