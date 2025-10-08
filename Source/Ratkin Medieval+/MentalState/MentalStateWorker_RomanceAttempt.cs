using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse.AI;
using Verse;

namespace RkM 
{
   
    public class MentalState_RomanceAttempt : MentalState
    {
        private const int InteractionCooldownTicks = 300;      
        private const float NearTargetDistance = 3f;           
        private const int StandingSpotMinRadius = 1;            
        private const int StandingSpotMaxRadius = 3;           
        private const float ChitchatChance = 0.5f;          
        private const float DeepTalkChance = 0.25f;          
        private const float RomanceAttemptChance = 0.25f;               
        
        public Pawn target;
        public bool attemptedRomanceWithTargetAtLeastOnce;
        public int lastRomanceAttemptTicks = -999999;
        
        private static List<Pawn> candidates = new List<Pawn>();

        public override string InspectLine
        {
            get
            {
                return string.Format(this.def.baseInspectLine, this.target.LabelShort);
            }
        }

        protected override bool CanEndBeforeMaxDurationNow
        {
            get
            {
                return this.attemptedRomanceWithTargetAtLeastOnce;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.target, "target", false);
            Scribe_Values.Look<bool>(ref this.attemptedRomanceWithTargetAtLeastOnce, "attemptedRomanceWithTargetAtLeastOnce", false, false);
            Scribe_Values.Look<int>(ref this.lastRomanceAttemptTicks, "lastRomanceAttemptTicks", 0, false);
        }

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void MentalStateTick(int delta)
        {
            if (this.target != null && (!this.target.Spawned || !this.pawn.CanReach(this.target, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn) || !this.target.Awake()))
            {
                Pawn oldTarget = this.target;
                if (!this.TryFindNewTarget())
                {
                    base.RecoverFromState();
                    return;
                }
                Messages.Message("MessageTargetedRomanceAttemptChangedTarget".Translate(this.pawn.LabelShort, oldTarget.Label, this.target.Label, this.pawn.Named("PAWN"), oldTarget.Named("OLDTARGET"), this.target.Named("TARGET")).AdjustedFor(this.pawn, "PAWN", true), this.pawn, MessageTypeDefOf.NegativeEvent, true);
            }
            else if (this.target == null || !RomanceAttemptMentalStateUtility.CanChaseAndAttemptRomance(this.pawn, this.target, false, false))
            {
                base.RecoverFromState();
                return;
            }

            if (this.target != null)
            {
                this.TryStartRomanceJob();
            }

            base.MentalStateTick(delta);
        }

        private void TryStartRomanceJob()
        {
            int ticksSinceLastAttempt = Find.TickManager.TicksGame - this.lastRomanceAttemptTicks;
            bool isInCooldown = ticksSinceLastAttempt < InteractionCooldownTicks;
            bool isNearTarget = this.IsNearTarget();

            if (this.ShouldNotInterruptCurrentJob(isNearTarget, isInCooldown))
                return;

            if (isNearTarget && !isInCooldown)
            {
                if (this.TryInteractWithTarget())
                    return;
            }

            if (isNearTarget && isInCooldown)
            {
                Job waitJob = JobMaker.MakeJob(JobDefOf.Wait);
                waitJob.expiryInterval = InteractionCooldownTicks - ticksSinceLastAttempt;
                this.pawn.jobs.TryTakeOrderedJob(waitJob, JobTag.Misc);
                return;
            }

            if (this.pawn.CanReach(this.target, PathEndMode.OnCell, Danger.Deadly))
            {
                IntVec3 standingSpot = this.FindStandingSpotNearTarget();
                if (standingSpot.IsValid)
                {
                    Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, standingSpot);
                    gotoJob.locomotionUrgency = LocomotionUrgency.Jog;
                    gotoJob.expiryInterval = 300;
                    this.pawn.jobs.TryTakeOrderedJob(gotoJob, JobTag.Misc);
                }
            }
        }

        private bool ShouldNotInterruptCurrentJob(bool isNearTarget, bool isInCooldown)
        {
            if (this.pawn.jobs.curJob == null)
                return false;

            Job curJob = this.pawn.jobs.curJob;

            if (curJob.def == JobDefOf.StandAndBeSociallyActive && curJob.targetA.Thing == this.target)
                return true;

            if (curJob.def == JobDefOf.Goto && isNearTarget)
                return true;

            if (curJob.def == JobDefOf.Wait && isNearTarget && isInCooldown)
                return true;

            if (ModsConfig.BiotechActive && curJob.def == JobDefOf.TryRomance && curJob.targetA.Thing == this.target)
                return true;
                
            return false;
        }

        private bool TryInteractWithTarget()
        {
            float randomValue = Rand.Value;
            InteractionDef interaction;

            if (randomValue < ChitchatChance)
                interaction = InteractionDefOf.Chitchat;
            else if (randomValue < ChitchatChance + DeepTalkChance)
                interaction = InteractionDefOf.DeepTalk;
            else
                interaction = InteractionDefOf.RomanceAttempt;

            if (this.AttemptInteraction(interaction))
            {
                this.attemptedRomanceWithTargetAtLeastOnce = true;
                this.lastRomanceAttemptTicks = Find.TickManager.TicksGame;
                return true;
            }
            
            return false;
        }

        private bool AttemptInteraction(InteractionDef interaction)
        {
            return this.pawn.interactions.TryInteractWith(this.target, interaction);
        }

        private bool IsNearTarget()
        {
            if (this.target == null) return false;
            return this.pawn.Position.DistanceTo(this.target.Position) <= NearTargetDistance;
        }

        private IntVec3 FindStandingSpotNearTarget()
        {
            if (this.target == null) return IntVec3.Invalid;

            for (int radius = StandingSpotMinRadius; radius <= StandingSpotMaxRadius; radius++)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.target.Position, radius, true))
                {
                    if (this.IsValidStandingSpot(cell))
                    {
                        return cell;
                    }
                }
            }

            return this.target.Position;
        }

        private bool IsValidStandingSpot(IntVec3 cell)
        {
            Map map = this.pawn.Map;
            return cell.InBounds(map) && 
                   cell.Standable(map) && 
                   this.pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly) &&
                   cell.GetFirstPawn(map) == null; 
        }

        public override void PreStart()
        {
            base.PreStart();
            this.TryFindNewTarget();
        }

        private bool TryFindNewTarget()
        {
            RomanceAttemptMentalStateUtility.GetRomanceCandidatesFor(this.pawn, candidates, false);
            bool foundTarget = candidates.TryRandomElement(out this.target);
            candidates.Clear();
            return foundTarget;
        }

        public override void PostEnd()
        {
            base.PostEnd();
            if (this.target != null && PawnUtility.ShouldSendNotificationAbout(this.pawn))
            {
                Messages.Message("MessageNoLongerOnTargetedRomanceAttempt".Translate(this.pawn.LabelShort, this.target.Label, this.pawn.Named("PAWN"), this.target.Named("TARGET")), this.pawn, MessageTypeDefOf.SituationResolved, true);
            }
        }

        public override TaggedString GetBeginLetterText()
        {
            if (this.target == null)
            {
                Log.Error("No target. This should have been checked in this mental state's worker.");
                return "";
            }
            return this.def.beginLetter.Formatted(this.pawn.NameShortColored, this.target.NameShortColored, this.pawn.Named("PAWN"), this.target.Named("TARGET")).AdjustedFor(this.pawn, "PAWN", true).Resolve()
                .CapitalizeFirst();
        }
    }

    public class MentalStateWorker_RomanceAttempt : MentalStateWorker
    {
        private static List<Pawn> candidates = new List<Pawn>();

        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
            {
                return false;
            }
            
            RomanceAttemptMentalStateUtility.GetRomanceCandidatesFor(pawn, candidates, false);
            bool hasValidTargets = candidates.Count >= 1;
            candidates.Clear();
            return hasValidTargets;
        }
    }
}
