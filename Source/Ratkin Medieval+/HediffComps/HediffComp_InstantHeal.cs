using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;



public class HediffCompProperties_InstantHeal : HediffCompProperties
{
    public int woundsToHeal = 1;
    public bool healInfections = false;
    public bool healOldWounds = false;
    public float healingQuality = 1.0f;
}

public class HediffComp_InstantHeal : HediffComp
{
    public HediffCompProperties_InstantHeal Props => (HediffCompProperties_InstantHeal)props;

    public override void CompPostMake()
    {
        // 查找伤口
        // 按优先级治疗
        // 排除失血等特定类型
    }
}