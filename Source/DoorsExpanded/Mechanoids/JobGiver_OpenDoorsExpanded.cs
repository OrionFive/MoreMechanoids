using System;
using System.Collections.Generic;
using DoorsExpanded;
using MoreMechanoids;
using Verse;
using Verse.AI;

namespace MoreMechanoidsDoorsExpanded;

public class JobGiver_OpenDoorsExpanded : JobGiver_DoorOpener
{
    private static readonly Predicate<Thing> validDoor = t =>
        t is Building_DoorExpanded { Spawned: true, Destroyed: false, Open: false } door && door.def.useHitPoints &&
        !door.IsForcedOpen();

    protected override Building FindNearbyDoor(Pawn pawn, float maxDist)
    {
        return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetDoors(pawn),
            PathEndMode.Touch,
            TraverseParms.For(pawn), maxDist, validDoor) as Building;
    }

    private static IEnumerable<Building_DoorExpanded> GetDoors(Pawn pawn)
    {
        foreach (var door in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_DoorExpanded>())
        {
            yield return door;
        }
    }
}