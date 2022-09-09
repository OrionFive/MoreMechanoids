using DoorsExpanded;
using MoreMechanoids;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreMechanoidsDoorsExpanded
{
    public class Verb_UnlockDoorsExpanded : Verb_UnlockDoor
    {
        protected override bool AreAllowedThing(out ThingWithComps door)
        {
            bool isADoor = currentTarget.Thing is Building_DoorExpanded;
            door = currentTarget.Thing as ThingWithComps;
            return isADoor;
        }

        protected override bool AreDoorOpen(ThingWithComps thing)
        {
            return thing is Building_DoorExpanded door && (door.Open || door.IsForcedOpen());
        }

        public override bool IsUsableOn(Thing target)
        {
            return target is Building_DoorExpanded;
        }
    }
}
