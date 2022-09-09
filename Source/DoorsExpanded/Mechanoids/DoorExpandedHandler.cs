using DoorsExpanded;
using HarmonyLib;
using MoreMechanoids;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreMechanoidsDoorsExpanded.Mechanoids
{
    public class DoorExpandedHandler : DoorHandler
    {
        private const int DEFAULT_TICK_TO_CLOSE = 110;

        public override void Force(ThingWithComps thing, ref bool originalHoldOpenValue, ref bool forcedOpen)
        {
            Building_DoorExpanded door = thing as Building_DoorExpanded;
            Traverse traverse = Traverse.Create(door);

            originalHoldOpenValue = traverse.Field("holdOpenInt").GetValue<bool>();
            forcedOpen = true;
            traverse.Field("holdOpenInt").SetValue(true);

            MethodInfo doorOpen = AccessTools.Method(typeof(Building_DoorExpanded), "DoorOpen");
            doorOpen.Invoke(door, new object[] { DEFAULT_TICK_TO_CLOSE });
        }

        public override void Fix(ThingWithComps thing, bool originalHoldOpenValue, ref bool forcedOpen)
        {
            Building_DoorExpanded door = thing as Building_DoorExpanded;
            Traverse traverse = Traverse.Create(door);
            traverse.Field("holdOpenInt").SetValue(originalHoldOpenValue);
            forcedOpen = false;

            MethodInfo doorOpen = AccessTools.Method(typeof(Building_DoorExpanded), "DoorOpen");
            doorOpen.Invoke(door, new object[] { DEFAULT_TICK_TO_CLOSE });

        }
    }
}
