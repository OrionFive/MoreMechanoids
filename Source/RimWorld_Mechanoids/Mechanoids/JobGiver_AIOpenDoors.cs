using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MoreMechanoids {
    public class JobGiver_AIOpenDoors : ThinkNode_JobGiver
    {
        private const int CloseSearchRadius = 56;
        private static readonly Predicate<Thing> validDoor = t => t is Building_Door {Spawned: true, Destroyed: false, Open:false} door && !door.IsForcedOpen();
        private static IntRange expiryInterval = new IntRange(450, 500);

        public override Job TryGiveJob(Pawn pawn)
        {
            // Can be dormant and is dormant? Don't do this job
            if ((pawn.GetComp<CompCanBeDormant>()?.Awake ?? true) == false) return null;

            if (!pawn.HostileTo(Faction.OfPlayer)) return null;

            if (PawnUtility.EnemiesAreNearby(pawn)) return null;

            var door = FindNearbyDoor(pawn, CloseSearchRadius, validDoor);

            if (door == null) return null;

            return CreateJob(pawn, door);
        }

        private static Job CreateJob(Pawn pawn, Building_Door door)
        {
            var newReq = new CastPositionRequest {caster = pawn, target = door};
            
            foreach (var verb in pawn.VerbTracker.AllVerbs.InRandomOrder())
            {
                newReq.verb = verb;
                newReq.maxRangeFromTarget = Mathf.Max(verb.verbProps.range, 1.42f);

                if(verb.Available() && verb.IsUsableOn(door) && CastPositionFinder.TryFindCastPosition(newReq, out _))
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
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetDoors(pawn), PathEndMode.Touch, TraverseParms.For(pawn), maxDist, validator) as Building_Door;
        }

        private static IEnumerable<Building_Door> GetDoors(Pawn pawn)
        {
            foreach (var door in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Door>()) yield return door;

            // Add Doors Expanded doors as targets - or better don't, because it doesn't really work (can't easily hold them open)
            //var heronDef = DefDatabase<ThingDef>.GetNamed("HeronInvisibleDoor", false);
            //if (heronDef != null)
            //    foreach (var door in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(heronDef).OfType<Building_Door>())
            //    {
            //        //Log.Message($"Heron door: {door.Position}, {door.Label}, open = {door.Open}, forced open = {door.IsForcedOpen()}, verbs = {pawn.VerbTracker.AllVerbs.Where(v=>v.Available() && v.IsUsableOn(door)).Select(v=>v.ReportLabel).ToCommaList()}");
            //        yield return door;
            //    }
        }
    }
}
