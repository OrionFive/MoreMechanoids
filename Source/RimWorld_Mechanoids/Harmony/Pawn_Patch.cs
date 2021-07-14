using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MoreMechanoids.Harmony
{
    /// <summary>
    /// So Mammoths are accepted as sappers
    /// </summary>
    internal static class SappersUtility_Patch
    {
        [HarmonyPatch(typeof(SappersUtility), nameof(SappersUtility.HasBuildingDestroyerWeapon))]
        public class HasBuildingDestroyerWeapon
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn p, out bool __result)
            {
                __result = true;
                if (p.meleeVerbs == null) return true;
                var allVerbs = p.meleeVerbs.GetUpdatedAvailableVerbsList(false).Select(v=>v.verb);
                return allVerbs.All(verb => !verb.verbProps.ai_IsBuildingDestroyer);
            }
        }

        [HarmonyPatch(typeof(SappersUtility), nameof(SappersUtility.CanMineReasonablyFast))]
        public class CanMineReasonablyFast
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn p, out bool __result)
            {
                __result = true;
                return !p.RaceProps.IsMechanoid;
            }
        }
    }
}