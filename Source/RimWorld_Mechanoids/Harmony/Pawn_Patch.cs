using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace MoreMechanoids.Harmony
{
    /// <summary>
    /// So Mammoths are accepted as sappers
    /// </summary>
    internal static class SappersUtility_Patch
    {
        [HarmonyPatch(typeof(SappersUtility), "HasBuildingDestroyerWeapon")]
        public class HasBuildingDestroyerWeapon
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn p, out bool __result)
            {
                Log.Message("Checking "+p.LabelShort);
                __result = true;
                if (p.meleeVerbs == null) return true;
                var allVerbs = p.meleeVerbs.GetUpdatedAvailableVerbsList(false).Select(v=>v.verb);
                Log.Message("Has "+allVerbs.Count() +" verbs");
                foreach (var verb in allVerbs)
                {
                    if (verb.verbProps.ai_IsBuildingDestroyer)
                    {
                        Log.Message("Building destroyer found");
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SappersUtility), "CanMineReasonablyFast")]
        public class CanMineReasonablyFast
        {
            [HarmonyPrefix]
            internal static bool Prefix(Pawn p, out bool __result)
            {
                __result = true;
                if (p.RaceProps.IsMechanoid)
                {
                    return false;
                }
                return true;
            }
        }
    }
}