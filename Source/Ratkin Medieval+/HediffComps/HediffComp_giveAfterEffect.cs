using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace RkM
{

    public class HediffCompProperties_giveAfterEffect : HediffCompProperties
    {
        public HediffDef hediffDef;
        public float severity = 1.0f;
        public float chance = 1.0f;
        public HediffCompProperties_giveAfterEffect()
        {
            compClass = typeof(HediffComp_giveAfterEffect);
        }
    }

    public class HediffComp_giveAfterEffect : HediffComp
    {
        public HediffCompProperties_giveAfterEffect Props => (HediffCompProperties_giveAfterEffect)props;

        public override void CompPostPostRemoved()
        {
            float r = Rand.Range(0, 1f);
            if (r < Props.chance)
            {
                Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, parent.pawn);
                float effect = ((!(Props.severity > 0f)) ? Props.hediffDef.initialSeverity : Props.severity);
                hediff.Severity = effect;
                parent.pawn.health.AddHediff(hediff);
            }
         
        }
    }
}
