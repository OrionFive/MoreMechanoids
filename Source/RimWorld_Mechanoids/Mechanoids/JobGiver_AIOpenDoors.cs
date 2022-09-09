using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class JobGiver_AIOpenDoors : JobGiver_DoorOpener    {
        

        private static readonly Predicate<Thing> validDoor = t => t is Building_Door
        {
            Spawned: true, Destroyed: false, Open: false, def.useHitPoints: true
        } door && !door.IsForcedOpen();
       

        protected override Building FindNearbyDoor(Pawn pawn, float maxDist)
        {
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetDoors(pawn), PathEndMode.Touch, TraverseParms.For(pawn), maxDist, validDoor) as Building_Door;
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
