using System.Collections.Generic;
using DoorsExpanded;
using HarmonyLib;
using Verse;

namespace MoreMechanoids;

/// <summary>
///     So HoldOpen can be disabled until the door is fixed
/// </summary>
internal static class Building_DoorExpanded_Patch
{
    [HarmonyPatch(typeof(Building_DoorExpanded), nameof(Building_DoorExpanded.GetGizmos))]
    public class GetGizmos
    {
        internal static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_DoorExpanded __instance)
        {
            foreach (var gizmo in __result)
            {
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