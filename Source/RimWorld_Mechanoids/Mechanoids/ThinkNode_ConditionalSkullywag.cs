using RimWorld;
using Verse;
using Verse.AI;

namespace MoreMechanoids;

public class ThinkNode_ConditionalSkullywag : ThinkNode_Conditional
{
    public override bool Satisfied(Pawn pawn)
    {
        return pawn.def.defName == "Mech_Skullywag" && pawn.Faction != null && pawn.Faction != Faction.OfPlayer;
    }
}