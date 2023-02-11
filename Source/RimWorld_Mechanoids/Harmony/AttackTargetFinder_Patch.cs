using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids.Harmony;

internal static class AttackTargetFinder_Patch
{
    [HarmonyPatch(typeof(AttackTargetFinder), "BestAttackTarget")]
    public class FindBestReachableMeleeTarget
    {
        [HarmonyPostfix]
        internal static void Prefix(IAttackTargetSearcher searcher, ref Predicate<IAttackTarget> validator)
        {
            if (searcher.Thing.def.defName != "Mech_Skullywag")
            {
                return;
            }

            var oldValidator = validator;
            validator = attackTarget =>
                oldValidator(attackTarget) && attackTarget.Thing is Pawn pawn &&
                GenericUtility.ValidSkullywagPawn(pawn) ||
                attackTarget.Thing is Building_Door;
        }
    }
}