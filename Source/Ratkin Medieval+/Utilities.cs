using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI;
using Verse;

namespace RkM
{
    public static class RomanceAttemptMentalStateUtility
    {
        public static bool CanChaseAndAttemptRomance(Pawn pawn, Pawn target, bool checkIfJobBlocked = false, bool checkIfMentallyCapable = true)
        {
            if (target == null || target.Dead || target.Downed)
                return false;

            if (target == pawn)
                return false;

            if (checkIfMentallyCapable && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                return false;

            if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                return false;

            if (target.InMentalState)
                return false;

            if (pawn.relations.OpinionOf(target) < 0)
                return false;

            return true;
        }

        public static void GetRomanceCandidatesFor(Pawn pawn, List<Pawn> outCandidates, bool allowPrisoners)
        {
            outCandidates.Clear();
            List<Pawn> allPawnsSpawned = (List<Pawn>)pawn.Map.mapPawns.AllPawnsSpawned;

            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                Pawn candidate = allPawnsSpawned[i];

                if (candidate == pawn)
                    continue;

                if (candidate.Dead || candidate.Downed)
                    continue;

                if (!allowPrisoners && candidate.IsPrisoner)
                    continue;

                if (candidate.Faction != pawn.Faction && candidate.Faction != null && !candidate.Faction.def.humanlikeFaction)
                    continue;

                if (candidate.RaceProps.Animal)
                    continue;

                if (!CanChaseAndAttemptRomance(pawn, candidate, checkIfJobBlocked: false, checkIfMentallyCapable: true))
                    continue;

                outCandidates.Add(candidate);
            }
        }
    }

}
