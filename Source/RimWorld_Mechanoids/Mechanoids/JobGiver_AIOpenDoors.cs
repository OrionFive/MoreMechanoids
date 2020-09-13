using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids {
    public class JobGiver_AIOpenDoors : ThinkNode_JobGiver
    {
        private const int CloseSearchRadius = 56;
        private static Predicate<Thing> validDoor = t => t is Building_Door door && !door.Destroyed && !door.Open;
        private static IntRange expiryInterval = new IntRange(450, 500);

        protected override Job TryGiveJob(Pawn pawn)
        {
            // Can be dormant and is dormant? Don't do this job
            if ((pawn.GetComp<CompCanBeDormant>()?.Awake ?? true) == false) return null;

            if (!pawn.HostileTo(Faction.OfPlayer)) return null;
            
            var door = FindNearbyDoor(pawn, CloseSearchRadius, validDoor);

            if (door == null) return null;

            return CreateJob(pawn, door);
        }

        private static Job CreateJob(Pawn pawn, Building_Door door)
        {
            foreach (var verb in pawn.VerbTracker.AllVerbs.InRandomOrder())
            {
                if(verb.Available() && verb.IsUsableOn(door) && !door.Open)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThing, door);
                    job.verbToUse = verb;
                    job.expiryInterval = expiryInterval.RandomInRange;
                    job.checkOverrideOnExpire = true;
                    job.expireRequiresEnemiesNearby = true;
                    job.attackDoorIfTargetLost = true;
                    return job;
                }
            }
            return null;
        }

        private static Building_Door FindNearbyDoor(Pawn pawn, float maxDist, Predicate<Thing> validator)
        {
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Door>(), PathEndMode.Touch, TraverseParms.For(pawn), maxDist, validator) as Building_Door;
        }

        private bool IsDoorInRange(Thing thing, Thing searcherThing, IntVec3 flagPosition, double radiusSquared)
        {
            return (thing.Position - flagPosition).LengthHorizontalSquared <= radiusSquared && searcherThing.HostileTo(thing) && validDoor(thing);
        }

    }
}
