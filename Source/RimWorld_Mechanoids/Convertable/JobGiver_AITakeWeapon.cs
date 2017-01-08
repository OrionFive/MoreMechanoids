using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    //private static Job MineOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
    //{
    //    if (!pawn.CanReserve(blocker, 1))
    //    {
    //        return new Job(JobDefOf.Goto, CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, 10), 500, true);
    //    }
    //    return new Job(JobDefOf.Mine, blocker)
    //    {
    //        ignoreDesignations = true,
    //        expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
    //        checkOverrideOnExpire = true
    //    };
    //}

    public class JobGiver_AITakeWeapon : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            //if (pawn.equipment.Primary != null) return null;

            //Log.Message("Take weapon:");
            Predicate<Thing> validator = t => t.def.IsWeapon
                                              && pawn.CanReserve(t) 
                                              && t.Position.DistanceToSquared(pawn.Position) < 20*20
                                              && (pawn.equipment.Primary == null || IsBetter(t, pawn.equipment.Primary)) 
                                              && pawn.CanReach(t, PathEndMode.Touch,Danger.Deadly, true);

            //var weapon = GenClosest.ClosestThing_Global_Reachable(pawn.Position,
            //    Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways), PathEndMode.InteractionCell,
            //    traverseParams, 50, validator);
            var weapon = Find.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
                .Where(t => validator(t))
                .OrderByDescending(t => t.MarketValue - t.Position.DistanceToSquared(pawn.Position)*2)
                .FirstOrDefault();

            if (weapon != null)
            {
                if (weapon.Position==pawn.Position || weapon.Position.AdjacentToCardinal(pawn.Position))
                {
                    Log.Message("Equiping " + weapon.Label);
                    return new Job(JobDefOf.Equip, weapon)
                    {
                        checkOverrideOnExpire = true,
                        expiryInterval = 500,
                        canBash = true,
                        locomotionUrgency = LocomotionUrgency.Sprint
                    };
                }
                return GotoForce(pawn, weapon, PathEndMode.Touch);
            }
            return null;
        }

        private static Job GotoForce(Pawn pawn, TargetInfo target, PathEndMode pathEndMode)
        {
            Log.Message(pawn+", "+target);
            PawnPath pawnPath = PathFinder.FindPath(pawn.Position, target, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAnything), pathEndMode);
            
            Thing thing = null;
            Log.Message("Path Length: "+pawnPath.NodesLeftCount);
            //if (pawnPath.NodesLeftCount >= 3)
            //{
                IntVec3 cellBeforeBlocker;
                thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker);
            //}
            pawnPath.ReleaseToPool();

            if (thing == null)
            {
                return new Job(JobDefOf.Goto, target, 500, true);
            }
            if (thing.def.mineable)
            {
                Log.Message(thing.def.defName);
                return null;// JobGiver_AISapper.MineOrWaitJob(pawn, thing, cellBeforeBlocker);
            }
            if (pawn.equipment.Primary != null)
            {
                Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
                if (primaryVerb.verbProps.ai_IsBuildingDestroyer 
                    && (!primaryVerb.verbProps.ai_IsIncendiary || thing.FlammableNow))
                {
                    return new Job(JobDefOf.UseVerbOnThing)
                    {
                        targetA = thing,
                        verbToUse = primaryVerb,
                        expiryInterval = 500
                    };
                }
            }
            //if (this.canMineNonMineables)
            //{
            //    return JobGiver_AISapper.MineOrWaitJob(pawn, thing, cellBeforeBlocker);
            //}
            return MeleeOrWaitJob(pawn, thing, cellBeforeBlocker);
            //Log.Message("Going to " + weapon.Label);
            //return new Job(DefDatabase<JobDef>.GetNamed("GotoForce"), position)
            //{
            //    //checkOverrideOnExpire = true,
            //    //expiryInterval = 500,
            //    canBash = true,
            //    locomotionUrgency = LocomotionUrgency.Sprint
            //};
        }

        private static Job MeleeOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
        {
            if (!pawn.CanReserve(blocker))
            {
                return new Job(JobDefOf.Goto, CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, 10), 100, true);
            }
            return new Job(JobDefOf.AttackMelee, blocker)
            {
                ignoreDesignations = true,
                expiryInterval = 500,
                checkOverrideOnExpire = true
            };
        }

        private static bool IsBetter(Thing newWeapon, ThingWithComps oldWeapon)
        {
            return newWeapon.MarketValue > oldWeapon.MarketValue*1.2f;
        }
    }
}