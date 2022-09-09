using MoreMechanoids;
using MoreMechanoidsDoorsExpanded.Mechanoids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreMechanoidsDoorsExpanded
{
    public class ComProperties_DoorExapandedForceable : CompProperties_Forceable
    {
        public ComProperties_DoorExapandedForceable()
        {
            compClass = typeof(CompForceable);
            DoorHandler = new DoorExpandedHandler();
        }
    }
}
