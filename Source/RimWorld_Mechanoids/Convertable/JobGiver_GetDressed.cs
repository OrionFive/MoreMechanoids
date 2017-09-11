using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class JobGiver_GetDressed : ThinkNode_JobGiver
    {
        public static readonly BodyPartGroupDef[] bodyparts = {BodyPartGroupDefOf.Legs, BodyPartGroupDefOf.Torso};
        private static readonly TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);

        protected override Job TryGiveJob(Pawn pawn)
        {
            Log.Message("Pawn: "+pawn);
            if (pawn == null) return null;
            if (pawn.apparel == null) return null;
            foreach (BodyPartGroupDef bodypart in bodyparts)
            {
                if (pawn.apparel.BodyPartGroupIsCovered(bodypart)) continue;
                
                Apparel apparel = FindGarmentCoveringPart(pawn, bodypart);
                if (apparel == null) continue;
                
                return new Job(JobDefOf.Wear, apparel)
                {
                    canBash = true,
                    locomotionUrgency = LocomotionUrgency.Sprint
                };
            }
            return null;
        }

        private static Apparel FindGarmentCoveringPart(Pawn pawn, BodyPartGroupDef bodyPartGroupDef)
        {
            Predicate<Thing> validator = apparel => apparel.def.apparel.bodyPartGroups.Contains(bodyPartGroupDef) & pawn.CanReserve(apparel);

            return (Apparel)GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel), PathEndMode.InteractionCell,
                traverseParams, 20, validator);
        }
    }
}
