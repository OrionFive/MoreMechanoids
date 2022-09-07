using Verse;

namespace MoreMechanoids
{
	/// <summary>
	/// So doors can be forced open
	/// </summary>
	public class CompProperties_Forceable : CompProperties
	{
		public DoorHandler DoorHandler;
		public CompProperties_Forceable()
		{
			compClass = typeof(CompForceable);
			DoorHandler = new DoorHandler();
		}
	}
}
