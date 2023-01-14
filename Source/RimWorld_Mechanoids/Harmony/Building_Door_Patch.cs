using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MoreMechanoids.Harmony;

/// <summary>
///     So HoldOpen can be disabled until the door is fixed
/// </summary>
internal static class Building_Door_Patch
{
    [HarmonyPatch(typeof(Building_Door), nameof(Building_Door.GetGizmos))]
    public class GetGizmos
    {
        internal static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_Door __instance)
        {
            foreach (var gizmo in __result)
            {
                //Log.Message($"{gizmo.GetType().Name} - {(gizmo as Command)?.Label}");
                if (gizmo is Command_Toggle t && t.defaultLabel == "CommandToggleDoorHoldOpen".Translate())
                {
                    var forcedOpen = __instance.IsForcedOpen();
                    gizmo.disabled = forcedOpen;
                    gizmo.disabledReason = forcedOpen ? "DisabledForcedOpen".Translate() : null;
                }

                yield return gizmo;
            }
        }
    }
}