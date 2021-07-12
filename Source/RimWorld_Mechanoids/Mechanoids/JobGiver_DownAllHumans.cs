using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class JobGiver_DownAllHumans : JobGiver_AIFightEnemies
    {
        public const float targetKeepRadius = 65;
        public const float targetAcquireRadius = 56;
        private readonly Predicate<Thing> validPawn = t => t is Pawn pawn && !pawn.Destroyed && !pawn.Downed && pawn.def.race != null && pawn.def.race.IsFlesh;

        public override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            return validPawn(target);
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            // Can be dormant and is dormant? Don't do this job
            if ((pawn.GetComp<CompCanBeDormant>()?.Awake ?? true) == false) return null;
            return base.TryGiveJob(pawn);
        }

        public override Thing FindAttackTarget(Pawn pawn)
        {
            TargetScanFlags flags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            //if (this.needLOSToAcquireNonPawnTargets)
            //    flags |= TargetScanFlags.NeedLOSToNonPawns;
            //if (this.PrimaryVerbIsIncendiary(pawn))
            //    flags |= TargetScanFlags.NeedNonBurning;

            var attackTarget = (Thing) AttackTargetFinder.BestAttackTarget(pawn, flags, x => ExtraTargetValidator(pawn, x), 0.0f, targetAcquireRadius, GetFlagPosition(pawn), GetFlagRadius(pawn));
            return attackTarget;
        }
    }
}
