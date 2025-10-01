using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RkM
{
    public class HediffCompProperties_givePermanentTrait : HediffCompProperties
    {
        public HediffDef hediffDef;
        public float severity;

        public HediffCompProperties_givePermanentTrait()
        {
            compClass = typeof(HediffComp_givePermanentTrait);
        }
    }

    public class HediffComp_givePermanentTrait : HediffComp
    {
        public HediffCompProperties_givePermanentTrait Props => (HediffCompProperties_givePermanentTrait)props;

    }
}
