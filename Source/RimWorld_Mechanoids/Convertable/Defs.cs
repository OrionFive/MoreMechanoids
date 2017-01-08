using Verse;
using BrokenStateDef = MoreMechanoids.BrokenStateDef;

internal static class Defs
{
    public static readonly JobDef StandbyDef = DefDatabase<JobDef>.GetNamed("Standby");
    public static readonly BrokenStateDef CrashedDef = DefDatabase<BrokenStateDef>.GetNamed("Crashed");
    public static readonly JobDef JobRepairDef = DefDatabase<JobDef>.GetNamed("RepairMechanoid");

    public static ThingDef LinkDef { get { return ThingDef.Named("PAL_Link"); } }
}