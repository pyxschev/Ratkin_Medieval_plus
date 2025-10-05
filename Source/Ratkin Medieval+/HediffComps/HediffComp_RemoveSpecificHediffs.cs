using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RkM
{
    public class HediffCompProperties_RemoveSpecificHediffs : HediffCompProperties
    {
        public List<HediffDef> hediffsToRemove = new List<HediffDef>();
        public bool removeOnAdd = true;      // Remove hediffs when this hediff is added
        public bool removeAll = false;       // Remove all instances or just first one
        public float chance = 1.0f;          // Chance to remove each hediff

        public HediffCompProperties_RemoveSpecificHediffs()
        {
            compClass = typeof(HediffComp_RemoveSpecificHediffs);
        }
    }

    public class HediffComp_RemoveSpecificHediffs : HediffComp
    {
        public HediffCompProperties_RemoveSpecificHediffs Props => (HediffCompProperties_RemoveSpecificHediffs)props;
        
        private bool hasRemovedOnAdd = false;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            
            if (Props.removeOnAdd && !hasRemovedOnAdd)
            {
                RemoveTargetHediffs();
                hasRemovedOnAdd = true;
            }
        }

        private void RemoveTargetHediffs()
        {
            if (base.Pawn.health?.hediffSet == null) return;

            foreach (var hediffDef in Props.hediffsToRemove)
            {
                if (Rand.Chance(Props.chance))
                {
                    if (Props.removeAll)
                    {
                        List<Hediff> toRemove = new List<Hediff>();
                        foreach (var hediff in base.Pawn.health.hediffSet.hediffs)
                        {
                            if (hediff.def == hediffDef)
                            {
                                toRemove.Add(hediff);
                            }
                        }
                        
                        foreach (var hediff in toRemove)
                        {
                            base.Pawn.health.RemoveHediff(hediff);
                        }
                    }
                    else
                    {
                        var hediff = base.Pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                        if (hediff != null)
                        {
                            base.Pawn.health.RemoveHediff(hediff);
                        }
                    }
                }
            }

            RemoveBuiltInHediffs();
        }

        private void RemoveBuiltInHediffs()
        {
            foreach (var hediffDefName in Props.hediffsToRemove.Select(h => h.defName))
            {
                if (hediffDefName == "ToxicBuildup")
                {
                    var hediff = base.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
                    if (hediff != null && Rand.Chance(Props.chance))
                    {
                        base.Pawn.health.RemoveHediff(hediff);
                    }
                }
                else if (hediffDefName == "FoodPoisoning")
                {
                    var hediff = base.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FoodPoisoning);
                    if (hediff != null && Rand.Chance(Props.chance))
                    {
                        base.Pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref hasRemovedOnAdd, "hasRemovedOnAdd", false);
        }
    }
}