using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class IngestionOutcomeDoer_RemoveSpecificHediffs : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffsToRemove;    // 要移除的特定hediff列表
        public float chance = 1.0f;                // 移除成功概率
        public bool removeAll = false;             // 是否移除所有实例

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            foreach (var hediffDef in hediffsToRemove)
            {
                if (Rand.Chance(chance))
                {
                    if (removeAll)
                    {
                        // 移除所有该类型的hediff
                        pawn.health.hediffSet.hediffs.RemoveAll(h => h.def == hediffDef);
                    }
                    else
                    {
                        // 只移除第一个
                        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                        if (hediff != null)
                        {
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                }
            }
        }
    }
}
