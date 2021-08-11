using RimWorld;

namespace MoreMechanoids
{
	public static class GenericUtility
	{
		public static bool IsForcedOpen(this Building_Door door)
		{
			var comp = door?.GetComp<CompForceable>();
			return comp == null || comp.forcedOpen; // So if the comp is missing, we don't try
		}

		public static void Fix(this Building_Door door)
		{
			door?.GetComp<CompForceable>()?.Fix();
		}
	}
}
