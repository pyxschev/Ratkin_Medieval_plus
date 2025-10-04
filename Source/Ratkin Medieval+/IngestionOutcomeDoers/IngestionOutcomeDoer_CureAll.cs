using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class IngestionOutcomeDoer_CureAll : IngestionOutcomeDoer
    {
        public List<HediffDef> excludedHediffs;       // 不移除的hediff(如慢性病)
        public List<string> excludedCategories;       // 不移除的hediff类别
        public bool cureAddictions = false;           // 是否治愈成瘾
        public bool cureDiseases = true;              // 是否治愈疾病
        public bool cureToxins = true;                // 是否治愈毒素
        public bool cureInjuries = false;             // 是否治愈伤口(保留给治疗药水)

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            List<Hediff> hediffsToRemove = new List<Hediff>();

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                // 跳过排除的hediff
                if (excludedHediffs?.Contains(hediff.def) == true)
                    continue;

                // 跳过排除的类别
                if (excludedCategories?.Contains(hediff.def.hediffClass.Name) == true)
                    continue;

                // 根据设置决定是否移除
                if (ShouldRemoveHediff(hediff))
                {
                    hediffsToRemove.Add(hediff);
                }
            }

            // 移除符合条件的hediff
            foreach (var hediff in hediffsToRemove)
            {
                pawn.health.RemoveHediff(hediff);
            }

            // 显示治愈效果
            //if (pawn.Spawned)
            //{
            //    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_HealingCross, 2.0f);
            //}
        }

        private bool ShouldRemoveHediff(Hediff hediff)
        {
            // 毒素类
            if (cureToxins && (hediff.def == HediffDefOf.ToxicBuildup ||
                              hediff.def == HediffDefOf.FoodPoisoning ||
                              hediff.def.defName.Contains("Poison")))
                return true;

            // 疾病类
            if (cureDiseases && (hediff.def.tendable || hediff.def.chronic || 
                                hediff.def.defName.Contains("Disease") ||
                                hediff.def.isInfection ||
                                hediff.def.defaultLabelColor == UnityEngine.Color.red))
                return true;

            // 成瘾类
            if (cureAddictions && hediff.def.IsAddiction)
                return true;

            // 伤口类(通常不治愈，保留给治疗药水)
            if (cureInjuries && hediff is Hediff_Injury)
                return true;

            return false;
        }
    }
}
