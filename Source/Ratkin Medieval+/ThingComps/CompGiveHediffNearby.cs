using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class CompProperties_GiveHediffNearby: CompProperties
    {
        public float radius = 0f;
        public HediffDef hediff;

        public CompProperties_GiveHediffNearby()
        {
            compClass = typeof(CompGiveHediffNearby);
        }
    }


    public class CompGiveHediffNearby: ThingComp
    {
        private CompProperties_GiveHediffNearby Props => (CompProperties_GiveHediffNearby)props;

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.IsHashIntervalTick(30))
            {
                return;
            }
            Map map = parent.Map;
            IntVec3 loc = parent.Position;
            List<Pawn> allHumanlikeSpawned = map.mapPawns.AllHumanlikeSpawned;
            for (int i = 0; i < allHumanlikeSpawned.Count; i++)
            {
                Pawn pawn = allHumanlikeSpawned[i];
                if (pawn.RaceProps.Humanlike && loc.InHorDistOf(pawn.PositionHeld, Props.radius))
                {
                    pawn.health.GetOrAddHediff(Props.hediff);
                }
            }
        }
    }
}
