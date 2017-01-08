using System;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class JobGiver_DownAllHumans : JobGiver_AIFightEnemies
    {
        public float targetKeepRadius = 72;
        public float targetAcquireRadius = 65;

        protected override void UpdateEnemyTarget(Pawn pawn)
        {
            var thing = pawn.mindState.enemyTarget;
            if (thing != null)
            {
                if (thing.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, true) || (pawn.Position - thing.Position).LengthHorizontalSquared > targetKeepRadius * targetKeepRadius)
                {
                    thing = null;
                }
                Pawn pawn2 = thing as Pawn;
                if (pawn2 != null && pawn2.Downed)
                {
                    thing = null;
                }
                Building_Door door = thing as Building_Door;
                if (door != null && door.Open)
                {
                    thing = null;
                }
            }
            // Select only flesh stuff
            Predicate<Thing> validatorPawn = t => t is Pawn && !t.Destroyed && !(t as Pawn).Downed && t.def.race != null && t.def.race.isFlesh;
            Predicate<Thing> validatorDoor = t => t is Building_Door && !t.Destroyed && !(t as Building_Door).Open;

            // Method is internal (duh)
            MethodInfo notifyEngagedTarget = typeof(Pawn_MindState).GetMethod("Notify_EngagedTarget",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (thing == null)
            {
                //Log.Message(pawn.Label + ": trying to find target...");
                const TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachable;

                thing=AttackTargetFinder.BestAttackTarget(pawn, validatorPawn, targetAcquireRadius, 0f, targetScanFlags);
                if (thing != null)
                {
                    //Log.Message("Selected pawn " + thing.Label);
                    notifyEngagedTarget.Invoke(pawn.mindState, null);
                    Brain brain = Find.SquadBrainManager.SquadBrainFor(pawn);
                    if (brain != null)
                    {
                        brain.Notify_PawnAcquiredTarget(pawn, thing);
                    }
                }
                else
                {
                    //Thing thing2 = GenAI.BestAttackTarget(pawn.Position, pawn, validatorDoor, targetAcquireRadius, 0f, targetScanFlags2);
                    Building_Door thing2 =
                        Find.ListerBuildings.AllBuildingsColonistOfClass<Building_Door>()
                            .Where(
                                b => validatorDoor(b) && Reachability.CanReach(b.Position, pawn.Position, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Deadly))
                            .OrderBy(door => door.Position.DistanceToSquared(pawn.Position)).FirstOrDefault();
                    if (thing2 != null && thing2 != thing)
                    {
                        notifyEngagedTarget.Invoke(pawn.mindState, null);
                        //Log.Message("Selected door " + thing2.Label);
                        thing = thing2;
                    }
                }
                if (thing != pawn.mindState.enemyTarget)
                {
                    if (pawn.CurJob != null)
                    {
                        pawn.CurJob.SetTarget(TargetIndex.A, thing);
                    }
                    pawn.mindState.enemyTarget = thing;
                }
            }
        }
    }
}