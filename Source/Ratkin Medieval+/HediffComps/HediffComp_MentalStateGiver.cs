using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RkM
{
    public class HediffCompProperties_MentalStateGiver : HediffCompProperties
    {
        public MentalStateDef mentalState;     
        public float mtbDays = 0.01f;          
        public float chance = 1.0f;            
        public bool triggerOnce = true;
        public bool triggerImmediately = true; 
        public bool endWithHediff = true;

        public HediffCompProperties_MentalStateGiver()
        {
            compClass = typeof(HediffComp_MentalStateGiver);
        }
    }

    public class HediffComp_MentalStateGiver : HediffComp
    {
        public HediffCompProperties_MentalStateGiver Props => (HediffCompProperties_MentalStateGiver)props;
        private bool hasTriggered = false;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            
            if (Props.triggerImmediately && !hasTriggered && Rand.Chance(Props.chance))
            {
                parent.pawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalState);
                hasTriggered = true;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            //预留，hediff时间内随机触发精神状态
            if (!Props.triggerImmediately && (!Props.triggerOnce || !hasTriggered))
            {
                if (Rand.MTBEventOccurs(Props.mtbDays, 60000f, 1f))
                {
                    if (Rand.Chance(Props.chance))
                    {
                        parent.pawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalState);
                        if (Props.triggerOnce) hasTriggered = true;
                    }
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Props.endWithHediff && hasTriggered && 
                parent.pawn.mindState.mentalStateHandler.CurStateDef == Props.mentalState)
            {
                parent.pawn.mindState.mentalStateHandler.Reset();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref hasTriggered, "hasTriggered", false);
        }
    }
}
