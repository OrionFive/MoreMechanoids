using RimWorld;

namespace MoreMechanoids
{
	public static class GenericUtility
	{
		public static bool IsForcedOpen(this Building_Door door)
        {
			// Indestructible doors can't be forced
            if (!door.def.useHitPoints) return false;

            var comp = door?.GetComp<CompForceable>();
            return comp == null || comp.forcedOpen; // So if the comp is missing, we don't try
        }

		public static void Fix(this Building_Door door)
		{
			door?.GetComp<CompForceable>()?.Fix();
		}
	}
}
