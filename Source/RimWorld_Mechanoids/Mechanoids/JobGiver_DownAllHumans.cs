using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MoreMechanoids
{
    public class JobGiver_DownAllHumans : JobGiver_AIFightEnemies
    {
        public float targetKeepRadius = 72f;
        public float targetAcquireRadius = 65f;
        private Predicate<Thing> validDoor = t => t is Building_Door door && !door.Destroyed && !door.Open;
        private Predicate<Thing> validPawn = t => t is Pawn pawn && !pawn.Destroyed && !pawn.Downed && pawn.def.race != null && pawn.def.race.IsFlesh;

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            return validDoor(target) || validPawn(target);
        }

        protected override void UpdateEnemyTarget(Pawn pawn)
        {
            if (pawn == null) return;
            var thing = pawn.mindState.enemyTarget;
            if (thing != null)
            {
                //Log.Message($"{pawn.Label}: was going for {thing.Label}.");
                if (thing.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, true)
                    || (pawn.Position - thing.Position).LengthHorizontalSquared > targetKeepRadius*targetKeepRadius)
                {
                    thing = null;
                }

                if (thing is Pawn pawn2 && pawn2.Downed)
                {
                    thing = null;
                }

                if (thing is Building_Door door && door.Open)
                {
                    thing = null;
                }
            }

            if (thing == null)
            {
                const TargetScanFlags targetScanFlags = TargetScanFlags.NeedReachable;

                thing = AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, validPawn, 0f, targetAcquireRadius) as Thing;
                if (thing == null)
                {
                    //Thing thing2 = GenAI.BestAttackTarget(pawn.Position, pawn, validatorDoor, targetAcquireRadius, 0f, targetScanFlags2);
                    var door = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Door>()
                            .Where(b => validDoor(b) && pawn.Map.reachability.CanReach(b.Position, pawn.Position, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Deadly))
                            .OrderBy(t => t.Position.DistanceToSquared(pawn.Position))
                            .FirstOrDefault();

                    if (door != null)
                    {
                        thing = door;
                    }
                }
                if (thing != null && thing != pawn.mindState.enemyTarget)
                {
                    //Log.Message(pawn.LabelShort + " attacking " + thing.LabelShort + " at " + thing.Position);
                    Traverse.Create(pawn.mindState).Method("Notify_EngagedTarget");

                    Lord lord = pawn.Map.lordManager.LordOf(pawn);

                    lord?.Notify_PawnAcquiredTarget(pawn, thing);

                    pawn.CurJob?.SetTarget(TargetIndex.A, thing);

                    pawn.mindState.enemyTarget = thing;
                }
                if(thing == null)
                {
                    pawn.mindState.enemyTarget = null;
                }
            }
        }
    }
}
