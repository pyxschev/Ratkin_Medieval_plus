using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class HediffCompProperties_givePermanentTrait : HediffCompProperties
    {
        public TraitDef traitDef;
        public int traitDegree = 0;
        public float chance = 1.0f;

        public HediffCompProperties_givePermanentTrait()
        {
            compClass = typeof(HediffComp_givePermanentTrait);
        }
    }

    public class HediffComp_givePermanentTrait : HediffComp
    {
        public HediffCompProperties_givePermanentTrait Props => (HediffCompProperties_givePermanentTrait)props;

        public override void CompPostPostRemoved()
        {
            if (Props.traitDef == null) return;
            
            float r = Rand.Range(0f, 1f);
            if (r < Props.chance)
            {
                Pawn p = parent.pawn;
                if (p?.story?.traits != null)
                {
                    if (p.story.traits.HasTrait(Props.traitDef))
                    {
                        return; 
                    }
                    
                    try
                    {
                        Trait newTrait = new Trait(Props.traitDef, Props.traitDegree);
                        p.story.traits.GainTrait(newTrait, suppressConflicts: true);

                        // if (p.Map != null)
                        // {
                        //     EffecterDefOf.Skip_ExitNoSound.Spawn(p, p.Map);
                        // }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[RkM] Failed to give permanent trait {Props.traitDef.defName} to {p.Name}: {ex.Message}");
                    }
                }
            }
        }

    }
}
