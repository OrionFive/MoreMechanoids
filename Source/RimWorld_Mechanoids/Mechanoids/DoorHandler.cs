using RimWorld;
using Verse;

namespace MoreMechanoids;

public class DoorHandler
{
    public virtual void Force(ThingWithComps thing, ref bool originalHoldOpenValue, ref bool forcedOpen)
    {
        if (thing is not Building_Door door)
        {
            Log.Error($"Cant get as Building_Door: {thing.GetType()}");
            return;
        }

        originalHoldOpenValue = door.holdOpenInt;
        forcedOpen = true;
        door.DoorOpen();
        door.holdOpenInt = true;
    }

    public virtual void Fix(ThingWithComps thing, bool originalHoldOpenValue, ref bool forcedOpen)
    {
        if (thing is not Building_Door door)
        {
            Log.Error($"Cant get as Building_Door: {thing.GetType()}");
            return;
        }

        door.holdOpenInt = originalHoldOpenValue;
        forcedOpen = false;
        door.DoorOpen();
    }
}