using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MoreMechanoids;

public abstract class JobGiver_DoorOpener : ThinkNode_JobGiver
{
    private const int CloseSearchRadius = 56;

    private static IntRange expiryInterval = new IntRange(450, 500);

    public override Job TryGiveJob(Pawn pawn)
    {
        // Can be dormant and is dormant? Don't do this job
        if ((pawn.GetComp<CompCanBeDormant>()?.Awake ?? true) == false)
        {
            return null;
        }

        if (!pawn.HostileTo(Faction.OfPlayer))
        {
            return null;
        }

        if (PawnUtility.EnemiesAreNearby(pawn))
        {
            return null;
        }

        var door = FindNearbyDoor(pawn, CloseSearchRadius);

        if (door == null)
        {
            return null;
        }

        return CreateJob(pawn, door);
    }

    private static Job CreateJob(Pawn pawn, Building door)
    {
        var newReq = new CastPositionRequest { caster = pawn, target = door };

        foreach (var verb in pawn.VerbTracker.AllVerbs.InRandomOrder())
        {
            newReq.verb = verb;
            newReq.maxRangeFromTarget = Mathf.Max(verb.verbProps.range, 1.42f);

            if (!verb.Available() || !verb.IsUsableOn(door) || !CastPositionFinder.TryFindCastPosition(newReq, out _))
            {
                continue;
            }

            var job = JobMaker.MakeJob(JobDefOf.UseVerbOnThing, door);
            job.verbToUse = verb;
            job.expiryInterval = expiryInterval.RandomInRange;
            job.checkOverrideOnExpire = true;
            job.expireRequiresEnemiesNearby = true;
            job.attackDoorIfTargetLost = true;
            return job;
        }

        return null;
    }

    protected abstract Building FindNearbyDoor(Pawn pawn, float maxDist);
}