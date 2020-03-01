using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class ThinkNode_ConditionalNonPlayerFaction : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.Faction != null && pawn.Faction != Faction.OfPlayer;
        }
    }
}
