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
        public float severity;

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
            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, parent.pawn);
            float effect = ((!(Props.severity > 0f)) ? Props.hediffDef.initialSeverity : Props.severity);
            hediff.Severity = effect;
            parent.pawn.health.AddHediff(hediff);
        }
    }
}
