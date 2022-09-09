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
			Props.DoorHandler.Force(parent, ref originalHoldOpenValue, ref forcedOpen);
		}

		public void Fix()
		{
			if (!forcedOpen) return;
            Props.DoorHandler.Fix(parent, originalHoldOpenValue, ref forcedOpen);
		}
	}
}
