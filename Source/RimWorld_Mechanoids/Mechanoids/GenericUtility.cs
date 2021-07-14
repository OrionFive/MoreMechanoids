using RimWorld;

namespace MoreMechanoids
{
	public static class GenericUtility
	{
		public static bool IsForcedOpen(this Building_Door door)
		{
			return door.GetComp<CompForceable>().forcedOpen;
		}

		public static void Fix(this Building_Door door)
		{
			door.GetComp<CompForceable>().Fix();
		}
	}
}
