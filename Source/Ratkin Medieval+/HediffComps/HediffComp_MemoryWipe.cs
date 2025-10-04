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
    public class HediffCompProperties_MemoryWipe : HediffCompProperties
    {
        public int wipeIntervalTicks = 300;          // 每次清除记忆的间隔(5秒)
        public int totalWipeDurationTicks = 3600;    // 总清除过程持续时间(1分钟)
        public bool clearSocialMemories = true;      // 清除社交记忆
        public bool clearCulturalMemories = true;    // 清除文化记忆
        public bool clearSkillMemories = false;      // 清除技能记忆
        public bool resetIdeology = true;            // 重置意识形态确信度
        public bool removeLoyaltyMark = true;        // 移除死忠标记
        public float memoryWipeIntensity = 1.0f;     // 每次清除的强度

        public HediffCompProperties_MemoryWipe()
        {
            this.compClass = typeof(HediffComp_MemoryWipe);
        }
    }

    public class HediffComp_MemoryWipe : HediffComp
    {
        public HediffCompProperties_MemoryWipe Props => (HediffCompProperties_MemoryWipe)props;
        private int ticksSinceLastWipe = 0;
        private int totalWipeTicks = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksSinceLastWipe++;
            totalWipeTicks++;

            // 检查是否到了清除记忆的时间
            if (ticksSinceLastWipe >= Props.wipeIntervalTicks)
            {
                PerformMemoryWipe();
                ticksSinceLastWipe = 0;
            }

            // 检查清除过程是否完成
            if (totalWipeTicks >= Props.totalWipeDurationTicks)
            {
                // 清除过程完成，移除这个hediff
                severityAdjustment = -1f; // 立即移除
            }
        }

        private void PerformMemoryWipe()
        {
            Pawn pawn = parent.pawn;

            // 清除社交记忆
            if (Props.clearSocialMemories)
            {
                ClearSocialMemoriesGradually(pawn);
            }

            // 重置文化记忆
            if (Props.clearCulturalMemories)
            {
                ClearCulturalMemoriesGradually(pawn);
            }

            // 移除死忠标记
            if (Props.removeLoyaltyMark)
            {
                RemoveLoyaltyMark(pawn);
            }

            // 显示记忆清除效果
            //if (pawn.Spawned)
            //{
            //    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_PsycastPsychicEffect, 1.0f);
            //}
        }

        private void ClearSocialMemoriesGradually(Pawn pawn)
        {
            // 每次随机移除一些记忆和关系
            if (pawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                var memories = pawn.needs.mood.thoughts.memories.Memories.ToList();
                var memoriesToRemove = memories.Where(m => Rand.Chance(0.3f)).Take(2).ToList();

                foreach (var memory in memoriesToRemove)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemory(memory);
                }
            }

            // 减弱社交关系
            if (pawn.relations?.DirectRelations != null)
            {
                var relations = pawn.relations.DirectRelations.ToList();
                foreach (var relation in relations.Where(r => Rand.Chance(0.2f)))
                {
                    // 减弱关系强度
                    relation.startTicks = Find.TickManager.TicksGame -
                        Mathf.RoundToInt((Find.TickManager.TicksGame - relation.startTicks) * 0.8f);
                }
            }
        }

        private void ClearCulturalMemoriesGradually(Pawn pawn)
        {
            // 重置意识形态确信度
            if (ModsConfig.IdeologyActive && Props.resetIdeology && pawn.ideo != null)
            {
                float reductionAmount = 0.1f;
                pawn.ideo.OffsetCertainty(-reductionAmount);
            }
        }

        private void RemoveLoyaltyMark(Pawn pawn)
        {
            // 移除死忠标记(如果存在的话)
            // 注意：这里需要根据实际的死忠系统实现来调整
            if (pawn.guest != null)
            {
                pawn.guest.resistance = Mathf.Max(0f, pawn.guest.resistance - 10f);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksSinceLastWipe, "ticksSinceLastWipe", 0);
            Scribe_Values.Look(ref totalWipeTicks, "totalWipeTicks", 0);
        }
    }
}
