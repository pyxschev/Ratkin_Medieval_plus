using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RkM
{
    public class IngestionOutcomeDoer_GiveRandomInspiration : IngestionOutcomeDoer
    {
        public float chance = 1.0f;                      // 给予灵感的概率
        public List<InspirationDef> allowedInspirations; // 允许的灵感类型(可选)
        public List<InspirationDef> excludedInspirations; // 排除的灵感类型
        public bool skipIfAlreadyInspired = true;         // 如果已有灵感则跳过

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            // 检查是否已有灵感
            if (skipIfAlreadyInspired && pawn.mindState.inspirationHandler.Inspired)
            {
                Messages.Message("已经有灵感了！", pawn, MessageTypeDefOf.RejectInput);
                return;
            }

            // 概率检查
            if (!Rand.Chance(chance))
                return;

            // 获取可用的灵感列表
            List<InspirationDef> availableInspirations = GetAvailableInspirations(pawn);

            if (availableInspirations.Any())
            {
                InspirationDef chosenInspiration = availableInspirations.RandomElement();
                pawn.mindState.inspirationHandler.TryStartInspiration(chosenInspiration);

                // 显示效果
                //if (pawn.Spawned)
                //{
                //    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_PsycastPsychicEffect, 1.5f);
                //}
            }
        }

        private List<InspirationDef> GetAvailableInspirations(Pawn pawn)
        {
            List<InspirationDef> available = new List<InspirationDef>();

            foreach (InspirationDef inspiration in DefDatabase<InspirationDef>.AllDefs)
            {
                // 跳过排除的灵感
                if (excludedInspirations?.Contains(inspiration) == true)
                    continue;

                // 如果指定了允许列表，只使用列表中的
                if (allowedInspirations?.Any() == true && !allowedInspirations.Contains(inspiration))
                    continue;

                // 检查pawn是否满足条件
                if (inspiration.Worker.InspirationCanOccur(pawn))
                {
                    available.Add(inspiration);
                }
            }

            return available;
        }
    }
}
