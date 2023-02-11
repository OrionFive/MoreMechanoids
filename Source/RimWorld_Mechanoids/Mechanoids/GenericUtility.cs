using RimWorld;
using Verse;

namespace MoreMechanoids;

public static class GenericUtility
{
    public static bool CanBeForcedOpen(this Thing thing)
    {
        return thing is ThingWithComps door && !door.def.useHitPoints && door.GetComp<CompForceable>() != null;
    }

    public static bool IsForcedOpen(this Building door)
    {
        // Indestructible doors can't be forced
        if (!door.def.useHitPoints)
        {
            return false;
        }

        var comp = door.GetComp<CompForceable>();
        return comp == null || comp.forcedOpen; // So if the comp is missing, we don't try
    }

    public static void Fix(this Building door)
    {
        door?.GetComp<CompForceable>()?.Fix();
    }

    public static bool ValidSkullywagTargetPawn(Pawn pawn)
    {
        return pawn is { Destroyed: false, Downed: false } && !pawn.IsInvisible() && pawn.def.race is { IsFlesh: true };
    }
}