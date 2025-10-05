using RimWorld;
using UnityEngine;
using Verse;

namespace RkM
{
    public class HediffCompProperties_RatkinInvisibility : HediffCompProperties_Invisibility
    {
        public float detectionReduction = 0.9f;    //被发现几率降低
        public bool breakOnDamage = true;          
        public float customAlpha = 0.2f;         

        public HediffCompProperties_RatkinInvisibility()
        {
            compClass = typeof(HediffComp_RatkinInvisibility);
        }
    }

    public class HediffComp_RatkinInvisibility : HediffComp_Invisibility
    {
        public new HediffCompProperties_RatkinInvisibility Props => (HediffCompProperties_RatkinInvisibility)props;
        public new float GetAlpha()
        {
            if (Props.visibleToPlayer)
            {
                return 1f;
            }
            
            if (ShouldBeForcedVisible())
            {
                return 1f;
            }
            
            return Props.customAlpha;
        }

        private bool ShouldBeForcedVisible()
        {
            
            if (base.Pawn.Downed)
            {
                return true;
            }
            
            if (base.Pawn.IsBurning())
            {
                return true;
            }
            
            if (base.Pawn.ParentHolder is Pawn_CarryTracker)
            {
                return true;
            }
            
            if (Props.affectedByDisruptor && base.Pawn.health.hediffSet.HasHediff(HediffDefOf.DisruptorFlash))
            {
                return true;
            }
            
            if (base.Pawn.health.hediffSet.HasHediff(HediffDefOf.CoveredInFirefoam))
            {
                return true;
            }
            
            return false;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            
            if (Props.breakOnDamage && totalDamageDealt > 0)
            {
                base.Pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            
            CheckCustomBreakConditions();
        }

        private void CheckCustomBreakConditions()
        {
            bool shouldBreak = false;
            
            if (base.Pawn.Downed)
            {
                shouldBreak = true;
            }
            
            if (base.Pawn.IsBurning())
            {
                shouldBreak = true;
            }
            
            if (base.Pawn.ParentHolder is Pawn_CarryTracker)
            {
                shouldBreak = true;
            }
            
            if (shouldBreak)
            {
                base.Pawn.health.RemoveHediff(parent);
            }
        }
    }
}
