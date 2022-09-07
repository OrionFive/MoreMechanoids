using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreMechanoids
{
    public class DoorHandler
    {
        public virtual void Force(ThingWithComps thing, ref bool originalHoldOpenValue, ref bool forcedOpen)
        {
            Building_Door door = thing as Building_Door;
            if( door == null)
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

            Building_Door door = thing as Building_Door;
            if (door == null)
            {
                Log.Error($"Cant get as Building_Door: {thing.GetType()}");
                return;
            }
            door.holdOpenInt = originalHoldOpenValue;
            forcedOpen = false;
            door.DoorOpen();
        }
    }
}
