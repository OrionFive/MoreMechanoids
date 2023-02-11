using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MoreMechanoids;

public class JobGiver_DownAllHumans : JobGiver_AIFightEnemies
{
    public new const float targetKeepRadius = 65;
    public new const float targetAcquireRadius = 56;

    private static IntRange expiryInterval = new IntRange(450, 500);

    public override bool ExtraTargetValidator(Pawn pawn, Thing target)
    {
        return GenericUtility.ValidSkullywagPawn(target as Pawn);
    }

    public override Job TryGiveJob(Pawn pawn)
    {
        // Can be dormant and is dormant? Don't do this job
        var dormancy = pawn.GetComp<CompCanBeDormant>();
        if (dormancy is { Awake: false })
        {
            return null;
        }

        UpdateEnemyTarget(pawn);
        var enemyTarget = pawn.mindState.enemyTarget;

        if (enemyTarget is not Pawn enemyPawn || !GenericUtility.ValidSkullywagPawn(enemyPawn))
        {
            return null;
        }

        return CreateJob(pawn, enemyPawn);
    }

    public override Thing FindAttackTarget(Pawn pawn)
    {
        var flags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos |
                    TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
        //if (this.needLOSToAcquireNonPawnTargets)
        //    flags |= TargetScanFlags.NeedLOSToNonPawns;
        //if (this.PrimaryVerbIsIncendiary(pawn))
        //    flags |= TargetScanFlags.NeedNonBurning;

        var attackTarget = (Thing)AttackTargetFinder.BestAttackTarget(pawn, flags, x => ExtraTargetValidator(pawn, x),
            0.0f, targetAcquireRadius, GetFlagPosition(pawn), GetFlagRadius(pawn));
        return attackTarget;
    }

    private static Job CreateJob(Pawn pawn, Pawn targetPawn)
    {
        var newReq = new CastPositionRequest { caster = pawn, target = targetPawn };
        var verb = pawn.verbTracker.AllVerbs.FirstOrDefault(verb => verb is Verb_ParalyzingPoke);

        newReq.verb = verb;
        newReq.maxRangeFromTarget = Mathf.Max(verb.verbProps.range, 1.1f);
        if (!verb.Available() || !verb.IsUsableOn(targetPawn) || !CastPositionFinder.TryFindCastPosition(newReq, out _))
        {
            return null;
        }

        var job = JobMaker.MakeJob(JobDefOf.AttackMelee, targetPawn);
        job.verbToUse = verb;
        job.expiryInterval = expiryInterval.RandomInRange;
        job.checkOverrideOnExpire = true;
        job.expireRequiresEnemiesNearby = true;
        return job;
    }
}