using RimWorld;
using UnityEngine;
using Verse;

namespace RkM
{
    public class HediffCompProperties_Invisibility : HediffCompProperties
    {
        public float detectionReduction = 0.9f;    // 被发现几率降低 (0.9 = 90% harder to detect)
        public bool breakOnDamage = true;          //受伤是否移除隐身hediff
        public float alphaWhenInvisible = 0.2f;    

        public HediffCompProperties_Invisibility()
        {
            compClass = typeof(HediffComp_Invisibility);
        }
    }

    public class HediffComp_Invisibility : HediffComp
    {
        public HediffCompProperties_Invisibility Props => (HediffCompProperties_Invisibility)props;

        public bool IsInvisible => true; 

        public float GetAlpha()
        {
            return Props.alphaWhenInvisible;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            BecomeInvisible();
            UpdateTargetCache();
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            BecomeVisible();
            UpdateTargetCache();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
          
            CheckBreakInvisibility();
        }

        private void CheckBreakInvisibility()
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
            
            if (base.Pawn.stances?.stunner?.Stunned == true)
            {
                shouldBreak = true;
            }
            
            if (shouldBreak)
            {
                RemoveHediff();
            }
        }

        private void RemoveHediff()
        {
            if (base.Pawn?.health?.hediffSet != null)
            {
                // 创建打破效果
                if (base.Pawn.Spawned)
                {
                    MoteMaker.MakeStaticMote(base.Pawn.Position, base.Pawn.Map, ThingDefOf.Mote_ExplosionFlash, 0.8f);
                }
                
                // 移除隐身hediff
                base.Pawn.health.RemoveHediff(parent);
            }
        }

        public void BecomeVisible()
        {
            if (base.Pawn.Spawned)
            {
                // 创建可见效果
                MoteMaker.MakeStaticMote(base.Pawn.Position, base.Pawn.Map, ThingDefOf.Mote_ExplosionFlash, 0.5f);
                base.Pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }

        public void BecomeInvisible()
        {
            if (base.Pawn.Spawned)
            {
                // 创建隐身效果
                MoteMaker.MakeStaticMote(base.Pawn.Position, base.Pawn.Map, ThingDefOf.Mote_PsycastSkipFlashEntry, 0.3f);
                base.Pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }

        private void UpdateTargetCache()
        {
            if (base.Pawn.Spawned)
            {
                base.Pawn.Map.attackTargetsCache.UpdateTarget(base.Pawn);
                
                if (base.Pawn.RaceProps.Humanlike)
                {
                    PortraitsCache.SetDirty(base.Pawn);
                }
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            
            if (Props.breakOnDamage && totalDamageDealt > 0)
            {
                RemoveHediff();
            }
        }
    }
}
