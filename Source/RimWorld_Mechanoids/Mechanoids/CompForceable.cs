using RimWorld;
using Verse;

namespace MoreMechanoids
{
	/// <summary>
	/// So doors can be forced open.
	/// </summary>
	public class CompForceable : ThingComp
	{
		private CompProperties_Forceable Props => (CompProperties_Forceable) props;
		private Building_Door Door => (Building_Door) parent;
		private bool originalHoldOpenValue;

		public bool forcedOpen;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref forcedOpen, "forcedOpen");
			Scribe_Values.Look(ref originalHoldOpenValue, "originalHoldOpenValue");
		}

		public void Force()
		{
			if (forcedOpen) return;
			originalHoldOpenValue = Door.holdOpenInt;
			forcedOpen = true;
			Door.DoorOpen();
			Door.holdOpenInt = true;
		}

		public void Fix()
		{
			if (!forcedOpen) return;
			Door.holdOpenInt = originalHoldOpenValue;
			forcedOpen = false;
			Door.DoorOpen(); // So it closes soon
		}
	}
}
