using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class JobGiver_Standby : ThinkNode_JobGiver
    {
        public Danger maxDanger = Danger.None;
        public bool instantly;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (this.instantly)
            {
                return new Job(MoreMechanoidsDefOf.Standby, pawn.Position);
            }
            Building_RestSpot restSpot = Building_RestSpot.Find(pawn);
            if (restSpot != null)
            {
                //if (spot.owner != pawn)
                //{
                //   spot.Claim(pawn as PawnConverted);
                //}
                return new Job(MoreMechanoidsDefOf.Standby, restSpot);
            }

            {
                IntVec3 restPos = GetRestSpot(pawn.Position, this.maxDanger, pawn);
                
                return new Job(MoreMechanoidsDefOf.Standby, restPos);
            }
        }

        private static IntVec3 GetRestSpot(IntVec3 originCell, Danger maxDanger, Pawn me)
        {
            if (!(me is PawnConverted)) return originCell;

            Danger currentDanger = me.Position.GetDangerFor(me);
            Danger danger = (Danger)Math.Max((int) maxDanger, (int)currentDanger);

            Predicate<IntVec3> validator = c => c.Standable(me.Map) && me.Map.roofGrid.Roofed(c) 
                && CoverUtility.TotalSurroundingCoverScore(c, me.Map) > 2.5f && !NextToDoor(c, me)
                && me.Map.reachability.CanReach(originCell, c, PathEndMode.OnCell, TraverseMode.PassDoors, danger);

            if (validator(originCell) && InRange(originCell, me, 20)) return originCell;

            for (int i = 0; i < 50; i++)
            {
                Thing thing = GetRandom(me.Map.listerBuildings.allBuildingsColonist);
                
                if (thing == null) thing = GetRandom(me.Map.listerBuildings.allBuildingsColonistCombatTargets);
                if (thing == null) thing = GetRandom(me.Map.mapPawns.FreeColonists);
                if (thing == null) thing = GetRandom(me.Map.mapPawns.FreeColonistsAndPrisoners);

                if (thing == null) break;
                if (CellFinder.TryFindRandomCellNear(thing.Position, me.Map, 10, validator, out IntVec3 result))
                {
                    return result;
                }
            }

            Predicate<IntVec3> simpleValidator = c => c.Standable(me.Map)
               && CoverUtility.TotalSurroundingCoverScore(c, me.Map) > 1 && !NextToDoor(c, me)
               && me.Map.reachability.CanReach(originCell, c, PathEndMode.OnCell, TraverseMode.PassDoors, danger);

            return CellFinder.TryFindRandomCellNear(originCell, me.Map, 20, simpleValidator, out IntVec3 randomCell) ? randomCell : originCell;
        }

        private static bool InRange(IntVec3 originCell, Pawn me, int range) => originCell.InHorDistOf(me.Position, range);

        private static T GetRandom<T>(IEnumerable<T> list) where T : class 
        {
            if (list == null) return null;
            T[] array = list as T[] ?? list.ToArray();
            return array.Any() ? array.RandomElement() : null;
        }

        private static bool NextToDoor(IntVec3 c, Pawn me)
        {
            // Any door or pawn (excluding myself)?
            Func<IntVec3, bool> predicate = a => a.InBounds(me.Map) && a.GetThingList(me.Map).Any(t =>
                                                                                      {
                                                                                          if (t == me) return false;
                                                                                          return t is Building_Door;
                                                                                      });

            return predicate(c) || c.GetThingList(me.Map).Any(t=>t != me && t is Pawn) || GenAdj.CellsAdjacentCardinal(c, new Rot4(), IntVec2.One).Any(predicate);
        }
    }
}