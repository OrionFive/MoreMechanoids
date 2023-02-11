using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids.Harmony;

internal static class JobGiver_AIAssultColony_Patches
{
    [HarmonyPatch]
    public class FindBestReachableMeleeTarget
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(JobGiver_AITrashBuildingsDistant), "TryGiveJob");
            yield return AccessTools.Method(typeof(JobGiver_AITrashColonyClose), "TryGiveJob");
            yield return AccessTools.Method(typeof(JobGiver_AITrashDutyFocus), "TryGiveJob");
            yield return AccessTools.Method(typeof(JobGiver_AISapper), "TryGiveJob");
            yield return AccessTools.Method(typeof(JobGiver_AIFightEnemy), "TryGiveJob");
            yield return AccessTools.Method(typeof(JobGiver_AIGotoNearestHostile), "TryGiveJob");
        }

        [HarmonyPrefix]
        internal static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (pawn.def.defName != "Mech_Skullywag")
            {
                return true;
            }

            __result = null;
            return false;
        }
    }
}