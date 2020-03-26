using HarmonyLib;
using RimWorld;
using Verse;

namespace MoreMechanoids.Harmony
{
    /// <summary>
    /// Don't spawn Mechanoid sapper attacks all the time when wealth is high
    /// </summary>
    public class RaidStrategyWorker_ImmediateAttackSappers_Patch
    {
        [HarmonyPatch(typeof(RaidStrategyWorker_ImmediateAttackSappers), nameof(RaidStrategyWorker_ImmediateAttackSappers.CanUseWith))]
        public class CanUseWith
        {
            [HarmonyPostfix]
            internal static void Postfix(IncidentParms parms, ref bool __result)
            {
                if (!__result) return;
                if (parms.faction != Faction.OfMechanoids) return;

                // Only do sappers 15% of the time
                __result = Rand.ChanceSeeded(0.15f, GenMath.RoundRandom(parms.points));
            }
        }
    }
}
