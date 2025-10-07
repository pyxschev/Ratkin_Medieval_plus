using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;

namespace RkM
{
    public class HediffCompProperties_InstantHeal : HediffCompProperties
    {
        public int woundsToHeal = 1;
        public bool healInfections = false;
        public bool healOldWounds = false;
        public float healingQuality = 1.0f;

        public HediffCompProperties_InstantHeal()
        {
            compClass = typeof(HediffComp_InstantHeal);
        }
    }

    public class HediffComp_InstantHeal : HediffComp
    {
        public HediffCompProperties_InstantHeal Props => (HediffCompProperties_InstantHeal)props;

        private bool hasHealed = false;

        public override void CompPostMake()
        {
            base.CompPostMake();
            if (!hasHealed)
            {
                PerformInstantHealing();
                hasHealed = true;
            }
        }

        private void PerformInstantHealing()
        {
            if (base.Pawn?.health?.hediffSet == null) return;

            var eligibleWounds = GetEligibleWounds();
            int woundsHealed = 0;

            foreach (var wound in eligibleWounds)
            {
                if (woundsHealed >= Props.woundsToHeal) break;

                PerformTendingOnWound(wound);
                woundsHealed++;
            }
        }

        private List<Hediff_Injury> GetEligibleWounds()
        {
            var wounds = new List<Hediff_Injury>();

            foreach (var hediff in base.Pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_Injury injury && injury.TendableNow())
                {
                    if (!Props.healOldWounds && injury.IsPermanent()) continue;
                    if (!Props.healInfections && IsInfectionType(injury)) continue;

                    wounds.Add(injury);
                }
            }
            wounds.Sort((a, b) => b.Severity.CompareTo(a.Severity));

            return wounds;
        }

        private bool IsInfectionType(Hediff_Injury injury)
        {
            return injury.def.defName.ToLower().Contains("infection") || 
                   injury.def.defName.ToLower().Contains("flu") ||
                   injury.def.defName.ToLower().Contains("plague") ||
                   injury.def.defName.ToLower().Contains("malaria");
        }

        private void PerformTendingOnWound(Hediff_Injury wound)
        {
            if (wound?.Part == null) return;
            ThingDef medicineDef = GetVirtualMedicineDef();
            float tendQuality = CalculateTendQuality();
            if (wound.TendableNow())
            {
                wound.Tended(tendQuality, tendQuality);
                if (Props.healingQuality > 0.8f) 
                {
                    ApplyMagicalHealing(wound);
                }
            }
        }

        private ThingDef GetVirtualMedicineDef()
        {
            if (Props.healingQuality >= 1.0f)
                return ThingDefOf.MedicineUltratech ?? ThingDefOf.MedicineIndustrial;
            else if (Props.healingQuality >= 0.8f)
                return ThingDefOf.MedicineIndustrial ?? ThingDefOf.MedicineHerbal;
            else
                return ThingDefOf.MedicineHerbal;
        }

        private float CalculateTendQuality()
        {
            float baseTendQuality = Props.healingQuality;
            float variance = Rand.Range(0.9f, 1.1f);
            return Mathf.Clamp01(baseTendQuality * variance * 1.2f);
        }

        private void ApplyMagicalHealing(Hediff_Injury wound)
        {
            float magicalHealAmount = wound.Severity * (Props.healingQuality - 0.8f) * 0.5f;
            if (magicalHealAmount > 0f)
            {
                wound.Severity = Mathf.Max(0f, wound.Severity - magicalHealAmount);
                
                if (wound.Severity <= 0.001f)
                {
                    base.Pawn.health.RemoveHediff(wound);
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref hasHealed, "hasHealed", false);
        }
    }
}

