using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    class ThinkNode_ConditionalNonPlayerFaction : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.Faction != null && pawn.Faction != Faction.OfPlayer;
        }
    }
}
