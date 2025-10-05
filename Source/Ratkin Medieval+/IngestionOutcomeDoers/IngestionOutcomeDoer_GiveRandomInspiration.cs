using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class IngestionOutcomeDoer_GiveRandomInspiration : IngestionOutcomeDoer
    {
        public float chance = 1.0f;                     
        public List<InspirationDef> allowedInspirations; 
        public List<InspirationDef> excludedInspirations;
        public bool skipIfAlreadyInspired = true;         

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
           
            if (skipIfAlreadyInspired && pawn.mindState.inspirationHandler.Inspired)
            {
                Messages.Message("已经有灵感了！", pawn, MessageTypeDefOf.RejectInput);
                return;
            }

            if (!Rand.Chance(chance))
                return;

            List<InspirationDef> availableInspirations = GetAvailableInspirations(pawn);

            if (availableInspirations.Any())
            {
                InspirationDef chosenInspiration = availableInspirations.RandomElement();
                pawn.mindState.inspirationHandler.TryStartInspiration(chosenInspiration);

                //
                //if (pawn.Spawned)
                //{
                //    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_PsycastPsychicEffect, 1.5f);
                //}
            }
        }

        private List<InspirationDef> GetAvailableInspirations(Pawn pawn)
        {
            List<InspirationDef> available = new List<InspirationDef>();

            foreach (InspirationDef inspiration in DefDatabase<InspirationDef>.AllDefs)
            {
                if (excludedInspirations?.Contains(inspiration) == true)
                    continue;

                if (allowedInspirations?.Any() == true && !allowedInspirations.Contains(inspiration))
                    continue;

                if (inspiration.Worker.InspirationCanOccur(pawn))
                {
                    available.Add(inspiration);
                }
            }

            return available;
        }
    }
}
