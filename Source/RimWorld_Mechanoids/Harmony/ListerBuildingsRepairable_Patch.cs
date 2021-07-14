using HarmonyLib;
using RimWorld;
using Verse;

namespace MoreMechanoids.Harmony
{
	/// <summary>
	/// So doors get fixed when repaired.
	/// </summary>
	internal static class ListerBuildingsRepairable_Patch
	{
		[HarmonyPatch(typeof(ListerBuildingsRepairable), nameof(ListerBuildingsRepairable.Notify_BuildingRepaired))]
		public class Notify_BuildingRepaired
		{
			internal static void Postfix(Building b)
			{
				if (b is Building_Door door && door.IsForcedOpen() && door.HitPoints >= door.MaxHitPoints)
				{
					door.Fix();
				}
			}
		}
	}
}
