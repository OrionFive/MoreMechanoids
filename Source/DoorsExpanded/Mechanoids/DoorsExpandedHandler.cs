using DoorsExpanded;
using HarmonyLib;
using MoreMechanoids;
using Verse;

namespace MoreMechanoidsDoorsExpanded.Mechanoids;

public class DoorsExpandedHandler : DoorHandler
{
    private const int DEFAULT_TICK_TO_CLOSE = 110;

    public override void Force(ThingWithComps thing, ref bool originalHoldOpenValue, ref bool forcedOpen)
    {
        var door = thing as Building_DoorExpanded;

        var traverse = Traverse.Create(door);

        originalHoldOpenValue = traverse.Field("holdOpenInt").GetValue<bool>();
        forcedOpen = true;
        traverse.Field("holdOpenInt").SetValue(true);

        var doorOpen = AccessTools.Method(typeof(Building_DoorExpanded), "DoorOpen");
        doorOpen.Invoke(door, new object[] { DEFAULT_TICK_TO_CLOSE });
    }

    public override void Fix(ThingWithComps thing, bool originalHoldOpenValue, ref bool forcedOpen)
    {
        var door = thing as Building_DoorExpanded;
        var traverse = Traverse.Create(door);
        traverse.Field("holdOpenInt").SetValue(originalHoldOpenValue);
        forcedOpen = false;

        var doorOpen = AccessTools.Method(typeof(Building_DoorExpanded), "DoorOpen");
        doorOpen.Invoke(door, new object[] { DEFAULT_TICK_TO_CLOSE });
    }
}